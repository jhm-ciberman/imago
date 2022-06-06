using System.Collections.Generic;

namespace LifeSim.Engine.Resources;

/// <summary>
/// A sprite is a container for one or more PackedTextures. The sprite can be animated.
/// </summary>
public class Sprite
{
    private readonly List<PackedTexture> _frames = new List<PackedTexture>();

    /// <summary>
    /// Gets a list of all frames of the sprite.
    /// </summary>
    public IReadOnlyList<PackedTexture> Frames => this._frames;

    /// <summary>
    /// Adds a frame to the sprite.
    /// </summary>
    /// <param name="frame">The frame to add.</param>
    public void AddFrame(PackedTexture frame)
    {
        this._frames.Add(frame);
    }

    /// <summary>
    /// Removes a frame from the sprite.
    /// </summary>
    /// <param name="frame">The frame to remove.</param>
    public void RemoveFrame(PackedTexture frame)
    {
        this._frames.Remove(frame);
    }
}