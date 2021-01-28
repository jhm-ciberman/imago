using System.Collections.Generic;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.Anim
{
    public class BindedAnimation
    {
        private struct BindedChannel
        {
            private readonly Node3D _target;
            public Node3D target => this._target;

            private readonly IReadOnlyList<Animation.IChannel> _channel;
            private int _lastTimeIndex; 

            public BindedChannel(Node3D target, IReadOnlyList<Animation.IChannel> channels) 
            {
                this._target = target;
                this._channel = channels;
                this._lastTimeIndex = 0;
            }

            public void Update(float time, bool loop)
            {
                for (int i = 0; i < this._channel.Count; i++) {
                    this._channel[i].UpdateTarget(this._target, time, loop, ref this._lastTimeIndex);
                }
            }
        }

        private readonly IList<BindedChannel> _channels = new List<BindedChannel>();

        private float _time = 0f;

        private readonly bool _loop = true;

        private readonly Animation _animation;

        public BindedAnimation(Animation animation)
        {
            this._animation = animation;
        }

        public IEnumerable<Node3D> nodes
        {
            get
            {
                foreach (var channel in this._channels) {
                    yield return channel.target;
                }
            }
        }

        public void AddChannel(Node3D target, IReadOnlyList<Animation.IChannel> channels)
        {
            this._channels.Add(new BindedChannel(target, channels));
        }

        public void Update(float deltaTime)
        {
            this._time += deltaTime;
            if (this._loop) {
                this._time %= this._animation.duration;
            }

            foreach (var channel in this._channels) {
                channel.Update(this._time, this._loop);
            }
        }
    }
}