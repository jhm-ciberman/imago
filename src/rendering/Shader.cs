using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace LifeSim.Rendering
{
    public class Shader : System.IDisposable
    {
        private readonly Veldrid.Shader _vertexShader;
        private readonly Veldrid.Shader _fragmentShader;
        private readonly ResourceLayout _worldTextureLayout;
        private readonly Pipeline _pipeline;
        public Pipeline pipeline => this._pipeline;

        public ResourceLayout worldTextureLayout => this._worldTextureLayout;

        public static ResourceLayout MakeProjectionViewResourceLayout(ResourceFactory factory)
        {
            return factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ProjectionBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("ViewBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
            ));
        }

        public Shader(ResourceFactory factory, Framebuffer framebuffer, ResourceLayout projectionViewLayout, string vertexCode, string fragmentCode)
        {
            var vertBytes = Encoding.UTF8.GetBytes(vertexCode);
            var fragBytes = Encoding.UTF8.GetBytes(fragmentCode);
            ShaderDescription vertexShaderDesc = new ShaderDescription(ShaderStages.Vertex, vertBytes, "main");
            ShaderDescription fragmentShaderDesc = new ShaderDescription(ShaderStages.Fragment, fragBytes, "main");
            
            var shaders = factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);
            this._vertexShader = shaders[0];
            this._fragmentShader = shaders[1];

            this._worldTextureLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment)
            ));

            var vertexLayoutDesc = new VertexLayoutDescription(
                new VertexElementDescription("position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("uv", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
            );

            var rasterizerState = new RasterizerStateDescription(
                FaceCullMode.Front,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false
            );

            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
            pipelineDescription.ShaderSet = new ShaderSetDescription(new [] { vertexLayoutDesc }, shaders);
            pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;
            pipelineDescription.DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual;
            pipelineDescription.RasterizerState = rasterizerState;
            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
            pipelineDescription.ResourceLayouts = new[] { projectionViewLayout, this._worldTextureLayout };
            pipelineDescription.Outputs = framebuffer.OutputDescription; 

            this._pipeline = factory.CreateGraphicsPipeline(pipelineDescription);
        }

        public void Dispose()
        {
            System.Console.WriteLine("Dispose Shader");
            this._worldTextureLayout.Dispose();
            this._vertexShader.Dispose();
            this._fragmentShader.Dispose();
            this._pipeline.Dispose();
        }
    }
}