using System.Collections.Generic;
using System.Numerics;
using Veldrid;

namespace LifeSim.Engine.Rendering;

public class Material
{
    private static int _count = 0;

    public int Id { get; private set; }
    public Shader Shader { get; private set; }
    public Shader ShadowmapShader { get; }
    public ResourceSet ResourceSet { get; internal set; } = null!;

    private bool _isDirty = true;
    private readonly BindableResource[] _resources;
    public Technique Definition { get; }
    private Texture _texture = Texture.Magenta;
    private readonly GraphicsDevice _gd;
    public Material(Technique definition)
    {
        this.Id = ++Material._count;
        this._gd = Renderer.Instance.GraphicsDevice;
        this.Definition = definition;
        this.Shader = definition.ForwardShader;
        this.ShadowmapShader = definition.ShadowMapShader;
        this._resources = new BindableResource[definition.ResourceCount];
        Renderer.Instance.OnMaterialDirty(this);
    }

    public void Update()
    {
        this.ResourceSet?.Dispose();

        this.ResourceSet = this._gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(this.Definition.ResourceLayout, this._resources));

        this._isDirty = false;
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

    protected void OnDirty()
    {
        if (!this._isDirty)
        {
            this._isDirty = true;
            Renderer.Instance.OnMaterialDirty(this);
        }
    }

    public void Dispose()
    {
        this.ResourceSet?.Dispose();
    }
}