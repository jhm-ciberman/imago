using System.Numerics;
using LifeSim.Engine.Rendering;

namespace LifeSim.Assets
{
    public class PackedTexture : IAsset
    {
        public readonly Vector2 uv1;

        public readonly Vector2 uv2;

        public readonly GPUTexture baseMap;
        
        public readonly GPUTexture? bumpMap;

        public PackedTexture(Vector2 uv1, Vector2 uv2, GPUTexture baseMap, GPUTexture? bumpMap = null) 
        {
            this.baseMap = baseMap;
            this.bumpMap = bumpMap;
            this.uv1 = new Vector2(uv1.X, uv2.Y);
            this.uv2 = new Vector2(uv2.X, uv1.Y);
        }

        public (float, float, float, float) GetUVs() 
        {
            return (this.uv1.X, 1f - this.uv1.Y, this.uv2.X, 1f - this.uv2.Y);
        }

        public Vector2 GetRealUV(Vector2 textureSpaceUV)
        {
            //textureSpaceUV.Y = 1f - textureSpaceUV.Y;
            return this.uv1 + (this.uv2 - this.uv1) * textureSpaceUV;
        }

        public Vector2Int GetSize()
        {
            var deltaUV = (this.uv2 - this.uv1);
            int w = (int) (this.baseMap.width  * deltaUV.X);
            int h = (int) (this.baseMap.height * deltaUV.Y);
            return new Vector2Int(w, h);
        }
    }
}

