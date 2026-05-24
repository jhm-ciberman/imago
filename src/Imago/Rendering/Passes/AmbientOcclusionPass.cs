using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Imago.Assets.Materials;
using Imago.Assets.Textures;
using Imago.SceneGraph;
using Imago.SceneGraph.Cameras;
using NeoVeldrid;

namespace Imago.Rendering.Passes;

/// <summary>
/// A screen-space ambient occlusion pass that darkens crevices and contact areas of the
/// opaque scene by sampling the forward depth buffer.
/// </summary>
/// <remarks>
/// Reconstructs view-space position and normal from depth (no normal buffer required), evaluates a
/// hemisphere kernel, and multiplies the resulting occlusion factor into the scene color in place.
/// Must run after the opaque geometry has been drawn but before the depth buffer is reused by later passes.
/// </remarks>
internal class AmbientOcclusionPass : IDisposable
{
    [StructLayout(LayoutKind.Sequential)]
    private struct SsaoParams
    {
        public Matrix4x4 InverseProjection;
        public Matrix4x4 Projection;
        public Vector4 Params0; // x = radius, y = bias, z = intensity, w = power
        public Vector4 Params1; // x = yFlip, y = sampleCount, z/w = unused
    }

    private readonly Renderer _renderer;
    private readonly GraphicsDevice _gd;
    private readonly Pipeline _pipeline;
    private readonly DeviceBuffer _vertexBuffer;
    private readonly DeviceBuffer _paramsBuffer;
    private readonly ResourceLayout _resourceLayout;
    private readonly Sampler _depthSampler;

    private Framebuffer? _framebuffer;
    private ResourceSet? _resourceSet;
    private uint _builtForWidth;
    private uint _builtForHeight;

    private const int SampleCount = 32;

    public AmbientOcclusionPass(Renderer renderer)
    {
        this._renderer = renderer;
        this._gd = renderer.GraphicsDevice;
        var factory = this._gd.ResourceFactory;

        this._resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("SsaoParams", ResourceKind.UniformBuffer, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("DepthTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("DepthSampler", ResourceKind.Sampler, ShaderStages.Fragment)
        ));

        this._paramsBuffer = factory.CreateBuffer(new BufferDescription(
            (uint)Marshal.SizeOf<SsaoParams>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

        this._depthSampler = this._gd.PointSampler;

        var shaders = ShaderCompiler.CompileShaders(this._gd, _vertexCode, _fragmentCode);

        var vertexLayout = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
        );

        this._pipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription
        {
            DepthStencilState = DepthStencilStateDescription.Disabled,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ShaderSet = new ShaderSetDescription([vertexLayout], shaders),
            BlendState = MultiplyBlendState(),
            RasterizerState = RasterizerStateDescription.CullNone,
            Outputs = new OutputDescription(null, new OutputAttachmentDescription(PixelFormat.R8_G8_B8_A8_UNorm)),
            ResourceLayouts = [this._resourceLayout],
        });

        this._vertexBuffer = factory.CreateBuffer(new BufferDescription(16 * 6, BufferUsage.VertexBuffer));
        this._gd.UpdateBuffer(this._vertexBuffer, 0, GetQuadVertices(this._gd.IsUvOriginTopLeft));
    }

    /// <summary>
    /// Computes ambient occlusion for the opaque scene and multiplies it into the color attachment of the given render texture.
    /// </summary>
    /// <param name="cl">The command list to record into.</param>
    /// <param name="renderTexture">The render texture holding the opaque scene color and depth.</param>
    /// <param name="camera">The camera used to render the scene.</param>
    /// <param name="settings">The ambient occlusion settings to apply.</param>
    public void Render(CommandList cl, RenderTexture renderTexture, Camera camera, AmbientOcclusionSettings settings)
    {
        this.EnsureResources(renderTexture);

        Matrix4x4.Invert(camera.ProjectionMatrix, out Matrix4x4 inverseProjection);

        var parameters = new SsaoParams
        {
            InverseProjection = inverseProjection,
            Projection = camera.ProjectionMatrix,
            Params0 = new Vector4(settings.Radius, settings.Bias, settings.Intensity, settings.Power),
            Params1 = new Vector4(this._gd.IsUvOriginTopLeft ? -1f : 1f, SampleCount, 0f, 0f),
        };
        cl.UpdateBuffer(this._paramsBuffer, 0, ref parameters);

        cl.SetFramebuffer(this._framebuffer);
        cl.SetFullViewports();
        cl.SetPipeline(this._pipeline);
        cl.SetVertexBuffer(0, this._vertexBuffer);
        cl.SetGraphicsResourceSet(0, this._resourceSet);
        cl.Draw(6);
    }

    private void EnsureResources(RenderTexture renderTexture)
    {
        if (this._framebuffer != null && this._builtForWidth == renderTexture.Width && this._builtForHeight == renderTexture.Height)
        {
            return;
        }

        if (this._framebuffer != null) this._renderer.DisposeWhenIdle(this._framebuffer);
        if (this._resourceSet != null) this._renderer.DisposeWhenIdle(this._resourceSet);

        var factory = this._gd.ResourceFactory;

        this._framebuffer = factory.CreateFramebuffer(new FramebufferDescription(
            null, renderTexture.ForwardColorTexture
        ));

        this._resourceSet = factory.CreateResourceSet(new ResourceSetDescription(
            this._resourceLayout,
            this._paramsBuffer,
            renderTexture.ForwardDepthTexture,
            this._depthSampler
        ));

        this._builtForWidth = renderTexture.Width;
        this._builtForHeight = renderTexture.Height;
    }

