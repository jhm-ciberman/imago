using LifeSim.Engine.SceneGraph;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class SurfaceMaterial : IMaterial
    {
        private readonly ResourceLayout _resourceLayout;
        public ResourceLayout resourceLayout => this._resourceLayout;

        private readonly ResourceSet _resourceSet;
        public ResourceSet resourceSet => this._resourceSet;

        private readonly IMaterialBuilder _materialManager;
        
        public bool castShadows = true;

        public Texture texture { get; private set; }

        public SurfaceMaterial(IMaterialBuilder builder, Texture texture) 
        {
            this._materialManager = builder;
            this._resourceLayout = builder.layouts.materials.surface;
            this.texture = texture;
            this._resourceSet = builder.CreateResourceSet(this, this.texture.deviceTexture, this.texture.sampler);
        }

        public ResourceLayout GetObjectResourceLayout(RenderNode3D renderable)
        {
            return (renderable is SkinRenderNode3D)
                ? this._materialManager.layouts.renderables.skinned
                : this._materialManager.layouts.renderables.regular;
        } 

        public void Dispose()
        {
            this._resourceSet.Dispose();
        }
    }
}