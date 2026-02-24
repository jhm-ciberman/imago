using System;
using System.Collections.Generic;
using System.Numerics;

namespace Imago.Assets.Meshes;

/// <summary>
/// Builds basic mesh data with position and texture coordinates, using incremental geometry accumulation.
/// </summary>
public class BasicMeshBuilder
{
    private static readonly ushort[] _quadIndicesTLBR = [0, 2, 1, 2, 3, 1];
    private static readonly ushort[] _quadIndicesBLTR = [0, 2, 3, 0, 3, 1];
    private static readonly ushort[] _triIndices = [0, 1, 2];

    private readonly List<Vector3> _positions = [];

    private readonly List<Vector2> _texCoords = [];

    private readonly List<ushort> _indices = [];

    /// <summary>
    /// Gets the number of vertices in the mesh.
    /// </summary>
    public int VerticesCount => this._positions.Count;

    /// <summary>
    /// Gets the number of remaining vertices that can be added to the mesh before it is full.
    /// </summary>
    public int RemainingCapacity => ushort.MaxValue - this._indices.Count;

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicMeshBuilder"/> class.
    /// </summary>
    public BasicMeshBuilder()
    {
        //
    }

    /// <summary>
    /// Constructs mesh data from the builder.
    /// </summary>
    /// <returns>The mesh data, or null if no geometry was added.</returns>
    public BasicMeshData? MakeMeshData()
    {
        if (this._positions.Count == 0) return null;

        return new BasicMeshData(this._indices.ToArray(), this._positions.ToArray(), normals: null, texCoords: this._texCoords.ToArray());
    }

    /// <summary>
    /// Adds a quad to the mesh with a quad diagonal from top-left to bottom-right.
    /// </summary>
    /// <param name="tl">The top-left vertex.</param>
    /// <param name="tr">The top-right vertex.</param>
    /// <param name="bl">The bottom-left vertex.</param>
    /// <param name="br">The bottom-right vertex.</param>
    public void AddQuadTLBR(BasicVertex tl, BasicVertex tr, BasicVertex bl, BasicVertex br)
    {
        ReadOnlySpan<BasicVertex> verts = [tl, tr, bl, br];
        this.AddRange(verts, _quadIndicesTLBR);
    }

    /// <summary>
    /// Adds a quad to the mesh with a quad diagonal from bottom-left to top-right.
    /// </summary>
    /// <param name="tl">The top-left vertex.</param>
    /// <param name="tr">The top-right vertex.</param>
    /// <param name="bl">The bottom-left vertex.</param>
    /// <param name="br">The bottom-right vertex.</param>
    public void AddQuadBLTR(BasicVertex tl, BasicVertex tr, BasicVertex bl, BasicVertex br)
    {
        ReadOnlySpan<BasicVertex> verts = [tl, tr, bl, br];
        this.AddRange(verts, _quadIndicesBLTR);
    }

    /// <summary>
    /// Adds a triangle to the mesh.
    /// </summary>
    /// <param name="a">The first vertex.</param>
    /// <param name="b">The second vertex.</param>
    /// <param name="c">The third vertex.</param>
    public void AddTriangle(BasicVertex a, BasicVertex b, BasicVertex c)
    {
        ReadOnlySpan<BasicVertex> verts = [a, b, c];
        this.AddRange(verts, _triIndices);
    }

    /// <summary>
    /// Adds a range of vertices and triangles to the mesh.
    /// </summary>
    /// <param name="verts">The vertices to add.</param>
    /// <param name="tris">The triangle indices relative to the start of the vertex range.</param>
    public void AddRange(ReadOnlySpan<BasicVertex> verts, ReadOnlySpan<ushort> tris)
    {
        var startIndex = this.VerticesCount;

        for (int i = 0; i < verts.Length; i++)
        {
            this._positions.Add(verts[i].Position);
            this._texCoords.Add(verts[i].TexCoord);
        }

        for (int i = 0; i < tris.Length; i++)
        {
            this._indices.Add((ushort)(tris[i] + startIndex));
        }
    }
}
