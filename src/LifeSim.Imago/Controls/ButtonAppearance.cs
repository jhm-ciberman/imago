using LifeSim.Imago.Controls.Drawing;
using LifeSim.Imago.Graphics;
using LifeSim.Support.Drawing;

namespace LifeSim.Imago.Controls;

public class ButtonAppearance
{
    private static readonly ButtonAppearance _default = new ButtonAppearance();

    public static ButtonAppearance Default => _default;

    public IBackground Idle { get; set; } = new ColorBackground(Color.White);

    public IBackground Hover { get; set; } = new ColorBackground(Color.White, 0.9f);

    public IBackground Pressed { get; set; } = new ColorBackground(Color.White, 0.8f);

    public IBackground Disabled { get; set; } = new ColorBackground(Color.White, 0.5f);


    public ButtonAppearance()
    {
        //
    }

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

    public static ButtonAppearance FromSprite(Sprite sprite)
    {
        return new ButtonAppearance()
        {
            Idle = new SpriteBackground(sprite, 0),
            Hover = new SpriteBackground(sprite, 1),
            Pressed = new SpriteBackground(sprite, 2),
            Disabled = new SpriteBackground(sprite, 3),
        };
    }

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
