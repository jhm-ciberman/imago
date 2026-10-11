namespace Imago.Rendering;

/// <summary>
/// Defines a contract for objects that draw ImGui windows.
/// </summary>
/// <remarks>
/// The <see cref="ImGuiLayer"/> calls it every frame on the mounted screen, on the stage layers, and on the
/// application.
/// </remarks>
public interface IImGuiDrawable
{
    /// <summary>
    /// Draws the ImGui windows of this object.
    /// </summary>
    public void RenderImGui();
}
