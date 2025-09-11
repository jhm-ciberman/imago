using System.Numerics;
using LifeSim.Imago.Controls.Drawing;
using LifeSim.Support.Drawing;
using LifeSim.Support.Numerics;

namespace LifeSim.Imago.Controls;

/// <summary>
/// Presents tooltip content in an overlay.
/// </summary>
public class TooltipPresenter : ContentControl
{
    private Tooltip? _tooltip;

    /// <summary>
    /// Initializes a new instance of the <see cref="TooltipPresenter"/> class.
    /// </summary>
    public TooltipPresenter()
    {
        this.Visibility = Visibility.Collapsed;
        this.Background = new ColorBackground(Color.Black, 0.8f);
        this.Padding = new Thickness(4);
    }

    /// <summary>
    /// Gets or sets the tooltip to display.
    /// </summary>
    public new Tooltip? Tooltip
    {
        get => this._tooltip;
        set
        {
            if (this._tooltip != value)
            {
                this._tooltip = value;
                this.UpdateContent();
            }
        }
    }

    /// <summary>
    /// Updates the content based on the current tooltip.
    /// </summary>
    private void UpdateContent()
    {
        if (this._tooltip == null)
        {
            this.Content = null;
            this.Visibility = Visibility.Collapsed;
            return;
        }

        if (this._tooltip.Content != null)
        {
            this.Content = this._tooltip.Content;
        }
        else if (!string.IsNullOrEmpty(this._tooltip.Text))
        {
            this.Content = new TextBlock
            {
                Text = this._tooltip.Text,
                Foreground = Color.White
            };
        }
        else
        {
            this.Content = null;
            this.Visibility = Visibility.Collapsed;
            return;
        }

        this.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// Shows the tooltip at the specified position.
    /// </summary>
    /// <param name="position">The position to show the tooltip at.</param>
    public void Show(Vector2 position)
    {
        if (this._tooltip == null) return;

        this.Left = position.X + 10; // Offset slightly from cursor
        this.Top = position.Y + 10;
        this.Visibility = Visibility.Visible;
        this.InvalidateArrange();
    }

    /// <summary>
    /// Hides the tooltip.
    /// </summary>
    public void Hide()
    {
        this.Visibility = Visibility.Collapsed;
        this.InvalidateMeasure();
    }
}
