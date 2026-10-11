namespace Imago.DearImGui;

/// <summary>
/// Defines a contract for objects that draw ImGui windows.
/// </summary>
public interface IImGuiDrawable
{
    /// <summary>
    /// Draws the ImGui windows of this object. The <see cref="ImGuiLayer"/> calls it once per frame.
    /// </summary>
    public void RenderImGui();
}
