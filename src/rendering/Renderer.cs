using System;
using System.Text;
using Veldrid;
using Veldrid.ImageSharp;
using Veldrid.Sdl2;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;

namespace LifeSim.Rendering
{
    public class Renderer
    {
        private GraphicsDevice _graphicsDevice;
        private ResourceFactory _factory;
        private Swapchain _swapchain;

        private CommandList _commandList;

        private ResourceLayout _projectionViewLayout;

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

            this._projectionViewLayout = Shader.MakeProjectionViewResourceLayout(this._factory);
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
        }

        public Material MakeMaterial(Shader shader, Texture texture)
        {
            return new Material(this._factory, this._graphicsDevice, shader, texture);
        }

        public Shader MakeShader(string vertexCode, string fragmentCode)
        {
            return new Shader(this._factory, this._swapchain.Framebuffer, this._projectionViewLayout, vertexCode, fragmentCode);
        }

        public Mesh MakeMesh(Mesh.VertData[] vertices, ushort[] indices)
        {
            return new Mesh(this._factory, this._graphicsDevice, vertices, indices);
        }

        public Camera MakeCamera()
        {
            return new Camera(this._factory, this._projectionViewLayout);
        }

        public void Render(Scene scene)
        {
            var camera = scene.camera;
            camera.UpdateMatrices(this._graphicsDevice);
            this._DrawBegin();
            var renderables = scene.renderables;
            for (int i = 0; i < renderables.Count; i++) {
                this._DrawRenderable(renderables[i], camera);
            }
            this._DrawEnd();
        }

        private void _DrawBegin()
        {
            this._commandList.Begin();
            this._commandList.SetFramebuffer(this._swapchain.Framebuffer);
            this._commandList.ClearColorTarget(0, RgbaFloat.Black);
            this._commandList.ClearDepthStencil(1f);
        }

        public void _DrawRenderable(Renderable renderable, Camera camera)
        {
            var mesh = renderable.mesh;
            var material = renderable.material;

            this._commandList.SetVertexBuffer(0, mesh.vertexBuffer);
            this._commandList.SetIndexBuffer(mesh.indexBuffer, IndexFormat.UInt16);
            this._commandList.SetPipeline(material.pipeline);
            this._commandList.SetGraphicsResourceSet(0, camera.projectionViewSet);
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
    }
}