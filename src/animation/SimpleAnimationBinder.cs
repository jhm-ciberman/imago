using System.Collections.Generic;
using LifeSim.SceneGraph;

namespace LifeSim.Anim
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

            foreach (var channelName in animation.channelNames) {
                if (dictionary.TryGetValue(channelName, out Node3D? node)) {
                    var channels = animation.FindChannels(channelName);
                    if (channels != null) {
                        binded.AddChannel(node, channels);
                        dictionary.Remove(channelName);
                        System.Console.WriteLine("BOUND channel: " + channelName);
                        
                    }
                } else {
                    System.Console.WriteLine("Unbound channel: " + channelName);
                }
            }

            foreach (var nodeName in dictionary.Keys) {
                System.Console.WriteLine("Unbound node: " + nodeName);
            }

            return binded;
        }

        private void _AddToDictionaryRecursive(Dictionary<string, Node3D> dictionary, Node3D node)
        {
            dictionary[node.name] = node;

            foreach (var child in node.children) {
                this._AddToDictionaryRecursive(dictionary, child);    
            }
        }
    }
}