using Imago.Support.Metrics;

namespace Imago.DearImGui;

/// <summary>
/// The metrics of the ImGui layer.
/// </summary>
public sealed class ImGuiMetrics : MetricGroup
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiMetrics"/> class.
    /// </summary>
    public ImGuiMetrics()
        : base("ImGui")
    {
        this.UpdateTime = this.Timer("Update time");
    }

    /// <summary>
    /// Gets the time of the update of the ImGui layer, which includes building the ImGui windows.
    /// </summary>
    public TimerMetric UpdateTime { get; }
}
