
using System;
using System.IO;
using System.Xml;

namespace LifeSim.Engine.AssetManagment;

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
public static class AssetXmlParser
{
    public static void RegisterAssetsFromXml(AssetManager assetManager, string xml)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml);

        var assetsNode = xmlDoc.SelectSingleNode("assets");

        if (assetsNode == null)
        {
            throw new ArgumentException("XML file does not contain an assets node.");
        }

        foreach (XmlNode assetNode in assetsNode.ChildNodes)
        {
            var asset = ParseAsset(assetNode);

            assetManager.RegisterAsset(asset);
        }
    }

    public static void RegisterAssetsFromXmlFile(AssetManager assetManager, string filePath)
    {
        var xml = File.ReadAllText(filePath);
        RegisterAssetsFromXml(assetManager, xml);
    }

    private static string ReadStringAttribute(XmlNode node, string attributeName)
    {
        var attribute = node.Attributes?[attributeName];

        if (attribute == null)
        {
            throw new ArgumentException($"Node {node.Name} does not contain attribute {attributeName}.");
        }

        return attribute.Value;
    }

    private static bool ReadBoolAttribute(XmlNode node, string attributeName)
    {
        var attribute = node.Attributes?[attributeName];

        if (attribute == null)
        {
            throw new ArgumentException($"Node {node.Name} does not contain attribute {attributeName}.");
        }

        return bool.Parse(attribute.Value);
    }

    private static IAssetDefinition ParseAsset(XmlNode assetNode)
    {
        return assetNode.Name switch
        {
            "animation" => ParseAnimation(assetNode),
            "scene" => ParseScene(assetNode),
            "texture" => ParseTexture(assetNode),
            "sound" => ParseSound(assetNode),
            _ => throw new ArgumentException($"Asset type {assetNode.Name} not supported."),
        };
    }

    private static AnimationDefinition ParseAnimation(XmlNode assetNode)
    {
        var key = ReadStringAttribute(assetNode, "key");
        var src = ReadStringAttribute(assetNode, "src");
        var name = ReadStringAttribute(assetNode, "name");

        return new AnimationDefinition(key, src, name);
    }

    private static SceneDefinition ParseScene(XmlNode assetNode)
    {
        var key = ReadStringAttribute(assetNode, "key");
        var src = ReadStringAttribute(assetNode, "src");
        var name = ReadStringAttribute(assetNode, "name");

        return new SceneDefinition(key, src, name);
    }

    private static TextureDefinition ParseTexture(XmlNode assetNode)
    {
        var key = ReadStringAttribute(assetNode, "key");
        var src = ReadStringAttribute(assetNode, "src");
        var srgb = ReadBoolAttribute(assetNode, "srgb");
        return new TextureDefinition(key, src, srgb);
    }

    private static SoundDefinition ParseSound(XmlNode assetNode)
    {
        var key = ReadStringAttribute(assetNode, "key");
        var src = ReadStringAttribute(assetNode, "src");
        return new SoundDefinition(key, src);
    }
}

