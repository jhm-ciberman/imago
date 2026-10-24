namespace Imago.SceneGraph;

/// <summary>
/// Provides access to the 3D scene of a <see cref="Stage"/> or a <see cref="Screen"/>.
/// </summary>
public static class Scene3DLayerExtensions
{
    extension(Stage stage)
    {
        /// <summary>
        /// Gets the first 3D scene on the stage, or null if the stage has none.
        /// </summary>
        public Scene3DLayer? Scene3D => stage.FindLayer<Scene3DLayer>();
    }

    extension(Screen screen)
    {
        /// <summary>
        /// Gets the first 3D scene of the screen, or null if the screen has none.
        /// </summary>
        public Scene3DLayer? Scene3D => screen.FindLayer<Scene3DLayer>();
    }
}
