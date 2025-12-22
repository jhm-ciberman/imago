using System;
using System.Collections.Generic;

namespace LifeSim.Imago.Assets.Animations;

/// <summary>
/// Represents a collection of animation channels that work together to animate a scene graph.
/// </summary>
public class Animation : IDisposable
{
    private readonly Dictionary<string, List<IChannel>> _channels = new();

    /// <summary>
    /// Gets the total duration of the animation in seconds.
    /// </summary>
    public float Duration { get; private set; }

    /// <summary>
    /// Gets the name of the animation.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets an enumerable collection of all unique target names affected by this animation's channels.
    /// </summary>
    public IEnumerable<string> ChannelNames => this._channels.Keys;

    /// <summary>
    /// Initializes a new instance of the <see cref="Animation"/> class.
    /// </summary>
    /// <param name="name">The name of the animation.</param>
    /// <param name="channels">A list of channels that make up the animation.</param>
    public Animation(string name, IReadOnlyList<IChannel> channels)
    {
        this.Name = name;

        foreach (var channel in channels)
        {
            this.AddChannel(channel);
        }
    }

    /// <summary>
    /// Adds a channel to the animation and updates the total duration if necessary.
    /// </summary>
    /// <param name="channel">The channel to add.</param>
    public void AddChannel(IChannel channel)
    {
        string key = channel.TargetName;
        key = key.Replace("mixamorig:", "");
        if (this._channels.TryGetValue(key, out List<IChannel>? list))
        {
            list.Add(channel);
        }
        else
        {
            this._channels.Add(key, new List<IChannel> { channel });
        }

        this.Duration = MathF.Max(channel.Duration, this.Duration);
    }

    /// <summary>
    /// Disposes the animation and its resources.
    /// </summary>
    public void Dispose()
    {
        // TODO: Dispose animation.
    }

    /// <summary>
    /// Finds all channels that affect a specific target node by name.
    /// </summary>
    /// <param name="targetName">The name of the target node.</param>
    /// <returns>A list of channels that affect the specified target, or null if no channels are found.</returns>
    public List<IChannel>? FindChannels(string targetName)
    {
        this._channels.TryGetValue(targetName, out List<IChannel>? list);
        return list;
    }
}
