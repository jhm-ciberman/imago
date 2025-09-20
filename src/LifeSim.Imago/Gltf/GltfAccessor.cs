using System;
using System.Numerics;
using LifeSim.Support.Numerics;
using static glTFLoader.Schema.Accessor;

namespace LifeSim.Imago.Gltf;

internal class GltfAccessor
{
    private readonly IGltfBufferView _bufferView;
    private readonly int _byteOffset;
    private readonly int _count;
    private readonly ComponentTypeEnum _componentType;
    private readonly bool _normalized;
    /// <summary>
    /// Gets the type of data stored in the accessor (e.g., SCALAR, VEC2, VEC3, VEC4, MAT2, MAT3, MAT4).
    /// </summary>
    public TypeEnum Type { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GltfAccessor"/> class.
    /// </summary>
    /// <param name="bufferView">The buffer view containing the accessor's data.</param>
    /// <param name="byteOffset">The byte offset of the accessor relative to the start of its buffer view.</param>
    /// <param name="count">The number of elements in the accessor.</param>
    /// <param name="componentType">The data type of components in the accessor.</param>
    /// <param name="type">The type of data (e.g., SCALAR, VEC2, VEC3, VEC4, MAT2, MAT3, MAT4).</param>
    /// <param name="normalized">Whether integer data values should be normalized into a range [0, 1] (for unsigned types) or [-1, 1] (for signed types).</param>
    public GltfAccessor(IGltfBufferView bufferView, int byteOffset, int count, ComponentTypeEnum componentType, TypeEnum type, bool normalized)
    {
        this._bufferView = bufferView;
        this._byteOffset = byteOffset;
        this._count = count;
        this._componentType = componentType;
        this.Type = type;
        this._normalized = normalized;
    }


    /// <summary>
    /// Reads the accessor's data and returns it as an array of unsigned shorts, suitable for index buffers.
    /// Handles conversion from different component types (UNSIGNED_BYTE, UNSIGNED_SHORT, UNSIGNED_INT).
    /// </summary>
    /// <returns>An array of <see cref="ushort"/> representing the indices.</returns>
    /// <exception cref="NotSupportedException">Thrown if the component type is not supported for index conversion.</exception>
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

    /// <summary>
    /// Reads the accessor's data and returns it as an array of single-precision floating-point numbers.
    /// </summary>
    /// <returns>An array of <see cref="float"/>.</returns>
    /// <exception cref="NotSupportedException">Thrown if the component type is not FLOAT.</exception>
    public float[] AsFloatArray()
    {
        return this._componentType switch
        {
            ComponentTypeEnum.FLOAT => this._bufferView.ReadFloatArray(this._byteOffset, this._count),
            _ => throw new NotSupportedException(),
        };
    }

    /// <summary>
    /// Reads the accessor's data and returns it as an array of <see cref="Vector2"/>.
    /// </summary>
    /// <returns>An array of <see cref="Vector2"/>.</returns>
    /// <exception cref="NotSupportedException">Thrown if the component type is not FLOAT.</exception>
    public Vector2[] AsVector2Array()
    {
        return this._componentType switch
        {
            ComponentTypeEnum.FLOAT => this._bufferView.ReadVector2Array(this._byteOffset, this._count),
            _ => throw new NotSupportedException(),
        };
    }

    /// <summary>
    /// Reads the accessor's data and returns it as an array of <see cref="Vector3"/>.
    /// </summary>
    /// <returns>An array of <see cref="Vector3"/>.</returns>
    /// <exception cref="NotSupportedException">Thrown if the component type is not FLOAT.</exception>
    public Vector3[] AsVector3Array()
    {
        return this._componentType switch
        {
            ComponentTypeEnum.FLOAT => this._bufferView.ReadVector3Array(this._byteOffset, this._count),
            _ => throw new NotSupportedException(),
        };
    }

    /// <summary>
    /// Reads the accessor's data and returns it as an array of <see cref="Vector4"/>.
    /// </summary>
    /// <returns>An array of <see cref="Vector4"/>.</returns>
    /// <exception cref="NotSupportedException">Thrown if the component type is not FLOAT.</exception>
    public Vector4[] AsVector4Array()
    {
        return this._componentType switch
        {
            ComponentTypeEnum.FLOAT => this._bufferView.ReadVector4Array(this._byteOffset, this._count),
            _ => throw new NotSupportedException(),
        };
    }

    /// <summary>
    /// Reads the accessor's data and returns it as an array of <see cref="Quaternion"/>.
    /// Handles normalization if the accessor is marked as normalized.
    /// </summary>
    /// <returns>An array of <see cref="Quaternion"/>.</returns>
    /// <exception cref="NotSupportedException">Thrown if the component type is not supported for quaternion conversion.</exception>
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


    /// <summary>
    /// Reads the accessor's data and returns it as an array of <see cref="Vector4UShort"/>.
    /// </summary>
    /// <returns>An array of <see cref="Vector4UShort"/>.</returns>
    /// <exception cref="NotSupportedException">Thrown if the component type is not supported for Vector4UShort conversion.</exception>
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

    /// <summary>
    /// Reads the accessor's data and returns it as an array of <see cref="Matrix4x4"/>.
    /// </summary>
    /// <returns>An array of <see cref="Matrix4x4"/>.</returns>
    public Matrix4x4[] AsMatrix4x4()
    {
        return this._bufferView.ReadMatrix4x4Array(this._byteOffset, this._count);
    }
}
