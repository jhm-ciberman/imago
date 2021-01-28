using System.Collections.Generic;
using static LifeSim.Assets.BinPacker;
using System.Numerics;
using LifeSim.Engine.Rendering;
using LifeSim.Engine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace LifeSim.Assets
{
    public class Atlas 
    {
        private uint _tileSize;
        private int _atlasSize;

        private List<UnpackedTexture> _unpacked = new List<UnpackedTexture>(); 
        
        private ResourceFactory _assetManager;

        private BinPacker _packer;
        private GPUTexture _texture;
        private Image<Rgba32> _image;

        public Atlas(ResourceFactory assetManager, int atlasSize, uint tileSize)
        {
            this._assetManager = assetManager;
            this._tileSize = tileSize;
            this._atlasSize = atlasSize;
            this._packer = new BinPacker((uint) (this._atlasSize / this._tileSize));


            this._image = new Image<Rgba32>(atlasSize, atlasSize);
            this._texture = this._assetManager.MakeTexture(this._image, (uint) this._tileSize);
        }

        public void Add(UnpackedTexture unpackedTexture)
        {
            this._unpacked.Add(unpackedTexture);
        }

        public void AddRange(IEnumerable<UnpackedTexture> unpackedTextures)
        {
            this._unpacked.AddRange(unpackedTextures);
        }

        public GPUTexture texture => this._texture;

        public (string, PackedTexture)[] Pack()
        {
            var sizes = this._GetBinRects(this._unpacked);
            var rects = this._packer.Fit(sizes);

            (string, PackedTexture)[] textures = new (string, PackedTexture)[this._unpacked.Count];
            int i = 0;
            foreach(var rect in rects) 
            {
                UnpackedTexture texture = rect.element;

                Vector2Int coord = new Vector2Int((int) rect.rect.x, (int) rect.rect.y);

                var op = new ImageDrawOperation(this._image, texture.baseMap, coord, this._tileSize);
                op.Draw();

                (Vector2 uv1, Vector2 uv2) = this._GetUVs(coord, texture.size);

                textures[i++] = (texture.id, new PackedTexture(uv1, uv2, this._texture));
            }

            this._texture.Update(this._image);

            return textures;
        }

        private (Vector2, Vector2) _GetUVs(Vector2Int coord, Vector2 size)
        {
            Vector2 imgSize = new Vector2(this._image.Width, this._image.Height);
            Vector2 tl = new Vector2(coord.x * this._tileSize, coord.y * this._tileSize);
            Vector2 uv1 = tl / imgSize;
            Vector2 uv2 = (tl + size) / imgSize;
            return (uv1, uv2);
        }

        protected BinRect<UnpackedTexture>[] _GetBinRects(List<UnpackedTexture> textures)
        {
            var rects = new BinRect<UnpackedTexture>[textures.Count];

            int i = 0;
            foreach (UnpackedTexture texture in textures) {
                var width  = (uint) MathF.Ceiling(texture.width  / this._tileSize);
                var height = (uint) MathF.Ceiling(texture.height / this._tileSize);
                rects[i++] =  new BinRect<UnpackedTexture>(width, height, texture);
            }

            return rects;
        }
    }
}