using Veldrid;
using System.Collections.Generic;
using System.Diagnostics;

namespace LifeSim.Rendering
{
    public class SceneRenderer
    {
        public static IPipelineProvider ForwardPass = null!;
        public static IPipelineProvider ShadowMapPass = null!;

        private readonly GraphicsDevice _gd;
        private readonly Veldrid.ResourceFactory _factory;
        private readonly CommandList _commandList;
        private readonly IRenderTexture _mainRenderTexture;

        private bool _hasCommandsToSubmit;

        private readonly ForwardPass _forwardPass;
        private readonly ShadowmapPass _shadowmapPass;
        private readonly ResourceLayout _instanceResourceLayout;
        private readonly ResourceLayout _transformResourceLayout;
        private readonly ResourceLayout _bonesResourceLayout;
        internal SceneStorage Storage { get; }

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

            this.Storage = new SceneStorage(gd, this._transformResourceLayout, this._instanceResourceLayout, this._bonesResourceLayout);
            this._shadowmapPass = new ShadowmapPass(gd, this);
            this._forwardPass = new ForwardPass(gd, this, this._mainRenderTexture, this._shadowmapPass.ShadowmapTexture);

            SceneRenderer.ShadowMapPass = this._shadowmapPass;
            SceneRenderer.ForwardPass = this._forwardPass;
        }

        public void Dispose()
        {
            this._commandList.Dispose();
        }

        public void Render(IReadOnlyList<Renderable> renderables, DirectionalLight mainLight, ColorF ambientColor, ColorF clearColor, ICamera camera)
        {
            this._commandList.Begin();
            this.Storage.UpdateBuffers(this._commandList);
            this._shadowmapPass.Render(this._commandList, renderables, camera, mainLight);
            this._forwardPass.Render(this._commandList, renderables, mainLight, ambientColor, clearColor, camera);
            this._commandList.End();

            this._hasCommandsToSubmit = true;
        }

        internal ResourceLayout[] GetShaderVariantResourceLayouts(ResourceLayout passResourceLayout, ShaderVariant shaderVariant)
        {
            Debug.Assert(shaderVariant.MaterialResourceLayout != null);

            var resources = new List<ResourceLayout> {
                passResourceLayout,
                this._transformResourceLayout,
                shaderVariant.MaterialResourceLayout,
                this._instanceResourceLayout
            };

            if (shaderVariant.VertexFormat.IsSkinned)
            {
                resources.Add(this._bonesResourceLayout);
            }

            return resources.ToArray();
        }

        public void Submit()
        {
            if (!this._hasCommandsToSubmit) return;
            this._gd.SubmitCommands(this._commandList);
            this._hasCommandsToSubmit = false;
        }
    }
}