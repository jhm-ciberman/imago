using System.Numerics;
using glTFLoader;
using LifeSim.Engine.Anim;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.GLTF
{
    public class GLTFLoader
    {
        private readonly ResourceFactory _assetsManager;
        private readonly string _path;
        private readonly glTFLoader.Schema.Gltf _model;

        private readonly GLTFBuffer?[] _buffersCache;
        private readonly GLTFNode?[] _nodesCache;
        private readonly SurfaceMaterial? _material;

        public GLTFLoader(ResourceFactory assetsManager, SurfaceMaterial? material, string path)
        {
            this._assetsManager = assetsManager;
            this._path = path;
            this._model = glTFLoader.Interface.LoadModel(path);
            this._buffersCache = new GLTFBuffer[this._model.Buffers.Length];
            this._nodesCache = new GLTFNode[this._model.Nodes.Length];
            this._material = material ?? assetsManager.defaultSurfaceMaterial;
        }

        internal string GetNodeName(int index)
        {
            var data = this._model.Nodes[index];
            return string.IsNullOrWhiteSpace(data.Name) ? "Node_" + index : data.Name;
        }

        internal GLTFNode GetNode(int index)
        {
            GLTFNode? node = this._nodesCache[index];
            if (node != null) return node;

            var data = this._model.Nodes[index];

            var name = this.GetNodeName(index);
            node = new GLTFNode(name);

            if (data.Mesh.HasValue) {
                node.material = this._material;
                node.mesh = this._GetMesh(data.Mesh.Value);
                if (data.Skin.HasValue) {
                    node.skin = this._GetSkin(data.Skin.Value);
                }
            }

            if (data.Matrix.Length == 0) {
                Matrix4x4.Decompose(this._ToMatrix(data.Matrix), out Vector3 scale, out Quaternion rotation, out Vector3 position);
                node.scale = scale;
                node.rotation = rotation;
                node.position = position;
            } else {
                var rot = data.Rotation;
                var scale = data.Scale;
                var trans = data.Translation;
                node.scale = new Vector3(scale[0], scale[1], scale[2]);
                node.rotation = new Quaternion(rot[0], rot[1], rot[2], rot[3]);
                node.position = new Vector3(trans[0], trans[1], trans[2]);
            }

            this._nodesCache[index] = node;

            if (data.Children != null) {
                foreach (var i in data.Children) {
                    node.Add(this.GetNode(i));
                }
            }
            return node;
        }



        private Matrix4x4 _ToMatrix(float[] m)
        {
            return new Matrix4x4(
                m[0], m[1], m[2], m[3],
                m[4], m[5], m[6], m[7],
                m[8], m[9], m[10], m[11],
                m[12], m[13], m[14], m[15]
            );
        }


        private Mesh _GetMesh(int index)
        {
            return this._assetsManager.MakeMesh(this.GetPrimitive(index).MakeMesh());
        }

        public Animation[] LoadAnimations()
        {
            Animation[] animations = new Animation[this._model.Animations.Length];
            for (int i = 0; i < this._model.Animations.Length; i++) {
                animations[i] = this.LoadAnimation(i);
            }
            return animations;
        }

        public Animation LoadAnimation(int index)
        {
            var data = this._model.Animations[index];

            var anim = new GLTFAnimation(this, data);
            return anim.LoadAnimation();
        }

        private Skin _GetSkin(int index)
        {
            var data = this._model.Skins[index];
            
            int? matricesIndex = data.InverseBindMatrices;
            Matrix4x4[] matrices;
            if (matricesIndex == null) {
                matrices = new GLTFBufferViewZeroed().ReadMatrix4x4Array(0, data.Joints.Length);
            } else {
                matrices = this.GetAccessor(matricesIndex.Value).AsMatrix4x4();
            }

            string[] joints = new string[data.Joints.Length];
            for (int i = 0; i < joints.Length; i++) {
                joints[i] = this.GetNodeName(data.Joints[i]);
            }
            
            string? root = data.Skeleton.HasValue ? this.GetNodeName(data.Skeleton.Value) : null;
            
            return new Skin(matrices, joints, root);
        }

        public GLTFScene LoadScene(int index = 0)
        {
            var data = this._model.Scenes[index];
            var name = data.Name ?? "Scene_" + index;
            var scene = new GLTFScene(name);
            foreach (var node in data.Nodes) {
                scene.Add(this.GetNode(node));
            }
            return scene;
        }

        internal GLTFPrimitive GetPrimitive(int index)
        {
            var data = this._model.Meshes[index].Primitives[0];
            return new GLTFPrimitive(this, data.Indices, data.Attributes);
        }

        internal GLTFAccessor GetAccessor(int index)
        {
            var data = this._model.Accessors[index];
            var bufferView = this._GetBufferView(data.BufferView);
            return new GLTFAccessor(bufferView, data.ByteOffset, data.Count, data.ComponentType, data.Type, data.Normalized);
        }

        private GLTFBuffer _GetBuffer(int index)
        {
            GLTFBuffer? buffer = this._buffersCache[index];
            if (buffer != null) return buffer;
            
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
            
            return new GLTFBufferView(buffer, data.ByteOffset, data.ByteStride);
        }
    }
}