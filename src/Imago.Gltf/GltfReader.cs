using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using glTFLoader;
using Imago.Graphics3D.Animations;
using Imago.Graphics3D.Meshes;
using static glTFLoader.Schema.AnimationChannelTarget;
using static glTFLoader.Schema.AnimationSampler;

namespace Imago.Gltf;

/// <summary>
/// Reads scenes, meshes and animations from a glTF 2.0 file.
/// </summary>
/// <remarks>
/// Only what you read is loaded. The results belong to the caller and keep working after the reader is disposed.
/// </remarks>
public sealed class GltfReader : IDisposable
{
    /// <summary>
    /// Gets the default scene of the file.
    /// </summary>
    public GltfNodeInfo Scene => this._scenes[0];

    /// <summary>
    /// Gets all the scenes of the file.
    /// </summary>
    public IReadOnlyList<GltfNodeInfo> Scenes => this._scenes;

    /// <summary>
    /// Gets the names of all the animations in the file.
    /// </summary>
    public IReadOnlyList<string> AnimationNames => this._animationNames;

    private readonly string _path;
    private readonly glTFLoader.Schema.Gltf _model;
    private readonly GltfBuffer?[] _buffersCache;
    private readonly Dictionary<int, GltfAccessor> _accessorsCache = new();
    private readonly GltfNodeInfo[] _scenes;
    private readonly string[] _animationNames;
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="GltfReader"/> class.
    /// </summary>
    /// <param name="path">The path to the glTF file.</param>
    public GltfReader(string path)
    {
        this._path = path;
        this._model = Interface.LoadModel(path);
        this._buffersCache = new GltfBuffer[this._model.Buffers?.Length ?? 0];
        this._scenes = this.CreateScenes();

        this._animationNames = new string[this._model.Animations?.Length ?? 0];
        for (int i = 0; i < this._animationNames.Length; i++)
        {
            this._animationNames[i] = this.GetAnimationName(i);
        }
    }

    /// <summary>
    /// Reads a scene, or a single node and its children.
    /// </summary>
    /// <param name="sceneName">The name of the scene. If null, the default scene is used.</param>
    /// <param name="nodeName">The name of the node to use as the root. If null, the whole scene is read.</param>
    /// <returns>The scene. The caller owns it.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the scene or the node is not found.</exception>
    public GltfScene ReadScene(string? sceneName = null, string? nodeName = null)
    {
        ObjectDisposedException.ThrowIf(this._isDisposed, this);

        GltfNodeInfo root = this.FindNode(sceneName, nodeName);
        var meshes = new Dictionary<int, Mesh[]>();
        var skins = new Dictionary<int, GltfSkinInfo>();
        this.ReadNodeResources(root, meshes, skins);

        return new GltfScene(root, meshes, skins);
    }

    /// <summary>
    /// Reads one mesh of a node.
    /// </summary>
    /// <param name="sceneName">The name of the scene. If null, the default scene is used.</param>
    /// <param name="nodeName">The name of the node. If null, the scene root is used.</param>
    /// <param name="index">The index of the mesh in the node, for nodes that have more than one.</param>
    /// <returns>The mesh. The caller owns it.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the scene or the node is not found.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the node has no mesh.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the node has no mesh at the given index.</exception>
    public Mesh ReadMesh(string? sceneName = null, string? nodeName = null, int index = 0)
    {
        ObjectDisposedException.ThrowIf(this._isDisposed, this);

        GltfNodeInfo node = this.FindNode(sceneName, nodeName);
        if (node.MeshIndex is not int meshIndex)
        {
            throw new InvalidOperationException($"The node {node.Name} has no mesh.");
        }

        var primitives = this._model.Meshes[meshIndex].Primitives;
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, primitives.Length);

