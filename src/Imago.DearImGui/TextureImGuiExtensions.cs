using Imago.Assets.Textures;

namespace Imago.DearImGui;

/// <summary>
/// Provides ImGui extensions for <see cref="Texture"/>.
/// </summary>
public static class TextureImGuiExtensions
{
    extension(Texture texture)
    {
        /// <summary>
        /// Gets the ImGui binding for this texture, allowing it to be displayed in an ImGui UI.
        /// </summary>
        public nint ImGuiBinding => ImGuiLayer.Instance.GetOrCreateTextureBinding(texture);
    }
}
