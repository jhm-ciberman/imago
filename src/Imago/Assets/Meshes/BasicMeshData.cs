using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid;

namespace Imago.Assets.Meshes;

/// <summary>
/// Represents mesh data for a basic 3D model, including positions, normals, and texture coordinates.
/// </summary>
public class BasicMeshData : MeshData
{
    /// <summary>
    /// Gets or sets the array of normal vectors for each vertex.
    /// </summary>
    public Vector3[] Normals { get; set; }

    /// <summary>
    /// Gets or sets the array of 2D texture coordinates for each vertex.
    /// </summary>
    public Vector2[] TexCoords { get; set; }

    /// <inheritdoc/>
    public override VertexFormat VertexFormat => BasicVertex.VertexFormat;

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicMeshData"/> class.
    /// </summary>
    /// <param name="indices">The array of indices defining the triangles.</param>
    /// <param name="positions">The array of vertex positions.</param>
    /// <param name="normals">Optional: The array of normal vectors. If null, normals will be recomputed.</param>
    /// <param name="texCoords">Optional: The array of texture coordinates. If null, default zero vectors will be used.</param>
    public BasicMeshData(ushort[] indices, Vector3[] positions, Vector3[]? normals, Vector2[]? texCoords) : base(indices, positions)
    {
        this.Normals = normals ?? new Vector3[positions.Length];
        this.TexCoords = texCoords ?? new Vector2[positions.Length];

        if (normals == null)
            this.RecomputeNormals();
    }

    /// <inheritdoc/>
    protected override void Validate()
    {
        base.Validate();

        if (this.Normals.Length != this.Positions.Length)
            throw new ArgumentException("The number of normals must match the number of positions.");

        if (this.TexCoords.Length != this.Positions.Length)
            throw new ArgumentException("The number of texture coordinates must match the number of positions.");
    }

    /// <inheritdoc/>
    public override DeviceBuffer CreateVertexBuffer(GraphicsDevice gd)
    {
        this.Validate();

        BasicVertex[] vertices = ArrayPool<BasicVertex>.Shared.Rent(this.Positions.Length);
        for (var i = 0; i < this.Positions.Length; i++)
        {
            vertices[i].Position = this.Positions[i];
            vertices[i].Normal = this.Normals[i];
            vertices[i].TexCoord = this.TexCoords[i];
        }
        uint sizeInBytes = (uint)(this.Positions.Length * Unsafe.SizeOf<BasicVertex>());
        DeviceBuffer vertexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription(sizeInBytes, BufferUsage.VertexBuffer));
        gd.UpdateBuffer(vertexBuffer, 0, new ReadOnlySpan<BasicVertex>(vertices, 0, this.Positions.Length));

        ArrayPool<BasicVertex>.Shared.Return(vertices);
        return vertexBuffer;
    }

    /// <summary>
    /// Creates a deep clone of this <see cref="BasicMeshData"/> instance.
    /// </summary>
    /// <returns>A new <see cref="BasicMeshData"/> object that is a copy of this instance.</returns>
    public BasicMeshData Clone()
    {
        ushort[] indices = (ushort[]) this.Indices.Clone();
        Vector3[] positions = (Vector3[]) this.Positions.Clone();
        Vector3[] normals = (Vector3[]) this.Normals.Clone();
        Vector2[] texCoords = (Vector2[]) this.TexCoords.Clone();
        return new BasicMeshData(indices, positions, normals, texCoords);
    }

    /// <summary>
    /// Recomputes the normal vectors for each vertex based on the face normals of the mesh.
    /// This method assumes a triangle list topology.
    /// </summary>
    public void RecomputeNormals()
    {
        for (var i = 0; i < this.Indices.Length; i += 3)
        {
            ushort index1 = this.Indices[i + 0];
            ushort index2 = this.Indices[i + 1];
            ushort index3 = this.Indices[i + 2];

            Vector3 p1 = this.Positions[index1];
            Vector3 p2 = this.Positions[index2];
            Vector3 p3 = this.Positions[index3];

            Vector3 normal = Vector3.Cross(p3 - p2, p1 - p2);

            normal = Vector3.Normalize(normal);

            this.Normals[index1] = normal;
            this.Normals[index2] = normal;
            this.Normals[index3] = normal;
        }
    }


    /// <summary>
    /// Inverts the direction of all normal vectors in the mesh data.
    /// </summary>
    public void FlipNormals()
    {
        for (int i = 0; i < this.Normals.Length; i++)
        {
            this.Normals[i] = -this.Normals[i];
        }
    }

    /// <summary>
    /// Flips the winding order of the faces and inverts the normal vectors.
    /// This effectively reverses the visible side of the mesh.
    /// </summary>
    public void FlipFaces()
    {
        this.FlipIndices();
        this.FlipNormals();
    }

    /// <summary>
    /// Merges the provided <see cref="BasicMeshData"/> into this instance.
    /// The new mesh data will be appended, and indices will be adjusted accordingly.
    /// </summary>
    /// <param name="mesh">The <see cref="BasicMeshData"/> to merge.</param>
    /// <returns>A new <see cref="BasicMeshData"/> instance containing the merged data.</returns>
    public BasicMeshData Merge(BasicMeshData mesh)
    {
        Vector3[] positions = new Vector3[mesh.Positions.Length + this.Positions.Length];
        Vector3[] normals = new Vector3[mesh.Normals.Length + this.Normals.Length];
        Vector2[] uvs = new Vector2[mesh.TexCoords.Length + this.TexCoords.Length];
        ushort[] indices = new ushort[mesh.Indices.Length + this.Indices.Length];

        Array.Copy(this.Positions, positions, this.Positions.Length);
        Array.Copy(mesh.Positions, 0, positions, this.Positions.Length, mesh.Positions.Length);

        Array.Copy(this.Normals, normals, this.Normals.Length);
        Array.Copy(mesh.Normals, 0, normals, this.Normals.Length, mesh.Normals.Length);

        Array.Copy(this.TexCoords, uvs, this.TexCoords.Length);
        Array.Copy(mesh.TexCoords, 0, uvs, this.TexCoords.Length, mesh.TexCoords.Length);

        // Indices should be offset by the number of vertices in the first mesh
        Array.Copy(this.Indices, indices, this.Indices.Length);
        for (int i = 0; i < mesh.Indices.Length; i++)
        {
            indices[i + this.Indices.Length] = (ushort)(mesh.Indices[i] + this.Positions.Length);
        }

        return new BasicMeshData(indices, positions, normals, uvs);
    }

}
