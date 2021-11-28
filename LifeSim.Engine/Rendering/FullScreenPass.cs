using System;
using System.Diagnostics;
using System.Numerics;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class FullScreenPass : IDisposable, IPipelineProvider
    {
        private ResourceSet? _resourceSet;

        private IRenderTexture _sourceTexture;

        private readonly IRenderTexture _destinationTexture;

        private bool _resourceSetDirty = true;

        public Shader Shader { get; private set; }

        private readonly GraphicsDevice _gd;
        private readonly Pipeline _pipeline;
        private readonly DeviceBuffer _vertexBuffer;

        public FullScreenPass(GraphicsDevice gd, IRenderTexture sourceRenderTexture, IRenderTexture destinationRenderTexture)
        {
            this._gd = gd;
            var factory = gd.ResourceFactory;

            this._sourceTexture = sourceRenderTexture;
            this._sourceTexture.OnResized += this._OnSourceTextureResized;

            this._destinationTexture = destinationRenderTexture;

            var vertexFormat = new VertexFormat(new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
            ));

            var resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("MainTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("MainSampler", ResourceKind.Sampler, ShaderStages.Fragment)
            ));

            this.Shader = new Shader(this, _vertexCode, _fragmentCode, resourceLayout);


            this._pipeline = this.Shader.GetPipeline(vertexFormat);

            this._vertexBuffer = factory.CreateBuffer(new BufferDescription(16 * 6, BufferUsage.VertexBuffer));
            (float top, float bottom) = gd.IsUvOriginTopLeft ? (1f, 0f) : (0f, 1f);
            gd.UpdateBuffer(this._vertexBuffer, 0, new[] {
                new Vector4(-1f, -1f, 0f, top   ), // x, y, u, v
                new Vector4( 1f, -1f, 1f, top   ),
                new Vector4( 1f,  1f, 1f, bottom),

                new Vector4(-1f, -1f, 0f, top   ),
                new Vector4( 1f,  1f, 1f, bottom),
                new Vector4(-1f,  1f, 0f, bottom),
            });
        }


        public void Dispose()
        {
            this._resourceSet?.Dispose();
            this._vertexBuffer.Dispose();
        }

        public void Render(CommandList cl)
        {
            if (this._resourceSetDirty || this._resourceSet == null)
            {
                this._resourceSetDirty = false;
                this._resourceSet?.Dispose();
                this._resourceSet = this.Shader.CreateResourceSet(this._sourceTexture.DeviceTexture, this._gd.LinearSampler);
            }

            cl.SetFramebuffer(this._destinationTexture.Framebuffer);
            cl.SetPipeline(this._pipeline);
            cl.SetVertexBuffer(0, this._vertexBuffer);
            cl.SetGraphicsResourceSet(0, this._resourceSet);
            cl.Draw(6);
        }

        public void SetSourceTexture(IRenderTexture sourceRenderTexture)
        {
            if (this._sourceTexture == sourceRenderTexture) return;

            this._sourceTexture.OnResized -= this._OnSourceTextureResized;
            this._sourceTexture = sourceRenderTexture;
            this._sourceTexture.OnResized += this._OnSourceTextureResized;
            this._resourceSetDirty = true;
        }

        private void _OnSourceTextureResized(IRenderTexture renderTexture)
        {
            this._resourceSetDirty = true;
        }

        Pipeline IPipelineProvider.MakePipeline(ShaderVariant shaderVariant)
        {
            Debug.Assert(shaderVariant.MaterialResourceLayout != null);

            var rasterizerState = new RasterizerStateDescription(
                FaceCullMode.None,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: true
            );

            return this._gd.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription()
            {
                DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ShaderSet = shaderVariant.ShaderSetDescription,
                BlendState = BlendStateDescription.SingleOverrideBlend,
                RasterizerState = rasterizerState,
                Outputs = this._destinationTexture.OutputDescription,
                ResourceLayouts = new ResourceLayout[] { shaderVariant.MaterialResourceLayout },
            });
        }

        private string _vertexCode => @"#version 450
            layout(location = 0) in vec4 Position; // xy = position, zw = uv

            layout(location = 0) out vec2 fsin_TexCoords;

            void main()
            {
                gl_Position = vec4(Position.xy, 0, 1);
                fsin_TexCoords = Position.zw;
            }";

        private string _fragmentCode => @"#version 450
            layout(location = 0) in vec2 fsin_TexCoords;

            layout(set = 0, binding = 0) uniform texture2D MainTexture;
            layout(set = 0, binding = 1) uniform sampler MainSampler;

            layout(location = 0) out vec4 fsout_color;

            void main()
            {
                fsout_color = texture(sampler2D(MainTexture, MainSampler), fsin_TexCoords);
            }";
    }
}