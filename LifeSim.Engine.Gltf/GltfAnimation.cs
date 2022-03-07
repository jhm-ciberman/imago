using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.Anim;
using static glTFLoader.Schema.AnimationChannelTarget;
using static glTFLoader.Schema.AnimationSampler;

namespace LifeSim.Engine.Gltf;

internal class GltfAnimation
{
    private readonly GltfLoader _model;

    private readonly glTFLoader.Schema.AnimationSampler[] _samplers;

    private readonly glTFLoader.Schema.AnimationChannel[] _channels;

    private readonly string _name;

    private readonly Dictionary<int, float[]> _inputsCache = new Dictionary<int, float[]>();

    internal GltfAnimation(GltfLoader model, glTFLoader.Schema.Animation animation)
    {
        this._model = model;
        this._samplers = animation.Samplers;
        this._channels = animation.Channels;
        this._name = animation.Name;
    }

    public Animation LoadAnimation()
    {
        List<IChannel> list = new List<IChannel>();

        foreach (var channel in this._channels)
        {
            var c = this.CreateChannel(channel);
            if (c != null)
            {
                list.Add(c);
            }
        }

        return new Animation(this._name, list);
    }

    private IChannel? CreateChannel(glTFLoader.Schema.AnimationChannel channel)
    {
        var targetIndex = channel.Target.Node;
        if (!targetIndex.HasValue) return null;

        var targetName = this._model.GetNode(targetIndex.Value).Name;
        var sampler = this._samplers[channel.Sampler];
        var input = this.GetSamplerInput(sampler.Input);
        var output = this._model.GetAccessor(sampler.Output);
        return MakeChannel(targetName, channel.Target.Path, input, output, sampler.Interpolation);
    }

    protected static InterpolationMode GetInterpolatorType(InterpolationEnum type)
    {
        return type switch
        {
            InterpolationEnum.STEP => InterpolationMode.Step,
            InterpolationEnum.LINEAR => InterpolationMode.Linear,
            InterpolationEnum.CUBICSPLINE => InterpolationMode.CubicSpline,
            _ => InterpolationMode.Step,
        };
    }

    private static IChannel MakeChannel(string targetName, PathEnum path, float[] input, GltfAccessor output, InterpolationEnum typeEnum)
    {
        var type = GetInterpolatorType(typeEnum);
        return path switch
        {
            PathEnum.translation => new PositionChannel(targetName, input, output.AsVector3Array(), type),
            PathEnum.rotation => new RotationChannel(targetName, input, output.AsQuaternionArray(), type),
            PathEnum.scale => new ScaleChannel(targetName, input, output.AsVector3Array(), type),
            PathEnum.weights => throw new System.NotImplementedException(),
            _ => throw new System.NotImplementedException(),
        };
    }


    private float[] GetSamplerInput(int index)
    {
        if (!this._inputsCache.ContainsKey(index))
        {
            float[] inputArr = this._model.GetAccessor(index).AsFloatArray();
            this._inputsCache.Add(index, inputArr);
            return inputArr;
        }

        return this._inputsCache[index];
    }

}