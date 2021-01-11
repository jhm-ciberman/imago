using System.Numerics;
using LifeSim.Rendering;

namespace LifeSim
{
    public class PackedTile : PackedTexture
    {

        public bool rotable = false;

        public PackedTile(Vector2 uv1, Vector2 uv2, GPUTexture baseMap) : base(uv1, uv2, baseMap, null)
        {
            //
        }

    }
}