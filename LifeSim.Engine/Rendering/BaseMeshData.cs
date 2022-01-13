using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering;

public abstract class BaseMeshData<TVertex> : IMeshData where TVertex : unmanaged
{
    private static VertexFormat? _cachedVertexFormat;

    public ushort[] Indices { get; private set; }

    public TVertex[] Vertices { get; private set; }

    public BaseMeshData(ushort[] indices, TVertex[] vertices)
    {
        this.Indices = indices;
        this.Vertices = vertices;
    }

    protected abstract VertexFormat MakeVertexFormat();
    protected abstract Vector3 GetPosition(int index);

    public void FlipIndices()
    {
        for (var i = 0; i < this.Indices.Length; i += 3)
        {
            var a = this.Indices[i + 0];
            var b = this.Indices[i + 1];
            var c = this.Indices[i + 2];

            this.Indices[i + 0] = c;
            this.Indices[i + 1] = b;
            this.Indices[i + 2] = a;
        }
    }

    public VertexFormat GetVertexFormat()
    {
        if (_cachedVertexFormat == null)
        {
            _cachedVertexFormat = this.MakeVertexFormat();
        }
        return _cachedVertexFormat;
    }

    public DeviceBuffer CreateVertexBuffer(ResourceFactory factory, CommandList cl)
    {
        DeviceBuffer vertexBuffer = factory.CreateBuffer(new BufferDescription((uint) (Marshal.SizeOf<TVertex>() * this.Vertices.Length), BufferUsage.VertexBuffer));
        cl.UpdateBuffer<TVertex>(vertexBuffer, 0, this.Vertices);
        return vertexBuffer;
    }

    public DeviceBuffer CreateIndexBuffer(ResourceFactory factory, CommandList cl, out int indexCount)
    {
        indexCount = this.Indices.Length;
        DeviceBuffer indexBuffer = factory.CreateBuffer(new BufferDescription((uint) (sizeof(ushort) * indexCount), BufferUsage.IndexBuffer));
        cl.UpdateBuffer<ushort>(indexBuffer, 0, this.Indices);
        return indexBuffer;
    }

    public bool RayCast(Ray ray, out float distance)
    {
        distance = float.MaxValue;
        bool result = false;
        for (int i = 0; i < this.Indices.Length - 2; i += 3)
        {
            Vector3 v0 = this.GetPosition(this.Indices[i + 0]);
            Vector3 v1 = this.GetPosition(this.Indices[i + 1]);
            Vector3 v2 = this.GetPosition(this.Indices[i + 2]);

            if (ray.Intersects(ref v0, ref v1, ref v2, out float newDistance))
            {
                if (newDistance < distance)
                {
                    distance = newDistance;
                }

                result = true;
            }
        }

        return result;
    }

    public int RayCast(Ray ray, List<float> distances)
    {
        int hits = 0;
        for (int i = 0; i < this.Indices.Length - 2; i += 3)
        {
            Vector3 v0 = this.GetPosition(this.Indices[i + 0]);
            Vector3 v1 = this.GetPosition(this.Indices[i + 1]);
            Vector3 v2 = this.GetPosition(this.Indices[i + 2]);

            if (ray.Intersects(ref v0, ref v1, ref v2, out float newDistance))
            {
                hits++;
                distances.Add(newDistance);
            }
        }

        return hits;
    }

    public Vector3[] GetVertexPositions()
    {
        var positions = new Vector3[this.Vertices.Length];
        for (int i = 0; i < 0; i++)
        {
            positions[i] = this.GetPosition(i);
        }
        return positions;
    }

    public ushort[] GetIndices()
    {
        return this.Indices;
    }

    public unsafe BoundingBox GetBoundingBox()
    {
        fixed (TVertex* vertexPtr = &this.Vertices[0])
        {
            Vector3* positionPtr = (Vector3*)vertexPtr;
            return BoundingBox.CreateFromPoints(
                positionPtr,
                this.Vertices.Length,
                Marshal.SizeOf<TVertex>(),
                Quaternion.Identity,
                Vector3.Zero,
                Vector3.One);
        }
    }

    public unsafe BoundingSphere GetBoundingSphere()
    {
        fixed (TVertex* vertexPtr = &this.Vertices[0])
        {
            Vector3* positionPtr = (Vector3*)vertexPtr;
            return BoundingSphere.CreateFromPoints(positionPtr, this.Vertices.Length, Marshal.SizeOf<TVertex>());
        }
    }
}