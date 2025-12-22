using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using FontStashSharp;
using LifeSim.Imago.Assets.Materials;
using LifeSim.Imago.Assets.Meshes;
using LifeSim.Imago.Assets.Textures;
using LifeSim.Support.Drawing;
using LifeSim.Support.Numerics;
using Veldrid;
using Shader = LifeSim.Imago.Assets.Materials.Shader;
using Texture = LifeSim.Imago.Assets.Textures.Texture;

namespace LifeSim.Imago.Rendering.Sprites;

/// <summary>
/// Provides a context for drawing 2D sprites, text, and other graphical elements.
/// </summary>
public class DrawingContext : IDisposable
{
    /// <summary>
    /// Gets the total number of sprites to draw.
    /// </summary>
    public int TotalSpritesToDraw { get; private set; } = 0;

    /// <summary>
    /// Gets the Veldrid vertex buffer.
    /// </summary>
    public DeviceBuffer VertexBuffer { get; private set; }

    /// <summary>
    /// Gets the resource layout for the pass.
    /// </summary>
    public ResourceLayout PassResourceLayout { get; }

    /// <summary>
    /// Gets the number of draw calls.
    /// </summary>
    public int DrawCallCount { get; private set; } = 0;

    private Shader _currentShaderInUse = null!;

    private readonly Shader _defaultShader;

    private readonly GraphicsDevice _gd;

    private readonly DeviceBuffer _indexBuffer;

    private readonly int _capacity = 1000;

    private readonly SpriteBatch _batch;

    private readonly VertexFormat _vertexFormat;

    private readonly FontStashAdapter _fontStashAdapter;

    private readonly ResourceSet _passResourceSet;

    private readonly DeviceBuffer _matrixBuffer;

    private readonly ResourceSetCache _resourceSetCache;

    private readonly Stack<Rect> _clipRectStack = new Stack<Rect>();

    private readonly Stack<float> _opacityStack = new Stack<float>();

    private RenderFlags _currentPipelineFlags = RenderFlags.None;

    private CommandList _commandList = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="DrawingContext"/> class.
    /// </summary>
    /// <param name="gd">The graphics device.</param>
    /// <param name="defaultShader">The default shader to use.</param>
    internal DrawingContext(GraphicsDevice gd, Shader defaultShader)
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

