using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using LifeSim.Imago.Graphics.Meshes;

namespace LifeSim.Imago.Wavefront;

public class ObjParser
{
    private readonly List<Vector3> _positions = new List<Vector3>();
    private readonly List<Vector3> _normals = new List<Vector3>();
    private readonly List<Vector2> _texCoords = new List<Vector2>();
    private readonly Dictionary<string, List<(int positionIndex, int? texCoordIndex, int? normalIndex)>> _groups = new Dictionary<string, List<(int, int?, int?)>>();
    private string _currentGroup = "default";

    public ObjNode LoadScene(string path)
    {
        this.ClearData();

        using (var reader = new StreamReader(path))
        {
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                this.ProcessLine(line);
            }
        }

        return this.CreateScene();
    }

    public Mesh? LoadMeshByGroupName(string path, string groupName)
    {
        this.ClearData();

        using (var reader = new StreamReader(path))
        {
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                this.ProcessLine(line);
            }
        }

        if (_groups.TryGetValue(groupName, out var groupData))
        {
            return this.CreateMesh(groupName, groupData);
        }

        return null;
    }

    private void ClearData()
    {
        this._positions.Clear();
        this._normals.Clear();
        this._texCoords.Clear();
        this._groups.Clear();
        this._currentGroup = "default";
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
                Console.WriteLine($"Unsupported line: {line}");
                break;
        }
    }

    private void ProcessGroup(string groupName)
    {
        this._currentGroup = groupName;
        if (!this._groups.ContainsKey(this._currentGroup))
        {
            this._groups[this._currentGroup] = new List<(int, int?, int?)>();
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
            int? texCoordIndex = (int?)int.Parse(vertexData[1]) - 1;
            int? normalIndex = (int?)int.Parse(vertexData[2]) - 1;

            this._groups[this._currentGroup].Add((positionIndex, texCoordIndex, normalIndex));
        }
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

    private ObjNode CreateScene()
    {
        var rootNode = new ObjNode { Name = "OBJ_Root" };

        foreach (var group in this._groups)
        {
            var mesh = this.CreateMesh(group.Key, group.Value);
            rootNode.Groups.Add(group.Key, mesh);
        }

        return rootNode;
    }

    private Mesh CreateMesh(string groupName, List<(int positionIndex, int? texCoordIndex, int? normalIndex)> groupData)
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


