using System;
using System.Numerics;
using Imago.Input;
using Imago.Rendering;
using Imago.Rendering.Sprites;
using Imago.SceneGraph;
using Imago.Support.Drawing;
using Imago.Support.Numerics;

namespace Imago.Controls;

/// <summary>
/// A dedicated layer for rendering the mouse cursor above all other content.
/// </summary>
/// <remarks>
/// This layer is automatically created and managed by <see cref="Stage"/>.
/// Access it via <see cref="Stage.CursorLayer"/>.
/// </remarks>
public class CursorLayer : IRenderLayer
{
    private readonly Viewport _viewport;
    private readonly InputManager _input;

    /// <inheritdoc />
    public int ZOrder => 20000;

    /// <inheritdoc />
    public LayerRenderTarget RenderTarget => LayerRenderTarget.Overlay;

    /// <inheritdoc />
    public bool IsVisible { get; set; } = true;

    /// <inheritdoc />
    public bool IsInputBlocked { get; set; }

    /// <inheritdoc />
    public Stage? Stage { get; private set; }

    /// <inheritdoc />
    public event EventHandler? Mounted;

    /// <inheritdoc />
    public event EventHandler? Unmounting;

    /// <inheritdoc />
    public void Mount(Stage stage)
    {
        this.Stage = stage;
        this.Mounted?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public void Unmount()
    {
        this.Unmounting?.Invoke(this, EventArgs.Empty);
        this.Stage = null;
    }

    /// <summary>
    /// Gets or sets the cursor to display.
    /// </summary>
    public Cursor? Cursor { get; set; }

    /// <summary>
    /// Gets or sets the scale of the cursor.
    /// </summary>
    public float CursorScale { get; set; } = 1f;

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
    public void Render(RenderContext ctx)
    {
        this.Draw(ctx.SpriteBatch, new Vector2(ctx.Target.Width, ctx.Target.Height));
    }

    private void Draw(SpriteBatch batch, Vector2 targetSize)
    {
        if (this.Cursor == null || !this._input.IsCursorInsideWindow)
        {
            return;
        }

        var viewportSize = this._viewport.Size;
        var guiScale = this.Stage?.GuiScale ?? Vector2.One;
        var mousePosition = this._input.CursorPosition / guiScale;
        if (!new Rect(Vector2.Zero, viewportSize).Contains(mousePosition))
        {
            return;
        }

        batch.Begin(Matrix3x2.CreateScale(targetSize / viewportSize));

        var cursorSize = this.Cursor.TextureSize * this.CursorScale;
        var hotspot = this.Cursor.HotspotPixels * this.CursorScale;
        var cursorPosition = mousePosition - hotspot;
        cursorPosition = new Vector2(MathF.Floor(cursorPosition.X), MathF.Floor(cursorPosition.Y));

        batch.DrawTexture(this.Cursor.Texture, cursorPosition, cursorSize, Vector2.Zero, Vector2.One, Color.White);
        batch.End();
    }
}
