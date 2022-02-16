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
    private readonly int _maxBatchSize = 1000;

    public int TotalSpritesToDraw { get; private set; } = 0;

    private readonly List<SpriteBatch> _batches = new List<SpriteBatch>();
    private readonly SwapPopList<SpriteBatch> _freeList = new SwapPopList<SpriteBatch>();
    private readonly Shader _defaultShader;
    private readonly GraphicsDevice _gd;

    public SpriteBatcher(GraphicsDevice gd, Shader defaultShader)
    {
        this._gd = gd;
        this._defaultShader = defaultShader;

        var indexBufferSize = (uint) (sizeof(ushort) * 6 * this._maxBatchSize);

        var factory = this._gd.ResourceFactory;

        this.IndexBuffer = factory.CreateBuffer(new BufferDescription((uint)indexBufferSize, BufferUsage.IndexBuffer));
        ushort[] indices = new ushort[this._maxBatchSize * 6];

        for (int i = 0; i < this._maxBatchSize; i++)
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

        this._gd.UpdateBuffer(this.IndexBuffer, 0, indices);

    }

    private SpriteBatch FindBatch(Shader shader, ITexture texture)
    {
        for (int i = 0; i < this._batches.Count; i++)
        {
            var batch = this._batches[i];
            if (batch.Texture == texture && batch.Shader == shader && !batch.IsFull)
            {
                return batch;
            }
        }


        if (this._freeList.Count > 0)
        {
            for (int i = 0; i < this._freeList.Count; i++)
            {
                var batch = this._freeList[i];
                if (batch.Texture == texture && batch.Shader == shader && !batch.IsFull)
                {
                    this._batches.Add(batch);
                    this._freeList.RemoveAt(i);
                    return batch;
                }
            }

            var anyFreeBatch = this._freeList[0];
            anyFreeBatch.SetMaterial(shader, texture);
            this._batches.Add(anyFreeBatch);
            this._freeList.RemoveAt(0);
            return anyFreeBatch;

        }

        var newBatch = new SpriteBatch(this._gd, shader, texture, this._maxBatchSize);
        this._batches.Add(newBatch);
        return newBatch;
    }

    public void BeginBatch()
    {
        this.TotalSpritesToDraw = 0;

        for (int i = 0; i < this._batches.Count; i++)
        {
            this._batches[i].Clear();
            this._freeList.Add(this._batches[i]);
        }
        this._batches.Clear();
    }

    public DeviceBuffer IndexBuffer { get; }

    public IReadOnlyList<SpriteBatch> Batches => this._batches;

    public void Draw(Shader? shader, ITexture texture, Vector2 position, Vector2 size)
    {
        this.Draw(shader, texture, position, size, Vector2.Zero, Vector2.One, Color.White, 0f);
    }

    public void Draw(Shader? shader, ITexture texture, Vector2 position, Vector2 size, Vector2 uvTopLeft, Vector2 uvBottomRight, in Matrix3x2 transform, Color color, float depth = 0f)
    {
        this.FindBatch(shader ?? this._defaultShader, texture)
            .Draw(position, size, uvTopLeft, uvBottomRight, in transform, color, depth);

        this.TotalSpritesToDraw++;
    }

    public void Draw(Shader? shader, ITexture texture, Vector2 position, Vector2 size, Vector2 uvTopLeft, Vector2 uvBottomRight, Color color, float depth = 0f)
    {
        this.FindBatch(shader ?? this._defaultShader, texture)
            .Draw(position, size, uvTopLeft, uvBottomRight, color, depth);

        this.TotalSpritesToDraw++;
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
        this.FindBatch(this._defaultShader, (ITexture)texture)
            .Draw(position, sourceRectangle, color, rotation, origin, scale, depth);

        this.TotalSpritesToDraw++;
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

        float depth = 0f;

        if (drawCenter)
        {

            if (sizeSegmentCenter.X > 0 && sizeSegmentCenter.Y > 0)
            {
                this.Draw(shader, texture, -pivot + sizeTL, sizeSegmentCenter, uvTL, uvBR, in worldMatrix, color, depth);
            }
        }

        var posTL = new Vector2(0f, 0f);
        var posTR = new Vector2(size.X - sizeTL.X, 0f);
        var posBR = new Vector2(size.X - sizeTL.X, size.Y - sizeTL.Y);
        var posBL = new Vector2(0f, size.Y - sizeTL.Y);

        // Corner Top Left
        this.Draw(shader, texture, -pivot + posTL, sizeTL, Vector2.Zero, uvTL, in worldMatrix, color, depth);
        // Corner Top Right
        this.Draw(shader, texture, -pivot + posTR, sizeTR, new Vector2(uvBR.X, 0f), new Vector2(1f, uvTL.Y), in worldMatrix, color, depth);
        // Corner Bottom Left
        this.Draw(shader, texture, -pivot + posBL, sizeBL, new Vector2(0f, uvBR.Y), new Vector2(uvTL.X, 1f), in worldMatrix, color, depth);
        // Corner Bottom Right
        this.Draw(shader, texture, -pivot + posBR, sizeBR, uvBR, Vector2.One, in worldMatrix, color, depth);


        var sizeTop = new Vector2(size.X - sizeTL.X - sizeTR.X, sizeTL.Y);
        var sizeBottom = new Vector2(size.X - sizeBL.X - sizeBR.X, sizeBL.Y);
        var sizeLeft = new Vector2(sizeTL.X, size.Y - sizeTL.Y - sizeBL.Y);
        var sizeRight = new Vector2(sizeTR.X, size.Y - sizeTR.Y - sizeBR.Y);

        // Lateral Top
        this.Draw(shader, texture, -pivot + new Vector2(sizeTL.X, 0f), sizeTop, new Vector2(uvTL.X, 0f), new Vector2(uvBR.X, uvTL.Y), in worldMatrix, color, depth);
        // Lateral Bottom
        this.Draw(shader, texture, -pivot + new Vector2(sizeBL.X, size.Y - sizeBL.Y), sizeBottom, new Vector2(uvTL.X, uvBR.Y), new Vector2(uvBR.X, 1f), in worldMatrix, color, depth);
        // Lateral Left
        this.Draw(shader, texture, -pivot + new Vector2(0f, sizeTL.Y), sizeLeft, new Vector2(0f, uvTL.Y), new Vector2(uvTL.X, uvBR.Y), in worldMatrix, color, depth);
        // Lateral Right
        this.Draw(shader, texture, -pivot + new Vector2(size.X - sizeTR.X, sizeTR.Y), sizeRight, new Vector2(uvBR.X, uvTL.Y), new Vector2(1f, uvBR.Y), in worldMatrix, color, depth);
    }

    public void DrawText(Font font, string text, int fontSize, Vector2 position, Color color, float depth = 0f)
    {
        font.GetFont(fontSize).DrawText(this, text, position, color);
    }

    public void Dispose()
    {
        this.IndexBuffer.Dispose();
        for (int i = 0; i < this._batches.Count; i++)
        {
            this._batches[i].Dispose();
        }
        for (int i = 0; i < this._freeList.Count; i++)
        {
            this._freeList[i].Dispose();
        }
    }
}