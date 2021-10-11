using System.Collections.Generic;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.Anim
{
    public class SimpleAnimationBinder
    {
        public SimpleAnimationBinder()
        {

        }

        public BindedAnimation Bind(Node3D root, Animation animation)
        {
            BindedAnimation binded = new BindedAnimation(animation);

            Dictionary<string, Node3D> dictionary = new Dictionary<string, Node3D>();
            this._AddToDictionaryRecursive(dictionary, root);

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

        private void _AddToDictionaryRecursive(Dictionary<string, Node3D> dictionary, Node3D node)
        {
            if (node is Node3D spatialNode)
                dictionary[node.Name] = spatialNode;

            foreach (var child in node.Children)
            {
                this._AddToDictionaryRecursive(dictionary, child);
            }
        }
    }
}