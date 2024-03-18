using System;
using System.Collections.Generic;

namespace LifeSim.Imago.Animations;

/// <summary>
/// Represents an animation.
/// </summary>
public class Animation : IDisposable
{
    private readonly Dictionary<string, List<IChannel>> _channels = [];

    /// <summary>
    /// Gets the duration of the animation in seconds.
    /// </summary>
    public float Duration { get; private set; }

    /// <summary>
    /// Gets the name of the animation.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets an enumerable of all channel names.
    /// </summary>
    public IEnumerable<string> ChannelNames => this._channels.Keys;

    /// <summary>
    /// Initializes a new instance of the <see cref="Animation"/> class.
    /// </summary>
    /// <param name="name">The name of the animation.</param>
    /// <param name="channels">The channels of the animation.</param>
    public Animation(string name, IReadOnlyList<IChannel> channels)
    {
        this.Name = name;

        foreach (var channel in channels)
        {
            this.AddChannel(channel);
        }
    }

    /// <summary>
    /// Adds a channel to the animation.
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
            this._channels.Add(key, [channel]);
        }

        this.Duration = MathF.Max(channel.Duration, this.Duration);
    }

    /// <summary>
    /// Disposes the animation.
    /// </summary>
    public void Dispose()
    {
        // TODO: Dispose animation.
    }

    /// <summary>
    /// Finds all the channels that affect the given target.
    /// </summary>
    /// <param name="targetName">The name of the target.</param>
    /// <returns>An list of all the channels that affect the given target or null if no channels affect the target.</returns>
    public List<IChannel>? FindChannels(string targetName)
    {
        this._channels.TryGetValue(targetName, out List<IChannel>? list);
        return list;
    }
}
