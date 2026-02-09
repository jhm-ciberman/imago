using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using LifeSim.Imago.Assets.Materials;
using LifeSim.Imago.Assets.Meshes;
using LifeSim.Imago.Assets.Textures;
using LifeSim.Imago.Rendering.Internals;
using LifeSim.Imago.SceneGraph.Cameras;
using LifeSim.Imago.SceneGraph.Picking;
using LifeSim.Imago.Utilities;
using Veldrid;

namespace LifeSim.Imago.Rendering.Passes;

/// <summary>
/// The mouse picking pass is used to determine which object was clicked by the mouse.
/// </summary>
internal class MousePickingPass : IDisposable, IPipelineProvider
{
    [StructLayout(LayoutKind.Sequential)]
    private struct CameraDataBuffer
    {
        public Matrix4x4 ViewProjectionMatrix { get; set; }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct GlobalData
    {
        public Vector3 CameraPosition { get; set; }
        public float Time { get; set; }
    }

    private readonly Renderer _renderer;
    private readonly GraphicsDevice _gd;
    private readonly DeviceBuffer _camera3DInfoBuffer;
    private readonly DeviceBuffer _globalDataBuffer;
    private readonly ResourceLayout _resourceLayout;
    private readonly ResourceSet _resourceSet;
    private readonly RenderTexture _renderTexture;
    private readonly RenderBatcher _renderBatcher;
    private readonly Veldrid.Texture _pixelStagingTexture;
    private Vector2 _mousePosition;

    public MousePickingPass(Renderer renderer)
    {
        this._renderer = renderer;
        this._gd = renderer.GraphicsDevice;
        var factory = this._gd.ResourceFactory;

        this._resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("CameraDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("GlobalDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
        ));

        this._camera3DInfoBuffer = factory.CreateBuffer(new BufferDescription((uint)Marshal.SizeOf<CameraDataBuffer>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        this._globalDataBuffer = factory.CreateBuffer(new BufferDescription((uint)Marshal.SizeOf<GlobalData>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

        this._resourceSet = this._gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(this._resourceLayout, this._camera3DInfoBuffer, this._globalDataBuffer));

        this._renderTexture = renderer.MainRenderTexture;

        this._renderBatcher = new RenderBatcher(this._gd, RenderBatchPassType.Picking, renderer.Statistics);

        this._pixelStagingTexture = factory.CreateTexture(new TextureDescription(
            width: 1, height: 1, depth: 1, mipLevels: 1, arrayLayers: 1,
            PixelFormat.R32_UInt, TextureUsage.Staging, TextureType.Texture2D
        ));
    }

    /// <summary>
    /// Dispose the resources used by the mouse picking pass.
    /// </summary>
    public void Dispose()
    {
        this._resourceLayout.Dispose();
        this._resourceSet.Dispose();
        this._camera3DInfoBuffer.Dispose();
        this._globalDataBuffer.Dispose();
        this._pixelStagingTexture.Dispose();
        this._renderBatcher.Dispose();
    }

    /// <summary>
    /// Render the scene to the picking texture and update the highlighted pickable object in the stage.
    /// </summary>
    /// <param name="cl">The command list to use for rendering.</param>
    /// <param name="renderTexture">The render texture to render the scene to.</param>
    /// <param name="camera">The camera to use for rendering.</param>
    /// <param name="pickingManager">The picking manager to update with the result.</param>
    /// <param name="pickingRenderQueue">The render queue to use for rendering.</param>
    public void Render(CommandList cl, RenderTexture renderTexture, Camera camera, PickingManager pickingManager, RenderQueue pickingRenderQueue)
    {
        // Record a copy from the picking texture into a staging texture.
        // The actual Map/read and PickingManager update happen in ReadStagingResult(),
        // called after command submission.
        this.RecordCopy(cl);

        // Render the scene to the picking texture.
        var frustumForCulling = new BoundingFrustum(camera.FrustumCullingCamera.ViewProjectionMatrix);
        pickingRenderQueue.Update(frustumForCulling, camera.Position);

        cl.SetFramebuffer(renderTexture.PickingFramebuffer);
        cl.ClearColorTarget(0, RgbaFloat.Black);
        cl.ClearDepthStencil(1f);

        CameraDataBuffer cameraInfo = new CameraDataBuffer();
        cameraInfo.ViewProjectionMatrix = camera.ViewProjectionMatrix;

        GlobalData globalData = new GlobalData();
        globalData.CameraPosition = camera.Position;
        globalData.Time = this._renderer.TotalTime;

        cl.UpdateBuffer(this._camera3DInfoBuffer, 0, ref cameraInfo);
        cl.UpdateBuffer(this._globalDataBuffer, 0, ref globalData);

        this._renderBatcher.DrawRenderList(cl, this._resourceSet, pickingRenderQueue);
    }

    /// <summary>
    /// Set the mouse position in screen coordinates. This position will be used to read the pixel color from the picking texture.
    /// The pixel color will be used to determine the object that was clicked the last frame.
    /// </summary>
    /// <param name="mousePos">The mouse position in screen coordinates.</param>
    public void SetMousePosition(Vector2 mousePos)
    {
        this._mousePosition = mousePos;
    }

    /// <summary>
    /// Reads the picking result from the staging texture and updates the picking manager.
    /// Must be called after command submission so the copy has executed.
    /// </summary>
    /// <param name="pickingManager">The picking manager to update.</param>
    public void ReadStagingResult(PickingManager pickingManager)
    {
        var mappedResource = this._gd.Map<uint>(this._pixelStagingTexture, MapMode.Read);
        uint objectID = mappedResource[0, 0];
        this._gd.Unmap(this._pixelStagingTexture);

        pickingManager.HighlightedPickable = pickingManager.GetPickable(objectID);
    }

    private bool MouseIsInside(Vector2 mousePos)
    {
        if (mousePos.X < 0 || mousePos.Y < 0) return false;
        var texture = this._renderTexture.PickingColorTexture;
        if (mousePos.X >= texture.Width || mousePos.Y >= texture.Height) return false;
        return true;
    }

    private void RecordCopy(CommandList cl)
    {
        var mousePos = this._mousePosition;
        if (this.MouseIsInside(mousePos))
        {
            uint x = (uint)mousePos.X;
            uint y = this._gd.IsUvOriginTopLeft
                ? (uint)mousePos.Y
                : (uint)(this._renderTexture.PickingColorTexture.Height - 1 - mousePos.Y);

            cl.CopyTexture(
                source: this._renderTexture.PickingColorTexture,
                srcX: x, srcY: y, srcZ: 0, srcMipLevel: 0, srcBaseArrayLayer: 0,
                destination: this._pixelStagingTexture,
                dstX: 0, dstY: 0, dstZ: 0, dstMipLevel: 0, dstBaseArrayLayer: 0,
                width: 1, height: 1, depth: 1, layerCount: 1
            );
        }
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
