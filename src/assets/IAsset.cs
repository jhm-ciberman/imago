namespace LifeSim.Assets
{
    public interface IAsset
    {
        void Accept(IAssetProcessor assetProcessor);
    }
}