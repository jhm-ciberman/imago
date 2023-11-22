using Imago.SceneGraph.Nodes;

namespace Imago.Animations;

public abstract class ChannelBase<T> : IChannel where T : struct
{
    /// <summary>
    /// Gets the target node name of the channel.
    /// </summary>
    public string TargetName { get; }

    /// <summary>
    /// Creates a new instance of the <see cref="ChannelBase{T}"/> class.
    /// </summary>
    /// <param name="targetName">The target node name of the channel.</param>
    public ChannelBase(string targetName)
    {
        this.TargetName = targetName;
    }

    /// <summary>
    /// Gets the duration of the channel in seconds.
    /// </summary>
    public abstract float Duration { get; }

    /// <summary>
    /// Updates the target node according to the given time. This method
    /// should be overriden by derived classes.
    /// </summary>
    /// <param name="target">The target node.</param>
    /// <param name="time">The time in seconds.</param>
    public abstract void Update(Node3D target, float time);

}
