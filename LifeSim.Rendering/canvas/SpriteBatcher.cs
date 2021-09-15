using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using FontStashSharp.Interfaces;
using LifeSim.Core;
using Veldrid;

namespace LifeSim.Rendering
{
    public class SpriteBatcher : IFontStashRenderer, IDisposable
    {
        private int _maxBatchSize = 1000;
        private int _totalSpritesToDraw = 0;
        public int totalSpritesToDraw => this._totalSpritesToDraw;

        private List<SpriteBatch> _batches = new List<SpriteBatch>();
        private SwapPopList<SpriteBatch> _freeList = new SwapPopList<SpriteBatch>();
        
        private Shader _defaultShader;

        private readonly DeviceBuffer _indexBuffer;

        private SpritesPass _pass;

        private GraphicsDevice _gd;
        public SpriteBatcher(GraphicsDevice gd, SpritesPass pass)
        {
            this._gd = gd;
            this._pass = pass;
            this._defaultShader = pass.shader;

            var indexBufferSize = (uint) (sizeof(ushort) * 6 * this._maxBatchSize);

            var factory = this._gd.ResourceFactory;

            this._indexBuffer = factory.CreateBuffer(new BufferDescription((uint) indexBufferSize, BufferUsage.IndexBuffer));
            ushort[] indices = new ushort[this._maxBatchSize * 6];

            for (int i = 0; i < this._maxBatchSize; i++) {
                int j = i * 6;
                int offset = i * 4;
                indices[j + 0] = (ushort) (offset + 0);
                indices[j + 1] = (ushort) (offset + 2);
                indices[j + 2] = (ushort) (offset + 1);
                indices[j + 3] = (ushort) (offset + 0);
                indices[j + 4] = (ushort) (offset + 3);
                indices[j + 5] = (ushort) (offset + 2);
            }

            this._gd.UpdateBuffer(this._indexBuffer, 0, indices);

        }

        private SpriteBatch _FindBatch(Shader shader, Texture texture)
        {
            for (int i = 0; i < this._batches.Count; i++) {
                var batch = this._batches[i];
                if (batch.texture == texture && batch.shader == shader && ! batch.isFull) {
                    return batch;
                }
            }

            
            if (this._freeList.Count > 0) {
                for (int i = 0; i < this._freeList.Count; i++) {
                    var batch = this._freeList[i];
                    if (batch.texture == texture && batch.shader == shader && ! batch.isFull) {
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
            this._totalSpritesToDraw = 0;

            for (int i = 0; i < this._batches.Count; i++) {
                this._batches[i].Clear();
                this._freeList.Add(this._batches[i]);
            }
            this._batches.Clear();
        }

        public Veldrid.DeviceBuffer indexBuffer => this._indexBuffer;

        public IReadOnlyList<SpriteBatch> batches => this._batches;
        
        public void Draw(Texture texture, Vector2 position, Vector2 size)
        {
            this.Draw(texture, position, size, Vector2.Zero, Vector2.One, Color.white, 0f);
        }

        public void Draw(Texture texture, Vector2 position, Vector2 size, Vector2 uvTopLeft, Vector2 uvBottomRight, in Matrix3x2 transform, Color color, float depth = 0f)
        {
            this._FindBatch(this._defaultShader, texture)
                .Draw(position, size, uvTopLeft, uvBottomRight, in transform, color, depth);

            this._totalSpritesToDraw++;
        }

        public void Draw(Texture texture, Vector2 position, Vector2 size, Vector2 uvTopLeft, Vector2 uvBottomRight, Color color, float depth = 0f)
        {
            this._FindBatch(this._defaultShader, texture)
                .Draw(position, size, uvTopLeft, uvBottomRight, color, depth);

            this._totalSpritesToDraw++;
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
            this._FindBatch(this._defaultShader, (Texture) texture)
                .Draw(position, sourceRectangle, color, rotation, origin, scale, depth);

            this._totalSpritesToDraw++;
        }


        public void Dispose()
        {
            this._indexBuffer.Dispose();
            for (int i = 0; i < this._batches.Count; i++) {
                this._batches[i].Dispose();
            }
            for (int i = 0; i < this._freeList.Count; i++) {
                this._freeList[i].Dispose();
            }
        }
    }
}