using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LifeSim
{
    public class UnpackedTexture
    {
        public string id;

        public Image<Rgba32> baseMap;

        public Image<Rgba32>? normalMap;
        
        public int width => this.baseMap.Width;
        public int height => this.baseMap.Height;
        public Vector2Int size => new Vector2Int(this.baseMap.Width, this.baseMap.Height);

        public UnpackedTexture(string id, Image<Rgba32> baseMap, Image<Rgba32>? normalMap = null)
        {
            this.id = id;
            this.baseMap = baseMap;
            this.normalMap = normalMap;
        }
    }
}