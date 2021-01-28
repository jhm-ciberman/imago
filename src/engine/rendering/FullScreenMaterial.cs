using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class FullScreenMaterial : IMaterial
    {
        private readonly Pass _pass;
        public Pass pass => this._pass;

        public ResourceLayout resourceLayout { get; private set; }

        private readonly ResourceSet _resourceSet;
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