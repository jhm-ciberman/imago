using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.SceneGraph;

/// <summary>
/// An object that can draw in the immediate mode using the <see cref="IImediateRenderer"/>.
/// </summary>
public interface IImmediateRenderable
{
    /// <summary>
    /// Draws the object in the immediate mode.
    /// </summary>
    /// <param name="renderer">The <see cref="IImediateRenderer"/> to use.</param>
    void Draw(IImediateRenderer renderer);
}
