using System;
using System.Collections.Generic;
using System.Numerics;
using Imago.Input;
using Imago.Metrics;
using Imago.Rendering;
using Imago.Support.Metrics;
using ImGuiNET;
using NeoVeldrid;

namespace Imago.DearImGui;

/// <summary>
/// An ImGui window that shows the metrics of the application, as described by <see cref="FrameMetrics.Layout"/>.
/// </summary>
/// <remarks>
/// The window shows a chart of the last frames split by phase, the split of the frame time, the time tree, and the
/// count lines. Buttons in the window switch to a compact view with only the count lines, and pick if the values are
/// from the last frame or an average of the last frames. It starts hidden.
/// </remarks>
public sealed class MetricsOverlay
{
    private const float BudgetMilliseconds = 1000f / 60f;
    private const float MinWidth = 260f;
    private const float ChartHeight = 50f;
    private const float HotShareOfFrame = 0.25f;
    private const float UntrackedShare = 0.05f;
    private const double UntrackedHoldMilliseconds = 10000.0;

    private static readonly float[] _windowMilliseconds = [0f, 200f, 1000f];
    private static readonly string[] _windowLabels = ["Now", "0.2 s", "1 s"];

    private static readonly uint[] _phaseColors = [Rgba(224, 175, 104), Rgba(122, 162, 247), Rgba(187, 154, 247), Rgba(158, 206, 106)];
    private static readonly uint _waitColor = Rgba(59, 65, 82);
    private static readonly uint _chartBackground = Rgba(37, 42, 56);
    private static readonly uint _budgetColor = Rgba(158, 206, 106);
    private static readonly uint _overBudgetColor = Rgba(247, 118, 142);
    private static readonly Vector4 _hotText = new(0.88f, 0.69f, 0.41f, 1f);

    private readonly FrameMetrics _metrics;
    private readonly Renderer _renderer;
    private readonly InputManager _input;
    private readonly Key? _toggleKey;
    private readonly Dictionary<MetricsLine, MinuteCounter> _minuteCounters = [];
    private readonly int[] _windowFrames;
    private readonly Dictionary<MetricsTimeRow, double> _untrackedSeenAt = [];

