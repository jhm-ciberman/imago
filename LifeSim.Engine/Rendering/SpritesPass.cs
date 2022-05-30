using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using LifeSim.Engine.SceneGraph;
using Veldrid;

namespace LifeSim.Engine.Rendering;

public class SpritesPass : IDisposable, IPipelineProvider, IRenderingPass
{
    private readonly IRenderTexture _renderTexture;
    private readonly GraphicsDevice _gd;
    private readonly Shader _defaultShader;

    private readonly SpriteBatcher _spriteBatcher;

    public SpritesPass(Renderer renderer, IRenderTexture renderTexture)
    {
        this._gd = renderer.GraphicsDevice;
        this._defaultShader = new Shader(_vertexShader, _fragmentShader, new[] { "Main" });

        this._renderTexture = renderTexture;

        this._spriteBatcher = new SpriteBatcher(this._gd, this._defaultShader, this);
    }

    public void Dispose()
    {
        this._spriteBatcher.Dispose();
    }

    Pipeline IPipelineProvider.MakePipeline(ShaderVariant shaderVariant, RenderFlags flags)
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
            Outputs = this._renderTexture.OutputDescription,
            ResourceLayouts = new ResourceLayout[]
            {
                this._spriteBatcher.PassResourceLayout,
                shaderVariant.MaterialResourceLayout,
            },
        });
    }

    public void Render(CommandList cl, Scene scene)
    {
        var canvasLayers = scene.CanvasLayers;

        cl.SetFramebuffer(this._renderTexture.Framebuffer);
        cl.ClearDepthStencil(1f);
        for (int i = 0; i < canvasLayers.Count; i++)
        {
            var canvasLayer = canvasLayers[i];
            this._spriteBatcher.Begin(cl, canvasLayer.ViewProjectionMatrix);

            for (int j = 0; j < canvasLayer.Items.Count; j++)
            {
                canvasLayer.Items[j].Render(this._spriteBatcher);
            }

            this._spriteBatcher.End();
        }

        if (scene.UILayer != null)
        {
            this._spriteBatcher.Begin(cl, scene.UILayer.ViewProjectionMatrix);
            scene.UILayer.Draw(this._spriteBatcher);
            this._spriteBatcher.FlushBatch();
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