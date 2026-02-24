using System.Numerics;
using Imago.Support.Numerics;

namespace Imago.Assets.Gltf;

/// <summary>
/// Represents a special glTF buffer view that always returns zeroed or identity data.
/// This is used when an accessor references a buffer view that is not present, providing default values.
/// </summary>
internal class GltfBufferViewZeroed : IGltfBufferView
{
    public static GltfBufferViewZeroed Instance { get; } = new GltfBufferViewZeroed();

    private GltfBufferViewZeroed()
    {
        // Use the static Instance property instead.
    }

    /// <inheritdoc/>
    public Vector2[] ReadVector2Array(int offset, int count)
    {
        return new Vector2[count];
    }

    /// <inheritdoc/>
    public Vector3[] ReadVector3Array(int offset, int count)
    {
        return new Vector3[count];
    }

    /// <inheritdoc/>
    public Vector4[] ReadVector4Array(int offset, int count)
    {
        return new Vector4[count];
    }

    /// <inheritdoc/>
    public ushort[] ReadUShortArray(int offset, int count)
    {
        return new ushort[count];
    }

    /// <inheritdoc/>
    public Vector4UShort[] ReadUShort4Array(int offset, int count)
    {
        return new Vector4UShort[count];
    }

    /// <inheritdoc/>
    public uint[] ReadUIntArray(int offset, int count)
    {
        return new uint[count];
    }

    /// <inheritdoc/>
    public byte[] ReadByteArray(int offset, int count)
    {
        return new byte[count];
    }

    /// <inheritdoc/>
    public float[] ReadFloatArray(int offset, int count)
    {
        return new float[count];
    }

    /// <inheritdoc/>
    public sbyte[] ReadSByteArray(int offset, int count)
    {
        return new sbyte[count];
    }

    /// <inheritdoc/>
    public short[] ReadShortArray(int offset, int count)
    {
        return new short[count];
    }

    /// <inheritdoc/>
    public Quaternion[] ReadQuaternionArray(int offset, int count)
    {
        return new Quaternion[count];
    }

    /// <inheritdoc/>
    public Matrix4x4[] ReadMatrix4x4Array(int offset, int count)
    {
        Matrix4x4[] matrices = new Matrix4x4[count];
        for (int i = 0; i < matrices.Length; i++)
        {
            matrices[i] = Matrix4x4.Identity;
        }
        return matrices;
    }
}
