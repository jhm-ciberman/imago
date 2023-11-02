using System;
using System.Numerics;
using Imago.Support;
using static glTFLoader.Schema.Accessor;

namespace Imago.Gltf;

internal class GltfAccessor
{
    private readonly IGltfBufferView _bufferView;
    private readonly int _byteOffset;
    private readonly int _count;
    private readonly ComponentTypeEnum _componentType;
    private readonly bool _normalized;
    public TypeEnum Type { get; }

    public GltfAccessor(IGltfBufferView bufferView, int byteOffset, int count, ComponentTypeEnum componentType, TypeEnum type, bool normalized)
    {
        this._bufferView = bufferView;
        this._byteOffset = byteOffset;
        this._count = count;
        this._componentType = componentType;
        this.Type = type;
        this._normalized = normalized;
    }


    public ushort[] AsIndicesArray()
    {
        return this._componentType switch
        {
            ComponentTypeEnum.UNSIGNED_BYTE => Byte2UShort(this._bufferView.ReadByteArray(this._byteOffset, this._count)),
            ComponentTypeEnum.UNSIGNED_SHORT => this._bufferView.ReadUShortArray(this._byteOffset, this._count),
            ComponentTypeEnum.UNSIGNED_INT => Int2UShort(this._bufferView.ReadUIntArray(this._byteOffset, this._count)),
            _ => throw new NotSupportedException(),
        };
    }

    private static ushort[] Int2UShort(uint[] sourceArr)
    {
        var arr = new ushort[sourceArr.Length];
        for (int i = 0; i < sourceArr.Length; i++)
        {
            arr[i] = (ushort)sourceArr[i];
        }
        return arr;
    }

    private static ushort[] Byte2UShort(byte[] sourceArr)
    {
        var arr = new ushort[sourceArr.Length];
        for (int i = 0; i < sourceArr.Length; i++)
        {
            arr[i] = (ushort)sourceArr[i];
        }
        return arr;
    }

    public float[] AsFloatArray()
    {
        return this._componentType switch
        {
            ComponentTypeEnum.FLOAT => this._bufferView.ReadFloatArray(this._byteOffset, this._count),
            _ => throw new NotSupportedException(),
        };
    }

    public Vector2[] AsVector2Array()
    {
        return this._componentType switch
        {
            ComponentTypeEnum.FLOAT => this._bufferView.ReadVector2Array(this._byteOffset, this._count),
            _ => throw new NotSupportedException(),
        };
    }

    public Vector3[] AsVector3Array()
    {
        return this._componentType switch
        {
            ComponentTypeEnum.FLOAT => this._bufferView.ReadVector3Array(this._byteOffset, this._count),
            _ => throw new NotSupportedException(),
        };
    }

    public Vector4[] AsVector4Array()
    {
        return this._componentType switch
        {
            ComponentTypeEnum.FLOAT => this._bufferView.ReadVector4Array(this._byteOffset, this._count),
            _ => throw new NotSupportedException(),
        };
    }

    public Quaternion[] AsQuaternionArray()
    {
        return this._componentType switch
        {
            ComponentTypeEnum.FLOAT => this._bufferView.ReadQuaternionArray(this._byteOffset, this._count),
            ComponentTypeEnum.BYTE => this.NormalizeToQuaternion(this._bufferView.ReadSByteArray(this._byteOffset, this._count)),
            ComponentTypeEnum.UNSIGNED_BYTE => this.NormalizeToQuaternion(this._bufferView.ReadByteArray(this._byteOffset, this._count)),
            ComponentTypeEnum.SHORT => this.NormalizeToQuaternion(this._bufferView.ReadShortArray(this._byteOffset, this._count)),
            ComponentTypeEnum.UNSIGNED_SHORT => this.NormalizeToQuaternion(this._bufferView.ReadUShortArray(this._byteOffset, this._count)),
            _ => throw new NotSupportedException(),
        };
    }


    public Vector4UShort[] AsUShort4Array()
    {
        return this._componentType switch
        {
            ComponentTypeEnum.UNSIGNED_SHORT => this._bufferView.ReadUShort4Array(this._byteOffset, this._count),
            ComponentTypeEnum.UNSIGNED_BYTE => this.ReadVector4UShortFromByteArray(),
            _ => throw new NotSupportedException(),
        };
    }

    private Vector4UShort[] ReadVector4UShortFromByteArray()
    {
        byte[] arr = this._bufferView.ReadByteArray(this._byteOffset / 4, this._count * 4);
        Vector4UShort[] result = new Vector4UShort[arr.Length / 4];
        for (int i = 0; i < arr.Length; i += 4)
        {
            result[i / 4] = new Vector4UShort(arr[i], arr[i + 1], arr[i + 2], arr[i + 3]);
        }
        return result;
    }


    private Quaternion[] NormalizeToQuaternion(byte[] sourceArr)
    {
        if (!this._normalized) throw new NotSupportedException();

        var arr = new Quaternion[sourceArr.Length / 4];
        for (int i = 0; i < arr.Length; i++)
        {
            float x = sourceArr[i + 0] / 255f;
            float y = sourceArr[i + 1] / 255f;
            float z = sourceArr[i + 2] / 255f;
            float w = sourceArr[i + 3] / 255f;
            arr[i] = new Quaternion(x, y, z, w);
        }
        return arr;
    }

    private Quaternion[] NormalizeToQuaternion(ushort[] sourceArr)
    {
        if (!this._normalized) throw new NotSupportedException();

        var arr = new Quaternion[sourceArr.Length / 4];
        for (int i = 0; i < arr.Length; i++)
        {
            float x = sourceArr[i + 0] / 65535f;
            float y = sourceArr[i + 1] / 65535f;
            float z = sourceArr[i + 2] / 65535f;
            float w = sourceArr[i + 3] / 65535f;
            arr[i] = new Quaternion(x, y, z, w);
        }
        return arr;
    }

    private Quaternion[] NormalizeToQuaternion(sbyte[] sourceArr)
    {
        if (!this._normalized) throw new NotSupportedException();

        var arr = new Quaternion[sourceArr.Length / 4];
        for (int i = 0; i < arr.Length; i++)
        {
            float x = MathF.Max(sourceArr[i + 0] / 127f, -1f);
            float y = MathF.Max(sourceArr[i + 1] / 127f, -1f);
            float z = MathF.Max(sourceArr[i + 2] / 127f, -1f);
            float w = MathF.Max(sourceArr[i + 3] / 127f, -1f);
            arr[i] = new Quaternion(x, y, z, w);
        }
        return arr;
    }

    private Quaternion[] NormalizeToQuaternion(short[] sourceArr)
    {
        if (!this._normalized) throw new NotSupportedException();

        var arr = new Quaternion[sourceArr.Length / 4];
        for (int i = 0; i < arr.Length; i++)
        {
            float x = MathF.Max(sourceArr[i + 0] / 32767f, -1f);
            float y = MathF.Max(sourceArr[i + 1] / 32767f, -1f);
            float z = MathF.Max(sourceArr[i + 2] / 32767f, -1f);
            float w = MathF.Max(sourceArr[i + 3] / 32767f, -1f);
            arr[i] = new Quaternion(x, y, z, w);
        }
        return arr;
    }

    public Matrix4x4[] AsMatrix4x4()
    {
        return this._bufferView.ReadMatrix4x4Array(this._byteOffset, this._count);
    }
}
