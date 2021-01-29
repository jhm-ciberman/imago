using System;
using System.Numerics;
using glTFLoader.Schema;
using LifeSim.Engine.Rendering;
using static glTFLoader.Schema.Accessor;

namespace LifeSim.Engine.GLTF
{
    internal class GLTFAccessor
    {
        private readonly IGLTFBufferView _bufferView;
        private readonly int _byteOffset;
        private readonly int _count;
        private readonly ComponentTypeEnum _componentType;
        private readonly bool _normalized;
        public TypeEnum type { get; }

        public GLTFAccessor(IGLTFBufferView bufferView, int byteOffset, int count, ComponentTypeEnum componentType, TypeEnum type, bool normalized)
        {
            this._bufferView = bufferView;
            this._byteOffset = byteOffset;
            this._count = count;
            this._componentType = componentType;
            this.type = type;
            this._normalized = normalized;
        }


        public ushort[] AsIndicesArray()
        {
            return this._componentType switch {
                ComponentTypeEnum.UNSIGNED_BYTE  => this._Byte2UShort(this._bufferView.ReadByteArray(this._byteOffset, this._count)),
                ComponentTypeEnum.UNSIGNED_SHORT => this._bufferView.ReadUShortArray(this._byteOffset, this._count),
                ComponentTypeEnum.UNSIGNED_INT   => this._Int2UShort(this._bufferView.ReadUIntArray(this._byteOffset, this._count)),
                _ => throw new NotSupportedException(),
            };
        }

        private ushort[] _Int2UShort(uint[] sourceArr)
        {
            var arr = new ushort[sourceArr.Length];
            for (int i = 0; i < sourceArr.Length; i++) {
                arr[i] = (ushort) sourceArr[i];
            }
            return arr;
        }

        private ushort[] _Byte2UShort(byte[] sourceArr)
        {
            var arr = new ushort[sourceArr.Length];
            for (int i = 0; i < sourceArr.Length; i++) {
                arr[i] = (ushort) sourceArr[i];
            }
            return arr;
        }

        public float[] AsFloatArray()
        {
            return this._componentType switch {
                ComponentTypeEnum.FLOAT => this._bufferView.ReadFloatArray(this._byteOffset, this._count),
                _ => throw new System.NotSupportedException(),
            };
        }

        public Vector2[] AsVector2Array() 
        {
            return this._componentType switch {
                ComponentTypeEnum.FLOAT => this._bufferView.ReadVector2Array(this._byteOffset, this._count),
                _ => throw new System.NotSupportedException(),
            };
        }

        public Vector3[] AsVector3Array() 
        {
            return this._componentType switch {
                ComponentTypeEnum.FLOAT => this._bufferView.ReadVector3Array(this._byteOffset, this._count),
                _ => throw new System.NotSupportedException(),
            };
        }

        public Vector4[] AsVector4Array()
        {
            return this._componentType switch {
                ComponentTypeEnum.FLOAT => this._bufferView.ReadVector4Array(this._byteOffset, this._count),
                _ => throw new System.NotSupportedException(),
            };
        }

        public Quaternion[] AsQuaternionArray()
        {
            return this._componentType switch {
                ComponentTypeEnum.FLOAT          => this._bufferView.ReadQuaternionArray(this._byteOffset, this._count),
                ComponentTypeEnum.BYTE           => this._normalizeToQuaternion(this._bufferView.ReadSByteArray(this._byteOffset, this._count)),
                ComponentTypeEnum.UNSIGNED_BYTE  => this._normalizeToQuaternion(this._bufferView.ReadByteArray(this._byteOffset, this._count)),
                ComponentTypeEnum.SHORT          => this._normalizeToQuaternion(this._bufferView.ReadShortArray(this._byteOffset, this._count)),
                ComponentTypeEnum.UNSIGNED_SHORT => this._normalizeToQuaternion(this._bufferView.ReadUShortArray(this._byteOffset, this._count)),
                _ => throw new System.NotSupportedException(),
            };
        }


        public UShort4[] AsUShort4Array()
        {
            return this._componentType switch {
                ComponentTypeEnum.UNSIGNED_SHORT => this._bufferView.ReadUShort4Array(this._byteOffset, this._count),
                _ => throw new System.NotSupportedException(),
            };
        }


        private Quaternion[] _normalizeToQuaternion(byte[] sourceArr)
        {
            if (! this._normalized) throw new System.NotSupportedException();

            var arr = new Quaternion[sourceArr.Length / 4];
            for (int i = 0; i < arr.Length; i++) {
                float x = sourceArr[i + 0] / 255f;
                float y = sourceArr[i + 1] / 255f;
                float z = sourceArr[i + 2] / 255f;
                float w = sourceArr[i + 3] / 255f;
                arr[i] = new Quaternion(x, y, z, w); 
            }
            return arr;
        }

        private Quaternion[] _normalizeToQuaternion(ushort[] sourceArr)
        {
            if (! this._normalized) throw new System.NotSupportedException();

            var arr = new Quaternion[sourceArr.Length / 4];
            for (int i = 0; i < arr.Length; i++) {
                float x = sourceArr[i + 0] / 65535f;
                float y = sourceArr[i + 1] / 65535f;
                float z = sourceArr[i + 2] / 65535f;
                float w = sourceArr[i + 3] / 65535f;
                arr[i] = new Quaternion(x, y, z, w); 
            }
            return arr;
        }

        private Quaternion[] _normalizeToQuaternion(sbyte[] sourceArr)
        {
            if (! this._normalized) throw new System.NotSupportedException();

            var arr = new Quaternion[sourceArr.Length / 4];
            for (int i = 0; i < arr.Length; i++) {
                float x = System.MathF.Max(sourceArr[i + 0] / 127f, -1f);
                float y = System.MathF.Max(sourceArr[i + 1] / 127f, -1f);
                float z = System.MathF.Max(sourceArr[i + 2] / 127f, -1f);
                float w = System.MathF.Max(sourceArr[i + 3] / 127f, -1f);
                arr[i] = new Quaternion(x, y, z, w); 
            }
            return arr;
        }

        private Quaternion[] _normalizeToQuaternion(short[] sourceArr)
        {
            if (! this._normalized) throw new System.NotSupportedException();

            var arr = new Quaternion[sourceArr.Length / 4];
            for (int i = 0; i < arr.Length; i++) {
                float x = System.MathF.Max(sourceArr[i + 0] / 32767f, -1f);
                float y = System.MathF.Max(sourceArr[i + 1] / 32767f, -1f);
                float z = System.MathF.Max(sourceArr[i + 2] / 32767f, -1f);
                float w = System.MathF.Max(sourceArr[i + 3] / 32767f, -1f);
                arr[i] = new Quaternion(x, y, z, w); 
            }
            return arr;
        }
    
        //public Matrix3x2[] GetMatrix2x3() => this.bufferView.ReadMatrix2x2Array(this.byteOffset, this.count);
        //public Matrix4x4[] GetMatrix3x3() => this.bufferView.ReadMatrix3x3Array(this.byteOffset, this.count);
        public Matrix4x4[] AsMatrix4x4()
        {
            return this._bufferView.ReadMatrix4x4Array(this._byteOffset, this._count);
        }


    }
}