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

using LifeSim.Engine.Gltf;

namespace LifeSim.Engine.AssetManagment;


public class AnimationDefinition : IAssetDefinition
{
    public string Key { get; } = string.Empty;
    public string Name { get; } = string.Empty;
    public string Path { get; } = string.Empty;

    public AnimationDefinition() { }

    public AnimationDefinition(string key, string name, string path)
    {
        this.Key = key;
        this.Name = name;
        this.Path = path;
    }

    public object Load()
    {
        return GltfLoader.Load(this.Path).GetAnimation(this.Name);
    }
}

