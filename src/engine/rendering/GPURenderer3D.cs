using Veldrid;
using System.Collections.Generic;
using LifeSim.Engine.SceneGraph;
using System.Runtime.CompilerServices;

namespace LifeSim.Engine.Rendering
{
    public class GPURenderer3D
    {
        private readonly GraphicsDevice _graphicsDevice;
        
        private readonly Veldrid.ResourceFactory _factory;
        private readonly CommandList _commandList;

        private readonly IRenderTexture _renderTexture;
        private readonly SceneManager _sceneManager;
        private readonly PSOManager _psoManager;

        private readonly RenderQueue _baseRQ = new RenderQueue();
        private readonly RenderQueue _shadowmapRQ = new RenderQueue();

        private Pipeline? _currentPipeline;
        private GPUMesh? _currentMesh;
        private Pass? _currentPass;
        private IMaterial? _currentMaterial;

        public FrameProfiler frameProfilerBase = new FrameProfiler();
        public FrameProfiler frameProfilerShadowmap = new FrameProfiler();

        private bool _hasCommandsToSubmit;

        public GPURenderer3D(GraphicsDevice graphicsDevice, PSOManager psoManager, GPUResourceManager resources)
        {
            this._graphicsDevice = graphicsDevice;
            this._renderTexture = resources.mainRenderTexture;
            this._psoManager = psoManager;
            this._factory = this._graphicsDevice.ResourceFactory;
            this._sceneManager = resources.sceneManager;
            this._commandList = this._factory.CreateCommandList();
        }

        public void Dispose()
        {
            this._commandList.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void Render(Scene3D scene, Camera3D camera)
        {
            this._currentPipeline = null;
            this._currentMesh = null;
            this._currentPass = null;
            this._currentMaterial = null;

            scene.UpdateWorldMatrices();

            this._commandList.Begin();

            this._baseRQ.Update(scene, camera);
            this._shadowmapRQ.Update(scene, scene.mainLight, camera);
            this._baseRQ.Sort();
            this._shadowmapRQ.Sort();
            
            this._RenderShadowmap(camera, scene.mainLight);
            this._RenderCamera(scene, camera);
            
            this._commandList.End();
            this._hasCommandsToSubmit = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void _RenderShadowmap(Camera3D camera, DirectionalLight mainLight)
        {
            // Shadowmap
            this.frameProfilerShadowmap.BeginFrame();
            this._commandList.SetFramebuffer(this._sceneManager.shadowmapFramebuffer);
            this._commandList.ClearDepthStencil(1f);
            this._sceneManager.SetupShadowMapBuffer(this._commandList, camera, mainLight);
            foreach (var renderable in this._shadowmapRQ) {
                if (renderable.material == null) continue;
                this._DrawRenderable(renderable, renderable.material.shadowmapPass, this.frameProfilerShadowmap);
            }
            this.frameProfilerShadowmap.EndFrame();
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void _RenderCamera(Scene3D scene, Camera3D camera)
        {
            // Opaques
            this._sceneManager.SetupLightInfoBuffer(this._commandList, scene);

            this._commandList.SetFramebuffer(this._renderTexture.framebuffer);
            //this._commandList.SetViewport(0, new Veldrid.Viewport(camera.viewport.x, camera.viewport.y, camera.viewport.width, camera.viewport.height, 0f, 1f));
            //this._commandList.SetViewport(1, new Veldrid.Viewport(camera.viewport.x, camera.viewport.y, camera.viewport.width, camera.viewport.height, 0f, 1f));
            //this._commandList.SetScissorRect(0, camera.viewport.x, camera.viewport.y, camera.viewport.width, camera.viewport.height);
            //this._commandList.SetScissorRect(1, camera.viewport.x, camera.viewport.y, camera.viewport.width, camera.viewport.height);
            this._commandList.ClearColorTarget(0, scene.clearColor);
            this._commandList.ClearColorTarget(1, RgbaFloat.Black);
            this.frameProfilerBase.BeginFrame();
            this._commandList.ClearDepthStencil(1f);
            this._sceneManager.SetupCamera3DInfoBuffer(this._commandList, camera, scene.mainLight);
            foreach (var renderable in this._baseRQ) {
                if (renderable.material == null) continue;
                this._DrawRenderable(renderable, renderable.material.pass, this.frameProfilerBase);
            }
            this.frameProfilerBase.EndFrame();
        }

        public void _DrawRenderable(RenderNode3D renderable, Pass pass, FrameProfiler frameProfiler)
        {
            var mesh = renderable.mesh;
            var material = renderable.material;
            if (mesh == null || material == null) return;
            
            var objectResourceSet = this._sceneManager.GetObjectResourceSet(renderable);

            this._UpdatePerObjectBuffers(renderable);

            if (this._currentMesh != mesh) {
                this._commandList.SetVertexBuffer(0, mesh.vertexBuffer);
                this._commandList.SetIndexBuffer(mesh.indexBuffer, IndexFormat.UInt16);
                this._currentMesh = mesh;
                frameProfiler.ChangeMesh(mesh);
            }

            var pipeline = this._psoManager.GetPipeline(pass, material, renderable);
            if (this._currentPipeline != pipeline) {
                this._commandList.SetPipeline(pipeline);
                this._commandList.SetGraphicsResourceSet(0, pass.resourceSet); // Per pass
                this._currentPipeline = pipeline;
                this._currentPass = pass;
                this._currentMaterial = null;
                frameProfiler.ChangePipeline(pipeline);
            }

            if (this._currentMaterial != material) {
                this._commandList.SetGraphicsResourceSet(1, material.resourceSet); // Per Material
                this._currentMaterial = material;
                frameProfiler.ChangeMaterial(material);
            }

            this._commandList.SetGraphicsResourceSet(2, objectResourceSet); // Per object

            this._commandList.DrawIndexed(
                indexCount: mesh.indexCount,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0
            );
            frameProfiler.DrawCall();
        }

        private void _UpdatePerObjectBuffers(RenderNode3D renderable)
        {
            this._sceneManager.SetupObjectInfoBuffer(this._commandList, renderable);
            if (renderable is SkinRenderNode3D skinnedRenderable) {
                this._sceneManager.SetupBonesInfoBuffer(this._commandList, skinnedRenderable);
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