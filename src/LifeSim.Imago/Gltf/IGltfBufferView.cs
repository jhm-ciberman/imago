using System.Numerics;
using LifeSim.Support.Numerics;

namespace LifeSim.Imago.Gltf;

internal interface IGltfBufferView
{
    Vector2[] ReadVector2Array(int offset, int count);
    Vector3[] ReadVector3Array(int offset, int count);
    Vector4[] ReadVector4Array(int offset, int count);
    ushort[] ReadUShortArray(int offset, int count);
    Vector4UShort[] ReadUShort4Array(int offset, int count);
    uint[] ReadUIntArray(int offset, int count);
    byte[] ReadByteArray(int offset, int count);
    float[] ReadFloatArray(int offset, int count);
    Matrix4x4[] ReadMatrix4x4Array(int offset, int count);
    Quaternion[] ReadQuaternionArray(int offset, int count);
    sbyte[] ReadSByteArray(int offset, int count);
    short[] ReadShortArray(int offset, int count);
}
