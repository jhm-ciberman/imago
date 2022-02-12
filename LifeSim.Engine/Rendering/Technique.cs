using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;

namespace LifeSim.Engine.Rendering;

public class Technique
{
    public interface IUniform
    {
        string Name { get; }
        void CopyTo(Span<byte> dest);
    }

    private readonly Dictionary<string, int> _instanceUniformData = new Dictionary<string, int>();
    private readonly Dictionary<string, int> _textures = new Dictionary<string, int>();

    public int ResourceCount { get; }
    public IReadOnlyDictionary<string, int> Textures => this._textures;

    public int InstanceDataBlockSize { get; }
    public ResourceLayout ResourceLayout { get; }
    private readonly List<Shader> _shaders = new List<Shader>();
    private readonly Memory<byte> _instanceDefaultData;

    public Technique(IUniform[] uniforms, string[] textures)
    {
        this._textures = new Dictionary<string, int>();
        this.ResourceCount = textures.Length * 2;
        var elements = new ResourceLayoutElementDescription[this.ResourceCount];
        for (int i = 0, j = 0; i < textures.Length; i++)
        {
            var name = textures[i];
            this._textures.Add(name, i);
            elements[j++] = new ResourceLayoutElementDescription(name + "Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment);
            elements[j++] = new ResourceLayoutElementDescription(name + "Sampler", ResourceKind.Sampler, ShaderStages.Fragment);
        }

        this.ResourceLayout = Renderer.Instance.GraphicsDevice.ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription(elements));

        this.InstanceDataBlockSize = uniforms.Length * 16;
        this._instanceDefaultData = new Memory<byte>(new byte[this.InstanceDataBlockSize]);
        for (int i = 0; i < uniforms.Length; i++)
        {
            this._instanceUniformData.Add(uniforms[i].Name, i * 16);
            var dest = this._instanceDefaultData.Span.Slice(i * 16, 16);
            uniforms[i].CopyTo(dest);
        }

    }

    public int GetInstanceUniformDataOffset(string name)
    {
        if (this._instanceUniformData.TryGetValue(name, out int offset))
        {
            return offset;
        }
        throw new ArgumentException($"Uniform {name} not found");
    }

    public Technique AddPass(IPipelineProvider pass, string vertexCode, string fragmentCode)
    {
        this._shaders.Add(new Shader(pass, vertexCode, fragmentCode, this.ResourceLayout));
        return this;
    }

    public Span<byte> GetDefaultInstanceData()
    {
        return this._instanceDefaultData.Span;
    }

    public Shader GetShader(IPipelineProvider pass)
    {
        for (int i = 0; i < this._shaders.Count; i++)
        {
            if (this._shaders[i].Pass == pass)
            {
                return this._shaders[i];
            }
        }
        throw new System.Exception("The material does not support this kind of pass");
    }

}