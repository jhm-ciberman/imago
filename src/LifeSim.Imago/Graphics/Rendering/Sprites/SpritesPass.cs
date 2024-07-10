using System;
using LifeSim.Imago.Graphics.Materials;
using LifeSim.Imago.Graphics.Textures;
using LifeSim.Imago.SceneGraph;
using Veldrid;
using Shader = LifeSim.Imago.Graphics.Materials.Shader;

namespace LifeSim.Imago.Graphics.Rendering.Sprites;

public class SpritesPass : IDisposable, IPipelineProvider
{
    /// <summary>
    /// Gets the number of draw calls made by the last call to <see cref="Render"/>.
    /// </summary>
    public int DrawCallCount { get; private set; }

    private readonly GraphicsDevice _gd;

    private readonly Shader _defaultShader;

    private readonly DrawingContext _drawingContext;

    private readonly Renderer _renderer;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpritesPass"/> class.
    /// </summary>
    /// <param name="renderer">The renderer.</param>
    public SpritesPass(Renderer renderer)
    {
        this._renderer = renderer;
        this._gd = renderer.GraphicsDevice;
        this._defaultShader = new Shader(renderer, this, _vertexShader, _fragmentShader, ["Main"]);

        this._drawingContext = new DrawingContext(this._gd, this._defaultShader);
    }

    /// <summary>
    /// Disposes of the resources used by this <see cref="SpritesPass"/>.
    /// </summary>
    public void Dispose()
    {
        this._drawingContext.Dispose();
    }

    /// <inheritdoc/>
    Pipeline IPipelineProvider.MakePipeline(ShaderVariant shaderVariant, RenderFlags flags, TextureSampleCount sampleCount)
    {
        var scissorTestEnabled = flags.HasFlag(RenderFlags.ScisorTest);
        return this._gd.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription()
        {
            DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ShaderSet = shaderVariant.ShaderSetDescription,
            BlendState = BlendStateDescription.SingleAlphaBlend,
            RasterizerState = new RasterizerStateDescription(
                FaceCullMode.Back,
                PolygonFillMode.Solid,
                FrontFace.CounterClockwise,
                depthClipEnabled: true,
                scissorTestEnabled
            ),
            Outputs = this._renderer.MainRenderTexture.OutputDescription,
            ResourceLayouts = new ResourceLayout[]
            {
                this._drawingContext.PassResourceLayout,
                shaderVariant.MaterialResourceLayout,
            },
        });
    }

    /// <summary>
    /// Renders the specified stage to the render texture.
    /// </summary>
    /// <param name="cl">The command list to use.</param>
    /// <param name="stage">The stage to render.</param>
    /// <param name="renderTexture">The render texture to render to.</param>
    public void Render(CommandList cl, Stage stage, RenderTexture renderTexture)
    {
        var layers = stage.Scene.Layers2D;
        if (layers.Count == 0) return;

        this.DrawCallCount = 0;

        for (int i = 0; i < layers.Count; i++)
        {
            var layer = layers[i];
            cl.SetFramebuffer(renderTexture.Framebuffer);
            cl.ClearDepthStencil(1f);

            this._drawingContext.Begin(cl, layer.ViewProjectionMatrix);
            layer.Draw(this._drawingContext);
            this._drawingContext.End();

            this.DrawCallCount += this._drawingContext.DrawCallCount;
        }
    }

    private static readonly string _fragmentShader = @"
        #version 450
        layout(location = 0) in vec2 fsin_TexCoords;
        layout(location = 1) in vec4 fsin_Color;

        layout(set = 1, binding = 0) uniform texture2D MainTexture;
        layout(set = 1, binding = 1) uniform sampler MainSampler;

        layout(location = 0) out vec4 fsout_color;

        void main()
        {
            vec4 textureColor = texture(sampler2D(MainTexture, MainSampler), fsin_TexCoords);
            fsout_color = textureColor * fsin_Color;
        }
    ";

    private static readonly string _vertexShader = @"
        #version 450
        layout(set = 0, binding = 0, std140) uniform CameraDataBuffer {
            mat4 ViewProjection;
        };

        layout(location = 0) in vec3 Position;
        layout(location = 1) in vec2 TextureCoords;
        layout(location = 2) in vec4 Color;

        layout(location = 0) out vec2 fsin_TexCoords;
        layout(location = 1) out vec4 fsin_Color;

        void main()
        {
            gl_Position = ViewProjection * vec4(Position, 1);
            fsin_TexCoords = TextureCoords;
            fsin_Color = Color;
        }
    ";
}
