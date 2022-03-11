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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LifeSim.Engine.AssetManagment;

public class ImageAsset : IAsset
{
    public string Key { get; } = string.Empty;
    public string Path { get; } = string.Empty;

    public ImageAsset() { }

    public ImageAsset(string key, string path)
    {
        this.Key = key;
        this.Path = path;
    }

    public virtual object Load()
    {
        return Image.Load<Rgba32>(this.Path);
    }
}

