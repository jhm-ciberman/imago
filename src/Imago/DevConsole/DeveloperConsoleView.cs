using System;
using System.Numerics;
using FontStashSharp;
using Imago.Controls;
using Imago.Controls.Drawing;
using Imago.Input;
using Imago.Rendering.Sprites;
using Imago.Support.Drawing;
using Imago.Support.Numerics;
using Veldrid;

namespace Imago.DevConsole;

/// <summary>
/// The view for the developer console, rendering the console model.
/// </summary>
public class DeveloperConsoleView : DockPanel
{
    private static readonly Color _backgroundColor = TailwindColors.Slate900.WithAlpha(230f / 255f);
    private static readonly Color _inputBackgroundColor = TailwindColors.Slate800;
    private static readonly Color _separatorColor = TailwindColors.Slate700;
    private static readonly Color _outputTextColor = TailwindColors.Slate300;
    private static readonly Color _commandTextColor = TailwindColors.Slate100;
    private static readonly Color _promptColor = TailwindColors.Emerald400;
    private static readonly Color _errorTextColor = TailwindColors.Red400;

    private static readonly TextFont _consoleFont = new(FontLoader.Load("res/fonts/basis33.ttf"), 16f, 16f);
    private const float OutputPadding = 8f;
    private const float InputRowHeight = 28f;
    private const string PromptSymbol = "> ";

    private readonly DeveloperConsole _console;
    private readonly TextBox _inputBox;
    private readonly InputManager _input;

    private int _scrollOffset;
    private bool _autoScrollToBottom = true;
    private int _historyIndex = -1;
    private string _savedInput = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeveloperConsoleView"/> class.
    /// </summary>
    /// <param name="console">The console model to render.</param>
    public DeveloperConsoleView(DeveloperConsole console)
    {
        this._console = console;

        this.VerticalAlignment = VerticalAlignment.Top;
        this.HorizontalAlignment = HorizontalAlignment.Stretch;
        this.Height = 300;
        this.Background = new ColorBackground(_backgroundColor);
        this.LastChildFill = true;

        var promptLabel = new TextBlock(PromptSymbol)
        {
            Font = _consoleFont,
            Foreground = _promptColor,
            Dock = Dock.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 0, 0),
        };

