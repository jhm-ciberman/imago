using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using FontStashSharp;
using LifeSim.Engine.Rendering;
using Veldrid;

namespace LifeSim.Engine.Controls;

public class TextField : Control
{
    public bool IsFocused { get; private set; } = true;

    public Color Foreground { get; set; } = Color.Black;

    private string? _fontFamily = null;
    private int _fontSize = 30;
    private int _outline = 0;
    private int _blur = 0;

    protected bool SetFontProperty<T>(ref T backingField, T value)
    {
        if (!EqualityComparer<T>.Default.Equals(backingField, value))
        {
            backingField = value;
            this._font = null;
            return true;
        }
        return false;
    }

    public string? FontFamily
    {
        get => this._fontFamily;
        set => this.SetFontProperty(ref this._fontFamily, value);
    }

    public int FontSize
    {
        get => this._fontSize;
        set => this.SetFontProperty(ref this._fontSize, value);
    }

    public int Outline
    {
        get => this._outline;
        set => this.SetFontProperty(ref this._outline, value);
    }

    public int Blur
    {
        get => this._blur;
        set => this.SetFontProperty(ref this._blur, value);
    }

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

    protected string _text = string.Empty;

    /// <summary>
    /// Gets or sets the text of the text block.
    /// </summary>
    public string Text
    {
        get => this._text;
        set
        {
            if (this._text != value)
            {
                this._text = value;
                this._caretIndex = Math.Min(this._caretIndex, this._text.Length);
                this._caretBlinkTimer = this.CaretBlinkSpeed;
                this._caretVisible = true;
                this.InvalidateMeasure();
            }
        }
    }

    private float _caretBlinkTimer = 0f;

    private bool _caretVisible = true;

    private SpriteFontBase? _font = null;

    public SpriteFontBase GetFont()
    {
        if (this._font == null)
        {
            this._font = Font.GetFont(this.FontFamily, this.FontSize, this.Outline, this.Blur);
        }

        return this._font;
    }

    protected override Vector2 MeasureCore(Vector2 availableSize)
    {
        return this.GetFont().MeasureString(this.Text);
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
                this.CaretIndex = 0;
                break;
            case Key.End:
                this.CaretIndex = this.Text.Length;
                break;
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

            var typedCharacters = InputManager.Current.TypedCharacters;
            if (typedCharacters.Count > 0)
            {
                string typedText = string.Join(string.Empty, typedCharacters);
                this.Text = this.Text.Insert(this.CaretIndex, typedText);
                this.CaretIndex += typedText.Length;
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

    protected override void DrawCore(SpriteBatcher spriteBatcher)
    {
        // Base method draws the text. We'll draw the caret after that.
        base.DrawCore(spriteBatcher);

        var font = this.GetFont();
        spriteBatcher.DrawText(font, this.Text, this.Position, this.Foreground);

        if (this._caretVisible)
        {
            Vector2 size = Vector2.Zero;
            if (this.Text.Length > 0)
            {
                size = font.MeasureString(this.Text[..this.CaretIndex]);
            }
            ColorF caretColor = this.CaretColor ?? this.Foreground;
            Vector2 caretPos = new Vector2(this.Position.X + size.X, this.Position.Y);
            Vector2 caretSize = new Vector2(this.CaretWidth, this.FontSize);
            spriteBatcher.DrawRectangle(caretPos, caretSize, caretColor);
        }
    }

    public override void OnAddedToVisualTree(UIPage page)
    {
        base.OnAddedToVisualTree(page);

        this.IsFocused = true;
        this.CaretIndex = this.Text.Length;

        InputManager.Current.KeyPressed += this.InputManager_KeyPressed;
    }

    public override void OnRemovedFromVisualTree(UIPage page)
    {
        base.OnRemovedFromVisualTree(page);

        InputManager.Current.KeyPressed -= this.InputManager_KeyPressed;
    }


}

