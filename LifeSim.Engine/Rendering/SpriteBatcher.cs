using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using FontStashSharp.Interfaces;
using LifeSim.Engine.Resources;
using LifeSim.Engine.SceneGraph;
using Veldrid;

namespace LifeSim.Engine.Rendering;

public class SpriteBatcher : IFontStashRenderer, IDisposable
{




    private struct CachedResourceSetKey
    {
        public ITexture Texture { get; set; }
        public Shader Shader { get; set; }

        public CachedResourceSetKey(ITexture texture, Shader shader)
        {
            this.Texture = texture;
            this.Shader = shader;
        }
    }

    private readonly Dictionary<CachedResourceSetKey, ResourceSet> _cachedResourceSets = new Dictionary<CachedResourceSetKey, ResourceSet>();


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

    public SpriteBatcher(GraphicsDevice gd, Shader defaultShader, ResourceSet passResourceSet)
    {
        this._gd = gd;
        this._defaultShader = defaultShader;
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

        this._passResourceSet = passResourceSet;
    }



    private void Prepare(Shader shader, ITexture texture)
    {
        if (this._batch.Shader == shader && this._batch.Texture == texture)
        {
            return;
        }

        this.FlushBatch();

        this._batch.Shader = shader;
        this._batch.Texture = texture;
    }

