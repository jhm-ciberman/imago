using Veldrid;

namespace LifeSim.Rendering
{
    public class FullScreenMaterial : IMaterial
    {
        private Pass _pass;
        public Pass pass => this._pass;

        public ResourceLayout resourceLayout { get; private set; }

        private ResourceSet _resourceSet;
        public ResourceSet resourceSet => this._resourceSet;

        public FullScreenMaterial(IMaterialBuilder builder, Texture texture) 
        {
            this._pass = builder.passes.fullscreen;
            this.resourceLayout = builder.layouts.materials.fullscreen;
            this._resourceSet = builder.CreateResourceSet(this, texture, builder.linearSampler);
        }

        public void Dispose()
        {
            this._resourceSet.Dispose();
        }
    }
}