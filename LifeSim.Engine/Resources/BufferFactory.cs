using System.Runtime.CompilerServices;
using Veldrid;

namespace LifeSim.Engine.Resources;

public static class BufferFactory
{
    public static DeviceBuffer CreateVertexBuffer<T>(GraphicsDevice gd, T[] vertices) where T : unmanaged
    {
        var buffer = gd.ResourceFactory.CreateBuffer(new BufferDescription(
            (uint)(vertices.Length * Unsafe.SizeOf<T>()), BufferUsage.VertexBuffer));
        gd.UpdateBuffer(buffer, 0, vertices);
        return buffer;
    }

    public static DeviceBuffer CreateIndexBuffer(GraphicsDevice gd, ushort[] indices)
    {
        var buffer = gd.ResourceFactory.CreateBuffer(new BufferDescription(
            (uint)(indices.Length * sizeof(ushort)), BufferUsage.IndexBuffer));
        gd.UpdateBuffer(buffer, 0, indices);
        return buffer;
    }
}