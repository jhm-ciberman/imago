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
        public void Render(Scene3D scene)
        {
            this._currentPipeline = null;
            this._currentMesh = null;
            this._currentPass = null;
            this._currentMaterial = null;

            var camera = scene.activeCamera;
            if (camera != null) {
                scene.UpdateWorldMatrices();
                this._baseRQ.Update(scene, camera);
                this._shadowmapRQ.Update(scene, scene.mainLight, camera);

                //if (! Input.GetKey(Key.Space)) {
                    this._baseRQ.Sort();
                    this._shadowmapRQ.Sort();
                //}
            }

            //System.Console.WriteLine(this._baseRQ.count + " sh: " + this._shadowmapRQ.count);

            this._commandList.Begin();

            if (camera != null) {
                // Shadowmap
                this.frameProfilerShadowmap.BeginFrame();
                this._commandList.SetFramebuffer(this._sceneManager.shadowmapFramebuffer);
                this._commandList.ClearDepthStencil(1f);
                this._sceneManager.SetupShadowMapBuffer(this._commandList, camera, scene.mainLight);
                foreach (var renderable in this._shadowmapRQ) {
                    if (renderable.material == null) continue;
                    this._DrawRenderable(renderable, renderable.material.shadowmapPass, this.frameProfilerShadowmap);
                }
                this.frameProfilerShadowmap.EndFrame();
            }

            // Opaques
            this._commandList.SetFramebuffer(this._renderTexture.framebuffer);
            this._sceneManager.SetupLightInfoBuffer(this._commandList, scene);

            this._commandList.ClearColorTarget(0, scene.clearColor);
            this._commandList.ClearColorTarget(1, RgbaFloat.Black);

            if (camera != null) {
                this.frameProfilerBase.BeginFrame();
                this._commandList.ClearDepthStencil(1f);
                this._sceneManager.SetupCamera3DInfoBuffer(this._commandList, camera, scene.mainLight);
                foreach (var renderable in this._baseRQ) {
                    if (renderable.material == null) continue;
                    this._DrawRenderable(renderable, renderable.material.pass, this.frameProfilerBase);
                }
                this.frameProfilerBase.EndFrame();
            }
            this._commandList.End();
            this._hasCommandsToSubmit = true;
        }

        public void _DrawRenderable(Renderable3D renderable, Pass pass, FrameProfiler frameProfiler)
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
                this._currentPipeline = pipeline;
                this._currentPass = null;
                this._currentMaterial = null;
                frameProfiler.ChangePipeline(pipeline);
            }

            if (this._currentPass != pass) {
                this._commandList.SetGraphicsResourceSet(0, pass.resourceSet); // Per pass
                this._currentPass = pass;
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

        private void _UpdatePerObjectBuffers(Renderable3D renderable)
        {
            this._sceneManager.SetupObjectInfoBuffer(this._commandList, renderable);
            if (renderable is SkinnedRenderable3D skinnedRenderable) {
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