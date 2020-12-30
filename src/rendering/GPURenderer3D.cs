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
        [StructLayout(LayoutKind.Sequential)]
        struct CameraInfo
        {
            public Matrix4x4 viewMatrix;
            public Matrix4x4 projectionMatrix;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct LightInfo
        {
            public Vector3 ambientColor;
            private float _padding0;
            public Vector3 mainLightColor;
            private float _padding1;
            public Vector3 mainLightPosition;
            private float _padding2;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct ModelInfo
        {
            public Matrix4x4 modelMatrix;
        }

        private GraphicsDevice _graphicsDevice;
        
        private ResourceFactory _factory;
        private CommandList _commandList;

        private DeviceBuffer _lightInfoBuffer;
        private DeviceBuffer _cameraInfoBuffer;
        private DeviceBuffer _modelInfoBuffer;
        private DeviceBuffer _bonesInfoBuffer;

        private ResourceSet _globalInfoSet;

        private PipelineManager _pipelineManager;

        private BonesInfo _bonesInfo = BonesInfo.New();

        public GPURenderer3D(GraphicsDevice graphicsDevice, OutputDescription outputDescription)
        {
            this._graphicsDevice = graphicsDevice;
            this._factory = this._graphicsDevice.ResourceFactory;
            this._commandList = this._factory.CreateCommandList();

            this._modelInfoBuffer = this._factory.CreateBuffer(new BufferDescription((uint) Marshal.SizeOf<ModelInfo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            this._bonesInfoBuffer = this._factory.CreateBuffer(new BufferDescription(64 * BonesInfo.maxNumberOfBones, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            this._cameraInfoBuffer = this._factory.CreateBuffer(new BufferDescription((uint) Marshal.SizeOf<CameraInfo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            this._lightInfoBuffer  = this._factory.CreateBuffer(new BufferDescription((uint) Marshal.SizeOf<LightInfo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            this._pipelineManager = new PipelineManager(this._factory, outputDescription);

            this._globalInfoSet = this._factory.CreateResourceSet(
                new ResourceSetDescription(this._pipelineManager.globalInfoLayout, this._cameraInfoBuffer, this._lightInfoBuffer)
            );
        }

        public void Dispose()
        {
            this._commandList.Dispose();
            this._graphicsDevice.Dispose();

            this._cameraInfoBuffer.Dispose();
            this._globalInfoSet.Dispose();
        }

        private void _SetupCamera(Camera3D camera)
        {
            CameraInfo cameraInfo;
            cameraInfo.projectionMatrix = camera.projectionMatrix;
            cameraInfo.viewMatrix = camera.viewMatrix;
            this._graphicsDevice.UpdateBuffer(this._cameraInfoBuffer, 0, ref cameraInfo);

            this._commandList.ClearColorTarget(0, camera.clearColor);
            this._commandList.ClearDepthStencil(1f);
        }

        private List<Renderable3D> _renderList = new List<Renderable3D>();

        private void _UpdateRenderList(Container3D node)
        {
            if (node is Renderable3D renderable) {
                this._renderList.Add(renderable);
            }
            foreach (var child in node.children) {
                this._UpdateRenderList(child);
            }
        }

        private void _SetupLightInfo(Scene3D scene)
        {
            LightInfo lightInfo = new LightInfo();
            lightInfo.ambientColor = scene.ambientColor;
            lightInfo.mainLightColor = scene.sunColor;
            lightInfo.mainLightPosition = scene.sunPosition;
            this._commandList.UpdateBuffer(this._lightInfoBuffer, 0, ref lightInfo);
        }

        public void Render(MainRenderTexture renderTexture, Scene3D scene)
        {
            this._renderList.Clear();
            this._UpdateRenderList(scene);
            scene.UpdateWorldMatrices();


            this._commandList.Begin();
            this._commandList.SetFramebuffer(renderTexture.framebuffer);
            this._SetupLightInfo(scene);

            foreach (var camera in scene.cameras) {
                this._SetupCamera(camera);
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

            this._commandList.UpdateBuffer(this._modelInfoBuffer, 0, renderable.worldMatrix);

            this._commandList.SetVertexBuffer(0, mesh.vertexBuffer);
            this._commandList.SetIndexBuffer(mesh.indexBuffer, IndexFormat.UInt16);
            
            if (renderable is SkinnedRenderable3D skinnedRenderable) {
                skinnedRenderable.CopyMatricesToBuffer(ref this._bonesInfo);
                this._commandList.UpdateBuffer(this._bonesInfoBuffer, 0, this._bonesInfo.GetBlittable());
            }

            var pipeline = this._pipelineManager.GetPipeline(material.pass);
            this._commandList.SetPipeline(pipeline.pipeline);
            
            if (material.resourceSetIsDirty) {
                material.resourceSet?.Dispose();
                var arr = (material.pass.shader.isSkinned)
                    ? new BindableResource[] {this._modelInfoBuffer, material.texture.textureView, this._graphicsDevice.PointSampler, this._bonesInfoBuffer}
                    : new BindableResource[] {this._modelInfoBuffer, material.texture.textureView, this._graphicsDevice.PointSampler};

                var desc = new ResourceSetDescription(pipeline.resourceLayouts[1], arr);
                material.resourceSet = this._factory.CreateResourceSet(desc);
                material.resourceSetIsDirty = false;
            }

            this._commandList.SetGraphicsResourceSet(0, this._globalInfoSet);
            this._commandList.SetGraphicsResourceSet(1, material.resourceSet);
            this._commandList.DrawIndexed(
                indexCount: mesh.indexCount,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0
            );
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