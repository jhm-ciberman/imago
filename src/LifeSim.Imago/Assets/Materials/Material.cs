using System;
using System.Collections.Generic;
using LifeSim.Imago.Rendering;
using Veldrid;
using Texture = LifeSim.Imago.Assets.Textures.Texture;

namespace LifeSim.Imago.Assets.Materials;

/// <summary>
/// Base class for materials that define the appearance of rendered objects.
/// </summary>
public abstract class Material : IDisposable
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

    internal RenderFlags RenderFlags { get; set; } = RenderFlags.DepthTest | RenderFlags.DepthWrite | RenderFlags.ReceiveShadows | RenderFlags.ColorWrite | RenderFlags.PixelPerfectShadows;

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
    private readonly Renderer _renderer;

    /// <summary>
    /// Initializes a new instance of the <see cref="Material"/> class.
    /// </summary>
    /// <param name="shaders">The compiled shaders for all render passes.</param>
    protected Material(ShaderSet shaders)
    {
        this._renderer = Renderer.Instance;
        this.Id = ++_count;

        if (shaders.Forward.MaterialResourceLayout != shaders.Shadow.MaterialResourceLayout)
            throw new ArgumentException("Forward and shadow shaders must use the same resource layout.");

        this.ForwardShader = shaders.Forward;
        this.ShadowMapShader = shaders.Shadow;
        this.PickingShader = shaders.Picking;

        this._resources = new BindableResource[shaders.Forward.Textures.Length * 2];
        for (int i = 0; i < shaders.Forward.Textures.Length; i++)
        {
            this._resources[i * 2 + 0] = Texture.Magenta.VeldridTexture;
            this._resources[i * 2 + 1] = Texture.Magenta.VeldridSampler;
        }

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
    /// Gets or sets a value indicating whether pixel-perfect shadow sampling is enabled.
    /// </summary>
    public bool PixelPerfectShadows { get => this.GetRenderFlag(RenderFlags.PixelPerfectShadows); set => this.SetRenderFlag(RenderFlags.PixelPerfectShadows, value); }

    /// <summary>
    /// Gets the material parameters buffer, if any. Override in derived classes to provide custom parameters.
    /// </summary>
    private protected virtual DeviceBuffer? ParamsBuffer => null;

    /// <summary>
    /// Updates the material parameters buffer. Override in derived classes to upload parameter data.
    /// </summary>
    /// <param name="cl">The command list to use for the update.</param>
    private protected virtual void UpdateParamsBuffer(CommandList cl)
    {
    }

    /// <summary>
    /// Creates a uniform buffer for material parameters.
    /// </summary>
    /// <param name="size">The size of the buffer in bytes.</param>
    /// <returns>The created device buffer.</returns>
    protected DeviceBuffer CreateParamsBuffer(uint size)
    {
        return this._renderer.GraphicsDevice.ResourceFactory.CreateBuffer(
            new BufferDescription(size, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
    }

    /// <summary>
    /// Updates the material's internal resource set if it is dirty.
    /// </summary>
    public void Update()
    {
        if (!this._resourceSetDirty) return;
        this._resourceSetDirty = false;

        this.ResourceSet?.Dispose();

        var factory = this._renderer.GraphicsDevice.ResourceFactory;

        BindableResource[] resources;
        if (this.ParamsBuffer != null)
        {
            resources = new BindableResource[this._resources.Length + 1];
            Array.Copy(this._resources, resources, this._resources.Length);
            resources[this._resources.Length] = this.ParamsBuffer;
        }
        else
        {
            resources = this._resources;
        }

        this.ResourceSet = factory.CreateResourceSet(new ResourceSetDescription(this.ForwardShader.MaterialResourceLayout, resources));
    }

    /// <summary>
    /// Updates the material's GPU resources. Called by the renderer before drawing.
    /// </summary>
    /// <param name="cl">The command list to use for the update.</param>
    internal void UpdateResources(CommandList cl)
    {
        this.UpdateParamsBuffer(cl);
    }

    /// <summary>
    /// Sets a texture at the specified index in the material's resource array.
    /// </summary>
    /// <param name="field">The backing field to update.</param>
    /// <param name="index">The texture index.</param>
    /// <param name="value">The texture to set, or null to use the default magenta texture.</param>
    protected void SetTexture(ref Texture? field, int index, Texture? value)
    {
        if (EqualityComparer<Texture>.Default.Equals(field, value)) return;

        field = value;

        var tex = value ?? Texture.Magenta;
        this._resources[index * 2 + 0] = tex.VeldridTexture;
        this._resources[index * 2 + 1] = tex.VeldridSampler;
        this.NotifyResourcesDirty();
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
    /// </summary>
    /// <typeparam name="T">The type of the property being set.</typeparam>
    /// <param name="field">The reference to the backing field of the property.</param>
    /// <param name="value">The new value to set for the property.</param>
    /// <returns>True if the value was changed; otherwise, false.</returns>
    protected bool SetProperty<T>(ref T field, T value)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;

        field = value;
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
    /// Disposes the material and its resources.
    /// </summary>
    public void Dispose()
    {
        if (this.IsDisposed) return;
        this.IsDisposed = true;

        this.ResourceSet?.Dispose();
        this.ParamsBuffer?.Dispose();
        this._resourceSetDirty = false;
        this._renderer.UnregisterDisposable(this);
    }
}
