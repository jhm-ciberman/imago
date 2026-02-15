using System.Numerics;
using LifeSim.Imago.Input;
using LifeSim.Imago.Rendering;
using LifeSim.Imago.Rendering.Sprites;
using LifeSim.Imago.SceneGraph;
using LifeSim.Support.Drawing;

namespace LifeSim.Imago.Controls;

/// <summary>
/// A dedicated layer for rendering the mouse cursor above all other content.
/// </summary>
/// <remarks>
/// This layer is automatically created and managed by <see cref="Stage"/>.
/// Access it via <see cref="Stage.CursorLayer"/>.
/// </remarks>
public class CursorLayer : ILayer2D
{
    private readonly Viewport _viewport;
    private readonly InputManager _input;

    /// <inheritdoc />
    public int ZOrder => 20000;

    /// <inheritdoc />
    public bool IsVisible { get; set; } = true;

    /// <inheritdoc />
    public bool IsInputBlocked { get; set; }

    /// <inheritdoc />
    public Stage? Stage { get; set; }

    /// <inheritdoc />
    public bool IsCursorOverElement => false;

    /// <summary>
    /// Gets or sets the cursor to display.
    /// </summary>
    public Cursor? Cursor { get; set; }

    /// <summary>
    /// Gets or sets the scale of the cursor.
    /// </summary>
    public float CursorScale { get; set; } = 0.5f;

    /// <summary>
    /// Initializes a new instance of the <see cref="CursorLayer"/> class.
    /// </summary>
    /// <param name="viewport">The viewport to use for rendering. If null, uses the default GUI viewport.</param>
    internal CursorLayer(Viewport? viewport = null)
    {
        this._viewport = viewport ?? Renderer.Instance.GuiViewport;
        this._input = InputManager.Instance;
    }

    /// <inheritdoc />
    public void Update(float deltaTime)
    {
    }

    /// <inheritdoc />
    public void Draw(DrawingContext ctx)
    {
        if (this.Cursor == null)
        {
            return;
        }

        var viewportSize = this._viewport.Size;
        var position = this._viewport.Position;
        var viewProjectionMatrix = Matrix4x4.CreateOrthographicOffCenter(
            position.X,
            viewportSize.X,
            viewportSize.Y,
            position.Y,
            -10f,
            100f
        );

        ctx.SetViewProjectionMatrix(viewProjectionMatrix);

        var guiScale = this.Stage?.GuiScale ?? Vector2.One;
        var mousePosition = (this._input.CursorPosition - position) / guiScale;
        var cursorSize = this.Cursor.TextureSize * this.CursorScale;
        var hotspot = this.Cursor.HotspotPixels * this.CursorScale;
        var cursorPosition = mousePosition - hotspot;

        ctx.DrawTexture(this.Cursor.Texture, cursorPosition, cursorSize, Vector2.Zero, Vector2.One, Color.White);
    }
}
