using LifeSim.View3D;

namespace LifeSim.Assets
{
    public interface IAssetProcessor
    {
        void PackedTexture(PackedTexture asset);
        void Tilemap(Tilemap asset);
        void ApertureAsset(ApertureAsset asset);
        void PlantAsset(PlantAsset asset);
        void ObjectAsset(ObjectAsset asset);
        void FontAsset(FontAsset asset);
    }
}