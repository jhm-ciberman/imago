using System;
using System.ComponentModel;
using System.Numerics;
using Imago.Input;
using Imago.Rendering;
using Support;
using Veldrid;

namespace Imago.Controls;

/// <summary>
/// Represents a text box that can be used to enter text.
/// </summary>
public class TextBox : Control
{
    /// <summary>
    /// Gets whether the text box is focused.
    /// </summary>
    public bool IsFocused { get; private set; } = true;

    private int _caretIndex = 0;

    /// <summary>
    /// Gets or sets the caret index position.
    /// </summary>
    public int CaretIndex
    {
        get => this._caretIndex;
        set
        {
            if (value < 0)
            {
                value = 0;
            }
            else if (value > this.Text.Length)
            {
                value = this.Text.Length;
            }

            this._caretIndex = value;
            this._caretVisible = true;
            this._caretBlinkTimer = this.CaretBlinkSpeed;
        }
    }

    /// <summary>
    /// Gets or sets the width of the caret.
    /// </summary>
    public float CaretWidth { get; set; } = 2f;

    /// <summary>
    /// Gets or sets the caret blink speed in seconds. Zero means the caret is always visible.
    /// </summary>
    public float CaretBlinkSpeed { get; set; } = 0.5f;

    /// <summary>
    /// Gets or sets the caret color. If null, the color of the text will be used.
    /// </summary>
    public Color? CaretColor { get; set; } = null;

    /// <summary>
    /// Gets or sets the text of the text block.
    /// </summary>
    public string Text
    {
        get => this.TextBlock.Text;
        set => this.TextBlock.Text = value;
    }

    /// <summary>
    /// Gets or sets the style of the inner text block.
    /// </summary>
    public IStyle? TextBlockStyle
    {
        get => this.TextBlock.Style;
        set => this.TextBlock.Style = value;
    }

    private float _caretBlinkTimer = 0f;

    private bool _caretVisible = true;


    private void TextBlock_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(this.TextBlock.Text)
            || e.PropertyName == nameof(this.TextBlock.FontSize)
            || e.PropertyName == nameof(this.TextBlock.FontFamily))
        {
            this._caretIndex = Math.Min(this._caretIndex, this.Text.Length);
            this._caretBlinkTimer = this.CaretBlinkSpeed;
            this._caretVisible = true;
            this.InvalidateMeasure();
        }
    }

    private TextBlock? _textBlock = null;

    /// <summary>
    /// Gets or sets the inner text block used by the text box.
    /// </summary>
    public TextBlock TextBlock
    {
        get
        {
            if (this._textBlock == null)
            {
                this._textBlock = new TextBlock();
                this._textBlock.PropertyChanged += this.TextBlock_PropertyChanged;
                this.AddVisualChild(this._textBlock);
            }

            return this._textBlock;
        }
        set
        {
            if (this._textBlock != value)
            {
                if (this._textBlock != null)
                {
                    this._textBlock.PropertyChanged -= this.TextBlock_PropertyChanged;
                    this.RemoveVisualChild(this._textBlock);
                }

                this._textBlock = value;
                this._textBlock.PropertyChanged += this.TextBlock_PropertyChanged;
                this.AddVisualChild(this._textBlock);
            }
        }
    }

    private Thickness _padding = new Thickness(0);

    /// <summary>
    /// Gets or sets the padding of the text box.
    /// </summary>
    public Thickness Padding
    {
        get => this._padding;
        set => this.SetPropertyAndInvalidateMeasure(ref this._padding, value);
    }

    private void InputManager_KeyPressed(object? sender, KeyEvent e)
    {
        if (!this.IsFocused) return;

        switch (e.Key)
        {
            case Key.BackSpace:
                this.CaretIndex--;
                this.RemoveCharacter(this.CaretIndex);
                break;
            case Key.Delete:
                this.RemoveCharacter(this.CaretIndex);
                break;
            case Key.Left:
                this.CaretIndex--;
                break;
            case Key.Right:
                this.CaretIndex++;
                break;
            case Key.Home:
            case Key.Up:
                this.CaretIndex = 0;
                break;
            case Key.End:
            case Key.Down:
                this.CaretIndex = this.Text.Length;
                break;
        }
    }

    private void InputManager_TextEntered(object? sender, TextEventArgs e)
    {
        if (this.IsFocused && e.Characters.Count > 0)
        {
            string typedText = string.Join(string.Empty, e.Characters);
            this.Text = this.Text.Insert(this.CaretIndex, typedText);
            this.CaretIndex += typedText.Length;
        }
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        if (this.IsFocused)
        {
            // Update caret blink timer.
            this._caretBlinkTimer -= deltaTime;
            if (this._caretBlinkTimer <= 0f)
            {
                this._caretVisible = !this._caretVisible;
                this._caretBlinkTimer = this.CaretBlinkSpeed;
            }
        }
    }

    protected void RemoveCharacter(int index)
    {
        if (index >= this.Text.Length || index < 0) return;

        this.Text = char.IsSurrogate(this.Text, index)
            ? this.Text.Remove(index - 1, 2)
            : this.Text.Remove(index, 1);
    }

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        if (this.Text.Length == 0)
        {
            return new Vector2(0f, this.TextBlock.ActualLineHeight) + this.Padding.Total;
        }

        var padding = this.Padding.Total;
        availableSize -= padding;
        this.TextBlock.Measure(availableSize);
        return this.TextBlock.DesiredSize + padding;
    }

    protected override Rect ArrangeOverride(Rect finalRect)
    {
        Rect rect = finalRect.Deflate(this.Padding);
        this.TextBlock.Arrange(rect);
        return finalRect;
    }

    protected override void DrawCore(SpriteBatcher spriteBatcher)
    {
        base.DrawCore(spriteBatcher);

        var tb = this.TextBlock;
        tb.Draw(spriteBatcher);

        if (this._caretVisible)
        {
            Vector2 size = Vector2.Zero;
            if (tb.Text.Length > 0)
            {
                size = tb.MeasureString(this.CaretIndex);
            }
            ColorF caretColor = this.CaretColor ?? tb.Foreground;
            Vector2 caretPos = new Vector2(this.Position.X + size.X, this.Position.Y) + this.Padding.TopLeft;
            Vector2 caretSize = new Vector2(this.CaretWidth, tb.FontSize);
            spriteBatcher.DrawRectangle(caretPos, caretSize, caretColor);
        }
    }

    public override void OnAddedToStage(GuiLayer guiLayer)
    {
        base.OnAddedToStage(guiLayer);

        this.IsFocused = true;
        this.CaretIndex = this.Text.Length;

        InputManager.Current.KeyPressed += this.InputManager_KeyPressed;
        InputManager.Current.TextEntered += this.InputManager_TextEntered;
    }

    public override void OnRemovedFromStage(GuiLayer stage)
    {
        base.OnRemovedFromStage(stage);

        InputManager.Current.KeyPressed -= this.InputManager_KeyPressed;
        InputManager.Current.TextEntered -= this.InputManager_TextEntered;
    }
}

