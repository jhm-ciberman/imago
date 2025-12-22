using System.Drawing;
using System.Numerics;
using LifeSim.Imago.Rendering.Sprites;
using LifeSim.Imago.Assets.Textures;

namespace LifeSim.Imago.Controls;

/// <summary>
/// Represents a control that displays a texture or a region of a texture.
/// </summary>
public class TextureBlock : Control
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TextureBlock"/> class.
    /// </summary>
    public TextureBlock() : base()
    {
        //
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextureBlock"/> class with the specified texture and size.
    /// </summary>
    /// <param name="texture">The texture region to display.</param>
    /// <param name="size">The size of the texture block.</param>
    public TextureBlock(ITextureRegion texture, Vector2 size) : base()
    {
        this.Texture = texture;
        this.Size = size;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextureBlock"/> class with the specified texture.
    /// The size of the texture block will be set to the dimensions of the texture.
    /// </summary>
    /// <param name="texture">The texture region to display.</param>
    public TextureBlock(ITextureRegion texture)
    {
        this.Texture = texture;
        this.Size = new Vector2(texture.Texture.Width, texture.Texture.Height);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextureBlock"/> class with the specified size.
    /// </summary>
    /// <param name="size">The size of the texture block.</param>
    public TextureBlock(Vector2 size) : base()
    {
        this.Size = size;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextureBlock"/> class with the specified position and size.
    /// </summary>
    /// <param name="position">The position of the texture block.</param>
    /// <param name="size">The size of the texture block.</param>
    public TextureBlock(Vector2 position, Vector2 size) : base()
    {
        this.Position = position;
        this.Size = size;
    }

    /// <summary>
    /// Gets or sets the texture region to be displayed by this <see cref="TextureBlock"/>.
    /// </summary>
    public ITextureRegion? Texture { get; set; }

    /// <summary>
    /// Gets or sets the desired size of the texture block. If <see cref="float.NaN"/> for a dimension,
    /// the natural size of the <see cref="Texture"/> will be used for that dimension.
    /// </summary>
    public Vector2 Size { get; set; } = new Vector2(float.NaN, float.NaN);

    /// <summary>
    /// Gets or sets the color tint applied to the texture.
    /// </summary>
    public Color Color { get; set; } = Color.White;

    /// <summary>
    /// Gets or sets a value indicating whether the texture should be flipped horizontally.
    /// </summary>
    public bool FlipX { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the texture should be flipped vertically.
    /// </summary>
    public bool FlipY { get; set; } = false;

    /// <inheritdoc/>
    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        if (this.Texture == null)
        {
            if (float.IsNaN(this.Size.X) || float.IsNaN(this.Size.Y))
            {
                return Vector2.Zero;
            }
            else
            {
                return this.Size;
            }
        }

        float width = float.IsNaN(this.Size.X) ? this.Texture.Texture.Width : this.Size.X;
        float height = float.IsNaN(this.Size.Y) ? this.Texture.Texture.Height : this.Size.Y;
        return new Vector2(width, height);
    }

    /// <inheritdoc/>
    protected override void DrawCore(DrawingContext ctx)
    {
        base.DrawCore(ctx);

        if (this.Texture != null)
        {
            var coords = GetTextureCoordinates(this.Texture, this.FlipX, this.FlipY);
            ctx.DrawTexture(this.Texture.Texture, this.Position, this.ActualSize, coords.TopLeft, coords.BottomRight, this.Color);
        }
    }

    private static (Vector2 TopLeft, Vector2 BottomRight) GetTextureCoordinates(ITextureRegion texture, bool flipX, bool flipY)
    {
        Vector2 tl = texture.TopLeft;
        Vector2 br = texture.BottomRight;

        if (flipX) (tl.X, br.X) = (br.X, tl.X);
        if (flipY) (tl.Y, br.Y) = (br.Y, tl.Y);

        return (tl, br);
    }
}