        this._inputBox = new TextBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Padding = new Thickness(2, 4, 8, 4),
        };
        this._inputBox.TextBlock.Font = _consoleFont;
        this._inputBox.TextBlock.Foreground = _commandTextColor;
        this._inputBox.CaretColor = _promptColor;

        var inputRow = new DockPanel
        {
            Dock = Dock.Bottom,
            Height = InputRowHeight,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = new ColorBackground(_inputBackgroundColor),
            LastChildFill = true,
        };
        inputRow.Items.Add(promptLabel);
        inputRow.Items.Add(this._inputBox);

        var separator = new Control
        {
            Dock = Dock.Bottom,
            Height = 1,
            Background = new ColorBackground(_separatorColor),
        };

        var outputPanel = new ConsoleOutputControl(this)
        {
            Margin = new Thickness(OutputPadding),
            VerticalAlignment = VerticalAlignment.Top,
        };

        this.Items.Add(inputRow);
        this.Items.Add(separator);
        this.Items.Add(outputPanel);

        this._input = InputManager.Instance;
        this._input.KeyPressed += this.InputManager_KeyPressed;
        this._console.OutputChanged += this.Console_OutputChanged;
        this._console.ExitRequested += (s, e) =>
        {
            if (this.Layer != null) this.Layer.IsVisible = false;
        };
    }

    private int VisibleLineCount
    {
        get
        {
            const float separatorHeight = 1f;
            float availableHeight = this.ActualSize.Y - InputRowHeight - separatorHeight - (OutputPadding * 2);
            return Math.Max(1, (int)(availableHeight / _consoleFont.LineHeight));
        }
    }

    private int MaxScrollOffset => Math.Max(0, this._console.Lines.Count - this.VisibleLineCount);

    private void Console_OutputChanged(object? sender, EventArgs e)
    {
        if (this._autoScrollToBottom)
        {
            this._scrollOffset = this.MaxScrollOffset;
        }
    }

    private void Scroll(int delta)
    {
        int oldOffset = this._scrollOffset;
        this._scrollOffset = Math.Clamp(this._scrollOffset + delta, 0, this.MaxScrollOffset);

        if (this._scrollOffset != oldOffset)
        {
            this._autoScrollToBottom = this._scrollOffset >= this.MaxScrollOffset;
        }
    }

    /// <inheritdoc/>
    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        if (this.Layer?.IsReceivingInput != true) return;

        float wheelDelta = this.Layer.Input.MouseScrollDelta;
        if (wheelDelta != 0)
        {
            int scrollLines = wheelDelta > 0 ? -3 : 3;
            this.Scroll(scrollLines);
        }
    }

    private void SubmitCommand()
    {
        var command = this._inputBox.Text.Trim();
        this._inputBox.Text = string.Empty;
        this._inputBox.CaretIndex = 0;
        this._historyIndex = -1;
        this._savedInput = string.Empty;

        this._console.SubmitCommand(command);
    }

    private void NavigateHistory(int direction)
    {
        var history = this._console.History;
        if (history.Count == 0)
        {
            return;
        }

        if (this._historyIndex == -1)
        {
            this._savedInput = this._inputBox.Text;
        }

        int newIndex = this._historyIndex + direction;
        newIndex = Math.Clamp(newIndex, -1, history.Count - 1);

        if (newIndex == this._historyIndex)
        {
            return;
        }

        this._historyIndex = newIndex;

        if (this._historyIndex == -1)
        {
            this._inputBox.Text = this._savedInput;
        }
        else
        {
            this._inputBox.Text = history[history.Count - 1 - this._historyIndex];
        }

        this._inputBox.CaretIndex = this._inputBox.Text.Length;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this._input.KeyPressed -= this.InputManager_KeyPressed;
            this._console.OutputChanged -= this.Console_OutputChanged;
        }

        base.Dispose(disposing);
    }

    private void InputManager_KeyPressed(object? sender, KeyboardEventArgs e)
    {
        if (this.Layer?.IsReceivingInput != true) return;

        if (e.Key == Key.Escape)
        {
            if (this.Layer != null) this.Layer.IsVisible = false;
            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            this.SubmitCommand();
            e.Handled = true;
        }
        else if (e.Key == Key.Up)
        {
            this.NavigateHistory(1);
            e.Handled = true;
        }
        else if (e.Key == Key.Down)
        {
            this.NavigateHistory(-1);
            e.Handled = true;
        }
    }

    /// <summary>
    /// Renders the console output with per-line coloring based on line kind.
    /// Uses <see cref="TextLayout"/> for proper inline content (emoji) rendering.
    /// </summary>
    private sealed class ConsoleOutputControl : Control
    {
        private readonly DeveloperConsoleView _owner;
        private readonly TextLayout _layout = new()
        {
            FontSize = _consoleFont.Size,
            LineHeight = _consoleFont.LineHeight,
        };

        private float _promptWidth = -1f;

        public ConsoleOutputControl(DeveloperConsoleView owner)
        {
            this._owner = owner;
        }

        /// <inheritdoc/>
        protected override void DrawCore(DrawingContext ctx)
        {
            base.DrawCore(ctx);

            var console = this._owner._console;
            var lines = console.Lines;
            if (lines.Count == 0) return;

            int startLine = Math.Clamp(this._owner._scrollOffset, 0, Math.Max(0, lines.Count - 1));
            int endLine = Math.Min(startLine + this._owner.VisibleLineCount, lines.Count);

            var font = _consoleFont.System.GetFont(_consoleFont.Size);
            this._layout.Font = font;

            if (this._promptWidth < 0f)
            {
                this._layout.Text = PromptSymbol;
                this._promptWidth = this._layout.GetLineWidth(0);
            }

            for (int i = startLine; i < endLine; i++)
            {
                var line = lines[i];
                float x = this.Position.X;
                float y = this.Position.Y + (i - startLine) * _consoleFont.LineHeight;

                switch (line.Kind)
                {
                    case ConsoleLineKind.Command:
                        this.DrawSegmentedText(ctx, font, PromptSymbol, x, y, _promptColor);
                        this.DrawSegmentedText(ctx, font, line.Text, x + this._promptWidth, y, _commandTextColor);
                        break;

                    case ConsoleLineKind.Error:
                        this.DrawSegmentedText(ctx, font, line.Text, x, y, _errorTextColor);
                        break;

                    default:
                        this.DrawSegmentedText(ctx, font, line.Text, x, y, _outputTextColor);
                        break;
                }
            }
        }

        private void DrawSegmentedText(DrawingContext ctx, SpriteFontBase font, string text, float x, float y, Color color)
        {
            this._layout.Text = text;

            var yOffset = MathF.Ceiling(this._layout.LineSpacing / 2f);
            float cursorX = x;
            float lineY = y + yOffset;

            for (int lineIdx = 0; lineIdx < this._layout.LineCount; lineIdx++)
            {
                var lineText = this._layout.GetLineText(lineIdx);
                int segCount = this._layout.GetLineSegmentCount(lineIdx);

                for (int s = 0; s < segCount; s++)
                {
                    var segment = this._layout.GetLineSegment(lineIdx, s);
                    if (segment.IsInlineContent)
                    {
                        var inlineYOffset = (this._layout.ActualLineHeight - segment.Size.Y) / 2f;
                        ctx.DrawTexture(
                            segment.Texture!,
                            new Vector2(cursorX, lineY + inlineYOffset - this._layout.LineSpacing / 2f),
                            segment.Size
                        );
                        cursorX += segment.Size.X;
                    }
                    else
                    {
                        var segText = lineText.Substring(segment.Start, segment.Length);
                        ctx.DrawText(font, segText, new Vector2(cursorX, lineY), color);
                        cursorX += font.MeasureString(segText).X;
                    }
                }

                lineY += this._layout.ActualLineHeight;
                cursorX = x;
            }
        }
    }
}
