using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace LifeSim.Rendering
{
    public class Material : IMaterial
    {
        private Pass _pass;
        public Pass pass => this._pass;

        private Pass _shadowMapPass;
        public Pass shadowmapPass => this._shadowMapPass;

        private ResourceSet _resourceSet;
        private GraphicsDevice _gd;

        public Material(Pass pass, Pass shadowMapPass, GraphicsDevice gd, ResourceLayout materialLayout, GPUTexture texture) 
        {
            this._pass = pass;
            this._shadowMapPass = shadowMapPass;
            this._gd = gd;
            var factory = this._gd.ResourceFactory;
            this._resourceSet = factory.CreateResourceSet(new ResourceSetDescription(
                materialLayout, texture.deviceTexture, texture.sampler
            ));
        }

        public ResourceSet GetResourceSet()
        {
            return this._resourceSet;
        }

        public void Dispose()
        {
            this._resourceSet.Dispose();
        }
    }
}