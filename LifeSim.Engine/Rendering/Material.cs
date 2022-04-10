using System;
using Veldrid;

namespace LifeSim.Engine.Rendering;

public abstract class MaterialBase
{
    public delegate void MaterialResourceSetDirtyHandler(MaterialBase material);
    public delegate void MaterialStateChangedHandler(MaterialBase material);
    public event MaterialStateChangedHandler? PipelineDirty;
    public static event MaterialResourceSetDirtyHandler? MaterialResourceSetDirty;

    private static int _count = 0;

    public int Id { get; private set; }

    private bool _resourceSetDirty = false;

    protected RenderFlags RenderFlags { get; private set; } = RenderFlags.Default;

    public bool DoubleSided { get => this.GetRenderFlag(RenderFlags.DoubleSided); set => this.SetRenderFlag(RenderFlags.DoubleSided, value); }

    public bool Wireframe { get => this.GetRenderFlag(RenderFlags.Wireframe); set => this.SetRenderFlag(RenderFlags.Wireframe, value); }

    public bool AlphaBlending { get => this.GetRenderFlag(RenderFlags.AlphaBlending); set => this.SetRenderFlag(RenderFlags.AlphaBlending, value); }

    public bool DepthTest { get => this.GetRenderFlag(RenderFlags.DepthTest); set => this.SetRenderFlag(RenderFlags.DepthTest, value); }

    public bool DepthWrite { get => this.GetRenderFlag(RenderFlags.DepthWrite); set => this.SetRenderFlag(RenderFlags.DepthWrite, value); }

    public MaterialBase()
    {
        this.Id = ++Material._count;
        this.NotifyResourcesDirty();
    }

    public void Update(ResourceFactory factory)
    {
        if (!this._resourceSetDirty) return;
        this._resourceSetDirty = false;

        this.UpdateResourceSet(factory);
    }

    protected void NotifyResourcesDirty()
    {
        if (!this._resourceSetDirty)
        {
            this._resourceSetDirty = true;
            MaterialResourceSetDirty?.Invoke(this);
        }
    }

    protected bool SetProperty<T>(ref T backingField, T newValue)
    {
        if (backingField == null && newValue == null) return false;
        if (backingField != null && newValue != null && backingField.Equals(newValue)) return false;

        backingField = newValue;
        this.NotifyResourcesDirty();
        return true;
    }

    protected void NotifyPipelineDirty()
    {
        this.PipelineDirty?.Invoke(this);
    }

    protected bool SetRenderFlag(RenderFlags flags, bool value)
    {
        if (value)
        {
            if ((this.RenderFlags & flags) == flags) return false;
            this.RenderFlags |= flags;
            this.NotifyPipelineDirty();
            return true;
        }
        else
        {
            if ((this.RenderFlags & flags) == 0) return false;
            this.RenderFlags &= ~flags;
            this.NotifyPipelineDirty();
            return true;
        }
    }

    protected bool GetRenderFlag(RenderFlags flag)
    {
        return (this.RenderFlags & flag) == flag;
    }

    protected abstract void UpdateResourceSet(ResourceFactory factory);
}

public class Material : MaterialBase
{
    public Shader ForwardShader { get; private set; }
    public Shader ShadowmapShader { get; }
    public ResourceSet ResourceSet { get; internal set; } = null!;
    private readonly BindableResource[] _resources;
    public Technique Definition { get; }
    private Texture _texture = Texture.Magenta;

    public Material(Technique definition)
    {
        this.Definition = definition;
        this.ForwardShader = definition.ForwardShader;
        this.ShadowmapShader = definition.ShadowMapShader;
        this._resources = new BindableResource[definition.ResourceCount];
    }

    protected override void UpdateResourceSet(ResourceFactory factory)
    {
        this.ResourceSet?.Dispose();

        this.ResourceSet = factory.CreateResourceSet(new ResourceSetDescription(this.Definition.ResourceLayout, this._resources));
    }

    public Texture Texture
    {
        get => this._texture;
        set
        {
            if (this._texture == value) return;

            this._texture = value;
            this.SetTexture(0, this._texture);
        }
    }

    protected void SetTexture(int textureIndex, Texture value)
    {
        this._resources[textureIndex * 2 + 0] = value.DeviceTexture;
        this._resources[textureIndex * 2 + 1] = value.Sampler;
        this.NotifyResourcesDirty();
    }

    public void Dispose()
    {
        this.ResourceSet?.Dispose();
    }

    public Pipeline GetForwardPipeline(Renderer renderer, VertexFormat vertexFormat, bool forceWireframe)
    {
        RenderFlags flags = this.RenderFlags;
        if (forceWireframe) flags |= RenderFlags.Wireframe;
        return this.ForwardShader.GetPipeline(renderer.ForwardPass, vertexFormat, flags);
    }

    public Pipeline GetShadowmapPipeline(Renderer renderer, VertexFormat vertexFormat)
    {
        return this.ShadowmapShader.GetPipeline(renderer.ShadowMapPass, vertexFormat, RenderFlags.Default);
    }
}