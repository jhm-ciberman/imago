using LifeSim.Imago.SceneGraph.Nodes;

namespace LifeSim.Imago.Assets.Animations;

/// <summary>
/// Represents an animation channel that animates a specific property of a target node.
/// </summary>
public interface IChannel
{
    /// <summary>
    /// Gets the name of the target node that this channel animates.
    /// </summary>
    public string TargetName { get; }

    /// <summary>
    /// Gets the duration of the animation channel in seconds.
    /// </summary>
    public float Duration { get; }

    /// <summary>
    /// Updates the target node's property based on the current animation time.
    /// </summary>
    /// <param name="target">The target node to update.</param>
    /// <param name="time">The current animation time in seconds.</param>
    public void Update(Node3D target, float time);
}
