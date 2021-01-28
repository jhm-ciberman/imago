using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class SpriteMaterial : IMaterial
    {
        private readonly Pass _pass;
        public Pass pass => this._pass;

        public ResourceLayout resourceLayout { get; private set; }

        private readonly ResourceSet _resourceSet;
        public ResourceSet resourceSet => this._resourceSet;

        public SpriteMaterial(IMaterialBuilder builder, Texture texture) 
        {
            this._pass = builder.passes.sprites;
            this.resourceLayout = builder.layouts.materials.sprites;
            this._resourceSet = builder.CreateResourceSet(this, texture, builder.linearSampler);
        }

        public void Dispose()
        {
            this._resourceSet.Dispose();
        }
    }
}