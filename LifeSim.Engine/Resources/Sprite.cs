using System;
using System.Collections.Generic;
using System.Numerics;

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

    /// <summary>
    /// Creates a new sprite from a sprite sheet.
    /// </summary>
    /// <param name="packedTexture">The sprite sheet.</param>
    /// <param name="frameWidth">The width of each frame in the sprite sheet.</param>
    /// <param name="frameHeight">The height of each frame in the sprite sheet.</param>
    /// <param name="maxFrameCount">The maximum number of frames in the sprite sheet.</param>
    /// <returns>The new sprite.</returns>
    public static Sprite FromSpriteSheet(PackedTexture packedTexture, int frameWidth, int frameHeight, int maxFrameCount = int.MaxValue)
    {
        var sprite = new Sprite();

        var atlasTexture = packedTexture.Texture;
        var spriteSheetSize = packedTexture.PixelSize;
        var offsetUv = packedTexture.TopLeft;

        var frameCountX = spriteSheetSize.X / frameWidth;
        var frameCountY = spriteSheetSize.Y / frameHeight;

        var deltaUv = new Vector2((float)frameWidth / (float)atlasTexture.Width, (float)frameHeight / (float)atlasTexture.Height);

        for (var y = 0; y < frameCountY; y++)
        {
            for (var x = 0; x < frameCountX; x++)
            {
                var frameUv = new Vector2(offsetUv.X + x * deltaUv.X, offsetUv.Y + y * deltaUv.Y);
                sprite.AddFrame(new PackedTexture(atlasTexture, frameUv, frameUv + deltaUv));

                if (sprite.Frames.Count >= maxFrameCount)
                {
                    return sprite;
                }
            }
        }

        return sprite;
    }

    /// <summary>
    /// Creates a sprite with a single frame.
    /// </summary>
    /// <param name="packedTexture">The texture of the frame.</param>
    /// <returns>The new sprite.</returns>
    public static Sprite FromSingleFrame(PackedTexture packedTexture)
    {
        var sprite = new Sprite();
        sprite.AddFrame(packedTexture);
        return sprite;
    }
}