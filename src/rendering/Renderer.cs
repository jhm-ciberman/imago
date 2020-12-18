using System.Text;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;

namespace LifeSim.Rendering
{
    public class Renderer
    {
        private GraphicsDevice _graphicsDevice;
        private ResourceFactory _factory;

        private CommandList _commandList;

        private ResourceLayout _projViewLayout;
        private DeviceBuffer _viewBuffer;
        private DeviceBuffer _projectionBuffer;
        private ResourceSet _projViewSet;

        public Renderer(Window window)
        {
            GraphicsDeviceOptions options = new GraphicsDeviceOptions(
                debug: false,
                swapchainDepthFormat: PixelFormat.R16_UNorm,
                syncToVerticalBlank: true,
                resourceBindingModel: ResourceBindingModel.Improved,
                preferDepthRangeZeroToOne: true,
                preferStandardClipSpaceYDirection: true
            );

            this._graphicsDevice = VeldridStartup.CreateGraphicsDevice(window.nativeWindow, options);

            System.Console.Write(this._graphicsDevice.BackendType);
            this._factory = this._graphicsDevice.ResourceFactory;
            this._commandList = this._factory.CreateCommandList();

            this._projViewLayout = this._factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ProjectionBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("ViewBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
            ));
            
            this._viewBuffer = this._factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            this._projectionBuffer = this._factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));

            this._projViewSet = this._factory.CreateResourceSet(
                new ResourceSetDescription(this._projViewLayout, this._projectionBuffer, this._viewBuffer)
            );
        }

        public void Dispose()
        {
            this._commandList.Dispose();
            this._graphicsDevice.Dispose();
            this._viewBuffer.Dispose();
            this._projectionBuffer.Dispose();
            this._projViewSet.Dispose();
        }

        public Material MakeMaterial(Shader shader)
        {
            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
            pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;

            pipelineDescription.DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual;

            //pipelineDescription.RasterizerState = RasterizerStateDescription.Default;
            
            pipelineDescription.RasterizerState = new RasterizerStateDescription(
                cullMode: FaceCullMode.None,
                fillMode: PolygonFillMode.Solid,
                frontFace: FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false
            );

            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
            pipelineDescription.ResourceLayouts = System.Array.Empty<ResourceLayout>();

            pipelineDescription.ShaderSet = shader.description;
            pipelineDescription.ResourceLayouts = new[] { this._projViewLayout };
        
            pipelineDescription.Outputs = this._graphicsDevice.SwapchainFramebuffer.OutputDescription;

            var pipeline = this._factory.CreateGraphicsPipeline(pipelineDescription);

            return new Material(pipeline);
        }

        public Shader MakeShader(string vertexCode, string fragmentCode)
        {
            var vertBytes = Encoding.UTF8.GetBytes(vertexCode);
            var fragBytes = Encoding.UTF8.GetBytes(fragmentCode);
            ShaderDescription vertexShaderDesc = new ShaderDescription(ShaderStages.Vertex, vertBytes, "main");
            ShaderDescription fragmentShaderDesc = new ShaderDescription(ShaderStages.Fragment, fragBytes, "main");

            var shaders = this._factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

            VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("uv", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
            );

            var description = new ShaderSetDescription(
                vertexLayouts: new VertexLayoutDescription[] { vertexLayout },
                shaders: shaders
            );
            
            return new Shader(description);
        }

        public Mesh MakeMesh(Mesh.VertData[] vertices, ushort[] indices)
        {
            return new Mesh(this._factory, this._graphicsDevice, vertices, indices);
        }

        public void DrawBegin()
        {
            this._commandList.Begin();
            this._commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
            this._commandList.ClearColorTarget(0, RgbaFloat.Black);
            this._commandList.ClearDepthStencil(1f);
        }

        public void DrawMesh(Mesh mesh, Material material, Camera camera)
        {
            camera.UpdateMatrices();
            this._graphicsDevice.UpdateBuffer(this._viewBuffer, 0, ref camera.viewMatrix);
            this._graphicsDevice.UpdateBuffer(this._projectionBuffer, 0, ref camera.projectionMatrix);

            this._commandList.SetVertexBuffer(0, mesh.vertexBuffer);
            this._commandList.SetIndexBuffer(mesh.indexBuffer, IndexFormat.UInt16);
            this._commandList.SetPipeline(material.pipeline);
            this._commandList.SetGraphicsResourceSet(0, this._projViewSet);
            this._commandList.DrawIndexed(
                indexCount: mesh.indexCount,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0
            );
        }

        public void DrawEnd()
        {
            this._commandList.End();
            this._graphicsDevice.SubmitCommands(_commandList);
            this._graphicsDevice.SwapBuffers();
        }
    }
}