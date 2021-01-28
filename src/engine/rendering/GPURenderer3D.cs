using Veldrid;
using System.Collections.Generic;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.Rendering
{
    public class GPURenderer3D
    {
        private readonly GraphicsDevice _graphicsDevice;
        
        private readonly Veldrid.ResourceFactory _factory;
        private readonly CommandList _commandList;

        private readonly IRenderTexture _renderTexture;
        private readonly SceneContext _sceneContext;
        private readonly PSOManager _psoManager;

        private readonly RenderList _renderList = new RenderList();
        private bool _hasCommandsToSubmit;

        public GPURenderer3D(GraphicsDevice graphicsDevice, PSOManager psoManager, GPUResourceManager resources)
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



            var camera = scene.activeCamera;
            if (camera != null) {
                // Shadowmap
                this._commandList.SetFramebuffer(this._sceneContext.shadowmapFramebuffer);
                this._commandList.ClearDepthStencil(1f);
                this._sceneContext.SetupShadowMapBuffer(this._commandList, camera, scene.mainLight);
                foreach (var renderable in this._renderList.shadowRenderables) {
                    if (renderable.material == null) continue;
                    this._DrawRenderable(renderable, renderable.material.shadowmapPass);
                }
            }

            // Opaques
            this._commandList.SetFramebuffer(this._renderTexture.framebuffer);
            this._sceneContext.SetupLightInfoBuffer(this._commandList, scene);

            this._commandList.ClearColorTarget(0, scene.clearColor);
            this._commandList.ClearColorTarget(1, RgbaFloat.Black);

            if (camera != null) {
                this._commandList.ClearDepthStencil(1f);
                this._sceneContext.SetupCamera3DInfoBuffer(this._commandList, camera, scene.mainLight);
                foreach (var renderable in this._renderList.baseRenderables) {
                    if (renderable.material == null) continue;
                    this._DrawRenderable(renderable, renderable.material.pass);
                }
            }

            this._commandList.End();
            this._hasCommandsToSubmit = true;
        }

        public void _DrawRenderable(Renderable3D renderable, Pass pass)
        {
            var mesh = renderable.mesh;
            var material = renderable.material;
            if (mesh == null || material == null) return;
            
            var objectResourceSet = this._sceneContext.GetObjectResourceSet(renderable);

            var pipeline = this._psoManager.GetPipeline(pass, material, renderable);

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
            if (! this._hasCommandsToSubmit) return;
            this._graphicsDevice.SubmitCommands(this._commandList);
            this._hasCommandsToSubmit = false;
        }

        ~GPURenderer3D() {
            this.Dispose();
        }
    }
}