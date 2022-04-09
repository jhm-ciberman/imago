using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.InteropServices;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.Rendering;
using Veldrid;

namespace LifeSim.Engine;

public class BasicMeshData : BaseMeshData
{
    public Vector3[] Normals { get; set; }

    public Vector2[] TexCoords { get; set; }

    public override VertexFormat VertexFormat => BasicVertex.VertexFormat;

    public BasicMeshData(ushort[] indices, Vector3[] positions, Vector3[]? normals, Vector2[]? texCoords) : base(indices, positions)
    {
        this.Normals = normals ?? new Vector3[positions.Length];
        this.TexCoords = texCoords ?? new Vector2[positions.Length];

        if (normals == null)
        {
            this.RecomputeNormals();
        }
    }

    protected override void Validate()
    {
        base.Validate();

        if (this.Normals.Length != this.Positions.Length)
        {
            throw new ArgumentException("The number of normals must match the number of positions.");
        }

        if (this.TexCoords.Length != this.Positions.Length)
        {
            throw new ArgumentException("The number of texture coordinates must match the number of positions.");
        }
    }

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

        DeviceBuffer vertexBuffer = BufferFactory.CreateVertexBuffer(gd, vertices);
        ArrayPool<BasicVertex>.Shared.Return(vertices);
        return vertexBuffer;
    }

    public BasicMeshData Clone()
    {
        ushort[] indices = (ushort[]) this.Indices.Clone();
        Vector3[] positions = (Vector3[]) this.Positions.Clone();
        Vector3[] normals = (Vector3[]) this.Normals.Clone();
        Vector2[] texCoords = (Vector2[]) this.TexCoords.Clone();
        return new BasicMeshData(indices, positions, normals, texCoords);
    }

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

            Vector3 normal = Vector3.Cross((p3 - p2), (p1 - p2));

            normal = Vector3.Normalize(normal);

            this.Normals[index1] = normal;
            this.Normals[index2] = normal;
            this.Normals[index3] = normal;
        }
    }


    public void FlipNormals()
    {
        for (int i = 0; i < this.Normals.Length; i++)
        {
            this.Normals[i] = -this.Normals[i];
        }
    }

    public void FlipFaces()
    {
        this.FlipIndices();
        this.FlipNormals();
    }

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