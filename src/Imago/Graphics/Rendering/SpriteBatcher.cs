using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using FontStashSharp.Interfaces;
using Imago.Graphics.Materials;
using Imago.Graphics.Meshes;
using Imago.Graphics.Textures;
using Imago.TexturePacking;
using Support;
using Veldrid;
using Shader = Imago.Graphics.Materials.Shader;
using Texture = Imago.Graphics.Textures.Texture;

namespace Imago.Graphics.Rendering;

public class SpriteBatcher : IFontStashRenderer2, IDisposable
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

    private readonly DeviceBuffer _matrixBuffer;

    public ResourceLayout PassResourceLayout { get; }

    private readonly ResourceSetCache _resourceSetCache;

    private readonly Stack<Rect> _clipRectStack = new Stack<Rect>();

    private readonly Stack<float> _opacityStack = new Stack<float>();

    public int DrawCallCount { get; private set; } = 0;

    public void PushOpacity(float opacity)
    {
        opacity *= this._opacityStack.Peek();
        this._opacityStack.Push(opacity);
        this._batch.Opacity = opacity;
    }

    public void PopOpacity()
    {
        this._opacityStack.Pop();
        this._batch.Opacity = this._opacityStack.Peek();
    }

    private RenderFlags _currentPipelineFlags = RenderFlags.None;

    internal SpriteBatcher(GraphicsDevice gd, Shader defaultShader)
    {
        this._gd = gd;
        this._defaultShader = defaultShader;
        var factory = gd.ResourceFactory;

        var vertexBufferSize = (uint) (Marshal.SizeOf<SpriteBatch.Item>() * 4 * this._capacity);
        this.VertexBuffer = factory.CreateBuffer(new BufferDescription(vertexBufferSize, BufferUsage.VertexBuffer | BufferUsage.Dynamic));

        var indexBufferSize = (uint) (sizeof(ushort) * 6 * this._capacity);
        this._indexBuffer = factory.CreateBuffer(new BufferDescription(indexBufferSize, BufferUsage.IndexBuffer));
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

        this._vertexFormat = new VertexFormat("SpriteVertex", new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("TextureCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Byte4_Norm)
        ));

        this.PassResourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("CameraDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
        ));

        this._matrixBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        this._passResourceSet = factory.CreateResourceSet(new ResourceSetDescription(this.PassResourceLayout, this._matrixBuffer));

        this._resourceSetCache = new ResourceSetCache(factory);

        this._textureManager = new FontStashTextureManager(this._gd);
    }



    private void Prepare(Shader shader, ITexture texture, int requiredQuads = 1)
    {
        if (this._batch.Shader == shader && this._batch.Texture == texture)
        {
            if (this._batch.Count + requiredQuads > this._batch.Items.Length)
                this.FlushBatch();
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
        this._batch.Reset();
        this._opacityStack.Clear();
        this._opacityStack.Push(1f);
        this._currentShaderInUse = null!;
        this._currentPipelineFlags = RenderFlags.None;
        this._clipRectStack.Clear();
        this.DrawCallCount = 0;

        cl.UpdateBuffer(this._matrixBuffer, 0, ref viewProjectionMatrix);
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

    public void DrawNinePatch(Shader? shader, PackedTexture texture, Vector2 position, Vector2 size, Thickness patchMargin, Color color, float scale)
    {
        var sizeTL = new Vector2(patchMargin.Left, patchMargin.Top);
        var sizeBR = new Vector2(patchMargin.Right, patchMargin.Bottom);
        var sizeTR = new Vector2(patchMargin.Top, patchMargin.Right);
        var sizeBL = new Vector2(patchMargin.Left, patchMargin.Bottom);

        var sizeTotal = texture.PixelSize;
        var uvExtTL = texture.TopLeft;
        var uvExtBR = texture.BottomRight;
        var uvIntTL = uvExtTL + sizeTL / sizeTotal * texture.Size;
        var uvIntBR = uvExtBR - sizeBR / sizeTotal * texture.Size;

        // Scale if the size is smaller (after calculating UVs)
        var minimumRequiredSize = (sizeTL + sizeBR) * scale;
        if (minimumRequiredSize.X > 0 && minimumRequiredSize.Y > 0)
        {
            if (size.X < minimumRequiredSize.X || size.Y < minimumRequiredSize.Y)
                scale *= MathF.Min(size.X, size.Y) / MathF.Min(minimumRequiredSize.X, minimumRequiredSize.Y);
        }

        sizeTL *= scale;
        sizeBR *= scale;
        sizeTR *= scale;
        sizeBL *= scale;
        var sizeSegmentCenter = size - sizeTL - sizeBR;

        this.Prepare(shader ?? this._defaultShader, texture.Texture, 9);

        if (sizeSegmentCenter.X > 0 && sizeSegmentCenter.Y > 0)
            this._batch.DrawCore(position + sizeTL, sizeSegmentCenter, uvIntTL, uvIntBR, color);

        var posTL = new Vector2(0f, 0f);
        var posTR = new Vector2(size.X - sizeTL.X, 0f);
        var posBR = new Vector2(size.X - sizeTL.X, size.Y - sizeTL.Y);
        var posBL = new Vector2(0f, size.Y - sizeTL.Y);

        // Corner Top Left
        this._batch.DrawCore(position + posTL, sizeTL,
            uvExtTL, uvIntTL, color);

        // Corner Top Right
        this._batch.DrawCore(position + posTR, sizeTR,
            new Vector2(uvIntBR.X, uvExtTL.Y), new Vector2(uvExtBR.X, uvIntTL.Y), color);

        // Corner Bottom Left
        this._batch.DrawCore(position + posBL, sizeBL,
            new Vector2(uvExtTL.X, uvIntBR.Y), new Vector2(uvIntTL.X, uvExtBR.Y), color);

        // Corner Bottom Right
        this._batch.DrawCore(position + posBR, sizeBR,
            uvIntBR, uvExtBR, color);



        var sizeTop = new Vector2(size.X - sizeTL.X - sizeTR.X, sizeTL.Y);
        var sizeBottom = new Vector2(size.X - sizeBL.X - sizeBR.X, sizeBL.Y);
        var sizeLeft = new Vector2(sizeTL.X, size.Y - sizeTL.Y - sizeBL.Y);
        var sizeRight = new Vector2(sizeTR.X, size.Y - sizeTR.Y - sizeBR.Y);

        // Lateral Top
        this._batch.DrawCore(position + new Vector2(sizeTL.X, 0f), sizeTop,
            new Vector2(uvIntTL.X, uvExtTL.Y), new Vector2(uvIntBR.X, uvIntTL.Y), color);

        // Lateral Bottom
        this._batch.DrawCore(position + new Vector2(sizeBL.X, size.Y - sizeBL.Y), sizeBottom,
            new Vector2(uvIntTL.X, uvIntBR.Y), new Vector2(uvIntBR.X, uvExtBR.Y), color);

        // Lateral Left
        this._batch.DrawCore(position + new Vector2(0f, sizeTL.Y), sizeLeft,
            new Vector2(uvExtTL.X, uvIntTL.Y), new Vector2(uvIntTL.X, uvIntBR.Y), color);

        // Lateral Right
        this._batch.DrawCore(position + new Vector2(size.X - sizeTR.X, sizeTR.Y), sizeRight,
            new Vector2(uvIntBR.X, uvIntTL.Y), new Vector2(uvExtBR.X, uvIntBR.Y), color);
    }

    public void DrawText(Font font, string text, Vector2 position, Color color)
    {
        // public float DrawText(IFontStashRenderer2 renderer, string text, Vector2 position, FSColor color, Vector2? scale = null, float rotation = 0, Vector2 origin = default(Vector2), float layerDepth = 0, float characterSpacing = 0, float lineSpacing = 0, TextStyle textStyle = TextStyle.None, FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0);

        var fsColor = new FontStashSharp.FSColor(color.R, color.G, color.B, color.A);
        var style = FontStashSharp.TextStyle.None;
        font.FontBase.DrawText(this, text, position, fsColor, Vector2.One, 0, Vector2.Zero, 0, 0, 0, style, font.Effect, font.EffectAmount);
    }

    private readonly ITexture2DManager _textureManager;

    ITexture2DManager IFontStashRenderer2.TextureManager => this._textureManager;

    void IFontStashRenderer2.DrawQuad(object texture, ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight, ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight)
    {
        var v1 = new SpriteBatch.Vertex(topLeft.Position, topLeft.TextureCoordinate, topLeft.Color.PackedValue);
        var v2 = new SpriteBatch.Vertex(topRight.Position, topRight.TextureCoordinate, topRight.Color.PackedValue);
        var v3 = new SpriteBatch.Vertex(bottomLeft.Position, bottomLeft.TextureCoordinate, bottomLeft.Color.PackedValue);
        var v4 = new SpriteBatch.Vertex(bottomRight.Position, bottomRight.TextureCoordinate, bottomRight.Color.PackedValue);
        this.DrawQuad((ITexture)texture, ref v1, ref v2, ref v3, ref v4);
    }

    public void DrawQuad(ITexture texture, ref SpriteBatch.Vertex topLeft, ref SpriteBatch.Vertex topRight, ref SpriteBatch.Vertex bottomLeft, ref SpriteBatch.Vertex bottomRight)
    {
        this.Prepare(this._defaultShader, texture, 1);
        this._batch.DrawCore(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
    }

    public void DrawRectangle(Vector2 position, Vector2 size, Color color)
    {
        this.DrawTexture(this._defaultShader, Texture.White, position, size, Vector2.Zero, Vector2.One, color);
    }

    public void PushScissorRectangle(Rect rect)
    {
        this.FlushBatch();
        this._clipRectStack.Push(rect);
        this._batch.RenderFlags |= RenderFlags.ScisorTest;
        this._commandList.SetScissorRect(0, (uint)rect.X, (uint)rect.Y, (uint)rect.Width, (uint)rect.Height);
    }

    public void PopScissorRectangle()
    {
        this.FlushBatch();

        if (this._clipRectStack.Count > 0)
        {
            this._clipRectStack.Pop();
            if (this._clipRectStack.Count > 0)
            {
                var rect = this._clipRectStack.Peek();
                this._batch.RenderFlags |= RenderFlags.ScisorTest;
                this._commandList.SetScissorRect(0, (uint)rect.X, (uint)rect.Y, (uint)rect.Width, (uint)rect.Height);
            }
            else
            {
                this._batch.RenderFlags &= ~RenderFlags.ScisorTest;
                this._commandList.SetFullScissorRect(0);
            }
        }
    }

    public void FlushBatch()
    {
        if (this._batch.Count == 0 || this._batch.Texture == null)
            return;

        this._commandList.UpdateBuffer(this.VertexBuffer, 0, this._batch.Items);

        if (this._batch.Shader != this._currentShaderInUse || this._batch.RenderFlags != this._currentPipelineFlags)
        {
            this._currentShaderInUse = this._batch.Shader ?? this._defaultShader;
            this._currentPipelineFlags = this._batch.RenderFlags;
            var pipeline = this._currentShaderInUse.GetPipeline(this._vertexFormat, this._currentPipelineFlags);

            this._commandList.SetPipeline(pipeline);
            this._commandList.SetGraphicsResourceSet(0, this._passResourceSet);
        }

        this._commandList.SetVertexBuffer(0, this.VertexBuffer);
        this._commandList.SetIndexBuffer(this._indexBuffer, IndexFormat.UInt16);

        var resourceSet = this._resourceSetCache.GetResourceSet(this._currentShaderInUse, this._batch.Texture);
        this._commandList.SetGraphicsResourceSet(1, resourceSet);
        this._commandList.DrawIndexed((uint)this._batch.Count * 6);

        this._batch.Clear();
        this.DrawCallCount++;
    }


    public void Dispose()
    {
        this._indexBuffer.Dispose();
        this.VertexBuffer.Dispose();
        this._matrixBuffer.Dispose();
        this.PassResourceLayout.Dispose();
        this._passResourceSet.Dispose();
        this._resourceSetCache.Dispose();
    }



    private class FontStashTextureManager : ITexture2DManager
    {
        private readonly GraphicsDevice _gd;

        public FontStashTextureManager(GraphicsDevice gd)
        {
            this._gd = gd;
        }

        object ITexture2DManager.CreateTexture(int width, int height)
        {
            return new DirectTexture(this._gd, (uint)width, (uint)height);
        }

        void ITexture2DManager.SetTextureData(object texture, System.Drawing.Rectangle bounds, byte[] data)
        {
            ((DirectTexture)texture).Update(data, bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }

        System.Drawing.Point ITexture2DManager.GetTextureSize(object texture)
        {
            var texture2D = (DirectTexture)texture;
            return new System.Drawing.Point((int)texture2D.Width, (int)texture2D.Height);
        }
    }
}
