using Veldrid;
using System.Collections.Generic;
using LifeSim.SceneGraph;

namespace LifeSim.Rendering
{
    public class GPURenderer3D
    {
        private GraphicsDevice _graphicsDevice;
        
        private ResourceFactory _factory;
        private CommandList _commandList;

        private IRenderTexture _renderTexture;
        private SceneContext _sceneContext;
        private PSOManager _psoManager;

        private RenderList _renderList = new RenderList();

        public GPURenderer3D(GraphicsDevice graphicsDevice, PSOManager psoManager, GPUResources resources)
        {
            this._graphicsDevice = graphicsDevice;
            this._renderTexture = resources.mainRenderTexture;
            this._psoManager = psoManager;
            this._factory = this._graphicsDevice.ResourceFactory;
            this._sceneContext = resources.sceneContext;
            this._commandList = this._factory.CreateCommandList();
        }

        public void Dispose()
        {
            this._commandList.Dispose();
        }

        public void Render(Scene3D scene)
        {
            this._renderList.UpdateRenderList(scene);
            scene.UpdateWorldMatrices();

            this._commandList.Begin();

            // Shadowmap
            this._commandList.SetFramebuffer(this._sceneContext.shadowmapFramebuffer);

            this._commandList.ClearDepthStencil(1f);
            this._sceneContext.SetupShadowMapBuffer(this._commandList, scene.mainLight);
            foreach (var renderable in this._renderList.renderables) {
                this._DrawRenderable(renderable, renderable.material.shadowmapPass);
            }

            // Opaques
            this._commandList.SetFramebuffer(this._renderTexture.framebuffer);
            this._sceneContext.SetupLightInfoBuffer(this._commandList, scene);

            foreach (var camera in scene.cameras) {
                this._commandList.ClearColorTarget(0, camera.clearColor);
                this._commandList.ClearColorTarget(1, RgbaFloat.Black);
                this._commandList.ClearDepthStencil(1f);
                this._sceneContext.SetupCamera3DInfoBuffer(this._commandList, camera, scene.mainLight);
                foreach (var renderable in this._renderList.renderables) {
                    this._DrawRenderable(renderable, renderable.material.pass);
                }
            }

            this._commandList.End();
        }

        public void _DrawRenderable(Renderable3D renderable, Pass pass)
        {
            var mesh = renderable.mesh;
            var material = renderable.material;

            var objectResourceSet = this._sceneContext.GetObjectResourceSet(renderable);

            var pipeline = this._psoManager.GetPipeline(pass, renderable.material, renderable);

            this._UpdatePerObjectBuffers(renderable);
            this._commandList.SetVertexBuffer(0, mesh.vertexBuffer);
            this._commandList.SetIndexBuffer(mesh.indexBuffer, IndexFormat.UInt16);
            this._commandList.SetPipeline(pipeline);
            this._commandList.SetGraphicsResourceSet(0, pass.resourceSet); // Per pass
            this._commandList.SetGraphicsResourceSet(1, material.resourceSet); // Per Material
            this._commandList.SetGraphicsResourceSet(2, objectResourceSet); // Per object
            this._commandList.DrawIndexed(
                indexCount: mesh.indexCount,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0
            );
        }

        private void _UpdatePerObjectBuffers(Renderable3D renderable)
        {
            this._sceneContext.SetupObjectInfoBuffer(this._commandList, renderable);
            if (renderable is SkinnedRenderable3D skinnedRenderable) {
                this._sceneContext.SetupBonesInfoBuffer(this._commandList, skinnedRenderable);
            }
        }

        public void Submit()
        {
            this._graphicsDevice.SubmitCommands(this._commandList);
        }

        ~GPURenderer3D() {
            this.Dispose();
        }
    }
}