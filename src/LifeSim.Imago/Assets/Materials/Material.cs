using System;
using LifeSim.Imago.Rendering;
using Veldrid;
using Texture = LifeSim.Imago.Assets.Textures.Texture;

namespace LifeSim.Imago.Assets.Materials;

/// <summary>
/// Defines the appearance of a rendered object by specifying its shaders and material properties.
/// </summary>
public class Material : IDisposable
{
    private static int _count = 0;

    /// <summary>
    /// Gets the unique identifier for this material.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this material has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; } = false;

    private bool _resourceSetDirty = false;

    internal RenderFlags RenderFlags { get; set; } = RenderFlags.DepthTest | RenderFlags.DepthWrite | RenderFlags.ReceiveShadows | RenderFlags.ColorWrite;

    /// <summary>
    /// Gets the default texture used when no texture is assigned.
    /// </summary>
    public static Texture DefaultTexture { get; } = Texture.Magenta;

    /// <summary>
    /// Gets the shader used for the forward rendering pass.
    /// </summary>
    public Shader ForwardShader { get; }

    /// <summary>
    /// Gets the shader used for the shadow map rendering pass.
    /// </summary>
    public Shader ShadowMapShader { get; }

    /// <summary>
    /// Gets the shader used for the picking pass.
    /// </summary>
    public Shader PickingShader { get; }

    /// <summary>
    /// Gets the Veldrid resource set for this material.
    /// </summary>
    public ResourceSet ResourceSet { get; internal set; } = null!;
    private readonly BindableResource[] _resources;
    private Texture? _texture = null;

    private readonly Renderer _renderer;

    /// <summary>
    /// Initializes a new instance of the <see cref="Material"/> class.
    /// </summary>
    /// <param name="forwardShader">The shader for the forward pass.</param>
    /// <param name="shadowMapShader">The shader for the shadow map pass.</param>
    /// <param name="pickingShader">The shader for the picking pass.</param>
    public Material(Shader forwardShader, Shader shadowMapShader, Shader pickingShader)
    {
        this._renderer = Renderer.Instance;
        this.Id = ++_count;

        if (forwardShader.MaterialResourceLayout != shadowMapShader.MaterialResourceLayout)
            throw new ArgumentException("Forward and shadowmap shaders must use the same resource layout.");

        this.ForwardShader = forwardShader;
        this.ShadowMapShader = shadowMapShader;
        this.PickingShader = pickingShader;

        this._resources = new BindableResource[forwardShader.Textures.Length * 2];
        this._resources[0] = DefaultTexture.VeldridTexture;
        this._resources[1] = DefaultTexture.VeldridSampler;

        this._renderer.RegisterDisposable(this);
        this.NotifyResourcesDirty();
    }

    /// <summary>
    /// Gets or sets a value indicating whether back-face culling is disabled.
    /// </summary>
    public bool DoubleSided { get => this.GetRenderFlag(RenderFlags.DoubleSided); set => this.SetRenderFlag(RenderFlags.DoubleSided, value); }

    /// <summary>
    /// Gets or sets a value indicating whether to render in wireframe mode.
    /// </summary>
    public bool Wireframe { get => this.GetRenderFlag(RenderFlags.Wireframe); set => this.SetRenderFlag(RenderFlags.Wireframe, value); }

    /// <summary>
    /// Gets or sets a value indicating whether depth testing is enabled.
    /// </summary>
    public bool DepthTest { get => this.GetRenderFlag(RenderFlags.DepthTest); set => this.SetRenderFlag(RenderFlags.DepthTest, value); }

    /// <summary>
    /// Gets or sets a value indicating whether writing to the depth buffer is enabled.
    /// </summary>
    public bool DepthWrite { get => this.GetRenderFlag(RenderFlags.DepthWrite); set => this.SetRenderFlag(RenderFlags.DepthWrite, value); }

    /// <summary>
    /// Gets or sets a value indicating whether alpha testing is enabled.
    /// </summary>
    public bool AlphaTest { get => this.GetRenderFlag(RenderFlags.AlphaTest); set => this.SetRenderFlag(RenderFlags.AlphaTest, value); }

