using LifeSim.Imago.SceneGraph.Nodes;
using LifeSim.Support.Drawing;

namespace LifeSim.Imago.SceneGraph;

/// <summary>
/// Provides utility methods for visualizing skeleton structures using debug gizmos.
/// </summary>
public static class SkeletonHelper
{
    /// <summary>
    /// Draws a visual representation of a skeleton hierarchy starting from the specified root node.
    /// </summary>
    /// <param name="rootNode">The root node of the skeleton to visualize.</param>
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
