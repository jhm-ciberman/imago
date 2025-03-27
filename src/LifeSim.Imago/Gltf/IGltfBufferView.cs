using System.Numerics;
using LifeSim.Support.Numerics;

namespace LifeSim.Imago.Gltf;

internal interface IGltfBufferView
{
    public Vector2[] ReadVector2Array(int offset, int count);
    public Vector3[] ReadVector3Array(int offset, int count);
    public Vector4[] ReadVector4Array(int offset, int count);
    public ushort[] ReadUShortArray(int offset, int count);
    public Vector4UShort[] ReadUShort4Array(int offset, int count);
    public uint[] ReadUIntArray(int offset, int count);
    public byte[] ReadByteArray(int offset, int count);
    public float[] ReadFloatArray(int offset, int count);
    public Matrix4x4[] ReadMatrix4x4Array(int offset, int count);
    public Quaternion[] ReadQuaternionArray(int offset, int count);
    public sbyte[] ReadSByteArray(int offset, int count);
    public short[] ReadShortArray(int offset, int count);
}
