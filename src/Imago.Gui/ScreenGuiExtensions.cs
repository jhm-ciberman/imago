using System.Collections.Generic;
using System.Linq;

namespace Imago.Gui;

/// <summary>
/// Provides GUI extensions for <see cref="Screen"/>.
/// </summary>
public static class ScreenGuiExtensions
{
    extension(Screen screen)
    {
        /// <summary>
        /// Gets the primary GUI layer of the screen (the first one added), if any.
        /// </summary>
        public GuiLayer? GuiLayer => screen.FindLayer<GuiLayer>();

        /// <summary>
        /// Gets all GUI layers in the screen, ordered by ZOrder.
        /// </summary>
        public IEnumerable<GuiLayer> GuiLayers => screen.Layers.OfType<GuiLayer>().OrderBy(l => l.ZOrder);
    }
}
