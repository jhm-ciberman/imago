using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Veldrid;
using Veldrid.ImageSharp;
using Veldrid.StartupUtilities;
using Veldrid.SPIRV;
using System.Collections.Generic;
using System.Diagnostics;
using LifeSim.SceneGraph;

namespace LifeSim.Rendering
{
    public class GPURenderer3D
    {
        private GraphicsDevice _graphicsDevice;
        
        private ResourceFactory _factory;
        private CommandList _commandList;

        private BonesInfo _bonesInfo = BonesInfo.New();

        private IRenderTexture _renderTexture;
        private SceneContext _sceneContext;

        private List<Renderable3D> _renderList = new List<Renderable3D>();

        public GPURenderer3D(GraphicsDevice graphicsDevice, SceneContext sceneContext, IRenderTexture renderTexture)
        {
            this._graphicsDevice = graphicsDevice;
            this._renderTexture = renderTexture;
            this._factory = this._graphicsDevice.ResourceFactory;
            this._sceneContext = sceneContext;
            this._commandList = this._factory.CreateCommandList();
        }

        public void Dispose()
        {
            this._commandList.Dispose();
        }

        private void _UpdateRenderList(Container3D node)
        {
            if (node is Renderable3D renderable) {
                this._renderList.Add(renderable);
            }
            foreach (var child in node.children) {
                this._UpdateRenderList(child);
            }
        }

        public void Render(Scene3D scene)
        {
            this._renderList.Clear();
            this._UpdateRenderList(scene);
            scene.UpdateWorldMatrices();


            this._commandList.Begin();
            this._commandList.SetFramebuffer(this._renderTexture.framebuffer);
            this._sceneContext.SetupLightInfoBuffer(this._commandList, scene);

            foreach (var camera in scene.cameras) {
                this._commandList.ClearColorTarget(0, camera.clearColor);
                this._commandList.ClearDepthStencil(1f);
                this._sceneContext.SetupCameraInfoBuffer(this._commandList, camera);
                foreach (var renderable in this._renderList) {
                    this._DrawRenderable(renderable, camera);
                }

                this._commandList.ClearDepthStencil(1f);
            }
            this._commandList.End();
        }

        public void _DrawRenderable(Renderable3D renderable, Camera3D camera)
        {
            var mesh = renderable.mesh;
            var material = renderable.material;
            var pass = material.pass;

            this._UpdatePerObjectBuffers(renderable);
            var objectResourceSet = this._sceneContext.GetObjectResourceSet(renderable);

            this._commandList.SetVertexBuffer(0, mesh.vertexBuffer);
            this._commandList.SetIndexBuffer(mesh.indexBuffer, IndexFormat.UInt16);
            this._commandList.SetPipeline(pass.pipeline);
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
            this._commandList.UpdateBuffer(this._sceneContext.modelInfoBuffer, 0, renderable.worldMatrix);
            if (renderable is SkinnedRenderable3D skinnedRenderable) {
                skinnedRenderable.CopyMatricesToBuffer(ref this._bonesInfo);
                this._commandList.UpdateBuffer(this._sceneContext.bonesInfoBuffer, 0, this._bonesInfo.GetBlittable());
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