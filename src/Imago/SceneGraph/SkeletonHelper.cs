using Imago.SceneGraph.Nodes;
using Support.Drawing;

namespace Imago.SceneGraph;

public static class SkeletonHelper
{
    public static void DrawSkeleton(Node3D rootNode)
    {
        var position = rootNode.WorldMatrix.Translation;

        for (var i = 0; i < rootNode.Children.Count; i++)
        {
            var node = rootNode.Children[i];
            var childPosition = node.WorldMatrix.Translation;
            GizmosLayer.Default.DrawLine(position, childPosition, Color.Red);

            DrawSkeleton(node);
        }
    }
}
