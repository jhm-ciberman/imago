namespace LifeSim.Engine.AssetManagment;

public interface IAsset
{
    string Key { get; }
    object Load();
}