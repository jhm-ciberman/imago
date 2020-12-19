using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.ImageSharp;
using Veldrid.StartupUtilities;

namespace LifeSim.Rendering
{
    public class Renderer
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

        private ResourceLayout _cameraInfoLayout;
        private DeviceBuffer _cameraInfoBuffer;
        private ResourceSet _cameraInfoSet;

        public Renderer(Window window)
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

            this._cameraInfoLayout = this._factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("CameraInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex)
            ));
            this._cameraInfoBuffer = this._factory.CreateBuffer(new BufferDescription(128, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            this._cameraInfoSet = this._factory.CreateResourceSet(
                new ResourceSetDescription(this._cameraInfoLayout, this._cameraInfoBuffer)
            );

        }

        public Texture MakeTexture(string path)
        {
            ImageSharpTexture texture = new ImageSharpTexture(path, true);
            var deviceTexture = texture.CreateDeviceTexture(this._graphicsDevice, this._factory);
            var textureView = this._factory.CreateTextureView(deviceTexture);
            
            return new Texture(deviceTexture, textureView);
        }

        public GraphicsBackend backendType => this._graphicsDevice.BackendType;

        public void Dispose()
        {
            this._commandList.Dispose();
            this._swapchain.Dispose();
            this._graphicsDevice.Dispose();

            this._cameraInfoLayout.Dispose();
            this._cameraInfoBuffer.Dispose();
            this._cameraInfoSet.Dispose();
        }

        public Material MakeMaterial(Shader shader, Texture texture)
        {
            return new Material(this._factory, this._graphicsDevice, shader, this._worldBuffer, texture);
        }

        public Shader MakeShader(string vertexCode, string fragmentCode)
        {
            return new Shader(this._factory, this._swapchain.Framebuffer, this._cameraInfoLayout, vertexCode, fragmentCode);
        }

        public Mesh MakeMesh(MeshData meshData)
        {
            return new Mesh(this._factory, this._graphicsDevice, meshData);
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
            this._commandList.SetPipeline(material.pipeline);
            this._commandList.SetGraphicsResourceSet(0, this._cameraInfoSet);
            this._commandList.SetGraphicsResourceSet(1, material.textureSet);
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

        ~Renderer() {
            this.Dispose();
        }
    }
}