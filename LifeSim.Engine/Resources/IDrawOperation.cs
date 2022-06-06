using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LifeSim.Engine.Resources;

public interface IDrawOperation
{
    Vector2Int Size { get; }

    void Draw(Image<Rgba32> dst, Vector2Int coord, Vector2Int size);
}