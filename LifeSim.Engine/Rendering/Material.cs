using System;
using Veldrid;

namespace LifeSim.Engine.Rendering;

public abstract class MaterialBase
{
    private static int _count = 0;

    public int Id { get; private set; }

    private bool _isDirty = true;

    public MaterialBase()
    {
        this.Id = ++Material._count;
        Renderer.Instance.OnMaterialDirty(this);
    }

    public void Update(ResourceFactory factory)
    {
        if (!this._isDirty) return;
        this._isDirty = false;

        this.UpdateResources(factory);
    }

    protected void OnDirty()
    {
        if (this._isDirty) return;
        this._isDirty = true;

        Renderer.Instance.OnMaterialDirty(this);
    }

    protected abstract void UpdateResources(ResourceFactory factory);
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

    protected override void UpdateResources(ResourceFactory factory)
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
        this.OnDirty();
    }

    public void Dispose()
    {
        this.ResourceSet?.Dispose();
    }

    public Pipeline GetForwardPipeline(Renderer renderer, VertexFormat vertexFormat)
    {
        return this.ForwardShader.GetPipeline(renderer.ForwardPass, vertexFormat);
    }

    public Pipeline GetShadowmapPipeline(Renderer renderer, VertexFormat vertexFormat)
    {
        return this.ShadowmapShader.GetPipeline(renderer.ShadowMapPass, vertexFormat);
    }
}