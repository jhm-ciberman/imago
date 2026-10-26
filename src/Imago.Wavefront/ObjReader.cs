using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using Imago.Graphics3D.Meshes;

namespace Imago.Wavefront;

/// <summary>
/// Reads meshes and scenes from a Wavefront OBJ file.
/// </summary>
/// <remarks>
/// Only what you read is loaded on the GPU. The results belong to the caller and keep working after the reader is disposed.
/// </remarks>
public sealed class ObjReader : IDisposable
{
    /// <summary>
    /// Gets the names of all the groups in the file.
    /// </summary>
    public IReadOnlyList<string> GroupNames => this._groupNames;

    private readonly List<Vector3> _positions = new List<Vector3>();
    private readonly List<Vector3> _normals = new List<Vector3>();
    private readonly List<Vector2> _texCoords = new List<Vector2>();
    private readonly Dictionary<string, List<(int positionIndex, int? texCoordIndex, int? normalIndex)>> _groups = new Dictionary<string, List<(int, int?, int?)>>();
    private readonly List<string> _groupNames = new List<string>();
    private string _currentGroup = "default";
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjReader"/> class.
    /// </summary>
    /// <param name="path">The path to the OBJ file.</param>
    public ObjReader(string path)
    {
        using (var reader = new StreamReader(path))
        {
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                this.ProcessLine(line);
            }
        }
    }

    /// <summary>
    /// Reads the whole file as a scene, or a single group.
    /// </summary>
    /// <param name="groupName">The name of the group to read. If null, all the groups are read.</param>
    /// <returns>The scene. The caller owns it.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the group is not found.</exception>
    public ObjScene ReadScene(string? groupName = null)
    {
        ObjectDisposedException.ThrowIf(this._isDisposed, this);

        if (groupName != null)
        {
            Mesh mesh = this.ReadMesh(groupName) ?? throw new KeyNotFoundException($"Could not find group with name {groupName}");
            return new ObjScene(mesh);
        }

        var groups = new List<KeyValuePair<string, Mesh>>();
        foreach (string name in this._groupNames)
        {
            groups.Add(new KeyValuePair<string, Mesh>(name, this.CreateMesh(this._groups[name])));
        }

        return new ObjScene("OBJ_Root", groups);
    }

    /// <summary>
    /// Reads the mesh of a group.
    /// </summary>
    /// <param name="groupName">The name of the group.</param>
    /// <returns>The mesh, or null if the group is not found. The caller owns it.</returns>
    public Mesh? ReadMesh(string groupName)
    {
        ObjectDisposedException.ThrowIf(this._isDisposed, this);

        if (this._groups.TryGetValue(groupName, out var groupData))
        {
            return this.CreateMesh(groupData);
        }

        return null;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this._isDisposed = true;
        this._positions.Clear();
        this._normals.Clear();
        this._texCoords.Clear();
        this._groups.Clear();
    }

    private void ProcessLine(string line)
    {
        line = line.Trim();
        if (string.IsNullOrEmpty(line) || line.StartsWith('#')) return;

        string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return;

        var args = new Span<string>(parts, 1, parts.Length - 1);

        switch (parts[0])
        {
            case "v":
                this._positions.Add(ParseVector3(args));
                break;
            case "vn":
                this._normals.Add(ParseVector3(args));
                break;
            case "vt":
                var texCoord = ParseVector2(args);
                this._texCoords.Add(new Vector2(texCoord.X, 1f - texCoord.Y));
                break;
            case "f":
                this.ProcessFace(args);
                break;
            case "g":
            case "o":
                var group = args.Length > 0 ? args[0] : "unnamed_" + this._groups.Count;
                this.ProcessGroup(group);
                break;
            case "usemtl":
            case "mtllib":
                break; // Ignored. Materials should be defined by code.
            case "s":
                break; // Ignored. Smoothing groups are not supported. Just use normals.
            default:
#if DEBUG
                throw new NotSupportedException($"Unsupported OBJ directive '{parts[0]}' on line: {line}");
#else
                break;
#endif
        }
    }

    private void ProcessGroup(string groupName)
    {
        this._currentGroup = groupName;
        if (!this._groups.ContainsKey(this._currentGroup))
        {
            this._groups[this._currentGroup] = new List<(int, int?, int?)>();
            this._groupNames.Add(this._currentGroup);
        }
    }

    private static Vector3 ParseVector3(Span<string> args)
    {
        return new Vector3(
            float.Parse(args[0], CultureInfo.InvariantCulture),
            float.Parse(args[1], CultureInfo.InvariantCulture),
            float.Parse(args[2], CultureInfo.InvariantCulture)
        );
    }

    private static Vector2 ParseVector2(Span<string> args)
    {
        return new Vector2(
            float.Parse(args[0], CultureInfo.InvariantCulture),
            float.Parse(args[1], CultureInfo.InvariantCulture)
        );
    }

    private void ProcessFace(Span<string> args)
    {
        if (args.Length < 3) throw new InvalidOperationException("Face must have at least 3 vertices.");
        if (args.Length > 3) args = TriangulateFace(args);

        for (int i = 0; i < args.Length; i++)
        {
            string[] vertexData = args[i].Split('/');

            int positionIndex = int.Parse(vertexData[0]) - 1;
            int? texCoordIndex = ParseOptionalIndex(vertexData, 1);
            int? normalIndex = ParseOptionalIndex(vertexData, 2);

            this._groups[this._currentGroup].Add((positionIndex, texCoordIndex, normalIndex));
        }
    }

    private static int? ParseOptionalIndex(string[] vertexData, int part)
    {
        if (part >= vertexData.Length || vertexData[part].Length == 0) return null;

        return int.Parse(vertexData[part]) - 1;
    }

    private static Span<string> TriangulateFace(Span<string> args)
    {
        // Triangulate the face by creating a triangle fan.
        int size = (args.Length - 2) * 3;
        var result = new string[size];

        result[0] = args[0];
        result[1] = args[1];
        result[2] = args[2];
        int j = 3;
        for (int i = 3; i < args.Length; i++)
        {
            result[j++] = args[0];
            result[j++] = args[i - 1];
            result[j++] = args[i];
        }

        return result;
    }

    private Mesh CreateMesh(List<(int positionIndex, int? texCoordIndex, int? normalIndex)> groupData)
    {
        var uniqueVertices = new Dictionary<(int, int?, int?), int>();
        var positions = new List<Vector3>();
        var texCoords = new List<Vector2>();
        var normals = new List<Vector3>();
        var indices = new List<int>();

        foreach (var vertex in groupData)
        {
            if (!uniqueVertices.TryGetValue(vertex, out int index))
            {
                index = positions.Count;
                uniqueVertices[vertex] = index;

                positions.Add(this._positions[vertex.positionIndex]);
                if (vertex.texCoordIndex.HasValue) texCoords.Add(this._texCoords[vertex.texCoordIndex.Value]);
                if (vertex.normalIndex.HasValue) normals.Add(this._normals[vertex.normalIndex.Value]);
            }

            indices.Add(index);
        }

        var meshData = new BasicMeshData(
            indices.Select(i => (ushort)i).ToArray(),
            positions.ToArray(),
            normals.Count > 0 ? normals.ToArray() : null,
            texCoords.Count > 0 ? texCoords.ToArray() : null
        );

        return new Mesh(meshData);
    }
}
