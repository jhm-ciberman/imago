using System.Numerics;
using glTFLoader;
using LifeSim.Engine.Anim;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.Resources;
using LifeSim.Engine;

namespace LifeSim.Engine.Gltf;

public class GltfLoader
{
    private readonly string _path;
    private readonly glTFLoader.Schema.Gltf _model;

    private readonly GltfBuffer?[] _buffersCache;
    private readonly GLTFNode?[] _nodesCache;
    private readonly Material? _defaultMaterial;

    public GltfLoader(string path, Material? defaultMaterial = null)
    {
        this._path = path;
        this._model = glTFLoader.Interface.LoadModel(path);
        this._buffersCache = new GltfBuffer[this._model.Buffers.Length];
        this._nodesCache = new GLTFNode[this._model.Nodes.Length];
        this._defaultMaterial = defaultMaterial;
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

        if (data.Mesh.HasValue)
        {
            node.Material = this._defaultMaterial;
            node.Mesh = this.GetMesh(data.Mesh.Value);
            if (data.Skin.HasValue)
            {
                node.Skin = this.GetSkin(data.Skin.Value);
            }
        }

        if (data.Matrix.Length == 0)
        {
            Matrix4x4.Decompose(ToMatrix(data.Matrix), out Vector3 scale, out Quaternion rotation, out Vector3 position);
            node.Scale = scale;
            node.Rotation = rotation;
            node.Position = position;
        }
        else
        {
            var rot = data.Rotation;
            var scale = data.Scale;
            var trans = data.Translation;
            node.Scale = new Vector3(scale[0], scale[1], scale[2]);
            node.Rotation = new Quaternion(rot[0], rot[1], rot[2], rot[3]);
            node.Position = new Vector3(trans[0], trans[1], trans[2]);
        }

        this._nodesCache[index] = node;

        if (data.Children != null)
        {
            foreach (var i in data.Children)
            {
                node.Add(this.GetNode(i));
            }
        }
        return node;
    }



    private static Matrix4x4 ToMatrix(float[] m)
    {
        return new Matrix4x4(
            m[0], m[1], m[2], m[3],
            m[4], m[5], m[6], m[7],
            m[8], m[9], m[10], m[11],
            m[12], m[13], m[14], m[15]
        );
    }

    private Mesh GetMesh(int index)
    {
        var meshData = this.GetPrimitive(index).MakeMeshData();
        return new Mesh(meshData);
    }

    public Animation[] LoadAnimations()
    {
        Animation[] animations = new Animation[this._model.Animations.Length];
        for (int i = 0; i < this._model.Animations.Length; i++)
        {
            animations[i] = this.LoadAnimation(i);
        }
        return animations;
    }

    public Animation LoadAnimation(int index)
    {
        var data = this._model.Animations[index];

        var anim = new GltfAnimation(this, data);
        return anim.LoadAnimation();
    }

    private Skin GetSkin(int index)
    {
        var data = this._model.Skins[index];

        int? matricesIndex = data.InverseBindMatrices;
        Matrix4x4[] matrices;
        if (matricesIndex == null)
        {
            matrices = new GltfBufferViewZeroed().ReadMatrix4x4Array(0, data.Joints.Length);
        }
        else
        {
            matrices = this.GetAccessor(matricesIndex.Value).AsMatrix4x4();
        }

        string[] joints = new string[data.Joints.Length];
        for (int i = 0; i < joints.Length; i++)
        {
            joints[i] = this.GetNodeName(data.Joints[i]);
        }

        string? root = data.Skeleton.HasValue ? this.GetNodeName(data.Skeleton.Value) : null;

        return new Skin(matrices, joints, root);
    }

    public IScenePrefab LoadScene(int index = 0)
    {
        var data = this._model.Scenes[index];
        var name = data.Name ?? "Scene_" + index;
        var scene = new GLTFScene(name);
        foreach (var node in data.Nodes)
        {
            scene.Add(this.GetNode(node));
        }
        return scene;
    }

    internal GLTFPrimitive GetPrimitive(int index)
    {
        var data = this._model.Meshes[index].Primitives[0];
        return new GLTFPrimitive(this, data.Indices, data.Attributes);
    }

    internal GltfAccessor GetAccessor(int index)
    {
        var data = this._model.Accessors[index];
        var bufferView = this.GetBufferView(data.BufferView);
        return new GltfAccessor(bufferView, data.ByteOffset, data.Count, data.ComponentType, data.Type, data.Normalized);
    }

    private GltfBuffer GetBuffer(int index)
    {
        GltfBuffer? buffer = this._buffersCache[index];
        if (buffer != null) return buffer;

        var bytes = this._model.LoadBinaryBuffer(index, this._path);
        buffer = new GltfBuffer(bytes);
        this._buffersCache[index] = buffer;
        return buffer;
    }

    private IGltfBufferView GetBufferView(int? index)
    {
        if (!index.HasValue) return new GltfBufferViewZeroed();

        var data = this._model.BufferViews[index.Value];
        var buffer = this.GetBuffer(data.Buffer);

        return new GltfBufferView(buffer, data.ByteOffset, data.ByteStride);
    }
}