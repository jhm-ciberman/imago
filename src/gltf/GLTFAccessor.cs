using System.Numerics;
using glTFLoader.Schema;
using LifeSim.Rendering;
using static glTFLoader.Schema.Accessor;

namespace LifeSim.GLTF
{
    class GLTFAccessor
    {
        private IGLTFBufferView bufferView;
        private int byteOffset;
        private int count;
        private Accessor.TypeEnum type;
        private ComponentTypeEnum componentType;
        private bool normalized;

        public GLTFAccessor(IGLTFBufferView bufferView, int byteOffset, int count, ComponentTypeEnum componentType, Accessor.TypeEnum type, bool normalized)
        {
            this.bufferView = bufferView;
            this.byteOffset = byteOffset;
            this.count = count;
            this.componentType = componentType;
            this.type = type;
            this.normalized = normalized;
        }

        public ushort[] AsIndicesArray()
        {
            switch (this.componentType) {
                case ComponentTypeEnum.UNSIGNED_BYTE:
                    return this._Byte2UShort(this.bufferView.ReadByteArray(this.byteOffset, this.count));
                case ComponentTypeEnum.UNSIGNED_SHORT:
                    return this.bufferView.ReadUShortArray(this.byteOffset, this.count);
                case ComponentTypeEnum.UNSIGNED_INT:
                    return this._Int2UShort(this.bufferView.ReadUIntArray(this.byteOffset, this.count));
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

        public Vector2[] AsVector2Array() 
        {
            switch (this.componentType) {
                case ComponentTypeEnum.FLOAT:
                    return this.bufferView.ReadVector2Array(this.byteOffset, this.count);
            }
            throw new System.NotSupportedException();
        }

        public Vector3[] AsVector3Array() 
        {
            switch (this.componentType) {
                case ComponentTypeEnum.FLOAT:
                    return this.bufferView.ReadVector3Array(this.byteOffset, this.count);
            }
            throw new System.NotSupportedException();
        }

        public Vector4[] AsVector4Array()
        {
            System.Console.WriteLine("componentType " + this.componentType + " normalized " + this.normalized);          

            switch (this.componentType) {
                case ComponentTypeEnum.FLOAT:
                    return this.bufferView.ReadVector4Array(this.byteOffset, this.count);
            }
            throw new System.NotSupportedException();
        }


        public UShort4[] AsUShort4Array()
        {
            System.Console.WriteLine("componentType " + this.componentType + " normalized " + this.normalized);          

            switch (this.componentType) {
                case ComponentTypeEnum.UNSIGNED_SHORT:
                    return this.bufferView.ReadUShort4Array(this.byteOffset, this.count);
            }
            throw new System.NotSupportedException();
        }


        private Vector4[] _normalize(ushort[] sourceArr)
        {
            if (! this.normalized) throw new System.NotSupportedException();

            var arr = new Vector4[sourceArr.Length / 4];
            for (int i = 0; i < arr.Length; i++) {
                arr[i] = new Vector4(sourceArr[i + 0], sourceArr[i + 1], sourceArr[i + 2], sourceArr[i + 3]) / 255f; 
            }
            return arr;
        }
    
        //public Matrix3x2[] GetMatrix2x3() => this.bufferView.ReadMatrix2x2Array(this.byteOffset, this.count);
        //public Matrix4x4[] GetMatrix3x3() => this.bufferView.ReadMatrix3x3Array(this.byteOffset, this.count);
        public Matrix4x4[] AsMatrix4x4()
        {
            return this.bufferView.ReadMatrix4x4Array(this.byteOffset, this.count);
        }


    }
}