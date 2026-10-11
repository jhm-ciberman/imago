using Imago.Assets.Textures;
using Imago.Rendering.Sprites;
using NeoVeldrid;

namespace Imago.SceneGraph;

/// <summary>
/// Specifies where a frame is rendered to.
/// </summary>
public enum RenderOutputMode
{
    /// <summary>The frame that is shown on the screen.</summary>
    Screen,

    /// <summary>An offscreen render texture, such as a screenshot capture.</summary>
    Offscreen,
}

/// <summary>
/// Provides the GPU objects that an <see cref="IRenderLayer"/> uses to render a frame.
/// </summary>
public readonly struct RenderContext
{
    /// <summary>
    /// Gets the graphics device.
    /// </summary>
    public GraphicsDevice Device { get; }

    /// <summary>
    /// Gets the command list that records the frame.
    /// </summary>
    public CommandList CommandList { get; }

    /// <summary>
    /// Gets the render texture to draw into.
    /// </summary>
    public IRenderTexture Target { get; }

    /// <summary>
    /// Gets where the frame is rendered to.
    /// </summary>
    public RenderOutputMode OutputMode { get; }

    /// <summary>
    /// Gets the sprite batch that draws sprites and text into the target.
    /// </summary>
    public SpriteBatch SpriteBatch { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderContext"/> struct.
    /// </summary>
    /// <param name="device">The graphics device.</param>
    /// <param name="commandList">The command list that records the frame.</param>
    /// <param name="target">The render texture to draw into.</param>
    /// <param name="outputMode">Where the frame is rendered to.</param>
    /// <param name="spriteBatch">The sprite batch that draws sprites and text into the target.</param>
    internal RenderContext(GraphicsDevice device, CommandList commandList, IRenderTexture target, RenderOutputMode outputMode, SpriteBatch spriteBatch)
    {
        this.Device = device;
        this.CommandList = commandList;
        this.Target = target;
        this.OutputMode = outputMode;
        this.SpriteBatch = spriteBatch;
    }
}
