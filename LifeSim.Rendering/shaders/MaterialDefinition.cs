using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;

namespace LifeSim.Rendering
{
    public class MaterialDefinition
    {
        public interface IUniform
        {
            string name { get; }
            void CopyTo(Span<byte> dest);
        }

        private Dictionary<string, int> _instanceUniformData = new Dictionary<string, int>();
        private Dictionary<string, int> _textures = new Dictionary<string, int>();
        private int _resourceCount;

        public int resourceCount => this._resourceCount;
        public IReadOnlyDictionary<string, int> instanceUniformData => this._instanceUniformData;
        public IReadOnlyDictionary<string, int> textures => this._textures;

        public readonly int instanceDataBlockSize;

        private List<Shader> _shaders = new List<Shader>();

        private ResourceLayout _resourceLayout;

        private Memory<byte> _instanceDefaultData;

        public MaterialDefinition(VertexFormat[] vertexFormats, IUniform[] uniforms, string[] textures)
        {
            this._textures = new Dictionary<string, int>();
            this._resourceCount = textures.Length * 2;
            var elements = new ResourceLayoutElementDescription[this._resourceCount];
            for (int i = 0, j = 0; i < textures.Length; i++) {
                var name = textures[i];
                this._textures.Add(name, i);
                elements[j++] = new ResourceLayoutElementDescription(name + "Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment); 
                elements[j++] = new ResourceLayoutElementDescription(name + "Sampler", ResourceKind.Sampler, ShaderStages.Fragment); 
            }

            this._resourceLayout = Renderer.graphicsDevice.ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription(elements));

            this.instanceDataBlockSize = uniforms.Length * 16;
            this._instanceDefaultData = new Memory<byte>(new byte[this.instanceDataBlockSize]);
            for (int i = 0; i < uniforms.Length; i++) {
                this._instanceUniformData.Add(uniforms[i].name, i * 16);
                var dest = this._instanceDefaultData.Span.Slice(i * 16, 16);
                uniforms[i].CopyTo(dest);
            }

        }

        public MaterialDefinition AddPass(IPass pass, ShaderSource source)
        {
            this._shaders.Add(new Shader(pass, source, this._resourceLayout));
            return this;
        }

        public Span<byte> GetDefaultInstanceData()
        {
            return this._instanceDefaultData.Span;
        }

        public Shader GetShader(IPass pass)
        {
            for (int i = 0; i < this._shaders.Count; i++) {
                if (this._shaders[i].pass == pass) {
                    return this._shaders[i];
                }
            }
            throw new System.Exception("The material does not support this kind of pass");
        }

    }
}