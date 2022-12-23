using LifeSim.Support;

namespace LifeSim.Engine.Rendering;

public class RenderSettings : ObservableObject
{
    public Renderer Renderer { get; }

    public RenderSettings(Renderer renderer)
    {
        this.Renderer = renderer;
    }

    private bool _forceWireframe = false;

    /// <summary>
    /// Gets or sets whether to force wireframe rendering.
    /// </summary>
    public bool ForceWireframe
    {
        get => this._forceWireframe;
        set => this.SetProperty(ref this._forceWireframe, value);
    }

    private bool _enableFog = true;

    /// <summary>
    /// Gets or sets whether to enable fog.
    /// </summary>
    public bool EnableFog
    {
        get => this._enableFog;
        set => this.SetProperty(ref this._enableFog, value);
    }

    private bool _enablePixelPerfectShadows = true;

    /// <summary>
    /// Gets or sets whether to enable pixel perfect shadows.
    /// </summary>
    public bool EnablePixelPerfectShadows
    {
        get => this._enablePixelPerfectShadows;
        set => this.SetProperty(ref this._enablePixelPerfectShadows, value);
    }
}