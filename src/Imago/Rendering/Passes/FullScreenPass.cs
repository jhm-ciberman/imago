using System;
using System.Collections.Generic;
using System.Numerics;
using Imago.Assets.Materials;
using Imago.Assets.Textures;
using NeoVeldrid;
using NativeTexture = NeoVeldrid.Texture;

namespace Imago.Rendering.Passes;

internal class FullScreenPass : IDisposable
{
    private readonly GraphicsDevice _gd;

    private readonly DeviceBuffer _vertexBuffer;

    private readonly ResourceLayout _resourceLayout;

    private readonly VertexLayoutDescription _vertexLayout;

    private readonly NeoVeldrid.Shader[] _shaders;

    private readonly Dictionary<OutputDescription, Pipeline> _pipelines = new();

    private readonly Dictionary<NativeTexture, ResourceSet> _resourceSets = new();

    private readonly BlendStateDescription _blendState;

    public FullScreenPass(Renderer renderer, bool isPixelArt = false, bool isOverlay = false)
    {
        // An overlay blends over whatever is already on the target. A base layer instead replaces the
        // target outright, so its alpha (which transparent draws may have pushed below 1) never lets the
        // target's previous contents bleed through.
        this._blendState = isOverlay ? BlendStateDescription.SingleAlphaBlend : BlendStateDescription.SingleOverrideBlend;

        this._gd = renderer.GraphicsDevice;
        var factory = this._gd.ResourceFactory;

        this._vertexLayout = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
        );

        this._resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("MainTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("MainSampler", ResourceKind.Sampler, ShaderStages.Fragment)
        ));

        this._shaders = ShaderCompiler.CompileShaders(this._gd, _vertexCode, isPixelArt ? _pixelArtfragmentCode : _fragmentCode);

        this._vertexBuffer = factory.CreateBuffer(new BufferDescription(16 * 6, BufferUsage.VertexBuffer));
        var quadVertices = GetQuadVertices(this._gd.IsUvOriginTopLeft);
        this._gd.UpdateBuffer(this._vertexBuffer, 0, quadVertices);
    }

    private Pipeline GetPipeline(OutputDescription output)
    {
        if (!this._pipelines.TryGetValue(output, out var pipeline))
        {
            pipeline = this._gd.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription
            {
                DepthStencilState = output.DepthAttachment.HasValue
                    ? DepthStencilStateDescription.DepthOnlyLessEqual
                    : DepthStencilStateDescription.Disabled,
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ShaderSet = new ShaderSetDescription([this._vertexLayout], this._shaders),
                BlendState = this._blendState,
                RasterizerState = RasterizerStateDescription.CullNone,
                Outputs = output,
                ResourceLayouts = [this._resourceLayout],
            });

            this._pipelines.Add(output, pipeline);
        }

        return pipeline;
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

        foreach (var pipeline in this._pipelines.Values)
        {
            pipeline.Dispose();
        }

        foreach (var resourceSet in this._resourceSets.Values)
        {
            resourceSet.Dispose();
        }
    }

    public void Render(CommandList cl, IRenderTexture source, IRenderTexture destination)
    {
        cl.SetFramebuffer(destination.Framebuffer);
        cl.SetPipeline(this.GetPipeline(destination.OutputDescription));
        cl.SetVertexBuffer(0, this._vertexBuffer);

        var resourceSet = this.GetResourceSet(source.NativeTexture);
        cl.SetGraphicsResourceSet(0, resourceSet);
        cl.Draw(6);
    }

    private ResourceSet GetResourceSet(NativeTexture texture)
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
