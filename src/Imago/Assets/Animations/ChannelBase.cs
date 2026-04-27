namespace Imago.Assets.Animations;

/// <summary>
/// Provides a base implementation for animation channels.
/// </summary>
/// <typeparam name="T">The type of value this channel animates.</typeparam>
public abstract class ChannelBase<T> : IChannel where T : struct
{
    /// <inheritdoc />
    public string TargetName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelBase{T}"/> class.
    /// </summary>
    /// <param name="targetName">The name of the bone this channel animates.</param>
    public ChannelBase(string targetName)
    {
        this.TargetName = targetName;
    }

    /// <inheritdoc />
    public abstract float Duration { get; }

    /// <inheritdoc />
    public abstract void Sample(Pose pose, float time);
}
