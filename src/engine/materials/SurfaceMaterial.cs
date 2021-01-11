using System.Numerics;
using System.Runtime.InteropServices;
using LifeSim.SceneGraph;
using Veldrid;

namespace LifeSim.Rendering
{
    public class SurfaceMaterial : IMaterial
    {
        public Pass pass          => this._materialManager.passes.color;
        public Pass shadowmapPass => this._materialManager.passes.shadowMap;

        private ResourceLayout _resourceLayout;
        public ResourceLayout resourceLayout => this._resourceLayout;

        private ResourceSet _resourceSet;
        public ResourceSet resourceSet => this._resourceSet;

        private IMaterialBuilder _materialManager;

        public SurfaceMaterial(IMaterialBuilder builder, GPUTexture texture) 
        {
            this._materialManager = builder;
            this._resourceLayout = builder.layouts.materials.surface;
            this._resourceSet = builder.CreateResourceSet(this, texture.deviceTexture, texture.sampler);
        }

        public ResourceLayout GetObjectResourceLayout(Renderable3D renderable)
        {
            return (renderable is SkinnedRenderable3D)
                ? this._materialManager.layouts.renderables.skinned
                : this._materialManager.layouts.renderables.regular;
        } 

        public void Dispose()
        {
            this._resourceSet.Dispose();
        }
    }
}