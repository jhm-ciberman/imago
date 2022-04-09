using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using FontStashSharp;
using FontStashSharp.Interfaces;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.SceneGraph;
using Veldrid;

namespace LifeSim.Engine.Rendering;

public class SpriteBatcher : IFontStashRenderer, IDisposable
{

    private Shader _currentShaderInUse = null!;

    public int TotalSpritesToDraw { get; private set; } = 0;

    private readonly Shader _defaultShader;
    private readonly GraphicsDevice _gd;

    public DeviceBuffer VertexBuffer { get; private set; }

    public int Count { get; private set; } = 0;

    public readonly DeviceBuffer _indexBuffer;

    private readonly int _capacity = 1000;

    private readonly SpriteBatch _batch;

    private readonly VertexFormat _vertexFormat;
    private CommandList _commandList = null!;

    private readonly ResourceSet _passResourceSet;

    private readonly DeviceBuffer _camera2DInfoBuffer;

    public ResourceLayout PassResourceLayout { get; }

    private readonly ResourceSetCache _resourceSetCache;

    private readonly IPipelineProvider _pass;

    public SpriteBatcher(GraphicsDevice gd, Shader defaultShader, IPipelineProvider pass)
    {
        this._gd = gd;
        this._defaultShader = defaultShader;
        this._pass = pass;
        var factory = gd.ResourceFactory;

        var vertexBufferSize = (uint) (Marshal.SizeOf<SpriteBatch.Item>() * 4 * this._capacity);
        this.VertexBuffer = factory.CreateBuffer(new BufferDescription((uint)vertexBufferSize, BufferUsage.VertexBuffer | BufferUsage.Dynamic));

        var indexBufferSize = (uint) (sizeof(ushort) * 6 * this._capacity);
        this._indexBuffer = factory.CreateBuffer(new BufferDescription((uint)indexBufferSize, BufferUsage.IndexBuffer));
        ushort[] indices = new ushort[this._capacity * 6];


        for (int i = 0; i < this._capacity; i++)
        {
            int j = i * 6;
            int offset = i * 4;
            indices[j + 0] = (ushort)(offset + 0);
            indices[j + 1] = (ushort)(offset + 2);
            indices[j + 2] = (ushort)(offset + 1);
            indices[j + 3] = (ushort)(offset + 0);
            indices[j + 4] = (ushort)(offset + 3);
            indices[j + 5] = (ushort)(offset + 2);
        }

        this._gd.UpdateBuffer(this._indexBuffer, 0, indices);

        this._batch = new SpriteBatch(this._capacity);

        this._vertexFormat = new VertexFormat(new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("TextureCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Byte4_Norm)
        ));

        this.PassResourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("CameraDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
        ));

        this._camera2DInfoBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        this._passResourceSet = factory.CreateResourceSet(new ResourceSetDescription(this.PassResourceLayout, this._camera2DInfoBuffer));

