/*
Example of assets xml file. Each asset is defined by a "key" attribute.

<assets>
    <animation key="door-open-open" src="oven.gltf" name="Open"/>
    <animation key="avatar-open-door" src="oven.gltf" name="AvatarOpen"/>
    <scene key="door" src="oven.gltf" name="TheDoor"/>
    <texture key="oven-texture" src="oven.png"/>
    <sound key="oven-open"/>
</Assets>

First each asset is registered in the AssetManager with the method RegisterAsset.
For now, the possible types of assets are: Texture, Sound, Animation, Scene, Object.
Then, the asset is loaded by calling the method asset.Load();
*/

using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.AssetManagment;

public class TextureAsset : ImageAsset, IAsset
{
    public bool Srgb { get; } = false;

    public TextureAsset() { }

    public TextureAsset(string key, string path, bool srgb) : base(key, path)
    {
        this.Srgb = srgb;
    }

    public override object Load()
    {
        return new ImageTexture(this.Path, this.Srgb);
    }
}

