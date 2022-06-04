using System;
using System.Collections.Generic;

namespace LifeSim.Engine.Anim;

public class Animation : IDisposable
{
    private readonly Dictionary<string, List<IChannel>> _channels = new Dictionary<string, List<IChannel>>();

    public float Duration { get; private set; }

    public string Name { get; }

    public IEnumerable<string> ChannelNames => this._channels.Keys;

    public Animation(string name, IReadOnlyList<IChannel> channels)
    {
        this.Name = name;

        foreach (var channel in channels)
        {
            this.AddChannel(channel);
        }
    }

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

    public void Dispose()
    {
        // TODO: Dispose animation.
    }

    public IReadOnlyList<IChannel>? FindChannels(string targetName)
    {
        this._channels.TryGetValue(targetName, out List<IChannel>? list);
        return list;
    }
}