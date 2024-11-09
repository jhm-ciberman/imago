using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using glTFLoader;
using LifeSim.Imago.Animations;
using LifeSim.Imago.Meshes;
using static glTFLoader.Schema.AnimationChannelTarget;
using static glTFLoader.Schema.AnimationSampler;

namespace LifeSim.Imago.Gltf;

public class GltfReader
{
    private readonly string _path;
    private readonly glTFLoader.Schema.Gltf _model;
    private readonly GltfBuffer?[] _buffersCache;
    private readonly GltfNode?[] _nodesCache;
    private readonly Dictionary<int, GltfAccessor> _accessorsCache = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GltfReader"/> class.
    /// </summary>
    /// <param name="path">The path to the gltf file.</param>
    public GltfReader(string path)
    {
        this._path = path;
        this._model = Interface.LoadModel(path);
        this._buffersCache = new GltfBuffer[this._model.Buffers?.Length ?? 0];
        this._nodesCache = new GltfNode[this._model.Nodes?.Length ?? 0];
    }

    /// <summary>
    /// Loads all resources from the gltf file.
    /// </summary>
    /// <returns>A <see cref="GltfAsset"/> object containing all resources.</returns>
    public GltfAsset Load()
    {
        var scenes = this.LoadScenes();
        var animations = this.LoadAnimations();
        return new GltfAsset(scenes, animations);
    }

    /// <summary>
    /// Loads all scenes from the gltf file.
    /// </summary>
    /// <returns>A list of <see cref="GltfNode"/> objects of the root nodes of the scenes.</returns>
    public IReadOnlyList<GltfNode> LoadScenes()
    {
        var scenes = new List<GltfNode>();
        if (this._model.Scenes != null)
        {
            for (int i = 0; i < this._model.Scenes.Length; i++)
            {
                scenes.Add(this.LoadScene(i));
            }
        }
        return scenes;
    }

    /// <summary>
    /// Loads all animations from the gltf file.
    /// </summary>
    /// <returns>A list of <see cref="Animation"/> objects.</returns>
    public IReadOnlyList<Animation> LoadAnimations()
    {
        var animations = new List<Animation>();
        if (this._model.Animations != null)
        {
            for (int i = 0; i < this._model.Animations.Length; i++)
            {
                animations.Add(this.LoadAnimation(i));
            }
        }
        return animations;
    }

    private string GetNodeName(int index)
    {
        var data = this._model.Nodes[index];
        return string.IsNullOrWhiteSpace(data.Name) ? "Node_" + index : data.Name;
    }

    private GltfNode GetNode(int index)
    {
        GltfNode? node = this._nodesCache[index];
        if (node != null) return node;

        var data = this._model.Nodes[index];

        var name = this.GetNodeName(index);
        node = new GltfNode(name);

        if (data.Mesh.HasValue)
        {
            node.Meshes = this.GetMeshes(data.Mesh.Value);

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

    private Animation LoadAnimation(int index)
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

    private GltfSkinInfo GetSkin(int index)
    {
        var data = this._model.Skins[index];

        int? matricesIndex = data.InverseBindMatrices;

        Matrix4x4[] matrices = matricesIndex == null
            ? GltfBufferViewZeroed.Instance.ReadMatrix4x4Array(0, data.Joints.Length)
            : this.GetAccessor(matricesIndex.Value).AsMatrix4x4();

        string[] joints = new string[data.Joints.Length];
        for (int i = 0; i < joints.Length; i++)
        {
            joints[i] = this.GetNodeName(data.Joints[i]);
        }

        string? root = data.Skeleton.HasValue ? this.GetNodeName(data.Skeleton.Value) : null;

        return new GltfSkinInfo(matrices, joints, root);
    }

    private GltfNode LoadScene(int index)
    {
        var data = this._model.Scenes[index];
        var scene = new GltfNode(data.Name ?? "Scene_" + index);

        foreach (var node in data.Nodes ?? Array.Empty<int>())
        {
            scene.Add(this.GetNode(node));
        }

        return scene;
    }

    private Mesh[] GetMeshes(int index)
    {
        var mesh = this._model.Meshes[index];
        var primitives = new Mesh[mesh.Primitives.Length];

        for (int i = 0; i < primitives.Length; i++)
        {
            primitives[i] = new Mesh(this.GetPrimitiveData(mesh.Primitives[i]));
        }

        return primitives;
    }

    private MeshData GetPrimitiveData(glTFLoader.Schema.MeshPrimitive data)
    {
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

    private GltfAccessor GetAccessor(int index)
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
        if (!index.HasValue) return GltfBufferViewZeroed.Instance;

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

}
