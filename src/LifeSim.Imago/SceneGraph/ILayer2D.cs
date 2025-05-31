using LifeSim.Imago.Input;
using LifeSim.Imago.Rendering.Sprites;

namespace LifeSim.Imago.SceneGraph;

public interface ILayer2D
{
    public bool IsCursorOverElement { get; }
    public void Draw(DrawingContext ctx);
    public void Update(float deltaTime);
    public void HandleMousePressed(MouseButtonEventArgs e);
    public void HandleMouseReleased(MouseButtonEventArgs e);
    public void HandleMouseWheel(MouseWheelEventArgs e);
    public void HandleKeyPressed(KeyboardEventArgs e);
    public void HandleKeyReleased(KeyboardEventArgs e);
}
