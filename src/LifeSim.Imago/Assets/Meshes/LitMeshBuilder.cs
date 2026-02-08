using System;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Imago.Assets.Materials;
using LifeSim.Imago.Assets.Textures;
using LifeSim.Imago.SceneGraph.Nodes;

namespace LifeSim.Imago.Assets.Meshes;

/// <summary>
/// Builds lit mesh data with per-vertex lighting, texture coordinates, and material support.
/// </summary>
public class LitMeshBuilder
{
    private static readonly ushort[] _quadIndicesTLBR = [0, 2, 1, 2, 3, 1];
    private static readonly ushort[] _quadIndicesBLTR = [0, 2, 3, 0, 3, 1];
    private static readonly ushort[] _triIndices = [0, 1, 2];

    /// <summary>
    /// Gets or sets the texture of the mesh.
    /// </summary>
    public Texture Texture { get; set; }

    private readonly List<Vector3> _positions = [];

    private readonly List<Vector2> _texCoords = [];

    private readonly List<Vector2> _lights = [];

    private readonly List<ushort> _indices = [];

    /// <summary>
    /// Gets the number of vertices in the mesh.
    /// </summary>
    public int VerticesCount => this._positions.Count;

    /// <summary>
    /// Gets the number of remaining vertices that can be added to the mesh before it is full.
    /// </summary>
    public int RemainingCapacity => ushort.MaxValue - this._indices.Count;

    private Vector2 _uv1 = Vector2.Zero;

    private Vector2 _uv2 = Vector2.One;

    /// <summary>
    /// Initializes a new instance of the <see cref="LitMeshBuilder"/> class.
    /// </summary>
    /// <param name="texture">The texture.</param>
    public LitMeshBuilder(Texture texture)
    {
        this.Texture = texture;
    }

    /// <summary>
    /// Constructs mesh data from the builder. Call <see cref="LitMeshBuildData.CreateMesh"/> on the main thread to create GPU resources.
    /// </summary>
    /// <returns>The mesh data.</returns>
    public LitMeshBuildData MakeMeshData()
    {
        var meshData = new LitMeshData(this._indices.ToArray(), this._positions.ToArray(), null, this._texCoords.ToArray(), this._lights.ToArray());
        return new LitMeshBuildData(meshData, this.Texture);
    }

    /// <summary>
    /// Sets the texture rect of the mesh.
    /// </summary>
    /// <param name="uv1"></param>
    /// <param name="uv2"></param>
    /// <returns></returns>
    public LitMeshBuilder SetTextureRect(Vector2 uv1, Vector2 uv2)
    {
        this._uv1 = uv1;
        this._uv2 = uv2;
        return this;
    }

    /// <summary>
    /// Adds a quad to the mesh with a quad diagonal from top-left to bottom-right.
    /// </summary>
    /// <param name="tl"></param>
    /// <param name="tr"></param>
    /// <param name="bl"></param>
    /// <param name="br"></param>
    /// <param name="horizontalMirrorUV"></param>
    /// <param name="normalPointsNegative"></param>
    public void AddQuadTLBR(LitVertex tl, LitVertex tr, LitVertex bl, LitVertex br, bool horizontalMirrorUV = false, bool normalPointsNegative = false)
    {
        ReadOnlySpan<LitVertex> verts = [tl, tr, bl, br];
        this.AddRange(verts, _quadIndicesTLBR, horizontalMirrorUV, normalPointsNegative);
    }

    /// <summary>
    /// Adds a quad to the mesh with a quad diagonal from bottom-left to top-right.
    /// </summary>
    /// <param name="tl"></param>
    /// <param name="tr"></param>
    /// <param name="bl"></param>
    /// <param name="br"></param>
    /// <param name="horizontalMirrorUV"></param>
    /// <param name="normalPointsNegative"></param>
    public void AddQuadBLTR(LitVertex tl, LitVertex tr, LitVertex bl, LitVertex br, bool horizontalMirrorUV = false, bool normalPointsNegative = false)
    {
        ReadOnlySpan<LitVertex> verts = [tl, tr, bl, br];
        this.AddRange(verts, _quadIndicesBLTR, horizontalMirrorUV, normalPointsNegative);
    }

    /// <summary>
    /// Adds a triangle to the mesh.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="horizontalMirrorUV"></param>
    /// <param name="normalPointsNegative"></param>
    public void AddTriangle(LitVertex a, LitVertex b, LitVertex c, bool horizontalMirrorUV = false, bool normalPointsNegative = false)
    {
        ReadOnlySpan<LitVertex> verts = [a, b, c];
        this.AddRange(verts, _triIndices, horizontalMirrorUV, normalPointsNegative);
    }

    /// <summary>
    /// Adds a range of vertices and triangles to the mesh.
    /// </summary>
    /// <param name="verts"></param>
    /// <param name="tris"></param>
    /// <param name="horizontalMirrorUV"></param>
    /// <param name="normalPointsNegative"></param>
    public void AddRange(ReadOnlySpan<LitVertex> verts, ReadOnlySpan<ushort> tris, bool horizontalMirrorUV = false, bool normalPointsNegative = false)
    {
        var startIndex = this.VerticesCount;

        var delta = this._uv2 - this._uv1;
        var start = this._uv1;
        if (horizontalMirrorUV)
        {
            delta.X = -delta.X;
            start.X = this._uv2.X;
        }

        for (int i = 0; i < verts.Length; i++)
        {
            this._positions.Add(verts[i].Position);
            this._texCoords.Add(start + delta * verts[i].TexCoords);
            this._lights.Add(verts[i].Light);
        }

        if (normalPointsNegative)
        {
            for (int i = 0; i < tris.Length; i += 3)
            {
                this._indices.Add((ushort)(tris[i + 1] + startIndex));
                this._indices.Add((ushort)(tris[i + 0] + startIndex));
                this._indices.Add((ushort)(tris[i + 2] + startIndex));
            }
        }
        else
        {
            for (int i = 0; i < tris.Length; i += 3)
            {
                this._indices.Add((ushort)(tris[i + 0] + startIndex));
                this._indices.Add((ushort)(tris[i + 1] + startIndex));
                this._indices.Add((ushort)(tris[i + 2] + startIndex));
            }
        }
    }
}

/// <summary>
/// Intermediate mesh data that can be built on a worker thread.
/// </summary>
/// <param name="MeshData">The raw mesh data.</param>
/// <param name="Texture">The texture for the mesh.</param>
public readonly record struct LitMeshBuildData(LitMeshData MeshData, Texture Texture)
{
    /// <summary>
    /// Creates the final mesh render info. Must be called on the main thread.
    /// </summary>
    /// <param name="material">The material to use for rendering.</param>
    /// <returns>The mesh render info.</returns>
    public MeshRenderInfo CreateMesh(Material material)
    {
        var mesh = new Mesh(this.MeshData);
        return new MeshRenderInfo(material, mesh);
    }
}
