using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.Rendering;
using LifeSim.Engine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Linq;

namespace LifeSim.Assets
{
    public class Atlas 
    {
        public readonly struct Result<T>
        {
            public readonly Vector2 uv1;
            public readonly Vector2 uv2;
            public readonly T element;

            public Result(Vector2 uv1, Vector2 uv2, T element)
            {
                this.uv1 = uv1;
                this.uv2 = uv2;
                this.element = element;
            }
        }

        private uint _tileSize;
        private uint _atlasSize;
        
        private ResourceFactory _assetManager;

        private GPUTexture _texture;
        private Image<Rgba32> _image;
        private Node _root;

        private bool _isDirty = false;

        public bool isDirty => this._isDirty;

        public Atlas(ResourceFactory assetManager, uint atlasSize, uint tileSize)
        {
            tileSize = this._NextPowOfTwo(tileSize);
            atlasSize = (uint) 1 << BitOperations.Log2(atlasSize);
            var mipMapLevels = (uint) BitOperations.Log2(tileSize);

            this._assetManager = assetManager;
            this._root = new Node(0, 0, atlasSize / tileSize, atlasSize / tileSize);
            this._tileSize = tileSize;
            this._atlasSize = atlasSize;

            this._image = new Image<Rgba32>((int) atlasSize, (int) atlasSize);
            this._texture = this._assetManager.MakeTexture(this._image, mipMapLevels);
        }

        private uint _NextPowOfTwo(uint x)
        {
            --x;
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            return x + 1;
        }

        private int _GetFillArea(int size)
        {
            return (int) (MathF.Ceiling(size / this._tileSize) * this._tileSize);
        }

        public GPUTexture texture => this._texture;

        public void Apply()
        {
            if (! this._isDirty) return;
            this._texture.Update(this._image);
            this._isDirty = false;
        }

        public IEnumerable<Result<T>> Pack<T>(IEnumerable<T> elements) where T : IDrawOperation
        {
            var results = elements.OrderByDescending(a => a.size.x * a.size.y).Select(this.PackOne);
            this._texture.Update(this._image);
            return results;
        }

        public Result<T> PackOne<T>(T element) where T : IDrawOperation
        {
            uint w = (uint) MathF.Ceiling(element.size.x / (float) this._tileSize);
            uint h = (uint) MathF.Ceiling(element.size.y / (float) this._tileSize);
            Node? node = this._root.Find(w, h);
            if (node == null) {
                throw new System.Exception("Cannot fit the rectangles in the atlas");
            }

            node.Split(w, h);
            var coords = new Vector2Int(node.x, node.y) * this._tileSize;
            var fillSize = new Vector2Int(w * this._tileSize, h * this._tileSize);
            element.Draw(this._image, coords, fillSize);
            this._isDirty = true;

            Vector2 imgSize = new Vector2(this._image.Width, this._image.Height);
            Vector2 uv1 = coords / imgSize;
            Vector2 uv2 = (coords + element.size) / imgSize;
            return new Result<T>(uv1, uv2, element);
        }

        private class Node
        {
            public uint x;
            public uint y;
            public uint width;
            public uint height;
            public Node? down = null;
            public Node? right = null;

            public Node(uint x, uint y, uint width, uint height)
            {
                this.x = x;
                this.y = y;
                this.width = width;
                this.height = height;
            }

            public void Split(uint width, uint height) 
            {
                this.down  = new Node( this.x        , this.y + height, this.width        , this.height - height );
                this.right = new Node( this.x + width, this.y         , this.width - width, height               );
            }

            public Node? Find(uint width, uint height) 
            {
                if (this.right != null) {
                    return this.right.Find(width, height) ?? this.down?.Find(width, height);
                } else if ((width <= this.width) && (height <= this.height)) {
                    return this;
                } else {
                    return null;
                }
            }

        }
    }
}