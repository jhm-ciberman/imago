using System.Numerics;

namespace LifeSim.Engine.Gltf;

internal class GltfBufferViewZeroed : IGltfBufferView
{

    public Vector2[] ReadVector2Array(int offset, int count) => new Vector2[count];
    public Vector3[] ReadVector3Array(int offset, int count) => new Vector3[count];
    public Vector4[] ReadVector4Array(int offset, int count) => new Vector4[count];
    public ushort[] ReadUShortArray(int offset, int count) => new ushort[count];
    public Vector4UShort[] ReadUShort4Array(int offset, int count) => new Vector4UShort[count];
    public uint[] ReadUIntArray(int offset, int count) => new uint[count];
    public byte[] ReadByteArray(int offset, int count) => new byte[count];
    public float[] ReadFloatArray(int offset, int count) => new float[count];
    public sbyte[] ReadSByteArray(int offset, int count) => new sbyte[count];
    public short[] ReadShortArray(int offset, int count) => new short[count];
    public Quaternion[] ReadQuaternionArray(int offset, int count) => new Quaternion[count];

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