    private bool _isCompact;
    private int _windowIndex = 1;
    private int _windowFrameCount;
    private double _elapsedMilliseconds;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetricsOverlay"/> class.
    /// </summary>
    /// <param name="layer">The ImGui layer that draws the window.</param>
    /// <param name="metrics">The metrics to show.</param>
    /// <param name="renderer">The renderer, for the name of the graphics backend.</param>
    /// <param name="input">The input manager, for the key.</param>
    /// <param name="toggleKey">The key that shows and hides the window, or null to show and hide it only from code.</param>
    public MetricsOverlay(ImGuiLayer layer, FrameMetrics metrics, Renderer renderer, InputManager input, Key? toggleKey)
    {
        this._metrics = metrics;
        this._renderer = renderer;
        this._input = input;
        this._toggleKey = toggleKey;
        this._windowFrames = new int[metrics.Imago.FrameTime.History.Samples.Length];

        layer.FrameStarted += this.Layer_FrameStarted;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the window is shown.
    /// </summary>
    public bool IsVisible { get; set; }

    private void Layer_FrameStarted(object? sender, EventArgs e)
    {
        if (this._toggleKey is Key key && this._input.WasKeyPressedThisFrame(key))
        {
            this.IsVisible = !this.IsVisible;
        }

        this._elapsedMilliseconds += this._metrics.Imago.FrameTime.Value;
        this.CountRates(this._metrics.Layout.Lines, this._metrics.Imago.FrameTime.Value);

        if (!this.IsVisible) return;

        var io = ImGui.GetIO();
        float padding = 10f;
        ImGui.SetNextWindowPos(new Vector2(io.DisplaySize.X - padding, padding), ImGuiCond.Always, new Vector2(1f, 0f));
        ImGui.SetNextWindowBgAlpha(0.85f);

        var flags =
            ImGuiWindowFlags.NoResize |
            ImGuiWindowFlags.NoMove |
            ImGuiWindowFlags.AlwaysAutoResize |
            ImGuiWindowFlags.NoFocusOnAppearing |
            ImGuiWindowFlags.NoNav |
            ImGuiWindowFlags.NoTitleBar;

        if (ImGui.Begin("##MetricsOverlay", flags))
        {
            var layout = this._metrics.Layout;
            var frameTimes = this._metrics.Imago.FrameTime.History;

            this.CollectWindowFrames(frameTimes);
            float frameMilliseconds = this.Average(frameTimes);

            this.DrawHeader(frameMilliseconds);

            if (!this._isCompact)
            {
                DrawChart(layout, frameTimes);
                DrawLegend(layout);
                this.DrawFrameSplit(layout, frameMilliseconds);
                this.DrawTimeTree(layout, frameMilliseconds, frameTimes);
            }

            this.DrawLines(layout, frameTimes);
        }

        ImGui.End();
    }

    private void CollectWindowFrames(MetricHistory frameTimes)
    {
        // All groups are added before the first frame, so the same index is the same frame in every history.
        int length = frameTimes.Samples.Length;
        float windowMilliseconds = _windowMilliseconds[this._windowIndex];
        float milliseconds = 0f;
        this._windowFrameCount = 0;

        for (int i = 0; i < frameTimes.Count; i++)
        {
            int index = (frameTimes.Offset + frameTimes.Count - 1 - i + length) % length;
            this._windowFrames[this._windowFrameCount++] = index;
            milliseconds += frameTimes.Samples[index];

            if (milliseconds >= windowMilliseconds) break;
        }
    }

    private float Average(MetricHistory history)
    {
        if (this._windowFrameCount == 0) return 0f;

        float sum = 0f;
        for (int i = 0; i < this._windowFrameCount; i++)
        {
            sum += history.Samples[this._windowFrames[i]];
        }

        return sum / this._windowFrameCount;
    }

    private float Average(MetricsTimeRow row)
    {
        if (this._windowFrameCount == 0) return 0f;

        float sum = 0f;
        for (int i = 0; i < this._windowFrameCount; i++)
        {
            sum += row.GetSample(this._windowFrames[i]);
        }

        return sum / this._windowFrameCount;
    }

    private void DrawHeader(float frameMilliseconds)
    {
        float framesPerSecond = frameMilliseconds > 0f ? 1000f / frameMilliseconds : 0f;
        ImGui.TextUnformatted($"{framesPerSecond:0} FPS  {frameMilliseconds:0.00} ms");
        ImGui.SameLine();
        bool isInBudget = frameMilliseconds <= BudgetMilliseconds + 0.5f;
        DrawPill(isInBudget ? "in budget" : "over budget", isInBudget ? _budgetColor : _overBudgetColor);

        TextDim(this._renderer.BackendType.ToString());
        ImGui.SameLine();

        string modeLabel = this._isCompact ? "Compact" : "Full";
        string windowLabel = _windowLabels[this._windowIndex];
        var style = ImGui.GetStyle();
        float buttonsWidth =
            ImGui.CalcTextSize(modeLabel).X +
            ImGui.CalcTextSize(windowLabel).X +
            style.FramePadding.X * 4f +
            style.ItemSpacing.X;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + MathF.Max(ImGui.GetContentRegionAvail().X - buttonsWidth, 0f));

        if (ImGui.SmallButton($"{modeLabel}##mode"))
        {
            this._isCompact = !this._isCompact;
        }

        ImGui.SameLine();
        if (ImGui.SmallButton($"{windowLabel}##window"))
        {
            this._windowIndex = (this._windowIndex + 1) % _windowLabels.Length;
        }
    }

