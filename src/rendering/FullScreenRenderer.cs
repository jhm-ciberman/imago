using System.IO;
using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace LifeSim.Rendering
{
    public class FullScreenRenderer : System.IDisposable
    {
        private GraphicsDevice _gd;

        private DeviceBuffer _vertexBuffer;
        private Veldrid.Shader _vertexShader;
        private Veldrid.Shader _fragmentShader;
        private Pipeline _pipeline;
        private ResourceLayout _resourceLayout;
        private ResourceSet _resourceSet;
        private CommandList _commandList;

        private Framebuffer _outputFramebuffer;

        public FullScreenRenderer(GraphicsDevice gd, Framebuffer outputFramebuffer, MainRenderTexture renderTexture)
        {
            this._gd = gd;
            this._outputFramebuffer = outputFramebuffer;

            var factory = gd.ResourceFactory;


            string vertexCode   = File.ReadAllText("res/shaders/fullscreen.vert");
            string fragmentCode = File.ReadAllText("res/shaders/fullscreen.frag");
            var vertBytes = Encoding.UTF8.GetBytes(vertexCode);
            var fragBytes = Encoding.UTF8.GetBytes(fragmentCode);
            ShaderDescription vertexShaderDesc = new ShaderDescription(ShaderStages.Vertex, vertBytes, "main");
            ShaderDescription fragmentShaderDesc = new ShaderDescription(ShaderStages.Fragment, fragBytes, "main");
            var shaders = factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);
            this._vertexShader = shaders[0];
            this._fragmentShader = shaders[1];

            this._resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(new [] {
                new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment),
            }));

            var vertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
            );

            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
            pipelineDescription.ShaderSet = new ShaderSetDescription(new [] { vertexLayout }, shaders);
            pipelineDescription.BlendState = BlendStateDescription.SingleAlphaBlend;
            pipelineDescription.DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual;
            pipelineDescription.RasterizerState = RasterizerStateDescription.CullNone;
            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
            pipelineDescription.ResourceLayouts = new ResourceLayout[] { this._resourceLayout };
            pipelineDescription.Outputs = outputFramebuffer.OutputDescription;

            this._pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

            this._vertexBuffer = factory.CreateBuffer(new BufferDescription(16 * 6, BufferUsage.VertexBuffer));
            gd.UpdateBuffer(this._vertexBuffer, 0, new[] {
                new Vector4(-1f, -1f, 0f, 1f), // x, y, u, v
                new Vector4( 1f, -1f, 1f, 1f),
                new Vector4( 1f,  1f, 1f, 0f),

                new Vector4(-1f, -1f, 0f, 1f),
                new Vector4( 1f,  1f, 1f, 0f),
                new Vector4(-1f,  1f, 0f, 0f),
            });
            this._commandList = factory.CreateCommandList();

            this._resourceSet = this._CreateResourceSet(renderTexture);
        }

        public void Dispose()
        {
            this._vertexBuffer.Dispose();
            this._vertexShader.Dispose();
            this._fragmentShader.Dispose();
            this._pipeline.Dispose();
            this._resourceLayout.Dispose();
            this._resourceSet.Dispose();
            this._commandList.Dispose();
        }

        private ResourceSet _CreateResourceSet(MainRenderTexture renderTexture)
        {
            return this._gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                this._resourceLayout, renderTexture.colorTexture, this._gd.LinearSampler
            ));
        }

        public void SetTexture(MainRenderTexture renderTexture)
        {
            this._resourceSet.Dispose();
            this._resourceSet = this._CreateResourceSet(renderTexture);
        }

        public void Render()
        {
            this._commandList.Begin();
            this._commandList.SetFramebuffer(this._outputFramebuffer);
            this._commandList.SetPipeline(this._pipeline);
            this._commandList.SetVertexBuffer(0, this._vertexBuffer);
            this._commandList.SetGraphicsResourceSet(0, this._resourceSet);
            this._commandList.Draw(6);
            this._commandList.End();

            this._gd.SubmitCommands(this._commandList);
        }
    }
}