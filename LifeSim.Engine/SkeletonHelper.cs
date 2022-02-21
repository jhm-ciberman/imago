using System.Numerics;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine;

public static class SkeletonHelper
{
    public static void DrawSkeleton(Node3D rootNode)
    {
        var position = rootNode.WorldMatrix.Translation;

        foreach (var node in rootNode.Children)
        {
            var childPosition = node.WorldMatrix.Translation;
            GizmosLayer.Default.DrawLine(position, childPosition, Color.Red);

            DrawSkeleton(node);
        }
    }
}