    private static void DrawChart(MetricsLayout layout, MetricHistory frameTimes)
    {
        var drawList = ImGui.GetWindowDrawList();
        var origin = ImGui.GetCursorScreenPos();
        var size = new Vector2(GetWidth(), ChartHeight);
        float bottom = origin.Y + size.Y;
        float scale = BudgetMilliseconds * 2f;

        drawList.AddRectFilled(origin, origin + size, _chartBackground);

        int length = frameTimes.Samples.Length;
        float barWidth = size.X / length;
        var phases = layout.TimeRows;

        for (int i = 0; i < frameTimes.Count; i++)
        {
            int index = (frameTimes.Offset + i) % length;
            float x = origin.X + (length - frameTimes.Count + i) * barWidth;
            float y = bottom;

            for (int p = 0; p < phases.Count; p++)
            {
                float height = MathF.Min(phases[p].GetSample(index) / scale * size.Y, y - origin.Y);
                y = DrawBarSegment(drawList, x, y, barWidth, height, _phaseColors[p % _phaseColors.Length]);
            }

            if (frameTimes.Samples[index] > scale)
            {
                drawList.AddRectFilled(new Vector2(x, origin.Y), new Vector2(x + MathF.Max(barWidth, 1f), origin.Y + 3f), _overBudgetColor);
            }
        }

        float budgetY = bottom - BudgetMilliseconds / scale * size.Y;
        for (float x = origin.X; x < origin.X + size.X; x += 6f)
        {
            drawList.AddLine(new Vector2(x, budgetY), new Vector2(MathF.Min(x + 3f, origin.X + size.X), budgetY), _budgetColor);
        }

        ImGui.Dummy(size);
    }

    private static float DrawBarSegment(ImDrawListPtr drawList, float x, float bottom, float width, float height, uint color)
    {
        if (height > 0f)
        {
            drawList.AddRectFilled(new Vector2(x, bottom - height), new Vector2(x + width, bottom), color);
        }

        return bottom - height;
    }

    private static void DrawLegend(MetricsLayout layout)
    {
        var phases = layout.TimeRows;
        for (int p = 0; p < phases.Count; p++)
        {
            DrawSwatch(_phaseColors[p % _phaseColors.Length]);
            ImGui.SameLine();
            ImGui.TextUnformatted(phases[p].Label);
            ImGui.SameLine();
        }

        DrawSwatch(_waitColor);
        ImGui.SameLine();
        ImGui.TextUnformatted("Wait");
    }

    private void DrawFrameSplit(MetricsLayout layout, float frameMilliseconds)
    {
        TextDim(this._windowIndex == 0 ? "This frame" : $"Last {_windowLabels[this._windowIndex]}");

        var drawList = ImGui.GetWindowDrawList();
        var origin = ImGui.GetCursorScreenPos();
        var size = new Vector2(GetWidth(), 10f);
        float scale = frameMilliseconds > 0f ? size.X / frameMilliseconds : 0f;
        float x = origin.X;
        float used = 0f;
        var phases = layout.TimeRows;

        for (int p = 0; p < phases.Count; p++)
        {
            var phase = phases[p];
            uint color = _phaseColors[p % _phaseColors.Length];
            float phaseValue = this.Average(phase);
            float childrenTotal = 0f;

            for (int c = 0; c < phase.Children.Count; c++)
            {
                float value = this.Average(phase.Children[c]);
                childrenTotal += value;
                x = DrawFrameSegment(drawList, x, origin.Y, value * scale, size.Y, c % 2 == 0 ? color : Darken(color));
            }

            x = DrawFrameSegment(drawList, x, origin.Y, MathF.Max(phaseValue - childrenTotal, 0f) * scale, size.Y, Darken(Darken(color)));
            used += phaseValue;
        }

        float wait = MathF.Max(frameMilliseconds - used, 0f);
        DrawFrameSegment(drawList, x, origin.Y, MathF.Min(wait * scale, origin.X + size.X - x), size.Y, _waitColor);
        ImGui.Dummy(size);

        MetricsTimeRow? slowest = null;
        float slowestValue = 0f;
        foreach (var phase in phases)
        {
            this.FindSlowestLeaf(phase, ref slowest, ref slowestValue);
        }

        if (slowest != null && frameMilliseconds > 0f)
        {
            TextDim($"Slowest: {slowest.Label} {slowestValue:0.00} ms ({slowestValue / frameMilliseconds * 100f:0}%)");
        }
    }

