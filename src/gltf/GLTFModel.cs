using glTFLoader;

namespace LifeSim.GLTF
{
    class GLTFModel
    {
        private string _path;
        private glTFLoader.Schema.Gltf _model;

        private GLTFBuffer?[] _buffersCache;

        public GLTFModel(string path)
        {
            System.Console.WriteLine("Loading file " + path);
            this._path = path;
            this._model = glTFLoader.Interface.LoadModel(path);
            this._buffersCache = new GLTFBuffer[this._model.Buffers.Length];

        }

        public GLTFPrimitive GetPrimitive(int index)
        {
            var data = this._model.Meshes[index].Primitives[0];
            return new GLTFPrimitive(this, data.Indices, data.Attributes);
        }

        public GLTFAccessor GetAccessor(int index)
        {
            var data = this._model.Accessors[index];
            var bufferView = this._GetBufferView(data.BufferView);
            return new GLTFAccessor(bufferView, data.ByteOffset, data.Count, data.ComponentType, data.Normalized);
        }

        private GLTFBuffer _GetBuffer(int index)
        {
            GLTFBuffer? buffer = this._buffersCache[index];
            if (buffer != null) return buffer;
            
            var data = this._model.Buffers[index];
            var bytes = this._model.LoadBinaryBuffer(index, this._path);
            buffer = new GLTFBuffer(bytes);
            this._buffersCache[index] = buffer;
            return buffer;
        }

        private IGLTFBufferView _GetBufferView(int? index)
        {
            if (! index.HasValue) return new GLTFBufferViewZeroed();

            var data = this._model.BufferViews[index.Value];
            var buffer = this._GetBuffer(data.Buffer);
            
            return new GLTFBufferView(buffer, data.ByteLength, data.ByteOffset, data.ByteStride);
        }
    }
}