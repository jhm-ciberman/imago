using System.Numerics;
using FontStashSharp;
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

    // Text styling properties
    private FontSystem? _fontSystem;
    private float _fontSize = 12f;
    private float _lineHeight = 14f;
    private Color _foreground = Color.White;
    private ITextEffect? _textEffect;
    private HorizontalAlignment _textHorizontalAlignment = HorizontalAlignment.Left;
    private VerticalAlignment _textVerticalAlignment = VerticalAlignment.Top;

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
            if (this._tooltip == value) return;
            this._tooltip = value;
            this.UpdateContent();
        }
    }

    /// <summary>
    /// Gets or sets the font system for the tooltip text.
    /// </summary>
    public FontSystem? FontSystem
    {
        get => this._fontSystem;
        set => this._fontSystem = value;
    }

    /// <summary>
    /// Gets or sets the font size for the tooltip text.
    /// </summary>
    public float FontSize
    {
        get => this._fontSize;
        set => this._fontSize = value;
    }

    /// <summary>
    /// Gets or sets the line height for the tooltip text.
    /// </summary>
    public float LineHeight
    {
        get => this._lineHeight;
        set => this._lineHeight = value;
    }

    /// <summary>
    /// Gets or sets the foreground color for the tooltip text.
    /// </summary>
    public Color Foreground
    {
        get => this._foreground;
        set => this._foreground = value;
    }

    /// <summary>
    /// Gets or sets the text effect for the tooltip text.
    /// </summary>
    public ITextEffect? TextEffect
    {
        get => this._textEffect;
        set => this._textEffect = value;
    }

    /// <summary>
    /// Gets or sets the horizontal alignment for the tooltip text.
    /// </summary>
    public HorizontalAlignment TextHorizontalAlignment
    {
        get => this._textHorizontalAlignment;
        set => this._textHorizontalAlignment = value;
    }

    /// <summary>
    /// Gets or sets the vertical alignment for the tooltip text.
    /// </summary>
    public VerticalAlignment TextVerticalAlignment
    {
        get => this._textVerticalAlignment;
        set => this._textVerticalAlignment = value;
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

        (this._tooltip.Style ?? Tooltip.DefaultStyle)?.Apply(this);

        if (this._tooltip.Content != null)
        {
            this.Content = this._tooltip.Content;
        }
        else if (!string.IsNullOrEmpty(this._tooltip.Text))
        {
            this.Content = new TextBlock
            {
                Text = this._tooltip.Text,
                FontSystem = this._fontSystem,
                FontSize = this._fontSize,
                LineHeight = this._lineHeight,
                Foreground = this._foreground,
                TextEffect = this._textEffect,
                HorizontalAlignment = this._textHorizontalAlignment,
                VerticalAlignment = this._textVerticalAlignment
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