    private static float DrawFrameSegment(ImDrawListPtr drawList, float x, float y, float width, float height, uint color)
    {
        if (width > 0f)
        {
            drawList.AddRectFilled(new Vector2(x, y), new Vector2(x + width, y + height), color);
        }

        return x + width;
    }

    private void FindSlowestLeaf(MetricsTimeRow row, ref MetricsTimeRow? slowest, ref float slowestValue)
    {
        if (row.Children.Count == 0)
        {
            float value = this.Average(row);
            if (slowest == null || value > slowestValue)
            {
                slowest = row;
                slowestValue = value;
            }

            return;
        }

        foreach (var child in row.Children)
        {
            this.FindSlowestLeaf(child, ref slowest, ref slowestValue);
        }
    }

    private void DrawTimeTree(MetricsLayout layout, float frameMilliseconds, MetricHistory frameTimes)
    {
        if (!ImGui.BeginTable("##times", 3, ImGuiTableFlags.SizingFixedFit))
        {
            return;
        }

        ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
        ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, ImGui.GetColorU32(ImGuiCol.TableHeaderBg));
        ImGui.TableNextColumn();
        ImGui.TextUnformatted("ms");
        ImGui.TableNextColumn();
        TextRight(_windowLabels[this._windowIndex]);
        ImGui.TableNextColumn();
        TextRight("%");

        var phases = layout.TimeRows;
        for (int p = 0; p < phases.Count; p++)
        {
            this.DrawTimeRow(phases[p], frameMilliseconds, frameMilliseconds, frameTimes, _phaseColors[p % _phaseColors.Length], 0);
        }