    /// <summary>
    /// Gets or sets a value indicating whether this material supports transparency.
    /// </summary>
    public bool Transparent { get => this.GetRenderFlag(RenderFlags.Transparent); set => this.SetRenderFlag(RenderFlags.Transparent, value); }

    /// <summary>
    /// Gets or sets a value indicating whether writing to the color buffer is enabled.
    /// </summary>
    public bool ColorWrite { get => this.GetRenderFlag(RenderFlags.ColorWrite); set => this.SetRenderFlag(RenderFlags.ColorWrite, value); }

    /// <summary>
    /// Gets or sets a value indicating whether this material can receive shadows.
    /// </summary>
    public bool ReceiveShadows { get => this.GetRenderFlag(RenderFlags.ReceiveShadows); set => this.SetRenderFlag(RenderFlags.ReceiveShadows, value); }

    /// <summary>
    /// Updates the material's internal resource set if it is dirty.
    /// </summary>
    public void Update()
    {
        if (!this._resourceSetDirty) return;
        this._resourceSetDirty = false;

        this.ResourceSet?.Dispose();

        var factory = this._renderer.GraphicsDevice.ResourceFactory;
        this.ResourceSet = factory.CreateResourceSet(new ResourceSetDescription(this.ForwardShader.MaterialResourceLayout, this._resources));
    }

    /// <summary>
    /// Notifies the material that its resources (e.g., textures, shaders) have changed and need to be updated.
    /// This marks the internal resource set as dirty, triggering a recreation on the next <see cref="Update"/> call.
    /// </summary>
    protected void NotifyResourcesDirty()
    {
        if (this._resourceSetDirty) return;

        this._resourceSetDirty = true;
        this._renderer.NotifyMaterialResourcesDirty(this);
    }

    /// <summary>
    /// Sets the value of a backing field and, if the value has changed, marks the material's resources as dirty.
    /// This is a helper method for implementing properties that affect the material's rendering.
    /// </summary>
    /// <typeparam name="T">The type of the property being set.</typeparam>
    /// <param name="backingField">The reference to the backing field of the property.</param>
    /// <param name="newValue">The new value to set for the property.</param>
    /// <returns>True if the value was changed; otherwise, false.</returns>
    protected bool SetProperty<T>(ref T backingField, T newValue)
    {
        if (backingField == null && newValue == null) return false;
        if (backingField != null && newValue != null && backingField.Equals(newValue)) return false;

        backingField = newValue;
        this.NotifyResourcesDirty();
        return true;
    }

    private bool SetRenderFlag(RenderFlags flags, bool value)
    {
        if (value)
        {
            if ((this.RenderFlags & flags) == flags) return false;
            this.RenderFlags |= flags;
            return true;
        }
        else
        {
            if ((this.RenderFlags & flags) == 0) return false;
            this.RenderFlags &= ~flags;
            return true;
        }
    }

    private bool GetRenderFlag(RenderFlags flag)
    {
        return (this.RenderFlags & flag) == flag;
    }

    /// <summary>
    /// Gets or sets the primary texture for this material.
    /// </summary>
    public Texture? Texture
    {
        get => this._texture;
        set
        {
            if (this._texture == value) return;

            this._texture = value;
            this.SetTexture(0, this._texture ?? DefaultTexture);
        }
    }

    /// <summary>
    /// Sets a texture and its corresponding sampler at a specific index within the material's resources.
    /// This method also marks the material's resources as dirty.
    /// </summary>
    /// <param name="textureIndex">The index at which to set the texture and sampler.</param>
    /// <param name="value">The <see cref="Texture"/> to set.</param>
    protected void SetTexture(int textureIndex, Texture value)
    {
        this._resources[textureIndex * 2 + 0] = value.VeldridTexture;
        this._resources[textureIndex * 2 + 1] = value.VeldridSampler;
        this.NotifyResourcesDirty();
    }

    /// <summary>
    /// Disposes the material and its resources.
    /// </summary>
    public void Dispose()
    {
        if (this.IsDisposed) return;
        this.IsDisposed = true;

        this.ResourceSet?.Dispose();
        this._resourceSetDirty = false;
        this._renderer.UnregisterDisposable(this);
    }
}
