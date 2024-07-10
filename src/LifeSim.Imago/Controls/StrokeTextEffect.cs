using System.Numerics;
using LifeSim.Imago.Graphics;
using LifeSim.Imago.Graphics.Rendering;
using LifeSim.Support.Drawing;

namespace LifeSim.Imago.Controls;

public class StrokeTextEffect : ITextEffect
{
    /// <summary>
    /// Gets or sets the color of the stroke.
    /// </summary>
    public Color Color { get; set; } = Color.Black;

    /// <summary>
    /// Gets or sets the stroke thickness.
    /// </summary>
    public float Thickness { get; set; } = 1;

    /// <summary>
    /// Initializes a new instance of the <see cref="StrokeTextEffect"/> class.
    /// </summary>
    /// <param name="color">The color of the stroke.</param>
    /// <param name="thickness">The thickness of the stroke.</param>
    public StrokeTextEffect(Color color, float thickness = 1)
    {
        this.Color = color;
        this.Thickness = thickness;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StrokeTextEffect"/> class.
    /// </summary>
    public StrokeTextEffect()
    {
    }

    private static readonly Vector2[] _strokeOffsets = new Vector2[]
    {
        new Vector2(0, -1), new Vector2(1, 0), new Vector2(0, 1), new Vector2(-1, 0)
    };

    public void Draw(DrawingContext ctx, string text, Font font, Vector2 position, Color color)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var strokeColor = this.Color;
        var strokeThickness = this.Thickness;

        if (strokeColor.A > 0 && strokeThickness > 0)
        {
            for (var i = 0; i < _strokeOffsets.Length; i++)
            {
                var offset = _strokeOffsets[i];
                var strokePosition = position + offset * strokeThickness;
                ctx.DrawText(font, text, strokePosition, strokeColor);
            }
        }

        if (color.A > 0)
        {
            ctx.DrawText(font, text, position, color);
        }
    }
}

public class MultiTextEffect : ITextEffect
{
    /// <summary>
    /// Gets or sets the text effects.
    /// </summary>
    public ITextEffect[] Effects { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiTextEffect"/> class.
    /// </summary>
    /// <param name="effects">The text effects.</param>
    public MultiTextEffect(params ITextEffect[] effects)
    {
        this.Effects = effects;
    }

    public void Draw(DrawingContext ctx, string text, Font font, Vector2 position, Color color)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        foreach (var effect in this.Effects)
        {
            effect.Draw(ctx, text, font, position, color);
        }
    }
}