        this._resourceSetCache = new ResourceSetCache(factory);
    }



    private void Prepare(Shader shader, ITexture texture, int requiredQuads = 1)
    {
        if (this._batch.Shader == shader && this._batch.Texture == texture)
        {
            if (this._batch.Count + requiredQuads > this._batch.Items.Length)
            {
                this.FlushBatch();
            }
            return;
        }

        this.FlushBatch();

        this._batch.Shader = shader;
        this._batch.Texture = texture;
    }



    public void Begin(CommandList cl, Matrix4x4 viewProjectionMatrix)
    {
        this._commandList = cl;
        this.TotalSpritesToDraw = 0;
        this._batch.Clear();
        this._currentShaderInUse = null!;

        cl.UpdateBuffer(this._camera2DInfoBuffer, 0, ref viewProjectionMatrix);
    }

    public void End()
    {
        this.FlushBatch();
    }

    public void DrawTexture(Shader? shader, ITexture texture, Vector2 position, Vector2 size)
    {
        this.DrawTexture(shader, texture, position, size, Vector2.Zero, Vector2.One, Color.White);
    }

    public void DrawTexture(Shader? shader, ITexture texture, Vector2 position, Vector2 size, Vector2 uvTopLeft, Vector2 uvBottomRight, in Matrix3x2 transform, Color color)
    {
        this.Prepare(shader ?? this._defaultShader, texture);
        this._batch.DrawCore(position, size, uvTopLeft, uvBottomRight, in transform, color);
    }

    public void DrawTexture(Shader? shader, ITexture texture, Vector2 position, Vector2 size, Vector2 uvTopLeft, Vector2 uvBottomRight, Color color)
    {
        this.Prepare(shader ?? this._defaultShader, texture);
        this._batch.DrawCore(position, size, uvTopLeft, uvBottomRight, color);
    }

    void IFontStashRenderer.Draw(
        object texture,
        Vector2 position,
        System.Drawing.Rectangle? sourceRectangle,
        System.Drawing.Color color,
        float rotation,
        Vector2 origin,
        Vector2 scale,
        float depth
    )
    {
        ITexture tex = (ITexture)texture;
        this.Prepare(this._defaultShader, tex);

        Vector2 pos = new Vector2(position.X, position.Y);
        Vector2 textureSize = new Vector2(tex.Width, tex.Height);
        Vector2 size, uvTopLeft, uvBottomRight;
        if (sourceRectangle == null)
        {
            size = textureSize;
            uvTopLeft = Vector2.Zero;
            uvBottomRight = Vector2.One;
        }
        else
        {
            var r = sourceRectangle.Value;
            size = new Vector2(r.Width, r.Height);
            uvTopLeft = new Vector2(r.X, r.Y) / textureSize;
            uvBottomRight = uvTopLeft + size / textureSize;
        }

        this._batch.DrawCore(pos, size, uvTopLeft, uvBottomRight, color, scale, rotation, origin, depth);
    }

    public void DrawNinePatch(Shader? shader, ITexture texture, Thickness patchMargin, Vector2 size, Vector2 pivot, ref Matrix3x2 worldMatrix, Color color, bool drawCenter)
    {
        var sizeTotal = new Vector2(texture.Width, texture.Height);
        var sizeTL = new Vector2(patchMargin.Left, patchMargin.Top);
        var sizeBR = new Vector2(patchMargin.Right, patchMargin.Bottom);
        var sizeTR = new Vector2(patchMargin.Top, patchMargin.Right);
        var sizeBL = new Vector2(patchMargin.Left, patchMargin.Bottom);

        var uvTL = sizeTL / sizeTotal;
        var uvBR = Vector2.One - sizeBR / sizeTotal;

        float scale = 2f;

        // Scale if the size is smaller (after calculating UVs)
        var minimumRequiredSize = (sizeTL + sizeBR) * scale;
        if (minimumRequiredSize.X > 0 && minimumRequiredSize.Y > 0)
        {
            if (size.X < minimumRequiredSize.X || size.Y < minimumRequiredSize.Y)
            {
                scale *= MathF.Min(size.X, size.Y) / MathF.Min(minimumRequiredSize.X, minimumRequiredSize.Y);
            }
        }

        sizeTL *= scale;
        sizeBR *= scale;
        sizeTR *= scale;
        sizeBL *= scale;
        var sizeSegmentCenter = size - sizeTL - sizeBR;

        int requiredQuads = drawCenter ? 9 : 8;

        this.Prepare(shader ?? this._defaultShader, texture, requiredQuads);

        if (drawCenter)
        {

            if (sizeSegmentCenter.X > 0 && sizeSegmentCenter.Y > 0)
            {
                this._batch.DrawCore(-pivot + sizeTL, sizeSegmentCenter, uvTL, uvBR, in worldMatrix, color);
            }
        }

        var posTL = new Vector2(0f, 0f);
        var posTR = new Vector2(size.X - sizeTL.X, 0f);
        var posBR = new Vector2(size.X - sizeTL.X, size.Y - sizeTL.Y);
        var posBL = new Vector2(0f, size.Y - sizeTL.Y);

        // Corner Top Left
        this._batch.DrawCore(-pivot + posTL, sizeTL, Vector2.Zero, uvTL, in worldMatrix, color);
        // Corner Top Right
        this._batch.DrawCore(-pivot + posTR, sizeTR, new Vector2(uvBR.X, 0f), new Vector2(1f, uvTL.Y), in worldMatrix, color);
        // Corner Bottom Left
        this._batch.DrawCore(-pivot + posBL, sizeBL, new Vector2(0f, uvBR.Y), new Vector2(uvTL.X, 1f), in worldMatrix, color);
        // Corner Bottom Right
        this._batch.DrawCore(-pivot + posBR, sizeBR, uvBR, Vector2.One, in worldMatrix, color);


        var sizeTop = new Vector2(size.X - sizeTL.X - sizeTR.X, sizeTL.Y);
        var sizeBottom = new Vector2(size.X - sizeBL.X - sizeBR.X, sizeBL.Y);
        var sizeLeft = new Vector2(sizeTL.X, size.Y - sizeTL.Y - sizeBL.Y);
        var sizeRight = new Vector2(sizeTR.X, size.Y - sizeTR.Y - sizeBR.Y);

        // Lateral Top
        this._batch.DrawCore(-pivot + new Vector2(sizeTL.X, 0f), sizeTop, new Vector2(uvTL.X, 0f), new Vector2(uvBR.X, uvTL.Y), in worldMatrix, color);
        // Lateral Bottom
        this._batch.DrawCore(-pivot + new Vector2(sizeBL.X, size.Y - sizeBL.Y), sizeBottom, new Vector2(uvTL.X, uvBR.Y), new Vector2(uvBR.X, 1f), in worldMatrix, color);
        // Lateral Left
        this._batch.DrawCore(-pivot + new Vector2(0f, sizeTL.Y), sizeLeft, new Vector2(0f, uvTL.Y), new Vector2(uvTL.X, uvBR.Y), in worldMatrix, color);
        // Lateral Right
        this._batch.DrawCore(-pivot + new Vector2(size.X - sizeTR.X, sizeTR.Y), sizeRight, new Vector2(uvBR.X, uvTL.Y), new Vector2(1f, uvBR.Y), in worldMatrix, color);
    }

    public void DrawText(SpriteFontBase font, string text, Vector2 position, Color color)
    {
        font.DrawText(this, text, position, color);
    }

    public void DrawRectangle(Vector2 position, Vector2 size, Color color)
    {
        this.DrawTexture(this._defaultShader, Texture.White, position, size, Vector2.Zero, Vector2.One, color);
    }

    public void FlushBatch()
    {
        if (this._batch.Count == 0 || this._batch.Texture == null)
            return;

        this._commandList.UpdateBuffer(this.VertexBuffer, 0, this._batch.Items);

        if (this._batch.Shader != this._currentShaderInUse)
        {
            this._currentShaderInUse = this._batch.Shader ?? this._defaultShader;
            var pipeline = this._currentShaderInUse.GetPipeline(this._pass, this._vertexFormat);

            this._commandList.SetPipeline(pipeline);
            this._commandList.SetGraphicsResourceSet(0, this._passResourceSet);
        }

        this._commandList.SetVertexBuffer(0, this.VertexBuffer);
        this._commandList.SetIndexBuffer(this._indexBuffer, IndexFormat.UInt16);


        var resourceSet = this._resourceSetCache.GetResourceSet(this._currentShaderInUse, this._batch.Texture);
        this._commandList.SetGraphicsResourceSet(1, resourceSet);
        this._commandList.DrawIndexed((uint)this._batch.Count * 6);

        this._batch.Clear();
    }


    public void Dispose()
    {
        this._indexBuffer.Dispose();
        this.VertexBuffer.Dispose();
        this._camera2DInfoBuffer.Dispose();
        this.PassResourceLayout.Dispose();
        this._passResourceSet.Dispose();
        this._resourceSetCache.Dispose();
    }
}