using System.Collections.Generic;
using System.Numerics;
using Veldrid;

namespace LifeSim.Rendering
{
    public class Material
    {
        private static int _count = 0;

        public int Id { get; private set; }
        public Shader Shader { get; private set; }
        public Shader ShadowmapShader { get; private set; }

        private ResourceSet? _resourceSet = null;

        private bool _resourceSetDirty = true;
        private readonly BindableResource[] _resources;
        public readonly MaterialDefinition Definition;
        private readonly Dictionary<string, Texture> _textures = new Dictionary<string, Texture>();

        public Material(MaterialDefinition definition)
        {
            this.Id = ++Material._count;
            this.Definition = definition;
            this.Shader = definition.GetShader(SceneRenderer.ForwardPass);
            this.ShadowmapShader = definition.GetShader(SceneRenderer.ShadowMapPass);
            this._resources = new BindableResource[definition.ResourceCount];
        }

        internal Veldrid.ResourceSet GetMaterialResourceSet()
        {
            lock (this.Shader)
            {
                if (this._resourceSetDirty || this._resourceSet == null) {
                    this._resourceSetDirty = false;
                    this._resourceSet?.Dispose();
                    this._resourceSet = this.Shader.CreateResourceSet(this._resources);
                }

                return this._resourceSet; 
            }
        }

        public Texture GetTexture(string name)
        {
            return this._textures[name];
        }

        public void SetTexture(string name, Texture texture)
        {
            this._textures[name] = texture;
            int index = this.Definition.Textures[name];
            this._resources[index * 2 + 0] = texture.Resource;
            this._resources[index * 2 + 1] = texture.Sampler;
            this._resourceSetDirty = true;
        }

        public void Dispose()
        {
            this._resourceSet?.Dispose();
        }
    }
}