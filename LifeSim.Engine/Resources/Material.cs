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
    public ResourceSet? ResourceSet { get; internal set; }

    private bool _isDirty = true;
    private readonly BindableResource[] _resources;
    public Technique Definition { get; }
    private readonly Dictionary<string, Texture> _textures = new Dictionary<string, Texture>();
    private readonly GraphicsDevice _gd;
    public Material(Technique definition)
    {
        this.Id = ++Material._count;
        this._gd = Renderer.Instance.GraphicsDevice;
        this.Definition = definition;
        this.Shader = definition.GetShader(Renderer.Instance.ForwardPass);
        this.ShadowmapShader = definition.GetShader(Renderer.Instance.ShadowMapPass);
        this._resources = new BindableResource[definition.ResourceCount];
        Renderer.Instance.OnMaterialDirty(this);
    }

    public void Update()
    {
        this.ResourceSet?.Dispose();

        this.ResourceSet = this._gd.ResourceFactory.CreateResourceSet(
            new ResourceSetDescription(this.Definition.ResourceLayout, this._resources));

        this._isDirty = false;
    }

    public Texture GetTexture(string name)
    {
        return this._textures[name];
    }

    public void SetTexture(string name, Texture texture)
    {
        if (this._textures.TryGetValue(name, out var oldTexture))
        {
            if (oldTexture == texture) return;
        }

        this._textures[name] = texture;
        int index = this.Definition.Textures[name];
        this._resources[index * 2 + 0] = texture.DeviceTexture;
        this._resources[index * 2 + 1] = texture.Sampler;
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