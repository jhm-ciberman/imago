using System.Numerics;
using Imago.SceneGraph.Nodes;
using Imago.Support.Drawing;

namespace Imago.SceneGraph;

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
        DrawSkeletonFrom(rootNode.WorldMatrix.Translation, rootNode);
    }

    private static void DrawSkeletonFrom(Vector3 parentPosition, Node node)
    {
        for (var i = 0; i < node.Children.Count; i++)
        {
            var child = node.Children[i];
            if (child is Node3D node3D)
            {
                var childPosition = node3D.WorldMatrix.Translation;
                GizmosDrawer.Default.DrawLine(parentPosition, childPosition, Color.Red);
                DrawSkeletonFrom(childPosition, node3D);
            }
            else
            {
                // Transform-less nodes are transparent: relay the anchor to their children.
                DrawSkeletonFrom(parentPosition, child);
            }
        }
    }
}
