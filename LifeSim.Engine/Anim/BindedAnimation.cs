using System;
using System.Collections.Generic;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.Anim;

public class BindedAnimation
{
    private struct BindedChannel
    {
        public Node3D Target;

        public readonly IReadOnlyList<Animation.IChannel> Channels;
        public int LastTimeIndex;

        public BindedChannel(Node3D target, IReadOnlyList<Animation.IChannel> channels)
        {
            this.Target = target;
            this.Channels = channels;
            this.LastTimeIndex = 0;
        }
    }

    private readonly IList<BindedChannel> _bindedChannels = new List<BindedChannel>();

    private float _currentTime = 0f;

    public float CurrentTime
    {
        get => this._currentTime;
        set => this._currentTime = value % this._animation.Duration;
    }

    private readonly bool _loop = true;

    private readonly Animation _animation;

    public BindedAnimation(Animation animation)
    {
        this._animation = animation;
    }

    public IEnumerable<Node3D> Nodes
    {
        get
        {
            foreach (var channel in this._bindedChannels)
            {
                yield return channel.Target;
            }
        }
    }

    public void AddChannel(Node3D target, IReadOnlyList<Animation.IChannel> channels)
    {
        this._bindedChannels.Add(new BindedChannel(target, channels));
    }

    public void Update(float deltaTime)
    {
        this._currentTime += deltaTime;

        if (this._loop)
        {
            this._currentTime %= this._animation.Duration;
        }

        for (int i = 0; i < this._bindedChannels.Count; i++)
        {
            var bindedChannel = this._bindedChannels[i];
            for (int j = 0; j < bindedChannel.Channels.Count; j++)
            {
                bindedChannel.Channels[j].UpdateTarget(bindedChannel.Target, this._currentTime, this._loop, ref bindedChannel.LastTimeIndex);
            }
        }
    }
}