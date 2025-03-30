using System.Numerics;
using FontStashSharp;
using LifeSim.Imago.Rendering.Sprites;
using LifeSim.Support.Drawing;

namespace LifeSim.Imago.Controls;

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
    public MultiTextEffect(ITextEffect[] effects)
    {
        this.Effects = effects;
    }

    public void Draw(DrawingContext ctx, string text, SpriteFontBase font, Vector2 position, Color color)
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
