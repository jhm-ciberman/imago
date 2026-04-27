using System;
using System.Collections.Generic;
using Imago.SceneGraph.Nodes;

namespace Imago.Assets.Animations;

/// <summary>
/// Plays an <see cref="Animation"/> on a scene-graph hierarchy.
/// </summary>
/// <remarks>
/// Set <see cref="Root"/>, call <see cref="Play(Animation)"/> or <see cref="CrossFade"/> to choose a clip, and tick
/// <see cref="Update"/> every frame.
/// </remarks>
public class AnimationPlayer
{
    private readonly Dictionary<string, Node3D> _namesToNodes = new();

    private readonly Pose _currentPose = new();
    private readonly Pose _outgoingPose = new();
    private readonly Pose _finalPose = new();

    private Node3D? _root = null;
    private float _currentTime = 0f;
    private bool _hasAppliedPose = false;

    private float _blendDuration = 0f;
    private float _blendElapsed = 0f;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnimationPlayer"/> class.
    /// </summary>
    /// <param name="root">The root node of the hierarchy to be animated.</param>
    /// <param name="animation">The animation to play, or null to start idle.</param>
    public AnimationPlayer(Node3D? root = null, Animation? animation = null)
    {
        this.Root = root;
        if (animation != null)
        {
            this.Play(animation);
        }
    }

    /// <summary>
    /// Gets or sets the root node of the hierarchy this player drives.
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
    /// Gets the current <see cref="Animation"/> being played, or null if none is active.
    /// </summary>
    public Animation? Animation { get; private set; } = null;

    /// <summary>
    /// Gets the duration of the current animation in seconds, or 0 if none is active.
    /// </summary>
    public float Duration => this.Animation?.Duration ?? 0f;