        ImGui.EndTable();
    }

    private void DrawTimeRow(MetricsTimeRow row, float parentMilliseconds, float frameMilliseconds, MetricHistory frameTimes, uint color, int depth)
    {
        float value = this.Average(row);
        bool hasChildren = row.Children.Count > 0;
        bool isHot = depth > 0 && value > frameMilliseconds * HotShareOfFrame;

        ImGui.TableNextRow();
        ImGui.TableNextColumn();

        var flags = ImGuiTreeNodeFlags.SpanFullWidth;
        flags |= hasChildren ? ImGuiTreeNodeFlags.DefaultOpen : ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;

        ImGui.PushStyleColor(ImGuiCol.Text, GetRowTextColor(value, isHot));
        bool isOpen = ImGui.TreeNodeEx(row.Label, flags);
        if (ImGui.IsItemHovered())
        {
            DrawRowTooltip(row, frameTimes);
        }

        DrawRowValues(value, parentMilliseconds, color);
        ImGui.PopStyleColor();

        if (hasChildren && isOpen)
        {
            float childrenTotal = 0f;
            foreach (var child in row.Children)
            {
                childrenTotal += this.Average(child);
                this.DrawTimeRow(child, value, frameMilliseconds, frameTimes, color, depth + 1);
            }

            float untracked = MathF.Max(value - childrenTotal, 0f);
            if (row.Timers.Count > 0 && untracked > value * UntrackedShare && untracked > 0.05f)
            {
                this._untrackedSeenAt[row] = this._elapsedMilliseconds;
            }

            // The row stays for a while after the gap shrinks, so the rows below it don't jump up and down.
            if (this._untrackedSeenAt.TryGetValue(row, out double seenAt) && this._elapsedMilliseconds - seenAt < UntrackedHoldMilliseconds)
            {
                DrawUntrackedRow(untracked, value, color);
            }

            ImGui.TreePop();
        }
    }

    private static void DrawUntrackedRow(float value, float parentMilliseconds, uint color)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();

        var flags = ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;
        ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]);
        ImGui.TreeNodeEx("untracked", flags);
        DrawRowValues(value, parentMilliseconds, Darken(Darken(color)));
        ImGui.PopStyleColor();
    }

    private static void DrawRowValues(float value, float parentMilliseconds, uint color)
    {
        ImGui.TableNextColumn();
        TextRight($"{value:0.00}");

        ImGui.TableNextColumn();
        float share = parentMilliseconds > 0f ? Math.Clamp(value / parentMilliseconds, 0f, 1f) : 0f;
        DrawShareBar(share, color);
        ImGui.SameLine();
        TextRight($"{share * 100f:0}%");
    }

    private static Vector4 GetRowTextColor(float value, bool isHot)
    {
        if (isHot) return _hotText;

        var colors = ImGui.GetStyle().Colors;
        return value < 0.005f ? colors[(int)ImGuiCol.TextDisabled] : colors[(int)ImGuiCol.Text];
    }

    private static void DrawRowTooltip(MetricsTimeRow row, MetricHistory frameTimes)
    {
        if (frameTimes.Count == 0) return;

        int length = frameTimes.Samples.Length;
        float sum = 0f;
        float min = float.MaxValue;
        float max = 0f;
        for (int i = 0; i < frameTimes.Count; i++)
        {
            float sample = row.GetSample((frameTimes.Offset + i) % length);
            sum += sample;
            min = MathF.Min(min, sample);
            max = MathF.Max(max, sample);
        }

        ImGui.BeginTooltip();
        ImGui.TextUnformatted($"{row.Label}: avg {sum / frameTimes.Count:0.00}, min {min:0.00}, max {max:0.00}");
        ImGui.EndTooltip();
    }

    private static void DrawShareBar(float share, uint color)
    {
        var drawList = ImGui.GetWindowDrawList();
        var origin = ImGui.GetCursorScreenPos();
        float lineHeight = ImGui.GetTextLineHeight();
        var size = new Vector2(34f, 6f);
        var top = origin + new Vector2(0f, (lineHeight - size.Y) * 0.5f);

        drawList.AddRectFilled(top, top + size, _chartBackground);
        drawList.AddRectFilled(top, top + new Vector2(size.X * share, size.Y), color);
        ImGui.Dummy(new Vector2(size.X, lineHeight));
    }

    private void DrawLines(MetricsLayout layout, MetricHistory frameTimes)
    {
        ImGui.Separator();

        if (!ImGui.BeginTable("##lines", 2, ImGuiTableFlags.SizingFixedFit))
        {
            return;
        }

        foreach (var line in layout.Lines)
        {
            this.DrawLine(line, line.Label, frameTimes);

            foreach (var child in line.Children)
            {
                this.DrawLine(child, "  " + child.Label, frameTimes);
            }
        }

        ImGui.EndTable();
    }

    private void DrawLine(MetricsLine line, string label, MetricHistory frameTimes)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        TextDim(label);
        ImGui.TableNextColumn();

        if (line.Metric != null)
        {
            TextRight(this.FormatValue(line, frameTimes));
        }
        else if (line.Children.Count > 0)
        {
            TextRight($"{this.SumChildren(line):N0}");
        }
    }

    private float SumChildren(MetricsLine line)
    {
        float sum = 0f;
        foreach (var child in line.Children)
        {
            if (child.Metric != null)
            {
                sum += this.Average(child.Metric.History);
            }
        }

        return sum;
    }

    private string FormatValue(MetricsLine line, MetricHistory frameTimes)
    {
        var metric = line.Metric!;
        return line.Format switch
        {
            MetricsLineFormat.Fraction => $"{this.Average(metric.History):N0} of {this.Average(line.Whole!.History):N0}",
            MetricsLineFormat.Bytes => $"{this.Average(metric.History) / 1024f:0.0} KB/f  {GetPerSecond(metric.History, frameTimes) / (1024f * 1024f):0.00} MB/s",
            MetricsLineFormat.Rate => $"{GetPerSecond(metric.History, frameTimes):0.#}/s  {this.GetMinuteCounter(line).Total:N0}/min",
            _ => $"{this.Average(metric.History):N0}",
        };
    }

    private void CountRates(IReadOnlyList<MetricsLine> lines, float frameMilliseconds)
    {
        foreach (var line in lines)
        {
            if (line.Format == MetricsLineFormat.Rate && line.Metric != null)
            {
                this.GetMinuteCounter(line).Add(line.Metric.Value, frameMilliseconds);
            }

            this.CountRates(line.Children, frameMilliseconds);
        }
    }

    private MinuteCounter GetMinuteCounter(MetricsLine line)
    {
        if (!this._minuteCounters.TryGetValue(line, out var counter))
        {
            counter = new MinuteCounter();
            this._minuteCounters.Add(line, counter);
        }

        return counter;
    }

    private static float GetPerSecond(MetricHistory values, MetricHistory frameTimes)
    {
        int length = frameTimes.Samples.Length;
        float milliseconds = 0f;
        float total = 0f;
        for (int i = 0; i < frameTimes.Count && milliseconds < 1000f; i++)
        {
            int index = (frameTimes.Offset + frameTimes.Count - 1 - i + length) % length;
            milliseconds += frameTimes.Samples[index];
            total += values.Samples[index];
        }

        return milliseconds > 0f ? total * 1000f / milliseconds : 0f;
    }

    private static float GetWidth()
    {
        return MathF.Max(ImGui.GetContentRegionAvail().X, MinWidth);
    }

    private static void DrawPill(string text, uint color)
    {
        var drawList = ImGui.GetWindowDrawList();
        var origin = ImGui.GetCursorScreenPos();
        var size = ImGui.CalcTextSize(text) + new Vector2(8f, 0f);

        drawList.AddRect(origin, origin + size, color);
        drawList.AddText(origin + new Vector2(4f, 0f), color, text);
        ImGui.Dummy(size);
    }

    private static void TextRight(string text)
    {
        float width = ImGui.CalcTextSize(text).X;
        float space = ImGui.GetContentRegionAvail().X;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + MathF.Max(space - width, 0f));
        ImGui.TextUnformatted(text);
    }

    private static void TextDim(string text)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]);
        ImGui.TextUnformatted(text);
        ImGui.PopStyleColor();
    }

    private static void DrawSwatch(uint color)
    {
        var drawList = ImGui.GetWindowDrawList();
        var origin = ImGui.GetCursorScreenPos();
        float lineHeight = ImGui.GetTextLineHeight();
        var top = origin + new Vector2(0f, (lineHeight - 8f) * 0.5f);

        drawList.AddRectFilled(top, top + new Vector2(8f, 8f), color);
        ImGui.Dummy(new Vector2(8f, lineHeight));
    }

    private static uint Darken(uint color)
    {
        uint r = (color & 0xFF) * 3 / 4;
        uint g = ((color >> 8) & 0xFF) * 3 / 4;
        uint b = ((color >> 16) & 0xFF) * 3 / 4;
        return (color & 0xFF000000) | (b << 16) | (g << 8) | r;
    }

    private static uint Rgba(byte r, byte g, byte b, byte a = 255)
    {
        return (uint)(a << 24 | b << 16 | g << 8 | r);
    }

    private sealed class MinuteCounter
    {
        private readonly float[] _seconds = new float[60];
        private int _index;
        private float _currentTotal;
        private float _currentMilliseconds;

        public float Total
        {
            get
            {
                float total = this._currentTotal;
                foreach (float second in this._seconds)
                {
                    total += second;
                }

                return total;
            }
        }

        public void Add(float value, float frameMilliseconds)
        {
            this._currentTotal += value;
            this._currentMilliseconds += frameMilliseconds;

            if (this._currentMilliseconds < 1000f) return;

            this._seconds[this._index] = this._currentTotal;
            this._index = (this._index + 1) % this._seconds.Length;
            this._currentTotal = 0f;
            this._currentMilliseconds -= 1000f;
        }
    }
}
