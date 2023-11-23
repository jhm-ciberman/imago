using System;
using LifeSim.Imago.Graphics.Rendering;
using Veldrid;
using Texture = LifeSim.Imago.Graphics.Textures.Texture;

namespace LifeSim.Imago.Graphics.Materials;

public class Material
{
    private static int _count = 0;

    public int Id { get; private set; }

    private bool _resourceSetDirty = false;

    public RenderFlags RenderFlags { get; protected set; } = RenderFlags.DepthTest | RenderFlags.DepthWrite | RenderFlags.ReceiveShadows | RenderFlags.ColorWrite;


    public static Texture DefaultTexture { get; } = Texture.Magenta;

    public Shader ForwardShader { get; }
    public Shader ShadowMapShader { get; }
    public Shader PickingShader { get; }
    public ResourceSet ResourceSet { get; internal set; } = null!;
    private readonly BindableResource[] _resources;
    private Texture? _texture = null;

    private readonly Renderer _renderer;

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

        this.NotifyResourcesDirty();
    }

    public bool DoubleSided { get => this.GetRenderFlag(RenderFlags.DoubleSided); set => this.SetRenderFlag(RenderFlags.DoubleSided, value); }

    public bool Wireframe { get => this.GetRenderFlag(RenderFlags.Wireframe); set => this.SetRenderFlag(RenderFlags.Wireframe, value); }

    public bool DepthTest { get => this.GetRenderFlag(RenderFlags.DepthTest); set => this.SetRenderFlag(RenderFlags.DepthTest, value); }

    public bool DepthWrite { get => this.GetRenderFlag(RenderFlags.DepthWrite); set => this.SetRenderFlag(RenderFlags.DepthWrite, value); }

    public bool AlphaTest { get => this.GetRenderFlag(RenderFlags.AlphaTest); set => this.SetRenderFlag(RenderFlags.AlphaTest, value); }

    public bool Transparent { get => this.GetRenderFlag(RenderFlags.Transparent); set => this.SetRenderFlag(RenderFlags.Transparent, value); }

    public bool ColorWrite { get => this.GetRenderFlag(RenderFlags.ColorWrite); set => this.SetRenderFlag(RenderFlags.ColorWrite, value); }

    public bool ReceiveShadows { get => this.GetRenderFlag(RenderFlags.ReceiveShadows); set => this.SetRenderFlag(RenderFlags.ReceiveShadows, value); }


    public void Update(ResourceFactory factory)
    {
        if (!this._resourceSetDirty) return;
        this._resourceSetDirty = false;

        this.ResourceSet?.Dispose();

        this.ResourceSet = factory.CreateResourceSet(new ResourceSetDescription(this.ForwardShader.MaterialResourceLayout, this._resources));
    }

    protected void NotifyResourcesDirty()
    {
        if (this._resourceSetDirty) return;

        this._resourceSetDirty = true;
        this._renderer.NotifyMaterialResourcesDirty(this);
    }

    protected bool SetProperty<T>(ref T backingField, T newValue)
    {
        if (backingField == null && newValue == null) return false;
        if (backingField != null && newValue != null && backingField.Equals(newValue)) return false;

        backingField = newValue;
        this.NotifyResourcesDirty();
        return true;
    }

    protected bool SetRenderFlag(RenderFlags flags, bool value)
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

    protected bool GetRenderFlag(RenderFlags flag)
    {
        return (this.RenderFlags & flag) == flag;
    }

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

    protected void SetTexture(int textureIndex, Texture value)
    {
        this._resources[textureIndex * 2 + 0] = value.VeldridTexture;
        this._resources[textureIndex * 2 + 1] = value.VeldridSampler;
        this.NotifyResourcesDirty();
    }

    public void Dispose()
    {
        this.ResourceSet?.Dispose();
    }
}
