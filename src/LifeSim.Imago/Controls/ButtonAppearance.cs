using LifeSim.Imago.Controls.Drawing;
using LifeSim.Imago.Assets.Textures;
using LifeSim.Support.Drawing;

namespace LifeSim.Imago.Controls;

/// <summary>
/// Defines the visual appearance of a <see cref="Button"/> for different states (idle, hover, pressed, disabled).
/// </summary>
public class ButtonAppearance
{
    private static readonly ButtonAppearance _default = new ButtonAppearance();

    /// <summary>
    /// Gets the default button appearance.
    /// </summary>
    public static ButtonAppearance Default => _default;

    /// <summary>
    /// Gets or sets the background for the button's idle state.
    /// </summary>
    public IBackground Idle { get; set; } = new ColorBackground(Color.White);

    /// <summary>
    /// Gets or sets the background for the button's hover state.
    /// </summary>
    public IBackground Hover { get; set; } = new ColorBackground(Color.White, 0.9f);

    /// <summary>
    /// Gets or sets the background for the button's pressed state.
    /// </summary>
    public IBackground Pressed { get; set; } = new ColorBackground(Color.White, 0.8f);

    /// <summary>
    /// Gets or sets the background for the button's disabled state.
    /// </summary>
    public IBackground Disabled { get; set; } = new ColorBackground(Color.White, 0.5f);


    /// <summary>
    /// Initializes a new instance of the <see cref="ButtonAppearance"/> class.
    /// </summary>
    public ButtonAppearance()
    {
        //
    }

    /// <summary>
    /// Applies the appropriate background to the specified button based on its current state.
    /// </summary>
    /// <param name="button">The button to apply the appearance to.</param>
    public void Apply(Button button)
    {
        if (!button.IsEnabled)
        {
            button.Background = this.Disabled;
        }
        else if (button.IsPressed)
        {
            button.Background = this.Pressed;
        }
        else if (button.IsMouseOver)
        {
            button.Background = this.Hover;
        }
        else
        {
            button.Background = this.Idle;
        }
    }

    /// <summary>
    /// Creates a <see cref="ButtonAppearance"/> with color variations for different states.
    /// </summary>
    /// <param name="color">The base color for the button.</param>
    /// <returns>A new <see cref="ButtonAppearance"/> instance.</returns>
    public static ButtonAppearance FromColor(Color color)
    {
        return new ButtonAppearance()
        {
            Idle = new ColorBackground(color),
            Hover = new ColorBackground(color, 0.9f),
            Pressed = new ColorBackground(color, 0.8f),
            Disabled = new ColorBackground(color, 0.5f),
        };
    }

    /// <summary>
    /// Creates a <see cref="ButtonAppearance"/> using a sprite for different states.
    /// </summary>
    /// <remarks>
    /// The sprite should contain multiple frames representing different button states:
    /// - Frame 0: Idle
    /// - Frame 1: Hover (Optional, defaults to Idle if not present)
    /// - Frame 2: Pressed (Optional, defaults to Idle if not present)
    /// - Frame 3: Disabled (Optional, defaults to Idle if not present)
    /// </remarks>
    /// <param name="sprite">The sprite containing different frames for button states.</param>
    /// <returns>A new <see cref="ButtonAppearance"/> instance.</returns>
    public static ButtonAppearance FromSprite(Sprite sprite)
    {
        int frames = sprite.Frames.Count;
        return new ButtonAppearance()
        {
            Idle = new SpriteBackground(sprite, 0),
            Hover = new SpriteBackground(sprite, frames >= 2 ? 1 : 0),
            Pressed = new SpriteBackground(sprite, frames >= 2 ? 2 : 0),
            Disabled = new SpriteBackground(sprite, frames >= 3 ? 3 : 0),
        };
    }

    /// <summary>
    /// Creates a <see cref="ButtonAppearance"/> using a texture for all states.
    /// </summary>
    /// <param name="texture">The texture to use for all states.</param>
    /// <returns>A new <see cref="ButtonAppearance"/> instance.</returns>
    public static ButtonAppearance FromTexture(ITextureRegion texture)
    {
        var bg = new TextureBackground(texture);
        return new ButtonAppearance()
        {
            Idle = bg,
            Hover = bg,
            Pressed = bg,
            Disabled = bg,
        };
    }

    /// <summary>
    /// Creates a <see cref="ButtonAppearance"/> with explicitly defined colors for each state.
    /// </summary>
    /// <param name="defaultColor">The color for the idle state.</param>
    /// <param name="hoverColor">The color for the hover state.</param>
    /// <param name="pressedColor">The color for the pressed state.</param>
    /// <param name="disabledColor">The color for the disabled state.</param>
    /// <returns>A new <see cref="ButtonAppearance"/> instance.</returns>
    public static ButtonAppearance FromColors(Color defaultColor, Color hoverColor, Color pressedColor, Color disabledColor)
    {
        return new ButtonAppearance()
        {
            Idle = new ColorBackground(defaultColor),
            Hover = new ColorBackground(hoverColor),
            Pressed = new ColorBackground(pressedColor),
            Disabled = new ColorBackground(disabledColor),
        };
    }
}
