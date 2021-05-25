using Veldrid;
using LifeSim.Engine.SceneGraph;
using System.Runtime.InteropServices;
using System.Numerics;
using System.Collections.Generic;

namespace LifeSim.Engine.Rendering
{
    public class SceneRenderer
    {
        private readonly GraphicsDevice _gd;
        private readonly Veldrid.ResourceFactory _factory;
        private readonly CommandList _commandList;
        private readonly IRenderTexture _mainRenderTexture;

        private bool _hasCommandsToSubmit;

        private ForwardPass _forwardPass;
        private ShadowmapPass _shadowmapPass;

        internal SceneStorage sceneStorage;
        private readonly ResourceLayout _instanceResourceLayout;
        private readonly ResourceLayout _transformResourceLayout;
        private readonly ResourceLayout _bonesResourceLayout;

        private readonly Dictionary<RenderNode3D, ResourceSet> _resourceSets = new Dictionary<RenderNode3D, ResourceSet>();
        private Shader _surfaceShader;
        private Shader _shadowmapShader;
        public SceneRenderer(GraphicsDevice gd, RenderTexture mainRenderTexture)
        {
            this._gd = gd;
            this._mainRenderTexture = mainRenderTexture;
            var factory = this._gd.ResourceFactory;
            this._factory = factory;
            this._commandList = this._factory.CreateCommandList();

            this._instanceResourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("InstanceDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)
            ));
            this._instanceResourceLayout.Name = "InstanceData Resource Layout";

            this._transformResourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("TransformDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
            ));
            this._transformResourceLayout.Name = "TransformData Resource Layout";

            this._bonesResourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("BonesDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
            ));
            this._bonesResourceLayout.Name = "BonesData Resource Layout";

            this.sceneStorage = new SceneStorage(gd, this._transformResourceLayout, this._instanceResourceLayout, this._bonesResourceLayout);
            this._shadowmapPass = new ShadowmapPass(gd, this);
            this._forwardPass = new ForwardPass(gd, this, this._mainRenderTexture, this._shadowmapPass.shadowmapTexture);

            this._surfaceShader = ShaderRegistry.CreateBaseShader(this._gd, this._forwardPass);
            this._shadowmapShader = ShaderRegistry.CreateShadowmapShader(this._gd, this._shadowmapPass);
        }

        public void Dispose()
        {
            this._commandList.Dispose();
            this._surfaceShader.Dispose();
            this._shadowmapShader.Dispose();
        }

        public SurfaceMaterial CreateSurfaceMaterial(Texture texture)
        {
            return new SurfaceMaterial(this._surfaceShader, this._shadowmapShader, texture);
        }

        public void Render(Scene3D scene, Camera3D camera)
        {
            scene.UpdateWorldMatrices();
            scene.UpdateInstanceData();
            

            this._commandList.Begin();
            scene.storage.UpdateBuffers(this._commandList);
            this._shadowmapPass.Render(this._commandList, scene, camera, scene.mainLight);
            this._forwardPass.Render(this._commandList, scene, camera);
            this._commandList.End();

            this._hasCommandsToSubmit = true;
        }

        internal Veldrid.ResourceLayout[] GetShaderVariantResourceLayouts(Veldrid.ResourceLayout passResourceLayout, ShaderVariant shaderVariant)
        {
            var resources = new List<ResourceLayout>();
            resources.Add(passResourceLayout);
            resources.Add(this._transformResourceLayout);
            resources.Add(shaderVariant.materialResourceLayout);
            resources.Add(this._instanceResourceLayout);

            if (shaderVariant.vertexFormat == VertexFormat.Skinned) {
                resources.Add(this._bonesResourceLayout);
            }

            return resources.ToArray();
        }

        public void Submit()
        {
            if (! this._hasCommandsToSubmit) return;
            this._gd.SubmitCommands(this._commandList);
            this._hasCommandsToSubmit = false;
        }
    }
}