using System.Collections.Generic;
using System.Numerics;
using LifeSim.Rendering;
using static glTFLoader.Schema.AnimationChannelTarget;
using static glTFLoader.Schema.AnimationSampler;

namespace LifeSim.GLTF
{
    public class GLTFAnimation
    {
        private GLTFModel _model;

        private glTFLoader.Schema.AnimationSampler[] _samplers;

        private glTFLoader.Schema.AnimationChannel[] _channels;

        private string _name;

        private Dictionary<int, float[]> _inputsCache = new Dictionary<int, float[]>();

        internal GLTFAnimation(GLTFModel model, glTFLoader.Schema.Animation animation)
        {
            this._model = model;
            this._samplers = animation.Samplers;
            this._channels = animation.Channels;
            this._name = animation.Name;
        }

        public Animation LoadAnimation()
        {
            List<Animation.IChannel> list = new List<Animation.IChannel>();

            foreach (var channel in this._channels) {
                var c = this._CreateChannel(channel);
                if (c != null) {
                    list.Add(c);
                }
            }

            return new Animation(this._name, list);
        }

        private Animation.IChannel? _CreateChannel(glTFLoader.Schema.AnimationChannel channel)
        {
            var targetIndex = channel.Target.Node;
            if (! targetIndex.HasValue) return null;

            var factory = this._GetChannelFactory(channel.Target.Path);
            if (factory == null) return null;

            var targetName = this._model.GetNodeName(targetIndex.Value);
            var sampler = this._samplers[channel.Sampler];
            var input = this._GetSamplerInput(sampler.Input);
            var output = this._model.GetAccessor(sampler.Output);
            return factory.MakeChannel(targetName, input, output, sampler.Interpolation);
        }

        private IChannelFactory? _GetChannelFactory(PathEnum path)
        {
            switch (path) {
                case PathEnum.translation:
                    return new PositionChannelFactory();
                case PathEnum.rotation:
                    return new RotationChannelFactory();
                case PathEnum.scale:
                    return new ScaleChannelFactory();
                default: 
                    return null; // Not supported
            }
        }

        
        private float[] _GetSamplerInput(int index)
        {
            if (! this._inputsCache.ContainsKey(index)) {
                float[] inputArr = this._model.GetAccessor(index).AsFloatArray();
                this._inputsCache.Add(index, inputArr);
                return inputArr;
            }
            
            return this._inputsCache[index];
        }


        interface IChannelFactory
        {
            Animation.IChannel MakeChannel(string targetName, float[] input, GLTFAccessor output, InterpolationEnum type);
        }

        abstract class ChannelFactory<T> : IChannelFactory
        {
            public abstract Animation.IChannel MakeChannel(string targetName, float[] input, GLTFAccessor output, InterpolationEnum type);
            protected abstract Animation.IInterpolator<T> _MakeInterpolator();

            protected Animation.BaseSampler<T> _MakeSampler(float[] input, T[] values, InterpolationEnum type)
            {
                var interpolator = this._MakeInterpolator();
                switch (type) {
                    case InterpolationEnum.STEP:
                        return new Animation.SamplerStep<T>(input, values, interpolator);
                    case InterpolationEnum.LINEAR:
                        return new Animation.SamplerLinear<T>(input, values, interpolator);
                    case InterpolationEnum.CUBICSPLINE:
                        return new Animation.SamplerCubicSpline<T>(input, values, interpolator);
                    default: 
                        return new Animation.SamplerStep<T>(input, values, interpolator);
                }
            }
        }

        class PositionChannelFactory : ChannelFactory<Vector3>
        {
            public override Animation.IChannel MakeChannel(string targetName, float[] input, GLTFAccessor output, InterpolationEnum type)
            {
                var sampler = this._MakeSampler(input, output.AsVector3Array(), type);
                return new Animation.PositionChannel(targetName, sampler);
            }

            protected override Animation.IInterpolator<Vector3> _MakeInterpolator() => new Animation.Vector3Interpolator();
        }

        class RotationChannelFactory : ChannelFactory<Quaternion>
        {
            public override Animation.IChannel MakeChannel(string targetName, float[] input, GLTFAccessor output, InterpolationEnum type)
            {
                var sampler = this._MakeSampler(input, output.AsQuaternionArray(), type);
                return new Animation.RotationChannel(targetName, sampler);
            }

            protected override Animation.IInterpolator<Quaternion> _MakeInterpolator() => new Animation.QuaternionInterpolator();
        }

        class ScaleChannelFactory : ChannelFactory<Vector3>
        {
            public override Animation.IChannel MakeChannel(string targetName, float[] input, GLTFAccessor output, InterpolationEnum type)
            {
                var sampler = this._MakeSampler(input, output.AsVector3Array(), type);
                return new Animation.ScaleChannel(targetName, sampler);
            }

            protected override Animation.IInterpolator<Vector3> _MakeInterpolator() => new Animation.Vector3Interpolator();
        }
    }
}