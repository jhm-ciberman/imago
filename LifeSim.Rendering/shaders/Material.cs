using System.Collections.Generic;
using System.Numerics;
using Veldrid;

namespace LifeSim.Rendering
{
    public class Material
    {
        private static int _count = 0;

        public int id { get; private set; }
        public Shader shader { get; private set; }
        public Shader shadowmapShader { get; private set; }

        private ResourceSet? _resourceSet = null;

        private bool _resourceSetDirty = true;

        private BindableResource[] _resources;

        public readonly MaterialDefinition definition; 

        private Dictionary<string, Texture> _textures = new Dictionary<string, Texture>();

        public Material(MaterialDefinition definition)
        {
            this.id = ++Material._count;
            this.definition = definition;
            this.shader = definition.GetShader(SceneRenderer.forwardPass);
            this.shadowmapShader = definition.GetShader(SceneRenderer.shadowMapPass);
            this._resources = new BindableResource[definition.resourceCount];
        }

        internal Veldrid.ResourceSet GetMaterialResourceSet()
        {
            lock (this.shader)
            {
                if (this._resourceSetDirty || this._resourceSet == null) {
                    this._resourceSetDirty = false;
                    this._resourceSet?.Dispose();
                    this._resourceSet = this.shader.CreateResourceSet(this._resources);
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
            int index = this.definition.textures[name];
            this._resources[index * 2 + 0] = texture.deviceTexture;
            this._resources[index * 2 + 1] = texture.sampler;
            this._resourceSetDirty = true;
        }

        public void Dispose()
        {
            this._resourceSet?.Dispose();
        }
    }
}