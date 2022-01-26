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

    private readonly Renderer _renderer;

    private readonly GraphicsDevice _gd;
    public Material(Renderer renderer, Technique definition)
    {
        this.Id = ++Material._count;
        this._renderer = renderer;
        this._gd = renderer.GraphicsDevice;
        this.Definition = definition;
        this.Shader = definition.GetShader(this._renderer.ForwardPass);
        this.ShadowmapShader = definition.GetShader(this._renderer.ShadowMapPass);
        this._resources = new BindableResource[definition.ResourceCount];
        this._renderer.OnMaterialDirty(this);
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
            this._renderer.OnMaterialDirty(this);
        }
    }

    public void Dispose()
    {
        this.ResourceSet?.Dispose();
    }
}