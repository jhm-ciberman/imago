using System.Windows.Input;
using FontStashSharp;
using Imago.Support.Drawing;

namespace Imago.Controls;

/// <summary>
/// Represents a button control that displays text.
/// </summary>
public class TextButton : Button
{
    private readonly TextBlock _textBlock;

    /// <summary>
    /// Initializes a new instance of the <see cref="TextButton"/> class.
    /// </summary>
    public TextButton()
    {
        this._textBlock = new TextBlock();
        this.Content = this._textBlock;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextButton"/> class.
    /// </summary>
    /// <param name="text">The text to display.</param>
    public TextButton(string text) : this()
    {
        this._textBlock.Text = text;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextButton"/> class.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="command">The command to execute when the button is clicked.</param>
    /// <param name="commandParameter">The command parameter to pass to the command.</param>
    public TextButton(string text, ICommand command, object? commandParameter = null) : this(text)
    {
        this.Command = command;
        this.CommandParameter = commandParameter;
    }

    /// <summary>
    /// Gets or sets the text of the button.
    /// </summary>
    public string Text
    {
        get => this._textBlock.Text;
        set => this._textBlock.Text = value;
    }

    /// <summary>
    /// Gets or sets the font of the text block.
    /// </summary>
    public TextFont? Font
    {
        get => this._textBlock.Font;
        set => this._textBlock.Font = value;
    }

    /// <summary>
    /// Gets or sets the text effect of the text block.
    /// </summary>
    public ITextEffect? TextEffect
    {
        get => this._textBlock.TextEffect;
        set => this._textBlock.TextEffect = value;
    }

    /// <summary>
    /// Gets or sets the font system of the text block.
    /// </summary>
    public FontSystem? FontSystem
    {
        get => this._textBlock.FontSystem;
        set => this._textBlock.FontSystem = value;
    }

    /// <summary>
    /// Gets or sets the font size of the text block.
    /// </summary>
    public float FontSize
    {
        get => this._textBlock.FontSize;
        set => this._textBlock.FontSize = value;
    }

    /// <summary>
    /// Gets or sets the line height of the text block.
    /// </summary>
    public float LineHeight
    {
        get => this._textBlock.LineHeight;
        set => this._textBlock.LineHeight = value;
    }

    /// <summary>
    /// Gets or sets the text foreground color of the text block.
    /// </summary>
    public Color Foreground
    {
        get => this._textBlock.Foreground;
        set => this._textBlock.Foreground = value;
    }

    /// <summary>
    /// Gets or sets the style of the text block.
    /// </summary>
    public IStyle? TextStyle
    {
        get => this._textBlock.Style;
        set => this._textBlock.Style = value;
    }

    /// <summary>
    /// Gets or sets the text horizontal alignment.
    /// </summary>
    public HorizontalAlignment TextHorizontalAlignment
    {
        get => this._textBlock.HorizontalAlignment;
        set => this._textBlock.HorizontalAlignment = value;
    }

    /// <summary>
    /// Gets or sets the text vertical alignment.
    /// </summary>
    public VerticalAlignment TextVerticalAlignment
    {
        get => this._textBlock.VerticalAlignment;
        set => this._textBlock.VerticalAlignment = value;
    }
}
