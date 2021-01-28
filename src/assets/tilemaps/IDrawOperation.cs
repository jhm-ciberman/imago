using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static LifeSim.Assets.Atlas;

namespace LifeSim.Assets
{
    public interface IDrawOperation
    {
        Vector2Int size { get; }

        void Draw(Image<Rgba32> dst, Vector2Int coord, Vector2Int size);
    }
}