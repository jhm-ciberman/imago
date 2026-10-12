using Imago.SceneGraph;

namespace Imago.Gui;

/// <summary>
/// Provides GUI extensions for <see cref="Stage"/>.
/// </summary>
public static class StageGuiExtensions
{
    extension(Stage stage)
    {
        /// <summary>
        /// Gets the tooltip layer on this stage, or null if there is none.
        /// </summary>
        public TooltipLayer? TooltipLayer => stage.FindLayer<TooltipLayer>();

        /// <summary>
        /// Gets the cursor layer on this stage, or null if there is none.
        /// </summary>
        public CursorLayer? CursorLayer => stage.FindLayer<CursorLayer>();
    }
}
