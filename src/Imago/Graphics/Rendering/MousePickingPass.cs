using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Imago.Graphics.Materials;
using Imago.Graphics.Meshes;
using Imago.Graphics.Textures;
using Imago.SceneGraph;
using Veldrid;
using Veldrid.Utilities;

namespace Imago.Graphics.Rendering;

public class MousePickingPass : IDisposable, IPipelineProvider
{
    [StructLayout(LayoutKind.Sequential)]
    private struct CameraDataBuffer
    {
        public Matrix4x4 ViewProjectionMatrix { get; set; }
    }

    private readonly Renderer _renderer;
    private readonly GraphicsDevice _gd;
    private readonly DeviceBuffer _camera3DInfoBuffer;
    private readonly ResourceLayout _resourceLayout;
    private ResourceSet _resourceSet;
    private readonly RenderTexture _renderTexture;
    private readonly RenderBatcher _renderBatcher;
    private readonly Veldrid.Texture _pixelTexture;
    private Vector2 _mousePosition;

    public Materials.Shader DefaultShader { get; }

    public MousePickingPass(Renderer renderer, RenderTexture mainRenderTexture)
    {
        this._renderer = renderer;
        this._gd = renderer.GraphicsDevice;
        var factory = this._gd.ResourceFactory;

        this._resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("CameraDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
        ));

        this._camera3DInfoBuffer = factory.CreateBuffer(new BufferDescription((uint)Marshal.SizeOf<CameraDataBuffer>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

        this._resourceSet = this._gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(this._resourceLayout, this._camera3DInfoBuffer));

        this._renderTexture = mainRenderTexture;

        this._renderBatcher = new RenderBatcher(this._gd, RenderBatchPassType.Picking);

        RenderFlags supportedForwardFlags = RenderFlags.AlphaTest | RenderFlags.ReceiveShadows | RenderFlags.Fog | RenderFlags.PixelPerfactShadows | RenderFlags.ColorWrite;
        var baseVertex = ShaderLoader.Load("picking.vert.glsl");
        var baseFragment = ShaderLoader.Load("picking.frag.glsl");
        this.DefaultShader = new Materials.Shader(renderer, this, baseVertex, baseFragment, new[] { "Surface" }, supportedForwardFlags);

        // This is a 1x1 texture that will be used to read the pixel color from the mouse picking pass.
        this._pixelTexture = factory.CreateTexture(new TextureDescription(
            width: 1, height: 1, depth: 1, mipLevels: 1, arrayLayers: 1,
            PixelFormat.R32_UInt, TextureUsage.Staging, TextureType.Texture2D
        ));

    }

    public void Render(CommandList cl, Stage stage, RenderTexture renderTexture)
    {
        var scene = stage.Scene;
        var camera = scene.Camera;
        if (camera == null) return;

        // Step 1: Read the pixel color from the previous frame.
        uint objectID = this.ReadPixel(cl, stage);
        stage.Picking.HighlightedPickable = stage.Picking.GetPickable(objectID);

        // Step 2: Render the scene to the picking texture.
        var frustumForCulling = new BoundingFrustum(camera.FrustumCullingCamera.ViewProjectionMatrix);
        stage.PickingRenderQueue.Update(frustumForCulling, camera.Position);

        cl.SetFramebuffer(renderTexture.PickingFramebuffer);
        cl.ClearColorTarget(0, RgbaFloat.Black);
        cl.ClearDepthStencil(1f);

        CameraDataBuffer cameraInfo = new CameraDataBuffer();
        cameraInfo.ViewProjectionMatrix = camera.ViewProjectionMatrix;

        cl.UpdateBuffer(this._camera3DInfoBuffer, 0, ref cameraInfo);

        this._renderBatcher.DrawRenderList(cl, this._resourceSet, stage.PickingRenderQueue);
    }

    private bool MouseIsInside(Vector2 mousePos)
    {
        if (mousePos.X < 0 || mousePos.Y < 0) return false;
        var texture = this._renderTexture.PickingColorTexture;
        if (mousePos.X >= texture.Width || mousePos.Y >= texture.Height) return false;
        return true;
    }

    public void SetMousePosition(Vector2 mousePos)
    {
        this._mousePosition = mousePos;
    }


    private uint ReadPixel(CommandList cl, Stage stage)
    {
        var mousePos = this._mousePosition;
        if (this.MouseIsInside(mousePos))
        {
            uint x = (uint) mousePos.X;
            uint y = this._gd.IsUvOriginTopLeft
                ? (uint) mousePos.Y
                : (uint) (this._renderTexture.PickingColorTexture.Height - 1 - mousePos.Y);

            cl.CopyTexture(
                source: this._renderTexture.PickingColorTexture,
                srcX: x, srcY: y, srcZ: 0, srcMipLevel: 0, srcBaseArrayLayer: 0,
                destination: this._pixelTexture,
                dstX: 0, dstY: 0, dstZ: 0, dstMipLevel: 0, dstBaseArrayLayer: 0,
                width: 1, height: 1, depth: 1, layerCount: 1
            );
        }

        var mappedResource = this._gd.Map<uint>(this._pixelTexture, MapMode.Read);
        uint objectID = mappedResource[0, 0];
        this._gd.Unmap(this._pixelTexture);

        return objectID;
    }

    public void Dispose()
    {
        this._resourceLayout.Dispose();
        this._resourceSet.Dispose();
        this._camera3DInfoBuffer.Dispose();
        this._pixelTexture.Dispose();
    }

    Pipeline IPipelineProvider.MakePipeline(ShaderVariant shaderVariant, RenderFlags flags, TextureSampleCount sampleCount)
    {
        var cullMode = flags.HasFlag(RenderFlags.DoubleSided) ? FaceCullMode.None : FaceCullMode.Back;
        var depthTestEnabled = flags.HasFlag(RenderFlags.DepthTest);
        var depthWriteEnabled = flags.HasFlag(RenderFlags.DepthWrite);
        var fillMode = PolygonFillMode.Solid;
        var outputDescription = this._renderTexture.PickingOutputDescription;

        return this._gd.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription()
        {
            DepthStencilState = new DepthStencilStateDescription(
                depthTestEnabled, depthWriteEnabled, ComparisonKind.LessEqual
            ),
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ShaderSet = new ShaderSetDescription(GetVertexLayout(shaderVariant.VertexFormat), shaderVariant.VeldridShaders),
            BlendState = new BlendStateDescription(RgbaFloat.Black, BlendAttachmentDescription.Disabled),
            RasterizerState = new RasterizerStateDescription(
                cullMode,
                fillMode,
                FrontFace.CounterClockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false
            ),
            Outputs = outputDescription,
            ResourceLayouts = this.GetResourceLayouts(shaderVariant),
        });
    }

    private static VertexLayoutDescription[] GetVertexLayout(VertexFormat vertexFormat)
    {
        var list = new List<VertexLayoutDescription>(vertexFormat.Layouts.Length + 1)
        {
            new VertexLayoutDescription(stride: 16, instanceStepRate: 1,
                new VertexElementDescription("Offsets", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt4))
        };
        list.AddRange(vertexFormat.Layouts);
        return list.ToArray();
    }

    private ResourceLayout[] GetResourceLayouts(ShaderVariant shaderVariant)
    {
        var resources = new List<ResourceLayout>
        {
            this._resourceLayout,
            this._renderer.TransformResourceLayout,
            shaderVariant.MaterialResourceLayout,
            this._renderer.InstanceResourceLayout
        };

        if (shaderVariant.VertexFormat.IsSkinned)
            resources.Add(this._renderer.SkeletonResourceLayout);

        return resources.ToArray();
    }
}
