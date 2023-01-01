using System;
using System.Collections.Generic;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.Anim;

/// <summary>
/// Provides a way to play animations.
/// </summary>
public class AnimationPlayer
{
    private record class BoundChannel(Node3D Target, List<IChannel> Channels);

    private readonly List<BoundChannel> _boundChannels = new();

    private readonly Dictionary<string, Node3D> _namesToNodes = new();

    private Node3D? _root = null;

    private Animation? _animation = null;

    private float _currentTime = 0f;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnimationPlayer"/> class.
    /// </summary>
    /// <param name="root">The root node of the animation.</param>
    /// <param name="animation">The animation to play.</param>
    public AnimationPlayer(Node3D? root = null, Animation? animation = null)
    {
        this.Root = root;
        this.Animation = animation;
    }

    /// <summary>
    /// Gets or sets the root node.
    /// </summary>
    public Node3D? Root
    {
        get => this._root;
        set
        {
            if (this._root == value) return;
            this._root = value;
            this.Rebind();
        }
    }

    /// <summary>
    /// Gets or sets the current animation.
    /// </summary>
    public Animation? Animation
    {
        get => this._animation;
        set
        {
            if (this._animation == value) return;
            this._animation = value;

            if (this._animation == null)
            {
                this._currentTime = 0f;
            }

            this.RebuildChannelsList();
        }
    }

    /// <summary>
    /// Gets the current animation duration in seconds.
    /// </summary>
    public float Duration => this.Animation?.Duration ?? 0f;

    /// <summary>
    /// Gets or sets the current time of the animation in seconds.
    /// The value will be clamped to the animation duration.
    /// </summary>
    public float CurrentTime
    {
        get => this._currentTime;
        set
        {
            value = Math.Max(0f, Math.Min(this.Duration, value));
            this._currentTime = value;
        }
    }

    /// <summary>
    /// Gets or sets whether the animation is in loop mode.
    /// </summary>
    public bool IsLooping { get; set; } = true;


    /// <summary>
    /// Gets or sets the playback speed of the animation.
    /// A value of 1 means the animation will play at its original speed.
    /// A negative value means the animation will play backwards.
    /// </summary>
    public float PlaybackSpeed { get; set; } = 1f;

    /// <summary>
    /// Plays the given animation from the beginning.
    /// </summary>
    /// <param name="animation">The animation to play.</param>
    public void Play(Animation animation)
    {
        this.Animation = animation;
        this.CurrentTime = 0f;

        if (this.PlaybackSpeed == 0f)
        {
            this.PlaybackSpeed = 1f;
        }
    }

    /// <summary>
    /// Pauses the animation.
    /// </summary>
    public void Pause()
    {
        this.PlaybackSpeed = 0f;
    }

    /// <summary>
    /// Resumes the animation.
    /// </summary>
    public void Resume()
    {
        this.PlaybackSpeed = 1f;
    }

    /// <summary>
    /// Updates the animation player.
    /// </summary>
    /// <param name="deltaTime">The time since the last update in seconds.</param>
    public void Update(float deltaTime)
    {
        if (this._animation == null) return;

        this._currentTime += deltaTime * this.PlaybackSpeed;

        if (this.IsLooping)
        {
            this._currentTime %= this._animation.Duration;
        }

        for (int i = 0; i < this._boundChannels.Count; i++)
        {
            var bindedChannel = this._boundChannels[i];
            for (int j = 0; j < bindedChannel.Channels.Count; j++)
            {
                bindedChannel.Channels[j].Update(bindedChannel.Target, this._currentTime);
            }
        }
    }

    /// <summary>
    /// Rebinds all channels to the current root.
    /// This method can be called if some nodes were added or removed or changed their names.
    /// </summary>
    public void Rebind()
    {
        this.RebuildNamesDictionary();
        this.RebuildChannelsList();
    }

    private void RebuildNamesDictionary()
    {
        static void UpdateDict(Dictionary<string, Node3D> dict, Node3D node)
        {
            if (!string.IsNullOrEmpty(node.Name))
            {
                dict[node.Name] = node;
            }

            foreach (var child in node.Children)
            {
                UpdateDict(dict, child);
            }
        }

        this._namesToNodes.Clear();

        if (this._root != null)
        {
            UpdateDict(this._namesToNodes, this._root);
        }
    }

    private void RebuildChannelsList()
    {
        this._boundChannels.Clear();
        if (this._animation == null) return;

        foreach (var channelName in this._animation.ChannelNames)
        {
            if (this._namesToNodes.TryGetValue(channelName, out Node3D? node))
            {
                var channels = this._animation.FindChannels(channelName);
                if (channels != null)
                {
                    this._boundChannels.Add(new BoundChannel(node, channels));
                }
            }
            else
            {
                Console.WriteLine("Unbound channel: " + channelName);
            }
        }
    }
}
