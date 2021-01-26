using System.Collections.Generic;
using static LifeSim.Assets.BinPacker;
using System.Numerics;
using LifeSim.Engine.Rendering;
using LifeSim.Engine;

namespace LifeSim.Assets
{
    public class TexturePacker 
    {
        private int _mipmapLevels;
        private int _atlasSize;

        private List<UnpackedTexture> _unpacked = new List<UnpackedTexture>(); 
        
        private ResourceFactory _assetManager;

        private BinPacker _packer;
        private AtlasBuilder _atlasBuilder;
        private GPUTexture _texture;

        public TexturePacker(ResourceFactory assetManager, int mipmapLevels, int atlasSize)
        {
            this._assetManager = assetManager;
            this._mipmapLevels = mipmapLevels;
            this._atlasSize = atlasSize;
            this._packer = new BinPacker((uint) (this._atlasSize >> this._mipmapLevels));

            this._atlasBuilder = new AtlasBuilder(this._atlasSize, this._mipmapLevels);
            this._texture = this._assetManager.MakeTexture(this._atlasBuilder.image, (uint) this._mipmapLevels);
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
                this._atlasBuilder.Draw(texture.baseMap, coord);
                //if (texture.normalMap != null) normalAtlas.Draw(texture.normalMap, coord);

                (Vector2 uv1, Vector2 uv2) = this._GetUVs(coord, texture.size);

                textures[i++] = (texture.id, new PackedTexture(uv1, uv2, this._texture));
            }

            this._texture.Update(this._atlasBuilder.image);

            return textures;
        }

        private (Vector2, Vector2) _GetUVs(Vector2Int coord, Vector2 size)
        {
            Vector2 tl = new Vector2(coord.x << this._mipmapLevels, coord.y << this._mipmapLevels);
            Vector2 uv1 = tl / this._atlasSize;
            Vector2 uv2 = (tl + size) / this._atlasSize;
            return (uv1, uv2);
        }

        protected BinRect<UnpackedTexture>[] _GetBinRects(List<UnpackedTexture> textures)
        {
            var rects = new BinRect<UnpackedTexture>[textures.Count];

            int i = 0;
            foreach (UnpackedTexture texture in textures) {
                var width  = ((texture.width  - 1) >> this._mipmapLevels) + 1;
                var height = ((texture.height - 1) >> this._mipmapLevels) + 1;
                rects[i++] =  new BinRect<UnpackedTexture>((uint) width, (uint) height, texture);
            }

            return rects;
        }
    }
}