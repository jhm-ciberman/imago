namespace Imago.Assets.Animations;

/// <summary>
/// Represents an animation channel that animates a specific component of a single bone.
/// </summary>
public interface IChannel
{
    /// <summary>
    /// Gets the name of the bone this channel animates.
    /// </summary>
    public string TargetName { get; }

    /// <summary>
    /// Gets the duration of this channel in seconds.
    /// </summary>
    public float Duration { get; }

    /// <summary>
    /// Samples this channel at the given time and writes the result into <paramref name="pose"/>.
    /// </summary>
    /// <remarks>
    /// Only the component of the target bone that this channel animates is overwritten;
    /// other components are read from the existing entry (or <see cref="BoneTransform.Identity"/> if none).
    /// </remarks>
    /// <param name="pose">The destination pose.</param>
    /// <param name="time">The current animation time in seconds.</param>
    public void Sample(Pose pose, float time);
}
