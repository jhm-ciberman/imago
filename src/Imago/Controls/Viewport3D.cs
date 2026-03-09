using System.Numerics;
using Imago.Assets.Textures;
using Imago.Rendering;
using Imago.Rendering.Sprites;
using Imago.SceneGraph;

namespace Imago.Controls;

/// <summary>
/// A control that renders a <see cref="Scene3D"/> and displays the result as a 2D texture.
/// </summary>
public class Viewport3D : Control
{
    private Scene3D? _scene;
    private RenderTexture? _renderTexture;
    private Texture? _resolvedTexture;
    private bool _dirty;
    private bool _needsFlipY;

    /// <summary>
    /// Gets or sets the 3D scene to render. The caller owns the scene and is responsible for its disposal.
    /// </summary>
    public Scene3D? Scene3D
    {
        get => this._scene;
        set
        {
            if (this._scene == value) return;

            if (this.Layer != null)
            {
                if (this._scene != null) this._scene.Stage = null;
            }

            this._scene = value;

            if (this.Layer != null)
            {
                if (this._scene != null) this._scene.Stage = this.Layer.Stage;
            }

            this._dirty = true;
        }
    }

    /// <inheritdoc />
    public override void OnMounted(GuiLayer layer)
    {
        base.OnMounted(layer);
        if (this._scene != null) this._scene.Stage = layer.Stage;
    }

    /// <inheritdoc />
    public override void OnUnmounted()
    {
        if (this._scene != null) this._scene.Stage = null;
        base.OnUnmounted();
    }

    /// <summary>
    /// Gets or sets a value indicating whether the viewport re-renders every frame.
    /// When <c>false</c>, the viewport only renders when <see cref="Refresh"/> is called or
    /// when the <see cref="Scene3D"/> property changes.
    /// </summary>
    public bool AutoRefresh { get; set; }

    /// <summary>
    /// Marks the viewport as needing a re-render on the next frame.
    /// </summary>
    public void Refresh()
    {
        this._dirty = true;
    }

    /// <inheritdoc />
    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        float width = float.IsNaN(this.Width) ? availableSize.X : this.Width;
        float height = float.IsNaN(this.Height) ? availableSize.Y : this.Height;
        return new Vector2(width, height);
    }

    /// <inheritdoc />
    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        if (this._scene == null) return;

        uint width = (uint)this.ActualSize.X;
        uint height = (uint)this.ActualSize.Y;

        if (width == 0 || height == 0) return;

        this.EnsureResources(width, height);

        if (this.AutoRefresh)
        {
            this._dirty = true;
        }

        if (this._dirty)
        {
            Renderer.Instance.RenderToOffScreenTexture(this._scene, this._renderTexture!);
            Renderer.Instance.ResolveTexture(this._renderTexture!, this._resolvedTexture!);
            this._dirty = false;
        }
    }

    /// <inheritdoc />
    protected override void DrawCore(DrawingContext ctx)
    {
        base.DrawCore(ctx);

        if (this._resolvedTexture == null) return;

        ITextureRegion region = this._resolvedTexture;

        if (this._needsFlipY)
        {
            region = region.MirrorY();
        }

        ctx.DrawTexture(region, this.Position, this.ActualSize);
    }

    private void EnsureResources(uint width, uint height)
    {
        if (this._renderTexture != null && this._renderTexture.Width == width && this._renderTexture.Height == height)
        {
            return;
        }

        this.DisposeResources();

        this._renderTexture = new RenderTexture(width, height, Veldrid.TextureSampleCount.Count1);
        this._resolvedTexture = new Texture(width, height, mipLevels: 1, srgb: false);
        this._needsFlipY = !Renderer.Instance.IsUvOriginTopLeft;
        this._dirty = true;
    }

    private void DisposeResources()
    {
        this._renderTexture?.Dispose();
        this._resolvedTexture?.Dispose();
        this._renderTexture = null;
        this._resolvedTexture = null;
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.DisposeResources();
        }

        base.Dispose(disposing);
    }
}
