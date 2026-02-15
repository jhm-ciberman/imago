using System;
using System.ComponentModel;
using System.Numerics;
using LifeSim.Imago.Input;
using LifeSim.Imago.Rendering.Sprites;
using LifeSim.Support.Drawing;
using LifeSim.Support.Numerics;
using Veldrid;

namespace LifeSim.Imago.Controls;

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
            || e.PropertyName == nameof(this.TextBlock.Font)
            || e.PropertyName == nameof(this.TextBlock.FontSize)
            || e.PropertyName == nameof(this.TextBlock.FontSystem))
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

    private void InputManager_KeyPressed(object? sender, KeyboardEventArgs e)
    {
        if (this.Layer?.IsReceivingInput != true) return;
        if (!this.IsFocused) return;

        switch (e.Key)
        {
            case Key.BackSpace:
                this.DeleteClusterBackward();
                break;
            case Key.Delete:
                this.DeleteClusterForward();
                break;
            case Key.Left:
                this.MoveCaretLeft();
                break;
            case Key.Right:
                this.MoveCaretRight();
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
        if (this.Layer?.IsReceivingInput != true) return;

        if (this.IsFocused && e.TypedCharacters.Count > 0)
        {
            string typedText = string.Join(string.Empty, e.TypedCharacters);
            this.Text = this.Text.Insert(this.CaretIndex, typedText);
            this.CaretIndex += typedText.Length;
        }
    }

    /// <inheritdoc/>
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

    private void DeleteClusterForward()
    {
        if (this.CaretIndex >= this.Text.Length) return;

        if (this.TextBlock.TryGetInlineSegmentAt(this.CaretIndex, out var seg))
        {
            this.Text = this.Text.Remove(seg.Start, seg.Length);
            this.CaretIndex = seg.Start;
        }
        else if (char.IsHighSurrogate(this.Text[this.CaretIndex]))
        {
            this.Text = this.Text.Remove(this.CaretIndex, 2);
        }
        else
        {
            this.Text = this.Text.Remove(this.CaretIndex, 1);
        }
    }

    private void DeleteClusterBackward()
    {
        if (this.CaretIndex <= 0) return;

        // Compute the target caret position before changing text, because
        // setting Text triggers PropertyChanged which clamps _caretIndex.
        if (this.TextBlock.TryGetInlineSegmentAt(this.CaretIndex - 1, out var seg))
        {
            this.Text = this.Text.Remove(seg.Start, seg.Length);
            this.CaretIndex = seg.Start;
        }
        else if (char.IsLowSurrogate(this.Text[this.CaretIndex - 1]) && this.CaretIndex >= 2)
        {
            int newCaret = this.CaretIndex - 2;
            this.Text = this.Text.Remove(newCaret, 2);
            this.CaretIndex = newCaret;
        }
        else
        {
            int newCaret = this.CaretIndex - 1;
            this.Text = this.Text.Remove(newCaret, 1);
            this.CaretIndex = newCaret;
        }
    }

    private void MoveCaretLeft()
    {
        if (this.CaretIndex <= 0) return;

        // Skip over entire inline content clusters.
        if (this.TextBlock.TryGetInlineSegmentAt(this.CaretIndex - 1, out var seg))
        {
            this.CaretIndex = seg.Start;
            return;
        }

        this.CaretIndex--;

        // Skip over surrogate pairs so the caret never lands between them.
        if (this.CaretIndex > 0 && this.CaretIndex < this.Text.Length
            && char.IsLowSurrogate(this.Text[this.CaretIndex]))
        {
            this.CaretIndex--;
        }
    }

    private void MoveCaretRight()
    {
        if (this.CaretIndex >= this.Text.Length) return;

        // Skip over entire inline content clusters.
        if (this.TextBlock.TryGetInlineSegmentAt(this.CaretIndex, out var seg))
        {
            this.CaretIndex = seg.Start + seg.Length;
            return;
        }

        // Step over the entire surrogate pair at once.
        if (char.IsHighSurrogate(this.Text[this.CaretIndex]))
        {
            this.CaretIndex += 2;
        }
        else
        {
            this.CaretIndex++;
        }
    }

    /// <inheritdoc/>
    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        var padding = this.Padding.Total;
        availableSize -= padding;
        this.TextBlock.Measure(availableSize);

        if (this.Text.Length == 0)
        {
            return new Vector2(0f, this.TextBlock.ActualLineHeight) + padding;
        }

        return this.TextBlock.DesiredSize + padding;
    }

    /// <inheritdoc/>
    protected override Rect ArrangeOverride(Rect finalRect)
    {
        Rect rect = finalRect.Deflate(this.Padding);
        this.TextBlock.Arrange(rect);
        return finalRect;
    }

    /// <inheritdoc/>
    protected override void DrawCore(DrawingContext ctx)
    {
        base.DrawCore(ctx);

        var tb = this.TextBlock;
        tb.Draw(ctx);

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
            ctx.DrawRectangle(caretPos, caretSize, caretColor);
        }
    }

    /// <inheritdoc/>
    public override void OnAddedToLayer(GuiLayer layer)
    {
        base.OnAddedToLayer(layer);

        this.IsFocused = true;
        this.CaretIndex = this.Text.Length;

        layer.Input.KeyPressed += this.InputManager_KeyPressed;
        layer.Input.TextEntered += this.InputManager_TextEntered;
    }

    /// <inheritdoc/>
    public override void OnRemovedFromLayer(GuiLayer layer)
    {
        layer.Input.KeyPressed -= this.InputManager_KeyPressed;
        layer.Input.TextEntered -= this.InputManager_TextEntered;

        base.OnRemovedFromLayer(layer);
    }
}

