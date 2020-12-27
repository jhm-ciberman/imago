using System.Collections.Generic;

namespace LifeSim.Rendering
{
    public class BindedAnimation
    {
        private struct BindedChannel
        {
            private Node3D _target;
            private IReadOnlyList<Animation.IChannel> _channel;
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

        private IList<BindedChannel> _channels = new List<BindedChannel>();

        private float _time = 0f;

        private bool _loop = true;

        private Animation _animation;

        public BindedAnimation(Node3D root, Animation animation)
        {
            this._animation = animation;
            this._BindNodeRecursive(root, animation);
        }

        public void Update(float deltaTime)
        {
            this._time += deltaTime;
            if (this._loop) {
                this._time = this._time % this._animation.duration;
            }

            foreach (var channel in this._channels) {
                channel.Update(this._time, this._loop);
            }
        }

        private void _BindNodeRecursive(Node3D node, Animation animation)
        {
            var channels = animation.FindChannels(node.name);

            if (channels != null) {
                this._channels.Add(new BindedChannel(node, channels));
            }

            foreach (var child in node.children) {
                this._BindNodeRecursive(child, animation);    
            }
        }
    }
}