        return new Mesh(this.GetPrimitiveData(primitives[index]));
    }

    /// <summary>
    /// Reads an animation.
    /// </summary>
    /// <param name="name">The name of the animation. If null, the first animation is read.</param>
    /// <returns>The animation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the animation is not found.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the name is null and the file has no animations.</exception>
    public Animation ReadAnimation(string? name = null)
    {
        ObjectDisposedException.ThrowIf(this._isDisposed, this);

        if (string.IsNullOrEmpty(name))
        {
            if (this._animationNames.Length == 0)
            {
                throw new InvalidOperationException("The file has no animations.");
            }

            return this.LoadAnimation(0);
        }

        int index = Array.IndexOf(this._animationNames, name);
        if (index < 0)
        {
            throw new KeyNotFoundException($"Could not find animation with name {name}");
        }

        return this.LoadAnimation(index);
    }

    /// <summary>
    /// Reads all the animations of the file.
    /// </summary>
    /// <returns>The animations, in the order of <see cref="AnimationNames"/>.</returns>
    public IReadOnlyList<Animation> ReadAnimations()
    {
        ObjectDisposedException.ThrowIf(this._isDisposed, this);

        var animations = new Animation[this._animationNames.Length];
        for (int i = 0; i < animations.Length; i++)
        {
            animations[i] = this.LoadAnimation(i);
        }
        return animations;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this._isDisposed = true;
        Array.Clear(this._buffersCache);
        this._accessorsCache.Clear();
    }

    private GltfNodeInfo FindNode(string? sceneName, string? nodeName)
    {
        GltfNodeInfo scene = string.IsNullOrEmpty(sceneName)
            ? this.Scene
            : Array.Find(this._scenes, s => s.Name == sceneName) ?? throw new KeyNotFoundException($"Could not find scene with name {sceneName}");

        return string.IsNullOrEmpty(nodeName)
            ? scene
            : scene.Find(nodeName) ?? throw new KeyNotFoundException($"Could not find node with name {nodeName}");
    }

    private void ReadNodeResources(GltfNodeInfo node, Dictionary<int, Mesh[]> meshes, Dictionary<int, GltfSkinInfo> skins)
    {
        if (node.MeshIndex is int meshIndex && !meshes.ContainsKey(meshIndex))
        {
            meshes.Add(meshIndex, this.GetMeshes(meshIndex));
        }

        if (node.SkinIndex is int skinIndex && !skins.ContainsKey(skinIndex))
        {
            skins.Add(skinIndex, this.GetSkin(skinIndex));
        }

        foreach (GltfNodeInfo child in node.Children)
        {
            this.ReadNodeResources(child, meshes, skins);
        }
    }

    private GltfNodeInfo[] CreateScenes()
    {
        var nodes = new GltfNodeInfo[this._model.Nodes?.Length ?? 0];
        for (int i = 0; i < nodes.Length; i++)
        {
            nodes[i] = this.CreateNode(i);
        }

        for (int i = 0; i < nodes.Length; i++)
        {
            foreach (int child in this._model.Nodes![i].Children ?? Array.Empty<int>())
            {
                nodes[i].Add(nodes[child]);
            }
        }

        var scenes = new GltfNodeInfo[this._model.Scenes?.Length ?? 0];
        for (int i = 0; i < scenes.Length; i++)
        {
            var data = this._model.Scenes![i];
            scenes[i] = new GltfNodeInfo(data.Name ?? "Scene_" + i);

            foreach (int node in data.Nodes ?? Array.Empty<int>())
            {
                scenes[i].Add(nodes[node]);
            }
        }

        return scenes;
    }

    private string GetNodeName(int index)
    {
        var data = this._model.Nodes[index];
        return string.IsNullOrWhiteSpace(data.Name) ? "Node_" + index : data.Name;
    }

    private string GetAnimationName(int index)
    {
        var data = this._model.Animations[index];
        return string.IsNullOrWhiteSpace(data.Name) ? "Animation_" + index : data.Name;
    }

    private GltfNodeInfo CreateNode(int index)
    {
        var data = this._model.Nodes[index];

        Vector3 position;
        Quaternion rotation;
        Vector3 scale;

        if (ToMatrix(data.Matrix) != Matrix4x4.Identity)
        {
            Matrix4x4.Decompose(ToMatrix(data.Matrix), out scale, out rotation, out position);
        }
        else
        {
            var rot = data.Rotation;
            var scl = data.Scale;
            var trans = data.Translation;
            scale = new Vector3(scl[0], scl[1], scl[2]);
            rotation = new Quaternion(rot[0], rot[1], rot[2], rot[3]);
            position = new Vector3(trans[0], trans[1], trans[2]);
        }

        return new GltfNodeInfo(this.GetNodeName(index))
        {
            Position = position,
            Rotation = rotation,
            Scale = scale,
            MeshIndex = data.Mesh,
            SkinIndex = data.Mesh.HasValue ? data.Skin : null,
        };
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

        return new Animation(this._animationNames[index], list);
    }

    private static IChannel MakeChannel(string targetName, PathEnum path, float[] input, GltfAccessor output, InterpolationEnum typeEnum)
    {
        var type = ToInterpolationMode(typeEnum);

        if (path == PathEnum.translation)
        {
            return new PositionChannel(targetName, input, output.AsVector3Array(), type);
        }

        if (path == PathEnum.rotation)
        {
            return new RotationChannel(targetName, input, output.AsQuaternionArray(), type);
        }

        if (path == PathEnum.scale)
        {
            return new ScaleChannel(targetName, input, output.AsVector3Array(), type);
        }

        throw new System.NotImplementedException();
    }

    private static InterpolationMode ToInterpolationMode(InterpolationEnum interpolation)
    {
        if (interpolation == InterpolationEnum.STEP)
        {
            return InterpolationMode.Step;
        }

        if (interpolation == InterpolationEnum.CUBICSPLINE)
        {
            return InterpolationMode.CubicSpline;
        }

        return InterpolationMode.Linear;
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
