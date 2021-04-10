using LifeSim.Engine.SceneGraph;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class SurfaceMaterial : IMaterial
    {
        public Pass pass          => this._materialManager.colorPass;
        public Pass shadowmapPass => this._materialManager.shadowMapPass;

        private readonly ResourceLayout _resourceLayout;
        public ResourceLayout resourceLayout => this._resourceLayout;

        private readonly ResourceSet _resourceSet;
        public ResourceSet resourceSet => this._resourceSet;

        private readonly IMaterialBuilder _materialManager;
        
        public bool castShadows = true;

        public GPUTexture _texture;
        public GPUTexture texture => this._texture;

        public SurfaceMaterial(IMaterialBuilder builder, GPUTexture? texture) 
        {
            this._materialManager = builder;
            this._resourceLayout = builder.layouts.materials.surface;
            this._texture = texture ?? builder.pinkTexture;
            this._resourceSet = builder.CreateResourceSet(this, this._texture.deviceTexture, this._texture.sampler);
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