    private ResourceSet GetResourceSet(Shader shader, ITexture texture)
    {
        var key = new CachedResourceSetKey(texture, shader);
        if (this._cachedResourceSets.TryGetValue(key, out var resourceSet))
        {
            return resourceSet;
        }

        resourceSet = this._gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
            shader.MaterialResourceLayout, texture.DeviceTexture, texture.Sampler));

        this._cachedResourceSets.Add(key, resourceSet);

        return resourceSet;
    }

    public void BeginBatch(CommandList cl)
    {
        this._commandList = cl;
        this.TotalSpritesToDraw = 0;
        this._batch.Clear();
        this._currentShader = null!;
    }

    public void Draw(Shader? shader, ITexture texture, Vector2 position, Vector2 size)
    {
        this.Draw(shader, texture, position, size, Vector2.Zero, Vector2.One, Color.White);
    }

    public void Draw(Shader? shader, ITexture texture, Vector2 position, Vector2 size, Vector2 uvTopLeft, Vector2 uvBottomRight, in Matrix3x2 transform, Color color)
    {
        this.Prepare(shader ?? this._defaultShader, texture);
        this.DrawCore(position, size, uvTopLeft, uvBottomRight, in transform, color);
    }

    public void Draw(Shader? shader, ITexture texture, Vector2 position, Vector2 size, Vector2 uvTopLeft, Vector2 uvBottomRight, Color color)
    {
        this.Prepare(shader ?? this._defaultShader, texture);
        this.DrawCore(position, size, uvTopLeft, uvBottomRight, color);
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

        this.DrawCore(pos, size, uvTopLeft, uvBottomRight, color, scale, rotation, origin, depth);
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

        if (drawCenter)
        {

            if (sizeSegmentCenter.X > 0 && sizeSegmentCenter.Y > 0)
            {
                this.Draw(shader, texture, -pivot + sizeTL, sizeSegmentCenter, uvTL, uvBR, in worldMatrix, color);
            }
        }

        var posTL = new Vector2(0f, 0f);
        var posTR = new Vector2(size.X - sizeTL.X, 0f);
        var posBR = new Vector2(size.X - sizeTL.X, size.Y - sizeTL.Y);
        var posBL = new Vector2(0f, size.Y - sizeTL.Y);

        // Corner Top Left
        this.Draw(shader, texture, -pivot + posTL, sizeTL, Vector2.Zero, uvTL, in worldMatrix, color);
        // Corner Top Right
        this.Draw(shader, texture, -pivot + posTR, sizeTR, new Vector2(uvBR.X, 0f), new Vector2(1f, uvTL.Y), in worldMatrix, color);
        // Corner Bottom Left
        this.Draw(shader, texture, -pivot + posBL, sizeBL, new Vector2(0f, uvBR.Y), new Vector2(uvTL.X, 1f), in worldMatrix, color);
        // Corner Bottom Right
        this.Draw(shader, texture, -pivot + posBR, sizeBR, uvBR, Vector2.One, in worldMatrix, color);


        var sizeTop = new Vector2(size.X - sizeTL.X - sizeTR.X, sizeTL.Y);
        var sizeBottom = new Vector2(size.X - sizeBL.X - sizeBR.X, sizeBL.Y);
        var sizeLeft = new Vector2(sizeTL.X, size.Y - sizeTL.Y - sizeBL.Y);
        var sizeRight = new Vector2(sizeTR.X, size.Y - sizeTR.Y - sizeBR.Y);

        // Lateral Top
        this.Draw(shader, texture, -pivot + new Vector2(sizeTL.X, 0f), sizeTop, new Vector2(uvTL.X, 0f), new Vector2(uvBR.X, uvTL.Y), in worldMatrix, color);
        // Lateral Bottom
        this.Draw(shader, texture, -pivot + new Vector2(sizeBL.X, size.Y - sizeBL.Y), sizeBottom, new Vector2(uvTL.X, uvBR.Y), new Vector2(uvBR.X, 1f), in worldMatrix, color);
        // Lateral Left
        this.Draw(shader, texture, -pivot + new Vector2(0f, sizeTL.Y), sizeLeft, new Vector2(0f, uvTL.Y), new Vector2(uvTL.X, uvBR.Y), in worldMatrix, color);
        // Lateral Right
        this.Draw(shader, texture, -pivot + new Vector2(size.X - sizeTR.X, sizeTR.Y), sizeRight, new Vector2(uvBR.X, uvTL.Y), new Vector2(1f, uvBR.Y), in worldMatrix, color);
    }

    public void DrawText(Font font, string text, int fontSize, Vector2 position, Color color)
    {
        font.GetFont(fontSize).DrawText(this, text, position, color);
    }

    public void DrawCore(Vector2 position, Vector2 size, Vector2 uvTopLeft, Vector2 uvBottomRight, Color color)
    {
        this._batch.Add(new SpriteBatch.Item
        {
            TopLeft = new SpriteBatch.Vertex(position.X, position.Y, 0f, uvTopLeft.X, uvTopLeft.Y, color),
            TopRight = new SpriteBatch.Vertex(position.X + size.X, position.Y, 0f, uvBottomRight.X, uvTopLeft.Y, color),
            BottomLeft = new SpriteBatch.Vertex(position.X, position.Y + size.Y, 0f, uvTopLeft.X, uvBottomRight.Y, color),
            BottomRight = new SpriteBatch.Vertex(position.X + size.X, position.Y + size.Y, 0f, uvBottomRight.X, uvBottomRight.Y, color),
        });

        this.TotalSpritesToDraw++;
    }

    public void DrawCore(Vector2 position, Vector2 size, Vector2 uvTopLeft, Vector2 uvBottomRight, Color color, Vector2 scale, float rotation, Vector2 origin, float depth = 0f)
    {
        // Adapted from https://github.com/ThomasMiz/TrippyGL/blob/109eaf483d3289c0214963b7d22bdbd320d243ed/TrippyGL/TextureBatchItem.cs#L90
        // Thank you! :D 
        float sin = MathF.Sin(rotation);
        float cos = MathF.Cos(rotation);

        var tl = -origin * scale;
        var tr = new Vector2(tl.X + size.X * scale.X, tl.Y);
        var bl = new Vector2(tl.X, tl.Y + size.Y * scale.Y);
        var br = new Vector2(tr.X, bl.Y);

        var tlPos = new Vector3(cos * tl.X - sin * tl.Y + position.X, sin * tl.X + cos * tl.Y + position.Y, depth);
        var trPos = new Vector3(cos * tr.X - sin * tr.Y + position.X, sin * tr.X + cos * tr.Y + position.Y, depth);
        var blPos = new Vector3(cos * bl.X - sin * bl.Y + position.X, sin * bl.X + cos * bl.Y + position.Y, depth);
        var brPos = new Vector3(cos * br.X - sin * br.Y + position.X, sin * br.X + cos * br.Y + position.Y, depth);

        var tlUVs = uvTopLeft;
        var trUVs = new Vector2(uvBottomRight.X, uvTopLeft.Y);
        var blUVs = new Vector2(uvTopLeft.X, uvBottomRight.Y);
        var brUVs = uvBottomRight;

        this._batch.Add(new SpriteBatch.Item
        {
            TopLeft = new SpriteBatch.Vertex(tlPos, tlUVs, color),
            TopRight = new SpriteBatch.Vertex(trPos, trUVs, color),
            BottomLeft = new SpriteBatch.Vertex(blPos, blUVs, color),
            BottomRight = new SpriteBatch.Vertex(brPos, brUVs, color),
        });
    }

    public void DrawCore(Vector2 position, Vector2 size, Vector2 uvTopLeft, Vector2 uvBottomRight, in Matrix3x2 transform, Color color, float depth = 0f)
    {
        var tl = position;
        var tr = position + new Vector2(size.X, 0f);
        var bl = position + new Vector2(0f, size.Y);
        var br = position + size;

        var tlUVs = uvTopLeft;
        var trUVs = new Vector2(uvBottomRight.X, uvTopLeft.Y);
        var blUVs = new Vector2(uvTopLeft.X, uvBottomRight.Y);
        var brUVs = uvBottomRight;

        this._batch.Add(new SpriteBatch.Item
        {
            TopLeft = new SpriteBatch.Vertex(Vector2.Transform(tl, transform), depth, tlUVs, color),
            TopRight = new SpriteBatch.Vertex(Vector2.Transform(tr, transform), depth, trUVs, color),
            BottomLeft = new SpriteBatch.Vertex(Vector2.Transform(bl, transform), depth, blUVs, color),
            BottomRight = new SpriteBatch.Vertex(Vector2.Transform(br, transform), depth, brUVs, color),
        });
    }

    private Shader _currentShader = null!;

    public void FlushBatch()
    {
        if (this._batch.Count == 0 || this._batch.Texture == null)
            return;

        this._commandList.UpdateBuffer(this.VertexBuffer, 0, this._batch.Items);

        if (this._batch.Shader != this._currentShader)
        {
            this._currentShader = this._batch.Shader ?? this._defaultShader;
            var pipeline = this._currentShader.GetPipeline(this._vertexFormat);

            this._commandList.SetPipeline(pipeline);
            this._commandList.SetGraphicsResourceSet(0, this._passResourceSet);
        }

        this._commandList.SetVertexBuffer(0, this.VertexBuffer);
        this._commandList.SetIndexBuffer(this._indexBuffer, IndexFormat.UInt16);


        var resourceSet = this.GetResourceSet(this._currentShader, this._batch.Texture);
        this._commandList.SetGraphicsResourceSet(1, resourceSet);
        this._commandList.DrawIndexed(
            indexCount: (uint)this._batch.Count * 6,
            instanceCount: 1,
            indexStart: 0,
            vertexOffset: 0,
            instanceStart: 0
        );

        this._batch.Clear();
    }


    public void Dispose()
    {
        this._indexBuffer.Dispose();
        this.VertexBuffer.Dispose();
    }
}