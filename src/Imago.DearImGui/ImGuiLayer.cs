using System;
using System.Collections.Generic;
using Imago.Input;
using Imago.Rendering;
using Imago.Textures;
using ImGuiNET;
using NeoVeldrid;
using Texture = Imago.Textures.Texture;

namespace Imago.DearImGui;

/// <summary>
/// A layer that draws ImGui windows on top of the screen.
/// </summary>
public sealed class ImGuiLayer : IRenderLayer, IInputHandler, IMountListener, IDisposable
{
    /// <summary>
    /// Occurs when a new ImGui frame starts. ImGui windows are submitted from handlers of this event.
    /// </summary>
    public event EventHandler? FrameStarted;

    /// <inheritdoc />
    public event EventHandler? Mounted;

    /// <inheritdoc />
    public event EventHandler? Unmounting;

    /// <summary>
    /// Gets the global instance of the ImGui layer.
    /// </summary>
    public static ImGuiLayer Instance { get; private set; } = null!;

    /// <inheritdoc />
    public int ZOrder => 15000;

    /// <inheritdoc />
    public LayerRenderTarget RenderTarget => LayerRenderTarget.Overlay;

    /// <inheritdoc />
    public bool IsVisible { get; set; } = true;

    /// <inheritdoc />
    public bool IsInputBlocked { get; set; }

    /// <inheritdoc />
    public bool IsReceivingInput => this.IsVisible && !this.IsInputBlocked;

    /// <inheritdoc />
    public bool IsCursorOverElement => ImGui.GetIO().WantCaptureMouse;

    /// <inheritdoc />
    public Stage? Stage { get; private set; }

    private readonly GraphicsDevice _gd;
    private readonly IRenderTexture _renderTexture;
    private readonly ImGuiRenderer _imguiRenderer;
    private readonly List<IImGuiDrawable> _drawables = [];
    private readonly IImGuiDrawable? _application;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiLayer"/> class.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if an ImGui layer already exists.</exception>
    public ImGuiLayer()
    {
        if (Instance != null)
        {
            throw new InvalidOperationException("Only one instance of ImGuiLayer can be created.");
        }

        Instance = this;

        var renderer = Renderer.Instance;
        this._gd = renderer.GraphicsDevice;
        this._renderTexture = renderer.FullScreenRenderTexture;
        this._imguiRenderer = new ImGuiRenderer(
            this._gd,
            this._renderTexture.OutputDescription,
            (int)this._renderTexture.Width,
            (int)this._renderTexture.Height
        );

        unsafe
        {
            ImGui.GetIO().NativePtr->IniFilename = null;
        }

        this._renderTexture.Resized += this.RenderTexture_Resized;
        renderer.RegisterDisposable(this);

        this._application = Application.Instance as IImGuiDrawable;
    }

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

    /// <inheritdoc />
    public void Update(float deltaTime)
    {
        this._imguiRenderer.Update(deltaTime, InputManager.Instance.InputSnapshot);
        this.FrameStarted?.Invoke(this, EventArgs.Empty);

        for (int i = 0; i < this._drawables.Count; i++)
        {
            this._drawables[i].RenderImGui();
        }

        this._application?.RenderImGui();
    }

    /// <inheritdoc />
    public void NotifyItemMounted(IMountable item)
    {
        if (item is IImGuiDrawable drawable)
        {
            this._drawables.Add(drawable);
        }
    }

    /// <inheritdoc />
    public void NotifyItemUnmounted(IMountable item)
    {
        if (item is IImGuiDrawable drawable)
        {
            this._drawables.Remove(drawable);
        }
    }

    /// <inheritdoc />
    public void Render(RenderContext ctx)
    {
        // ImGui can draw each frame only once, so we only want to draw to the screen, not to offscreen render targets.
        if (ctx.OutputMode != RenderOutputMode.Screen) return;

        ctx.CommandList.SetFramebuffer(ctx.Target.Framebuffer);
        this._imguiRenderer.Render(ctx.Device, ctx.CommandList);
    }

    /// <summary>
    /// Gets or creates the ImGui binding for the given texture, so it can be shown in ImGui windows.
    /// </summary>
    /// <param name="texture">The texture to get or create the binding for.</param>
    /// <returns>The binding to pass to ImGui image calls.</returns>
    internal nint GetOrCreateTextureBinding(Texture texture)
    {
        return this._imguiRenderer.GetOrCreateImGuiBinding(this._gd.ResourceFactory, texture.NativeTexture);
    }

    /// <inheritdoc />
    public void HandleMousePressed(MouseButtonEventArgs e)
    {
        if (ImGui.GetIO().WantCaptureMouse)
        {
            e.Handled = true;
        }
    }

    /// <inheritdoc />
    public void HandleMouseReleased(MouseButtonEventArgs e)
    {
        // Releases always pass through, so a press that started outside ImGui still ends.
    }

    /// <inheritdoc />
    public void HandleMouseWheel(MouseWheelEventArgs e)
    {
        if (ImGui.GetIO().WantCaptureMouse)
        {
            e.Handled = true;
        }
    }

    /// <inheritdoc />
    public void HandleKeyPressed(KeyboardEventArgs e)
    {
        if (ImGui.GetIO().WantCaptureKeyboard)
        {
            e.Handled = true;
        }
    }

    /// <inheritdoc />
    public void HandleKeyReleased(KeyboardEventArgs e)
    {
        // Releases always pass through, so a press that started outside ImGui still ends.
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this._renderTexture.Resized -= this.RenderTexture_Resized;
        this._imguiRenderer.Dispose();
        Renderer.Instance.UnregisterDisposable(this);
        Instance = null!;
    }

    private void RenderTexture_Resized(object? sender, EventArgs e)
    {
        this._imguiRenderer.WindowResized((int)this._renderTexture.Width, (int)this._renderTexture.Height);
    }
}
