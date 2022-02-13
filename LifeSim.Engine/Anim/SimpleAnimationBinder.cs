using System.Collections.Generic;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.Anim;

public class SimpleAnimationBinder
{
    public SimpleAnimationBinder()
    {

    }

    public BindedAnimation Bind(Node3D root, Animation animation)
    {
        var binded = new BindedAnimation(animation);

        var dictionary = new Dictionary<string, Node3D>();
        this.AddToDictionaryRecursive(dictionary, root);

        foreach (var channelName in animation.ChannelNames)
        {
            if (dictionary.TryGetValue(channelName, out Node3D? node))
            {
                var channels = animation.FindChannels(channelName);
                if (channels != null)
                {
                    binded.AddChannel(node, channels);
                    dictionary.Remove(channelName);
                }
            }
            else
            {
                System.Console.WriteLine("Unbound channel: " + channelName);
            }
        }

        return binded;
    }

    private void AddToDictionaryRecursive(Dictionary<string, Node3D> dictionary, Node3D node)
    {
        dictionary[node.Name] = node;

        foreach (var child in node.Children)
        {
            this.AddToDictionaryRecursive(dictionary, child);
        }
    }
}