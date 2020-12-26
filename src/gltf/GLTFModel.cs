using System.Numerics;
using glTFLoader;
using LifeSim.Rendering;

namespace LifeSim.GLTF
{
    class GLTFModel
    {
        private string _path;
        private glTFLoader.Schema.Gltf _model;

        private GLTFBuffer?[] _buffersCache;
        private Node3D?[] _nodesCache;

        public GLTFModel(string path)
        {
            System.Console.WriteLine("Loading file " + path);
            this._path = path;
            this._model = glTFLoader.Interface.LoadModel(path);
            this._buffersCache = new GLTFBuffer[this._model.Buffers.Length];
            this._nodesCache = new Node3D[this._model.Nodes.Length];
        }


        public Node3D GetNode(int index)
        {
            Node3D? node = this._nodesCache[index];
            if (node != null) return node;

            var data = this._model.Nodes[index];
            var rot = data.Rotation;
            var scale = data.Scale;
            var trans = data.Translation;
            node = new Node3D();
            node.scale = new Vector3(scale[0], scale[1], scale[2]);
            node.rotation = new Quaternion(rot[0], rot[1], rot[2], rot[3]);
            node.position = new Vector3(trans[0], trans[1], trans[2]);
            this._nodesCache[index] = node;
            if (data.Children != null) {
                foreach (var i in data.Children) {
                    node.Add(this.GetNode(i));
                }
            }
            return node;
        }

        public Skin GetSkin(int index)
        {
            var data = this._model.Skins[index];
            
            int? matricesIndex = data.InverseBindMatrices;
            Matrix4x4[] matrices;
            if (matricesIndex == null) {
                throw new System.Exception();
                matrices = new GLTFBufferViewZeroed().ReadMatrix4x4Array(0, data.Joints.Length);
            } else {
                matrices = this.GetAccessor(matricesIndex.Value).AsMatrix4x4();
            }

            Node3D[] joints = new Node3D[data.Joints.Length];
            for (int i = 0; i < joints.Length; i++) {
                joints[i] = this.GetNode(data.Joints[i]);
            }
            
            Node3D? root = data.Skeleton.HasValue ? this.GetNode(data.Skeleton.Value) : null;
            
            return new Skin(matrices, joints, root);
        }

        public Scene3D GetScene(int index)
        {
            var data = this._model.Scenes[index];
            var scene = new Scene3D();
            foreach (var node in data.Nodes) {
                scene.Add(this.GetNode(node));
            }
            return scene;
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
            return new GLTFAccessor(bufferView, data.ByteOffset, data.Count, data.ComponentType, data.Type, data.Normalized);
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