    private static BlendStateDescription MultiplyBlendState()
    {
        // out.rgb = src.rgb * dst.rgb (occlusion darkens the existing color); out.a = dst.a (preserve).
        return new BlendStateDescription
        {
            AttachmentStates = [
                new BlendAttachmentDescription
                {
                    BlendEnabled = true,
                    SourceColorFactor = BlendFactor.DestinationColor,
                    DestinationColorFactor = BlendFactor.Zero,
                    ColorFunction = BlendFunction.Add,
                    SourceAlphaFactor = BlendFactor.Zero,
                    DestinationAlphaFactor = BlendFactor.One,
                    AlphaFunction = BlendFunction.Add
                }
            ],
        };
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
        if (this._framebuffer != null) this._framebuffer.Dispose();
        if (this._resourceSet != null) this._resourceSet.Dispose();
        this._vertexBuffer.Dispose();
        this._paramsBuffer.Dispose();
        this._resourceLayout.Dispose();
        this._pipeline.Dispose();
    }

    private static readonly string _vertexCode = @"
        #version 450
        layout(location = 0) in vec4 Position; // xy = position, zw = uv

        layout(location = 0) out vec2 fsin_TexCoords;

        void main()
        {
            gl_Position = vec4(Position.xy, 0, 1);
            fsin_TexCoords = Position.zw;
        }";

    private static readonly string _fragmentCode = @"
        #version 450

        layout(location = 0) in vec2 fsin_TexCoords;
        layout(location = 0) out vec4 fsout_Color;

        layout(set = 0, binding = 0, std140) uniform SsaoParams
        {
            mat4 InverseProjection;
            mat4 Projection;
            vec4 Params0; // x = radius, y = bias, z = intensity, w = power
            vec4 Params1; // x = yFlip, y = sampleCount
        };

        layout(set = 0, binding = 1) uniform texture2D DepthTexture;
        layout(set = 0, binding = 2) uniform sampler DepthSampler;

        float SampleDepth(vec2 uv)
        {
            return texture(sampler2D(DepthTexture, DepthSampler), uv).r;
        }

        // Reconstructs a view-space position from a uv and an NDC depth value.
        vec3 ViewPosition(vec2 uv, float depth)
        {
            float yFlip = Params1.x;
            vec3 ndc = vec3(uv.x * 2.0 - 1.0, (uv.y * 2.0 - 1.0) * yFlip, depth);
            vec4 view = InverseProjection * vec4(ndc, 1.0);
            return view.xyz / view.w;
        }

        void main()
        {
            float radius = Params0.x;
            float bias = Params0.y;
            float intensity = Params0.z;
            float power = Params0.w;
            float yFlip = Params1.x;
            int samples = int(Params1.y);

            float depth = SampleDepth(fsin_TexCoords);
            if (depth >= 0.9999)
            {
                fsout_Color = vec4(1.0); // sky / empty: no occlusion
                return;
            }

            vec3 p = ViewPosition(fsin_TexCoords, depth);

            // Geometric normal from the depth-reconstructed position.
            vec3 n = normalize(cross(dFdx(p), dFdy(p)));
            if (dot(n, p) > 0.0) n = -n; // orient toward the camera (camera at origin)

            // Deterministic tangent basis around the normal.
            vec3 up = abs(n.z) < 0.999 ? vec3(0.0, 0.0, 1.0) : vec3(1.0, 0.0, 0.0);
            vec3 t = normalize(cross(up, n));
            vec3 b = cross(n, t);

            float occlusion = 0.0;
            for (int i = 0; i < samples; i++)
            {
                // Fibonacci hemisphere direction, packed toward the center for nearby detail.
                float k = (float(i) + 0.5) / float(samples);
                float phi = float(i) * 2.3999632; // golden angle
                float cosTheta = 1.0 - k;
                float sinTheta = sqrt(max(0.0, 1.0 - cosTheta * cosTheta));
                vec3 h = vec3(cos(phi) * sinTheta, sin(phi) * sinTheta, cosTheta);
                float scale = mix(0.1, 1.0, k * k);

                vec3 dir = t * h.x + b * h.y + n * h.z;
                vec3 samplePos = p + dir * radius * scale;

                vec4 clip = Projection * vec4(samplePos, 1.0);
                vec3 sampleNdc = clip.xyz / clip.w;
                vec2 sampleUv = vec2(sampleNdc.x * 0.5 + 0.5, sampleNdc.y * yFlip * 0.5 + 0.5);
                if (sampleUv.x < 0.0 || sampleUv.x > 1.0 || sampleUv.y < 0.0 || sampleUv.y > 1.0)
                {
                    continue;
                }

                vec3 scenePos = ViewPosition(sampleUv, SampleDepth(sampleUv));

                // View space looks down -Z: a scene point closer to the camera has a larger (less negative) z.
                float rangeCheck = smoothstep(0.0, 1.0, radius / max(0.0001, abs(p.z - scenePos.z)));
                occlusion += (scenePos.z >= samplePos.z + bias ? 1.0 : 0.0) * rangeCheck;
            }

            float ao = 1.0 - (occlusion / float(samples)) * intensity;
            ao = pow(clamp(ao, 0.0, 1.0), power);
            fsout_Color = vec4(vec3(ao), 1.0);
        }";
}
