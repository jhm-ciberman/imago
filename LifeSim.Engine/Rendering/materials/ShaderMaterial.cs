using System.Collections.Generic;

namespace LifeSim.Engine.Rendering
{
    public class ShaderMaterial : Material
    {
        public Dictionary<string, Texture> _textures = new Dictionary<string, Texture>();

        private Dictionary<string, int> _bindingPoints = new Dictionary<string, int>();

        public ShaderMaterial(Shader shader) : base(shader)
        {
            for (int i = 0; i <  this.shader.resourceCount; i++) {
                this._bindingPoints[this.shader.GetResourceName(i)] = i;
            }
        }

        public void SetTexture(string name, Texture texture)
        {
            if (this._textures[name] != texture) {
                this._textures[name] = texture;
                this._SetResource(name + "Texture", texture.deviceTexture);
                this._SetResource(name + "Sampler", texture.sampler);
            }
        }

        private void _SetResource(string name, Veldrid.BindableResource resource)
        {
            int index = this._bindingPoints[name];
            this._resources[index] = resource;
        }

        public Texture GetTexture(string name)
        {
            return this._textures[name];
        }
    }
}