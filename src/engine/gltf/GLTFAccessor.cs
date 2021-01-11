using System;
using System.Numerics;
using glTFLoader.Schema;
using LifeSim.Engine.Rendering;
using static glTFLoader.Schema.Accessor;

namespace LifeSim.Engine.GLTF
{
    class GLTFAccessor
    {
        private IGLTFBufferView _bufferView;
        private int _byteOffset;
        private int _count;
        private Accessor.TypeEnum _type;
        private ComponentTypeEnum _componentType;
        private bool _normalized;

        public GLTFAccessor(IGLTFBufferView bufferView, int byteOffset, int count, ComponentTypeEnum componentType, Accessor.TypeEnum type, bool normalized)
        {
            this._bufferView = bufferView;
            this._byteOffset = byteOffset;
            this._count = count;
            this._componentType = componentType;
            this._type = type;
            this._normalized = normalized;
        }

        public TypeEnum type => this._type;

        public ushort[] AsIndicesArray()
        {
            switch (this._componentType) {
                case ComponentTypeEnum.UNSIGNED_BYTE:
                    return this._Byte2UShort(this._bufferView.ReadByteArray(this._byteOffset, this._count));
                case ComponentTypeEnum.UNSIGNED_SHORT:
                    return this._bufferView.ReadUShortArray(this._byteOffset, this._count);
                case ComponentTypeEnum.UNSIGNED_INT:
                    return this._Int2UShort(this._bufferView.ReadUIntArray(this._byteOffset, this._count));
            }
            throw new System.NotSupportedException();
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
            switch (this._componentType) {
                case ComponentTypeEnum.FLOAT:
                    return this._bufferView.ReadFloatArray(this._byteOffset, this._count);
            }
            throw new System.NotSupportedException();
        }

        public Vector2[] AsVector2Array() 
        {
            switch (this._componentType) {
                case ComponentTypeEnum.FLOAT:
                    return this._bufferView.ReadVector2Array(this._byteOffset, this._count);
            }
            throw new System.NotSupportedException();
        }

        public Vector3[] AsVector3Array() 
        {
            switch (this._componentType) {
                case ComponentTypeEnum.FLOAT:
                    return this._bufferView.ReadVector3Array(this._byteOffset, this._count);
            }
            throw new System.NotSupportedException();
        }

        public Vector4[] AsVector4Array()
        {
            switch (this._componentType) {
                case ComponentTypeEnum.FLOAT:
                    return this._bufferView.ReadVector4Array(this._byteOffset, this._count);
            }
            throw new System.NotSupportedException();
        }

        public Quaternion[] AsQuaternionArray()
        {
            switch (this._componentType) {
                case ComponentTypeEnum.FLOAT:
                    return this._bufferView.ReadQuaternionArray(this._byteOffset, this._count);
                case ComponentTypeEnum.BYTE:
                    return this._normalizeToQuaternion(this._bufferView.ReadSByteArray(this._byteOffset, this._count));
                case ComponentTypeEnum.UNSIGNED_BYTE:
                    return this._normalizeToQuaternion(this._bufferView.ReadByteArray(this._byteOffset, this._count));
                case ComponentTypeEnum.SHORT:
                    return this._normalizeToQuaternion(this._bufferView.ReadShortArray(this._byteOffset, this._count));
                case ComponentTypeEnum.UNSIGNED_SHORT:
                    return this._normalizeToQuaternion(this._bufferView.ReadUShortArray(this._byteOffset, this._count));
            }
            throw new System.NotSupportedException();
        }


        public UShort4[] AsUShort4Array()
        {
            switch (this._componentType) {
                case ComponentTypeEnum.UNSIGNED_SHORT:
                    return this._bufferView.ReadUShort4Array(this._byteOffset, this._count);
            }
            throw new System.NotSupportedException();
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