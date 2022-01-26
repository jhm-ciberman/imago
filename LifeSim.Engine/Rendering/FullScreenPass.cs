using System;
using System.Diagnostics;
using System.Numerics;
using Veldrid;

namespace LifeSim.Engine.Rendering;

public class FullScreenPass : IDisposable
{
    private ResourceSet? _resourceSet;

    private readonly IRenderTexture _sourceTexture;

    private readonly IRenderTexture _destinationTexture;

    private readonly GraphicsDevice _gd;
    private readonly Pipeline _pipeline;
    private readonly DeviceBuffer _vertexBuffer;

    private readonly ResourceLayout _resourceLayout;

    public FullScreenPass(Renderer renderer, IRenderTexture sourceRenderTexture, IRenderTexture destinationRenderTexture)
    {
        this._gd = renderer.GraphicsDevice;
        var factory = this._gd.ResourceFactory;

        this._sourceTexture = sourceRenderTexture;
        this._sourceTexture.OnResized += this.OnSourceTextureResized;

        this._destinationTexture = destinationRenderTexture;

        var vertexFormat = new VertexFormat(new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
        ));

        this._resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("MainTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("MainSampler", ResourceKind.Sampler, ShaderStages.Fragment)
        ));

        var shaderVariant = new ShaderVariant(this._gd, vertexFormat, this._resourceLayout, _vertexCode, _fragmentCode);
        this._pipeline = this.MakePipeline(shaderVariant);

        this._vertexBuffer = factory.CreateBuffer(new BufferDescription(16 * 6, BufferUsage.VertexBuffer));
        (float top, float bottom) = this._gd.IsUvOriginTopLeft ? (1f, 0f) : (0f, 1f);
        this._gd.UpdateBuffer(this._vertexBuffer, 0, new[] {
            new Vector4(-1f, -1f, 0f, top   ), // x, y, u, v
            new Vector4( 1f, -1f, 1f, top   ),
            new Vector4( 1f,  1f, 1f, bottom),

            new Vector4(-1f, -1f, 0f, top   ),
            new Vector4( 1f,  1f, 1f, bottom),
            new Vector4(-1f,  1f, 0f, bottom),
        });

        this.OnSourceTextureResized(this._sourceTexture);
    }


    public void Dispose()
    {
        this._resourceSet?.Dispose();
        this._vertexBuffer.Dispose();
        this._resourceLayout.Dispose();
    }

    public void Render(CommandList cl)
    {
        cl.SetFramebuffer(this._destinationTexture.Framebuffer);
        cl.SetPipeline(this._pipeline);
        cl.SetVertexBuffer(0, this._vertexBuffer);
        cl.SetGraphicsResourceSet(0, this._resourceSet);
        cl.Draw(6);
    }

    private void OnSourceTextureResized(IRenderTexture renderTexture)
    {
        this._resourceSet?.Dispose();
        this._resourceSet = this._gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
            this._resourceLayout, renderTexture.DeviceTexture, this._gd.LinearSampler));
    }

    private Pipeline MakePipeline(ShaderVariant shaderVariant)
    {
        Debug.Assert(shaderVariant.MaterialResourceLayout != null);

        var rasterizerState = new RasterizerStateDescription(
                FaceCullMode.None,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false
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

    private static readonly string _vertexCode = @"#version 450
            layout(location = 0) in vec4 Position; // xy = position, zw = uv

            layout(location = 0) out vec2 fsin_TexCoords;

            void main()
            {
                gl_Position = vec4(Position.xy, 0, 1);
                fsin_TexCoords = Position.zw;
            }";

    private static readonly string _fragmentCode = @"#version 450
            layout(location = 0) in vec2 fsin_TexCoords;

            layout(set = 0, binding = 0) uniform texture2D MainTexture;
            layout(set = 0, binding = 1) uniform sampler MainSampler;

            layout(location = 0) out vec4 fsout_Color;

            void main()
            {
                fsout_Color = texture(sampler2D(MainTexture, MainSampler), fsin_TexCoords);
            }";
}