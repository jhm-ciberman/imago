using System;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Imago.Textures;
using LifeSim.Imago.Materials;
using Veldrid;
using VeldridTexture = Veldrid.Texture;

namespace LifeSim.Imago.Rendering;

internal class FullScreenPass : IDisposable
{
    private readonly IRenderTexture _destinationTexture;

    private readonly GraphicsDevice _gd;

    private readonly Pipeline _pipeline;

    private readonly DeviceBuffer _vertexBuffer;

    private readonly ResourceLayout _resourceLayout;

    private readonly Dictionary<VeldridTexture, ResourceSet> _resourceSets = new();

    public FullScreenPass(Renderer renderer, bool isPixelArt = false)
    {
        this._gd = renderer.GraphicsDevice;
        var factory = this._gd.ResourceFactory;

        //this._sourceTexture = renderer.MainRenderTexture;
        //this._sourceTexture.Resized += (sender, args) => this.RegenerateResourceSet();

        this._destinationTexture = renderer.FullScreenRenderTexture;

        var vertexLayouts = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
        );

        this._resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("MainTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("MainSampler", ResourceKind.Sampler, ShaderStages.Fragment)
        ));

        var shaders = ShaderCompiler.CompileShaders(this._gd, _vertexCode, isPixelArt ? _pixelArtfragmentCode : _fragmentCode);

        this._pipeline = this._gd.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription
        {
            DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ShaderSet = new ShaderSetDescription([vertexLayouts], shaders),
            BlendState = BlendStateDescription.SingleAlphaBlend,
            RasterizerState = RasterizerStateDescription.CullNone,
            Outputs = this._destinationTexture.OutputDescription,
            ResourceLayouts = [this._resourceLayout],
        });

        this._vertexBuffer = factory.CreateBuffer(new BufferDescription(16 * 6, BufferUsage.VertexBuffer));
        var quadVertices = GetQuadVertices(this._gd.IsUvOriginTopLeft);
        this._gd.UpdateBuffer(this._vertexBuffer, 0, quadVertices);
    }

    private static Vector4[] GetQuadVertices(bool isUvOriginTopLeft)
    {
        (float top, float bottom) = isUvOriginTopLeft ? (1f, 0f) : (0f, 1f);
        return [
            new Vector4(-1f, -1f, 0f, top), // x, y, u, v
            new Vector4( 1f, -1f, 1f, top),
            new Vector4( 1f,  1f, 1f, bottom),

            new Vector4(-1f, -1f, 0f, top),
            new Vector4( 1f,  1f, 1f, bottom),
            new Vector4(-1f,  1f, 0f, bottom),
        ];
    }

    public void Dispose()
    {
        this._vertexBuffer.Dispose();
        this._resourceLayout.Dispose();
        this._pipeline.Dispose();

        foreach (var resourceSet in this._resourceSets.Values)
        {
            resourceSet.Dispose();
        }
    }

    public void Render(CommandList cl, IRenderTexture source)
    {
        cl.SetFramebuffer(this._destinationTexture.Framebuffer);
        cl.SetPipeline(this._pipeline);
        cl.SetVertexBuffer(0, this._vertexBuffer);

        var resourceSet = this.GetResourceSet(source.VeldridTexture);
        cl.SetGraphicsResourceSet(0, resourceSet);
        cl.Draw(6);
    }

    private ResourceSet GetResourceSet(VeldridTexture texture)
    {
        if (!this._resourceSets.TryGetValue(texture, out var resourceSet))
        {
            resourceSet = this._gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                this._resourceLayout, texture, this._gd.LinearSampler));
            this._resourceSets.Add(texture, resourceSet);
        }

        return resourceSet;
    }

    public void PruneResourceSets()
    {
        foreach (var texture in this._resourceSets.Keys)
        {
            if (texture.IsDisposed)
            {
                this._resourceSets[texture].Dispose();
                this._resourceSets.Remove(texture);
            }
        }
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

    private static readonly string _pixelArtfragmentCode = @"#version 450
            layout(location = 0) in vec2 fsin_TexCoords;
            layout(set = 0, binding = 0) uniform texture2D MainTexture;
            layout(set = 0, binding = 1) uniform sampler MainSampler;

            layout(location = 0) out vec4 fsout_Color;

            void main() {
                vec2 textureSize = vec2(textureSize(sampler2D(MainTexture, MainSampler), 0));

                // calculate the box filter size in texel units
                vec2 boxSize = clamp(fwidth(fsin_TexCoords) * textureSize, vec2(1e-5), vec2(1.0));

                // scale UV by texture size to get texel coordinate
                vec2 tx = fsin_TexCoords * textureSize - 0.5 * boxSize;

                // compute offset for pixel-sized box filter
                vec2 txOffset = clamp((fract(tx) - (vec2(1.0) - boxSize)) / boxSize, vec2(0.0), vec2(1.0));

                // compute bilinear sample UV coordinates
                vec2 uv = (floor(tx) + vec2(0.5) + txOffset) / textureSize;

                vec4 color = texture(sampler2D(MainTexture, MainSampler), uv);
                fsout_Color = color;
            }";
}
