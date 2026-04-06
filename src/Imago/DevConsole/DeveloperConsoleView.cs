using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using FontStashSharp;
using Imago.Controls;
using Imago.Controls.Drawing;
using Imago.Input;
using Imago.Rendering.Sprites;
using Imago.Support.Drawing;
using Imago.Support.Numerics;
using NeoVeldrid;

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
    private static readonly Color _popupBackgroundColor = TailwindColors.Slate800;
    private static readonly Color _popupBorderColor = TailwindColors.Slate600;
    private static readonly Color _popupSelectedColor = TailwindColors.Slate700;
    private static readonly Color _popupDescriptionColor = TailwindColors.Slate500;

    private static readonly TextFont _consoleFont = new(FontLoader.Load("res/fonts/basis33.ttf"), 16f, 16f);
    private const float OutputPadding = 8f;
    private const float InputRowHeight = 28f;
    private const string PromptSymbol = "> ";

    private readonly DeveloperConsole _console;
    private readonly TextBox _inputBox;
    private readonly InputManager _input;
    private readonly AutocompletePopup _autocomplete = new();

    private int _scrollOffset;
    private bool _autoScrollToBottom = true;
    private int _historyIndex = -1;
    private string _savedInput = string.Empty;
    private bool _suppressSuggestionUpdate;

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
        this._inputBox.TextBlock.PropertyChanged += this.InputBox_TextChanged;

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
        this._suppressSuggestionUpdate = true;
        var command = this._inputBox.Text.Trim();
        this._inputBox.Text = string.Empty;
        this._inputBox.CaretIndex = 0;
        this._historyIndex = -1;
        this._savedInput = string.Empty;
        this._suppressSuggestionUpdate = false;
        this._autocomplete.Dismiss();

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

        this._suppressSuggestionUpdate = true;

        if (this._historyIndex == -1)
        {
            this._inputBox.Text = this._savedInput;
        }
        else
        {
            this._inputBox.Text = history[history.Count - 1 - this._historyIndex];
        }

        this._inputBox.CaretIndex = this._inputBox.Text.Length;
        this._suppressSuggestionUpdate = false;
        this._autocomplete.Dismiss();
    }

    private void AcceptSuggestion()
    {
        var command = this._autocomplete.GetSelectedCommand();
        if (command == null) return;

        this._suppressSuggestionUpdate = true;
        this._inputBox.Text = command.FullName + " ";
        this._inputBox.CaretIndex = this._inputBox.Text.Length;
        this._suppressSuggestionUpdate = false;
        this._autocomplete.Dismiss();
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this._input.KeyPressed -= this.InputManager_KeyPressed;
            this._console.OutputChanged -= this.Console_OutputChanged;
            this._inputBox.TextBlock.PropertyChanged -= this.InputBox_TextChanged;
        }

        base.Dispose(disposing);
    }

    private void InputManager_KeyPressed(object? sender, KeyboardEventArgs e)
    {
        if (this.Layer?.IsReceivingInput != true) return;

        if (e.Key == Key.Escape)
        {
            if (this._autocomplete.IsVisible)
            {
                this._autocomplete.Dismiss();
            }
            else
            {
                if (this.Layer != null) this.Layer.IsVisible = false;
            }

            e.Handled = true;
        }
        else if (e.Key == Key.Tab)
        {
            if (this._autocomplete.HasSelectableSuggestions)
            {
                this.AcceptSuggestion();
            }
            else if (string.IsNullOrWhiteSpace(this._inputBox.Text))
            {
                this._autocomplete.ShowAll(this._console.Registry);
            }

            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            this.SubmitCommand();
            e.Handled = true;
        }
        else if (e.Key == Key.Up)
        {
            if (this._autocomplete.HasSelectableSuggestions)
            {
                this._autocomplete.Navigate(-1);
            }
            else
            {
                this.NavigateHistory(1);
            }

            e.Handled = true;
        }
        else if (e.Key == Key.Down)
        {
            if (this._autocomplete.HasSelectableSuggestions)
            {
                this._autocomplete.Navigate(1);
            }
            else
            {
                this.NavigateHistory(-1);
            }

            e.Handled = true;
        }
    }

    private void InputBox_TextChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TextBlock.Text))
        {
            this.UpdateSuggestions();
        }
    }

    private void UpdateSuggestions()
    {
        if (this._suppressSuggestionUpdate) return;
        this._autocomplete.Update(this._console.Registry, this._inputBox.Text);
    }

    /// <inheritdoc/>
    protected override void DrawCore(DrawingContext ctx)
    {
        base.DrawCore(ctx);

        var font = _consoleFont.System.GetFont(_consoleFont.Size);
        float popupBottomY = this.Position.Y + this.ActualSize.Y - InputRowHeight - 1f;
        this._autocomplete.Draw(ctx, font, new Vector2(this.Position.X, popupBottomY), this.ActualSize.X);
    }

    /// <summary>
    /// Manages the autocomplete popup state, including command suggestions
    /// and argument usage hints.
    /// </summary>
    private sealed class AutocompletePopup
    {
        private const int MaxVisible = 8;
        private const float ItemHeight = 20f;
        private const float PaddingX = 8f;

        private readonly List<ConsoleCommand> _suggestions = [];
        private ConsoleCommand? _argumentHint;
        private int _selectedIndex;
        private int _scrollOffset;

        /// <summary>
        /// Gets a value indicating whether the popup has any content to display.
        /// </summary>
        public bool IsVisible => this._suggestions.Count > 0 || this._argumentHint != null;

        /// <summary>
        /// Gets a value indicating whether there are selectable command suggestions.
        /// </summary>
        public bool HasSelectableSuggestions => this._suggestions.Count > 0;

        /// <summary>
        /// Updates the popup based on the current input text. Shows command suggestions
        /// for partial matches, or argument usage hints when the input exactly matches
        /// a command that accepts arguments.
        /// </summary>
        /// <param name="registry">The command registry to search.</param>
        /// <param name="input">The current input text.</param>
        public void Update(CommandRegistry registry, string input)
        {
            this._suggestions.Clear();
            this._argumentHint = null;
            this._selectedIndex = 0;
            this._scrollOffset = 0;

            if (string.IsNullOrWhiteSpace(input)) return;

            var trimmed = input.TrimEnd();

            var matches = registry.GetCommandsWithPrefix(trimmed)
                .Where(c => !c.IsHidden)
                .Where(c => !c.FullName.Equals(trimmed, StringComparison.OrdinalIgnoreCase))
                .OrderBy(c => c.FullName)
                .ToList();

            if (matches.Count > 0)
            {
                this._suggestions.AddRange(matches);
                return;
            }

            var tokens = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (registry.TryFindCommand(tokens, out var command, out _)
                && command!.Arguments.Count > 0)
            {
                this._argumentHint = command;
            }
        }

        /// <summary>
        /// Populates the popup with all non-hidden commands.
        /// </summary>
        /// <param name="registry">The command registry to enumerate.</param>
        public void ShowAll(CommandRegistry registry)
        {
            this._suggestions.Clear();
            this._argumentHint = null;
            this._selectedIndex = 0;
            this._scrollOffset = 0;

            var all = registry.Commands
                .Where(c => !c.IsHidden)
                .OrderBy(c => c.FullName)
                .ToList();

            this._suggestions.AddRange(all);
        }

        /// <summary>
        /// Moves the selection by the given direction, wrapping around at the edges.
        /// </summary>
        /// <param name="direction">Negative to move up, positive to move down.</param>
        public void Navigate(int direction)
        {
            if (this._suggestions.Count == 0) return;

            this._selectedIndex += direction;

            if (this._selectedIndex < 0)
            {
                this._selectedIndex = this._suggestions.Count - 1;
            }
            else if (this._selectedIndex >= this._suggestions.Count)
            {
                this._selectedIndex = 0;
            }

            if (this._selectedIndex < this._scrollOffset)
            {
                this._scrollOffset = this._selectedIndex;
            }
            else if (this._selectedIndex >= this._scrollOffset + MaxVisible)
            {
                this._scrollOffset = this._selectedIndex - MaxVisible + 1;
            }
        }

        /// <summary>
        /// Gets the currently selected command, or <see langword="null"/> if there are no suggestions.
        /// </summary>
        public ConsoleCommand? GetSelectedCommand()
        {
            if (this._suggestions.Count == 0) return null;
            return this._suggestions[this._selectedIndex];
        }

        /// <summary>
        /// Hides the popup and clears all state.
        /// </summary>
        public void Dismiss()
        {
            this._suggestions.Clear();
            this._argumentHint = null;
            this._selectedIndex = 0;
            this._scrollOffset = 0;
        }

        /// <summary>
        /// Draws the popup above the input row.
        /// </summary>
        /// <param name="ctx">The drawing context.</param>
        /// <param name="font">The font to render text with.</param>
        /// <param name="bottomLeft">The bottom-left anchor point (just above the separator).</param>
        /// <param name="width">The available width for the popup.</param>
        public void Draw(DrawingContext ctx, SpriteFontBase font, Vector2 bottomLeft, float width)
        {
            if (this._argumentHint != null)
            {
                this.DrawArgumentHint(ctx, font, bottomLeft, width);
                return;
            }

            if (this._suggestions.Count == 0) return;

            int visibleCount = Math.Min(this._suggestions.Count, MaxVisible);
            float popupHeight = visibleCount * ItemHeight;
            float popupTop = bottomLeft.Y - popupHeight;

            ctx.DrawRectangle(
                new Vector2(bottomLeft.X, popupTop),
                new Vector2(width, popupHeight),
                _popupBackgroundColor
            );

            ctx.DrawRectangle(
                new Vector2(bottomLeft.X, popupTop),
                new Vector2(width, 1f),
                _popupBorderColor
            );

            float textOffsetY = (ItemHeight - _consoleFont.LineHeight) / 2f;

            for (int i = 0; i < visibleCount; i++)
            {
                int idx = this._scrollOffset + i;
                var command = this._suggestions[idx];
                float itemY = popupTop + i * ItemHeight;

                if (idx == this._selectedIndex)
                {
                    ctx.DrawRectangle(
                        new Vector2(bottomLeft.X, itemY),
                        new Vector2(width, ItemHeight),
                        _popupSelectedColor
                    );
                }

                float textY = itemY + textOffsetY;

                ctx.DrawText(
                    font,
                    command.FullName,
                    new Vector2(bottomLeft.X + PaddingX, textY),
                    _commandTextColor
                );

                if (!string.IsNullOrEmpty(command.Description))
                {
                    float nameWidth = font.MeasureString(command.FullName).X;
                    ctx.DrawText(
                        font,
                        command.Description,
                        new Vector2(bottomLeft.X + PaddingX + nameWidth + 16f, textY),
                        _popupDescriptionColor
                    );
                }
            }
        }

        private void DrawArgumentHint(DrawingContext ctx, SpriteFontBase font, Vector2 bottomLeft, float width)
        {
            float popupTop = bottomLeft.Y - ItemHeight;

            ctx.DrawRectangle(
                new Vector2(bottomLeft.X, popupTop),
                new Vector2(width, ItemHeight),
                _popupBackgroundColor
            );

            ctx.DrawRectangle(
                new Vector2(bottomLeft.X, popupTop),
                new Vector2(width, 1f),
                _popupBorderColor
            );

            float textOffsetY = (ItemHeight - _consoleFont.LineHeight) / 2f;
            float textY = popupTop + textOffsetY;

            string usage = this._argumentHint!.GetUsage();
            ctx.DrawText(font, usage, new Vector2(bottomLeft.X + PaddingX, textY), _commandTextColor);

            if (!string.IsNullOrEmpty(this._argumentHint.Description))
            {
                float usageWidth = font.MeasureString(usage).X;
                ctx.DrawText(
                    font,
                    this._argumentHint.Description,
                    new Vector2(bottomLeft.X + PaddingX + usageWidth + 16f, textY),
                    _popupDescriptionColor
                );
            }
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