    /// <summary>
    /// Gets or sets the current playhead time of the active clip in seconds. The setter clamps to [0, <see cref="Duration"/>].
    /// </summary>
    public float CurrentTime
    {
        get => this._currentTime;
        set => this._currentTime = Math.Clamp(value, 0f, this.Duration);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the active clip should loop when its end is reached.
    /// </summary>
    public bool IsLooping { get; set; } = true;

    /// <summary>
    /// Gets or sets the playback speed multiplier. 1 is normal, 0 is paused, negative reverses, &gt; 1 fast-forwards.
    /// </summary>
    public float PlaybackSpeed { get; set; } = 1f;

    /// <summary>
    /// Gets a value indicating whether the player is currently fading in a new clip.
    /// </summary>
    public bool IsBlending { get; private set; } = false;

    /// <summary>
    /// Gets the progress of the active crossfade in the [0, 1] range. Returns 1 when no blend is in flight.
    /// </summary>
    public float BlendProgress
    {
        get
        {
            if (!this.IsBlending) return 1f;
            if (this._blendDuration <= 0f) return 1f;
            return Math.Clamp(this._blendElapsed / this._blendDuration, 0f, 1f);
        }
    }

    /// <summary>
    /// Plays the given animation immediately, replacing any active clip. No crossfade is performed.
    /// </summary>
    /// <param name="animation">The animation to play.</param>
    public void Play(Animation animation)
    {
        this.Play(animation, this.IsLooping);
    }

    /// <summary>
    /// Plays the given animation immediately with an explicit loop setting, replacing any active clip. No crossfade is performed.
    /// </summary>
    /// <param name="animation">The animation to play.</param>
    /// <param name="loop">Whether the new clip should loop.</param>
    public void Play(Animation animation, bool loop)
    {
        this.Animation = animation;
        this._currentTime = 0f;
        this.IsLooping = loop;
        this.IsBlending = false;
        this._blendDuration = 0f;
        this._blendElapsed = 0f;

        if (this.PlaybackSpeed == 0f)
        {
            this.PlaybackSpeed = 1f;
        }
    }

    /// <summary>
    /// Crossfades to the given animation over <paramref name="blendDuration"/> seconds.
    /// </summary>
    /// <remarks>
    /// The visible pose is captured as a frozen "fade from" snapshot. Chained crossfades during an in-flight blend remain
    /// pop-free: the currently-blended pose is what gets snapshotted. A non-positive <paramref name="blendDuration"/> or
    /// a crossfade with no prior active clip degrades to a hard cut.
    /// </remarks>
    /// <param name="animation">The animation to fade in.</param>
    /// <param name="blendDuration">The fade duration in real seconds.</param>
    /// <param name="loop">Whether the new clip should loop.</param>
    public void CrossFade(Animation animation, float blendDuration, bool loop)
    {
        if (blendDuration <= 0f || this.Animation == null || !this._hasAppliedPose)
        {
            this.Play(animation, loop);
            return;
        }

        this._outgoingPose.CopyFrom(this._finalPose);

        foreach (string boneName in animation.ChannelNames)
        {
            if (!this._outgoingPose.TryGet(boneName, out _) && this._namesToNodes.TryGetValue(boneName, out Node3D? node))
            {
                this._outgoingPose.Set(boneName, new BoneTransform(node.Position, node.Rotation, node.Scale));
            }
        }

        this.Animation = animation;
        this._currentTime = 0f;
        this.IsLooping = loop;
        this.IsBlending = true;
        this._blendDuration = blendDuration;
        this._blendElapsed = 0f;

        if (this.PlaybackSpeed == 0f)
        {
            this.PlaybackSpeed = 1f;
        }
    }

    /// <summary>
    /// Pauses the animation playback.
    /// </summary>
    public void Pause()
    {
        this.PlaybackSpeed = 0f;
    }

    /// <summary>
    /// Resumes the animation playback at normal speed.
    /// </summary>
    public void Resume()
    {
        this.PlaybackSpeed = 1f;
    }

    /// <summary>
    /// Advances the animation by <paramref name="deltaTime"/> seconds and writes the resulting pose to the bound hierarchy.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last update, in seconds.</param>
    public void Update(float deltaTime)
    {
        if (this.Animation == null) return;

        float scaledDelta = deltaTime * this.PlaybackSpeed;
        this._currentTime += scaledDelta;

        float duration = this.Animation.Duration;
        if (this.IsLooping && duration > 0f)
        {
            this._currentTime %= duration;
            if (this._currentTime < 0f) this._currentTime += duration;
        }
        else
        {
            this._currentTime = Math.Clamp(this._currentTime, 0f, duration);
        }

        this._currentPose.Clear();
        this.Animation.Sample(this._currentPose, this._currentTime);

        Pose poseToApply;
        if (this.IsBlending)
        {
            this._blendElapsed += scaledDelta;
            float t = this._blendDuration > 0f
                ? Math.Clamp(this._blendElapsed / this._blendDuration, 0f, 1f)
                : 1f;

            Pose.Lerp(this._outgoingPose, this._currentPose, t, this._finalPose);

            if (t >= 1f)
            {
                this.IsBlending = false;
            }

            poseToApply = this._finalPose;
        }
        else
        {
            this._finalPose.CopyFrom(this._currentPose);
            poseToApply = this._finalPose;
        }

        this.ApplyPose(poseToApply);
        this._hasAppliedPose = true;
    }

    /// <summary>
    /// Re-resolves bone names against the current root hierarchy. Call after nodes under <see cref="Root"/> have been added, removed, or renamed.
    /// </summary>
    public void Rebind()
    {
        this._namesToNodes.Clear();
        this._hasAppliedPose = false;

        if (this._root == null) return;

        BuildNamesDictionary(this._root, this._namesToNodes);
    }

    private void ApplyPose(Pose pose)
    {
        foreach (string boneName in pose.BoneNames)
        {
            if (this._namesToNodes.TryGetValue(boneName, out Node3D? node) && pose.TryGet(boneName, out BoneTransform value))
            {
                node.Position = value.Position;
                node.Rotation = value.Rotation;
                node.Scale = value.Scale;
            }
        }
    }

    private static void BuildNamesDictionary(Node3D node, Dictionary<string, Node3D> namesToNodes)
    {
        if (!string.IsNullOrEmpty(node.Name) && !namesToNodes.ContainsKey(node.Name))
        {
            namesToNodes[node.Name] = node;
        }

        for (int i = 0; i < node.Children.Count; i++)
        {
            BuildNamesDictionary(node.Children[i], namesToNodes);
        }
    }
}
