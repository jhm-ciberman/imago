using System.Collections.Generic;
using Imago.Assets.TexturePacking;
using Imago.Assets.Textures;

namespace Imago.Assets.Meshes;

/// <summary>
/// A class responsible for building a mesh with multiple textures.
/// </summary>
public class MultitextureLitMeshBuilder
{
    private readonly List<LitMeshBuilder> _meshParts = new List<LitMeshBuilder>(1);

    /// <summary>
    /// Initializes a new instance of the <see cref="MultitextureLitMeshBuilder"/> class.
    /// </summary>
    public MultitextureLitMeshBuilder()
    {
        //
    }

    /// <summary>
    /// Builds mesh data from the added vertices and triangles.
    /// </summary>
    /// <returns>An array of mesh data.</returns>
    public LitMeshBuildData[] MakeMeshesData()
    {
        var arr = new LitMeshBuildData[this._meshParts.Count];
        for (int i = 0; i < this._meshParts.Count; i++)
        {
            arr[i] = this._meshParts[i].MakeMeshData();
        }
        return arr;
    }

    private LitMeshBuilder FindMesh(Texture texture, int requiredVertexCount)
    {
        for (int i = 0; i < this._meshParts.Count; i++)
        {
            var part = this._meshParts[i];
            if (part.Texture == texture && part.RemainingCapacity >= requiredVertexCount)
            {
                return part;
            }
        }

        LitMeshBuilder meshBuilder = new LitMeshBuilder(texture);
        this._meshParts.Add(meshBuilder);
        return meshBuilder;
    }

    /// <summary>
    /// Adds a quad to the mesh with a quad diagonal from top-left to bottom-right.
    /// </summary>
    /// <param name="texture">The texture region to map onto the geometry.</param>
    /// <param name="tl">The top-left vertex.</param>
    /// <param name="tr">The top-right vertex.</param>
    /// <param name="bl">The bottom-left vertex.</param>
    /// <param name="br">The bottom-right vertex.</param>
    /// <param name="horizontalMirrorUV">Whether to mirror the texture coordinates horizontally.</param>
    /// <param name="normalPointsNegative">Whether to reverse the triangle winding so the face points the opposite way.</param>
    public void AddQuadTLBR(ITextureRegion texture, LitVertex tl, LitVertex tr, LitVertex bl, LitVertex br, bool horizontalMirrorUV = false, bool normalPointsNegative = false)
    {
        this.FindMesh(texture.Texture, 6)
            .SetTextureRect(texture.TopLeft, texture.BottomRight)
            .AddQuadTLBR(tl, tr, bl, br, horizontalMirrorUV, normalPointsNegative);
    }

    /// <summary>
    /// Adds a quad to the mesh with a quad diagonal from bottom-left to top-right.
    /// </summary>
    /// <param name="texture">The texture region to map onto the geometry.</param>
    /// <param name="tl">The top-left vertex.</param>
    /// <param name="tr">The top-right vertex.</param>
    /// <param name="bl">The bottom-left vertex.</param>
    /// <param name="br">The bottom-right vertex.</param>
    /// <param name="horizontalMirrorUV">Whether to mirror the texture coordinates horizontally.</param>
    /// <param name="normalPointsNegative">Whether to reverse the triangle winding so the face points the opposite way.</param>
    public void AddQuadBLTR(PackedTexture texture, LitVertex tl, LitVertex tr, LitVertex bl, LitVertex br, bool horizontalMirrorUV = false, bool normalPointsNegative = false)
    {
        this.FindMesh(texture.Texture, 6)
            .SetTextureRect(texture.TopLeft, texture.BottomRight)
            .AddQuadBLTR(tl, tr, bl, br, horizontalMirrorUV, normalPointsNegative);
    }

    /// <summary>
    /// Adds a triangle, defined in Counter Clockwise order (CCW).
    /// </summary>
    /// <param name="texture">The texture region to map onto the geometry.</param>
    /// <param name="a">The first vertex.</param>
    /// <param name="b">The second vertex.</param>
    /// <param name="c">The third vertex.</param>
    /// <param name="horizontalMirrorUV">Whether to mirror the texture coordinates horizontally.</param>
    /// <param name="normalPointsNegative">Whether to reverse the triangle winding so the face points the opposite way.</param>
    public void AddTriangle(PackedTexture texture, LitVertex a, LitVertex b, LitVertex c, bool horizontalMirrorUV = false, bool normalPointsNegative = false)
    {
        this.FindMesh(texture.Texture, 3)
            .SetTextureRect(texture.TopLeft, texture.BottomRight)
            .AddTriangle(a, b, c, horizontalMirrorUV, normalPointsNegative);
    }

    /// <summary>
    /// Adds a range of vertices and triangles to the mesh.
    /// </summary>
    /// <param name="texture">The texture region to map onto the geometry.</param>
    /// <param name="verts">The vertices to add.</param>
    /// <param name="tris">The triangle indices relative to the start of the vertex range.</param>
    /// <param name="horizontalMirrorUV">Whether to mirror the texture coordinates horizontally.</param>
    /// <param name="normalPointsNegative">Whether to reverse the triangle winding so the face points the opposite way.</param>
    public void AddRange(PackedTexture texture, LitVertex[] verts, ushort[] tris, bool horizontalMirrorUV = false, bool normalPointsNegative = false)
    {
        this.FindMesh(texture.Texture, tris.Length)
            .SetTextureRect(texture.TopLeft, texture.BottomRight)
            .AddRange(verts, tris, horizontalMirrorUV, normalPointsNegative);
    }
}
