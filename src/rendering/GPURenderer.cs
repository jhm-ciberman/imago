using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Veldrid;
using Veldrid.ImageSharp;
using Veldrid.StartupUtilities;
using Veldrid.SPIRV;

namespace LifeSim.Rendering
{
    public class GPURenderer
    {
        [StructLayout(LayoutKind.Sequential)]
        struct CameraInfo
        {
            public Matrix4x4 viewMatrix;
            public Matrix4x4 projectionMatrix;
        }

        private GraphicsDevice _graphicsDevice;
        private ResourceFactory _factory;
        private Swapchain _swapchain;

        private CommandList _commandList;

        private DeviceBuffer _worldBuffer;

        private DeviceBuffer _cameraInfoBuffer;
        private ResourceSet _cameraInfoSet;

        private PipelineManager _pipelineManager;

        public GPURenderer(Window window)
        {
            GraphicsDeviceOptions options = new GraphicsDeviceOptions(
                debug: false,
                swapchainDepthFormat: PixelFormat.R16_UNorm,
                syncToVerticalBlank: false,
                resourceBindingModel: ResourceBindingModel.Improved,
                preferDepthRangeZeroToOne: true,
                preferStandardClipSpaceYDirection: true
            );

            this._graphicsDevice = VeldridStartup.CreateGraphicsDevice(window.nativeWindow, options, GraphicsBackend.Vulkan);


            this._factory = this._graphicsDevice.ResourceFactory;
            this._commandList = this._factory.CreateCommandList();
            this._swapchain = this._graphicsDevice.MainSwapchain;
            this._worldBuffer = this._factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));


            this._cameraInfoBuffer = this._factory.CreateBuffer(new BufferDescription(128, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            this._pipelineManager = new PipelineManager(this._factory, this._swapchain.Framebuffer);

            this._cameraInfoSet = this._factory.CreateResourceSet(
                new ResourceSetDescription(this._pipelineManager.cameraInfoLayout, this._cameraInfoBuffer)
            );
        }

        public GPUTexture MakeTexture(string path)
        {
            ImageSharpTexture texture = new ImageSharpTexture(path, true);
            var deviceTexture = texture.CreateDeviceTexture(this._graphicsDevice, this._factory);
            var textureView = this._factory.CreateTextureView(deviceTexture);
            
            return new GPUTexture(deviceTexture, textureView);
        }

        public GraphicsBackend backendType => this._graphicsDevice.BackendType;

        public void Dispose()
        {
            this._commandList.Dispose();
            this._swapchain.Dispose();
            this._graphicsDevice.Dispose();

            this._cameraInfoBuffer.Dispose();
            this._cameraInfoSet.Dispose();
        }

        public Shader MakeShader(string vertexCode, string fragmentCode)
        {
            var vertBytes = Encoding.UTF8.GetBytes(vertexCode);
            var fragBytes = Encoding.UTF8.GetBytes(fragmentCode);
            ShaderDescription vertexShaderDesc = new ShaderDescription(ShaderStages.Vertex, vertBytes, "main");
            ShaderDescription fragmentShaderDesc = new ShaderDescription(ShaderStages.Fragment, fragBytes, "main");
            var shaders = this._factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);
            return new Shader(shaders);
        }

        public GPUMesh MakeMesh(LifeSim.Mesh meshData)
        {
            return new GPUMesh(this._factory, this._graphicsDevice, meshData);
        }

        private void _SetupCamera(Camera camera)
        {
            CameraInfo cameraInfo;
            cameraInfo.projectionMatrix = camera.projectionMatrix;
            cameraInfo.viewMatrix = camera.viewMatrix;
            this._graphicsDevice.UpdateBuffer(this._cameraInfoBuffer, 0, ref cameraInfo);

            this._commandList.Begin();
            this._commandList.SetFramebuffer(this._swapchain.Framebuffer);
            this._commandList.ClearColorTarget(0, new RgbaFloat(0.04f, 0.04f, 0.06f, 1.0f));
            this._commandList.ClearDepthStencil(1f);
        }

        public void Render(Scene scene)
        {
            foreach (var camera in scene.cameras) {
                this._SetupCamera(camera);
                var renderables = scene.renderables;
                foreach (var renderable in scene.renderables) {
                    this._DrawRenderable(renderable, camera);
                }
                this._DrawEnd();
            }
        }

        public void _DrawRenderable(Renderable renderable, Camera camera)
        {
            var mesh = renderable.mesh;
            var material = renderable.material;

            this._commandList.UpdateBuffer(this._worldBuffer, 0, renderable.transform.GetTransformMatrix());
            this._commandList.SetVertexBuffer(0, mesh.vertexBuffer);
            this._commandList.SetIndexBuffer(mesh.indexBuffer, IndexFormat.UInt16);
            
            var pipeline = this._pipelineManager.GetPipeline(material.pass);
            this._commandList.SetPipeline(pipeline.pipeline);
            
            if (material.resourceSetIsDirty) {
                material.resourceSet?.Dispose();
                var desc = new ResourceSetDescription(pipeline.resourceLayouts[1], this._worldBuffer, material.texture.textureView, this._graphicsDevice.PointSampler);
                material.resourceSet = this._factory.CreateResourceSet(desc);
                material.resourceSetIsDirty = false;
            }

            this._commandList.SetGraphicsResourceSet(0, this._cameraInfoSet);
            this._commandList.SetGraphicsResourceSet(1, material.resourceSet);
            this._commandList.DrawIndexed(
                indexCount: mesh.indexCount,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0
            );
        }

        private void _DrawEnd()
        {
            this._commandList.End();
            this._graphicsDevice.SubmitCommands(this._commandList);
            this._graphicsDevice.SwapBuffers(this._swapchain);
        }

        internal void Resize(uint width, uint height)
        {
            this._graphicsDevice.ResizeMainWindow(width, height);
        }

        ~GPURenderer() {
            this.Dispose();
        }
    }
}