        this._fontStashAdapter = new FontStashAdapter(this._gd, this);
    }


    private void Prepare(ITexture texture, int requiredQuads = 1)
    {
        Shader shader = this._defaultShader;
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

    /// <summary>
    /// Begins the frame and sets up the sprite batcher for drawing.
    /// This method should be called before any drawing.
    /// </summary>
    /// <param name="cl">The command list to use.</param>
    public void Begin(CommandList cl)
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
    }

    /// <summary>
    /// Sets the view projection matrix for rendering transformations.
    /// </summary>
    /// <param name="viewProjectionMatrix">The view projection matrix to use.</param>
    public void SetViewProjectionMatrix(Matrix4x4 viewProjectionMatrix)
    {
        this._commandList.UpdateBuffer(this._matrixBuffer, 0, ref viewProjectionMatrix);
    }

    /// <summary>
    /// Ends the frame and flushes any remaining batched sprites. This method should be called after all drawing.
    /// </summary>
    public void End()
    {
        this.FlushBatch();
    }

    /// <summary>
    /// Draws a texture at the specified position.
    /// </summary>
    /// <param name="texture">The texture to draw.</param>
    /// <param name="position">The position to draw the texture at.</param>
    /// <param name="size">The size of the texture to draw.</param>
    public void DrawTexture(ITexture texture, Vector2 position, Vector2 size)
    {
        this.DrawTexture(texture, position, size, Vector2.Zero, Vector2.One, Color.White);
    }

    /// <summary>
    /// Draws a texture at the specified position.
    /// </summary>
    /// <param name="texture">The texture to draw.</param>
    /// <param name="position">The position to draw the texture at.</param>
    /// <param name="size">The size of the texture to draw.</param>
    public void DrawTexture(ITextureRegion texture, Vector2 position, Vector2 size)
    {
        this.DrawTexture(texture.Texture, position, size, texture.TransformUV(Vector2.Zero), texture.TransformUV(Vector2.One), Color.White);
    }

    /// <summary>
    /// Draws a texture at the specified position with a transformation matrix.
    /// </summary>
    /// <param name="texture">The texture to draw.</param>
    /// <param name="position">The position to draw the texture at.</param>
    /// <param name="size">The size of the texture to draw.</param>
    /// <param name="uvTopLeft">The top left UV coordinate of the texture.</param>
    /// <param name="uvBottomRight">The bottom right UV coordinate of the texture.</param>
    /// <param name="transform">A transformation matrix to apply to the texture.</param>
    /// <param name="color">The color to draw the texture with.</param>
    public void DrawTexture(ITexture texture, Vector2 position, Vector2 size, Vector2 uvTopLeft, Vector2 uvBottomRight, in Matrix3x2 transform, Color color)
    {
        this.Prepare(texture);
        this._batch.DrawCore(position, size, uvTopLeft, uvBottomRight, in transform, color);
    }

    /// <summary>
    /// Draws a texture at the specified position.
    /// </summary>
    /// <param name="texture">The texture to draw.</param>
    /// <param name="position">The position to draw the texture at.</param>
    /// <param name="size">The size of the texture to draw.</param>
    /// <param name="uvTopLeft">The top left UV coordinate of the texture.</param>
    /// <param name="uvBottomRight">The bottom right UV coordinate of the texture.</param>
    /// <param name="color">The color to draw the texture with.</param>
    public void DrawTexture(ITexture texture, Vector2 position, Vector2 size, Vector2 uvTopLeft, Vector2 uvBottomRight, Color color)
    {
        this.Prepare(texture);
        this._batch.DrawCore(position, size, uvTopLeft, uvBottomRight, color);
    }

    /// <summary>
    /// Draws a texture using a 9-patch scaling algorithm. This method is useful for drawing scalable UI elements.
    /// </summary>
    /// <param name="texture">The texture to draw.</param>
    /// <param name="position">The position to draw the texture at.</param>
    /// <param name="size">The size of the texture to draw.</param>
    /// <param name="patchMargin">The margin of the unstretchable area of the texture. Any area inside this margin will be stretched.</param>
    /// <param name="color">The color to draw the texture with.</param>
    /// <param name="scale">The scale to apply to the texture.</param>
    public void DrawNinePatch(ITextureRegion texture, Vector2 position, Vector2 size, Thickness patchMargin, Color color, float scale)
    {
        var sizeTL = new Vector2(patchMargin.Left, patchMargin.Top);
        var sizeBR = new Vector2(patchMargin.Right, patchMargin.Bottom);
        var sizeTR = new Vector2(patchMargin.Top, patchMargin.Right);
        var sizeBL = new Vector2(patchMargin.Left, patchMargin.Bottom) ;

        var texelUV = Vector2.One / texture.Texture.Size;
        var uvExtTL = texture.TopLeft;
        var uvExtBR = texture.BottomRight;
        var uvIntTL = uvExtTL + sizeTL * texelUV;
        var uvIntBR = uvExtBR - sizeBR * texelUV;

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

        this.Prepare(texture.Texture, 9);

        if (sizeSegmentCenter.X > 0 && sizeSegmentCenter.Y > 0)
        {
            this._batch.DrawCore(position + sizeTL, sizeSegmentCenter, uvIntTL, uvIntBR, color);
        }

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

    /// <summary>
    /// Draws the given text at the specified position.
    /// </summary>
    /// <param name="font">The font to use.</param>
    /// <param name="text">The text to draw.</param>
    /// <param name="position">The position to draw the text at.</param>
    /// <param name="color">The color to draw the text with.</param>
    /// <param name="effect">The effect to apply to the text.</param>
    /// <param name="effectAmount">The amount of the effect to apply.</param>
    public void DrawText(SpriteFontBase font, string text, Vector2 position, Color color, FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0)
    {
        // TODO: Use a LRU cache for text strings. Use Span<char> for the "text" parameter and only call .ToString() if the string is not in the cache.
        // Idk if this is worth it, but it could be a nice optimization.

        var fsColor = new FSColor(color.R, color.G, color.B, color.A);
        var style = TextStyle.None;
        font.DrawText(this._fontStashAdapter, text, position, fsColor, Vector2.One, 0, Vector2.Zero, 0, 0, 0, style, effect, effectAmount);
    }

    /// <summary>
    /// Draws a quad with the specified vertices.
    /// </summary>
    /// <param name="texture">The texture to use.</param>
    /// <param name="topLeft">The top left vertex of the quad.</param>
    /// <param name="topRight">The top right vertex of the quad.</param>
    /// <param name="bottomLeft">The bottom left vertex of the quad.</param>
    /// <param name="bottomRight">The bottom right vertex of the quad.</param>
    public void DrawQuad(ITexture texture, ref SpriteVertex topLeft, ref SpriteVertex topRight, ref SpriteVertex bottomLeft, ref SpriteVertex bottomRight)
    {
        this.Prepare(texture, 1);
        this._batch.DrawCore(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
    }

    /// <summary>
    /// Draws a solid color rectangle at the specified position.
    /// </summary>
    /// <param name="position">The position to draw the rectangle at.</param>
    /// <param name="size">The size of the rectangle to draw.</param>
    /// <param name="color">The color to draw the rectangle with.</param>
    public void DrawRectangle(Vector2 position, Vector2 size, Color color)
    {
        this.DrawTexture(Texture.White, position, size, Vector2.Zero, Vector2.One, color);
    }

    /// <summary>
    /// Begins drawing with a new opacity value.
    /// </summary>
    /// <param name="opacity">The opacity to use.</param>
    public void PushOpacity(float opacity)
    {
        opacity *= this._opacityStack.Peek();
        this._opacityStack.Push(opacity);
        this._batch.Opacity = opacity;
    }

    /// <summary>
    /// Ends drawing with the current opacity value.
    /// </summary>
    public void PopOpacity()
    {
        this._opacityStack.Pop();
        this._batch.Opacity = this._opacityStack.Peek();
    }

    /// <summary>
    /// Begins drawing with a new scissor rectangle mask applied. This method will clip all drawing to the specified rectangle.
    /// </summary>
    /// <param name="rect">The rectangle to clip to.</param>
    public void PushScissorRectangle(Rect rect)
    {
        this.FlushBatch();
        this._clipRectStack.Push(rect);
        this._batch.RenderFlags |= RenderFlags.ScisorTest;
        this._commandList.SetScissorRect(0, (uint)rect.X, (uint)rect.Y, (uint)rect.Width, (uint)rect.Height);
    }

    /// <summary>
    /// Ends drawing with the current scissor rectangle mask applied.
    /// </summary>
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

    /// <summary>
    /// Flushes the current batch of sprites to the command list.
    /// </summary>
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

    /// <summary>
    /// Disposes of the sprite batcher.
    /// </summary>
    public void Dispose()
    {
        this._indexBuffer.Dispose();
        this.VertexBuffer.Dispose();
        this._matrixBuffer.Dispose();
        this.PassResourceLayout.Dispose();
        this._passResourceSet.Dispose();
        this._resourceSetCache.Dispose();
    }
}
