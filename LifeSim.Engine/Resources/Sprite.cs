using System;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.Rendering;
using LifeSim.Utils;

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
    /// Gets whether the sprite is animated.
    /// </summary>
    public bool IsAnimated => this._frames.Count > 1;

    private Thickness _nineSliceMargin  = new Thickness(0);

    /// <summary>
    /// Gets whether the sprite is a 9-slice.
    /// </summary>
    public bool IsNineSlice { get; private set; } = false;

    /// <summary>
    /// Gets or sets the margin to be used when the sprite is drawn as a 9-slice. 
    /// If the margin is all zero, the sprite will be drawn as a regular sprite.
    /// </summary>
    public Thickness NineSliceMargin
    {
        get => this._nineSliceMargin;
        set
        {
            this._nineSliceMargin = value;
            this.IsNineSlice = value.Left != 0 || value.Top != 0 || value.Right != 0 || value.Bottom != 0;
        }
    }

    /// <summary>
    /// Gets or sets the scale of the sprite.
    /// </summary>
    public float Scale { get; set; } = 1f;

    /// <summary>
    /// Initializes a new instance of the <see cref="Sprite"/> class.
    /// </summary>
    public Sprite()
    {
        //
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Sprite"/> class from a <see cref="PackedTexture"/>.
    /// </summary>
    /// <param name="texture">The texture to display.</param>
    public Sprite(PackedTexture texture)
    {
        this._frames.Add(texture);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Sprite"/> class from a list of <see cref="PackedTexture"/>.
    /// </summary>
    /// <param name="frames">The list of textures to display.</param>
    public Sprite(IEnumerable<PackedTexture> frames)
    {
        this._frames.AddRange(frames);

        // Validate that all frames have the same size
        var firstFrame = this._frames[0];
        foreach (var frame in this._frames)
        {
            if (frame.Size != firstFrame.Size)
            {
                throw new ArgumentException("All frames of a sprite must have the same size.");
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Sprite"/> class from a <see cref="Texture"/>.
    /// </summary>
    /// <param name="texture">The texture to display.</param>
    public Sprite(Texture texture)
        : this(new PackedTexture(texture, Vector2.Zero, Vector2.One))
    {
    }

    /// <summary>
    /// Adds a frame to the sprite.
    /// </summary>
    /// <param name="frame">The frame to add.</param>
    public void AddFrame(PackedTexture frame)
    {
        this._frames.Add(frame);

        // Validate that the frame has the same size as the first frame
        var firstFrame = this._frames[0];
        if (frame.Size != firstFrame.Size)
        {
            throw new ArgumentException("The added frame must have the same size as the first frame.");
        }
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
    /// <param name="frameCount">The number of frames in the sprite.</param>
    /// <param name="size">The size of each frame in pixels.</param>
    /// <param name="offset">The offset of each frame from the top-left corner of the sprite sheet in pixels.</param>
    /// <param name="nineSliceMargin">The margin to be used when the sprite is drawn as a 9-slice.
    /// <returns>The sprite.</returns>
    public static Sprite FromSpriteSheet(PackedTexture packedTexture, int frameCount, Vector2 size, Vector2 offset = default, Thickness nineSliceMargin = default)
    {
        var sprite = new Sprite();
        sprite.NineSliceMargin = nineSliceMargin;

        var atlasTexture = packedTexture.Texture;
        var spriteSheetSize = packedTexture.PixelSize;
        var offsetUv = packedTexture.TopLeft;


        var pixelsToUv = new Vector2(1f / (float)atlasTexture.Width, 1f / (float)atlasTexture.Height);

        int frameCountX = (int)(spriteSheetSize.X / size.X);
        for (int i = 0; i < frameCount; i++)
        {
            float x0 = (i % frameCountX) * size.X + offset.X;
            float y0 = (i / frameCountX) * size.Y + offset.Y;
            float x1 = x0 + size.X;
            float y1 = y0 + size.Y;
            var topLeft = new Vector2(x0, y0) * pixelsToUv + offsetUv;
            var bottomRight = new Vector2(x1, y1) * pixelsToUv + offsetUv;
            sprite.AddFrame(new PackedTexture(atlasTexture, topLeft, bottomRight));
        }

        return sprite;
    }
}