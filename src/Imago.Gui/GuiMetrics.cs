using Imago.Support.Metrics;

namespace Imago.Gui;

/// <summary>
/// The metrics of the GUI layers.
/// </summary>
public sealed class GuiMetrics : MetricGroup
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GuiMetrics"/> class.
    /// </summary>
    public GuiMetrics()
        : base("GUI")
    {
        this.UpdateTime = this.Timer("Update time");
    }

    /// <summary>
    /// Gets the time of the update of all GUI layers, which includes finding the control under the cursor.
    /// </summary>
    public TimerMetric UpdateTime { get; }
}
