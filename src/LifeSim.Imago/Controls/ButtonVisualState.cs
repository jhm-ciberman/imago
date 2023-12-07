using LifeSim.Imago.Controls.Drawing;
using LifeSim.Imago.Graphics;
using LifeSim.Support.Drawing;

namespace LifeSim.Imago.Controls;

public class ButtonVisualState
{
    private static readonly ButtonVisualState _default = new ButtonVisualState();

    public static ButtonVisualState Default => _default;

    public IBackground Idle { get; set; } = new ColorBackground(Color.White);

    public IBackground Hover { get; set; } = new ColorBackground(Color.White, 0.9f);

    public IBackground Pressed { get; set; } = new ColorBackground(Color.White, 0.8f);

    public IBackground Disabled { get; set; } = new ColorBackground(Color.White, 0.5f);


    public ButtonVisualState()
    {
        //
    }

    public void ApplyVisualState(Button button)
    {
        if (button.IsMouseOver)
        {
            button.Background = this.Hover;
        }
        else if (button.IsPressed)
        {
            button.Background = this.Pressed;
        }
        else if (!button.IsEnabled)
        {
            button.Background = this.Disabled;
        }
        else
        {
            button.Background = this.Idle;
        }
    }

    public static ButtonVisualState FromColor(Color color)
    {
        return new ButtonVisualState()
        {
            Idle = new ColorBackground(color),
            Hover = new ColorBackground(color, 0.9f),
            Pressed = new ColorBackground(color, 0.8f),
            Disabled = new ColorBackground(color, 0.5f),
        };
    }

    public static ButtonVisualState FromSprite(Sprite sprite)
    {
        return new ButtonVisualState()
        {
            Idle = new SpriteBackground(sprite, 0),
            Hover = new SpriteBackground(sprite, 1),
            Pressed = new SpriteBackground(sprite, 2),
            Disabled = new SpriteBackground(sprite, 3),
        };
    }

    public static ButtonVisualState FromColors(Color defaultColor, Color hoverColor, Color pressedColor, Color disabledColor)
    {
        return new ButtonVisualState()
        {
            Idle = new ColorBackground(defaultColor),
            Hover = new ColorBackground(hoverColor),
            Pressed = new ColorBackground(pressedColor),
            Disabled = new ColorBackground(disabledColor),
        };
    }
}
