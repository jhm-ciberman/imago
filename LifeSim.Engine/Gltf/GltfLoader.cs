using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using glTFLoader;
using LifeSim.Engine.Anim;
using LifeSim.Engine.Meshes;
using LifeSim.Engine.Rendering;
using static glTFLoader.Schema.AnimationChannelTarget;
using static glTFLoader.Schema.AnimationSampler;

namespace LifeSim.Engine.Gltf;

public class GltfLoader
{
    public class GltfAsset
    {
        public IReadOnlyList<IScenePrefab> Scenes { get; } = Array.Empty<IScenePrefab>();
        public IReadOnlyList<Animation> Animations { get; } = Array.Empty<Animation>();

        public GltfAsset(IReadOnlyList<IScenePrefab> scenes, IReadOnlyList<Animation> animations)
        {
            this.Scenes = scenes;
            this.Animations = animations;
        }
    }

    private static readonly Dictionary<string, GltfAsset> _cache = new Dictionary<string, GltfAsset>();

    public static GltfAsset LoadFile(string path)
    {
        if (!_cache.TryGetValue(path, out GltfAsset? asset))
        {
            asset = new GltfLoader(path).LoadAll();
            _cache.Add(path, asset);
        }

        return asset;
    }

    public static Animation LoadAnimation(string path, string? animationName = null)
    {
        var gltf = LoadFile(path);
        return string.IsNullOrEmpty(animationName)
            ? gltf.Animations[0]
            : gltf.Animations.First(a => a.Name == animationName);
    }

    public static IScenePrefab LoadScenePrefab(string path, string? sceneName = null)
    {
        var gltf = LoadFile(path);
        return string.IsNullOrEmpty(sceneName)
            ? gltf.Scenes[0]
            : gltf.Scenes.First(s => s.Name == sceneName);
    }

    private readonly string _path;
    private readonly glTFLoader.Schema.Gltf _model;

    private readonly GltfBuffer?[] _buffersCache;
    private readonly GLTFNode?[] _nodesCache;
    private readonly Material? _defaultMaterial;

    private readonly Dictionary<int, GltfAccessor> _accessorsCache = new Dictionary<int, GltfAccessor>();

    public GltfLoader(string path, Material? defaultMaterial = null)
    {
        this._path = path;
        this._model = Interface.LoadModel(path);
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
        var animationData = this._model.Animations[index];
        List<IChannel> list = new List<IChannel>();

        foreach (var channelData in animationData.Channels)
        {
            var targetIndex = channelData.Target.Node;
            if (!targetIndex.HasValue) continue;

            var targetName = this.GetNodeName(targetIndex.Value);
            var samplerData = animationData.Samplers[channelData.Sampler];
            var input = this.GetAccessor(samplerData.Input).AsFloatArray();
            var output = this.GetAccessor(samplerData.Output);
            var channel = MakeChannel(targetName, channelData.Target.Path, input, output, samplerData.Interpolation);
            list.Add(channel);
        }

        return new Animation(animationData.Name, list);
    }

    private static IChannel MakeChannel(string targetName, PathEnum path, float[] input, GltfAccessor output, InterpolationEnum typeEnum)
    {
        var type = typeEnum switch
        {
            InterpolationEnum.STEP => InterpolationMode.Step,
            InterpolationEnum.LINEAR => InterpolationMode.Linear,
            InterpolationEnum.CUBICSPLINE => InterpolationMode.CubicSpline,
            _ => InterpolationMode.Linear,
        };

        return path switch
        {
            PathEnum.translation => new PositionChannel(targetName, input, output.AsVector3Array(), type),
            PathEnum.rotation => new RotationChannel(targetName, input, output.AsQuaternionArray(), type),
            PathEnum.scale => new ScaleChannel(targetName, input, output.AsVector3Array(), type),
            PathEnum.weights => throw new System.NotImplementedException(),
            _ => throw new System.NotImplementedException(),
        };
    }

    private Skin GetSkin(int index)
    {
        var data = this._model.Skins[index];

        int? matricesIndex = data.InverseBindMatrices;

        Matrix4x4[] matrices = matricesIndex == null
            ? new GltfBufferViewZeroed().ReadMatrix4x4Array(0, data.Joints.Length)
            : this.GetAccessor(matricesIndex.Value).AsMatrix4x4();

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
        var scene = new GltfScene(name);
        foreach (var node in data.Nodes)
        {
            scene.Add(this.GetNode(node));
        }
        return scene;
    }

    internal IMeshData GetPrimitive(int index)
    {
        var data = this._model.Meshes[index].Primitives[0];
        var indicesAccessor = data.Indices.HasValue ? this.GetAccessor(data.Indices.Value) : null;
        var attributes = data.Attributes;

        var positionAccessor = this.GetPrimitiveAttributeAccessor(attributes, "POSITION");
        Debug.Assert(positionAccessor != null);
        var positions = positionAccessor.AsVector3Array();

        var texCoordAccessor = this.GetPrimitiveAttributeAccessor(attributes, "TEXCOORD_0");
        var texCoords = texCoordAccessor?.AsVector2Array();

        var normalAccessor   = this.GetPrimitiveAttributeAccessor(attributes, "NORMAL");
        var normals = normalAccessor?.AsVector3Array();

        var jointsAccessor   = this.GetPrimitiveAttributeAccessor(attributes, "JOINTS_0");
        var weightsAccessor  = this.GetPrimitiveAttributeAccessor(attributes, "WEIGHTS_0");

        var indices = indicesAccessor == null
            ? MakeFakeIndices(positions.Length)
            : indicesAccessor.AsIndicesArray();

        if (weightsAccessor != null && jointsAccessor != null)
        {
            var joints = jointsAccessor.AsUShort4Array();
            var weights = weightsAccessor.AsVector4Array();
            return new SkinnedMeshData(indices, positions, normals, texCoords, joints, weights);
        }
        else
        {
            return new BasicMeshData(indices, positions, normals, texCoords);
        }
    }

    private GltfAccessor? GetPrimitiveAttributeAccessor(Dictionary<string, int> attributes, string name)
    {
        if (attributes.TryGetValue(name, out int attributeId))
        {
            return this.GetAccessor(attributeId);
        }
        return null;
    }

    private Mesh GetMesh(int index)
    {
        return new Mesh(this.GetPrimitive(index));
    }


    internal GltfAccessor GetAccessor(int index)
    {
        if (!this._accessorsCache.TryGetValue(index, out GltfAccessor? accessor))
        {
            var data = this._model.Accessors[index];
            var bufferView = this.GetBufferView(data.BufferView);
            accessor = new GltfAccessor(bufferView, data.ByteOffset, data.Count, data.ComponentType, data.Type, data.Normalized);
            this._accessorsCache.Add(index, accessor);
        }

        return accessor;
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




    private static ushort[] MakeFakeIndices(int count)
    {
        var arr = new ushort[count];
        for (int i = 0; i < count; i++)
        {
            arr[i] = (ushort)i;
        }
        return arr;
    }

    public GltfAsset LoadAll()
    {
        var scenes = new List<IScenePrefab>();
        if (this._model.Scenes != null)
        {
            for (int i = 0; i < this._model.Scenes.Length; i++)
            {
                scenes.Add(this.LoadScene(i));
            }
        }
        var animations = new List<Animation>();

        if (this._model.Animations != null)
        {
            for (int i = 0; i < this._model.Animations.Length; i++)
            {
                animations.Add(this.LoadAnimation(i));
            }
        }
        return new GltfAsset(scenes, animations);
    }
}