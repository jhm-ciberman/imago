using System;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Imago.Assets.Materials;
using LifeSim.Imago.Assets.TexturePacking;
using LifeSim.Imago.SceneGraph.Nodes;
using LifeSim.Imago.Utilities;
using LifeSim.Support.Collections;
using LifeSim.Support.Numerics;

namespace LifeSim.Imago.SceneGraph;

/// <summary>
/// Provides extension methods for scene graph operations and node manipulation.
/// </summary>
public static class SceneGraphExtensions
{
    /// <summary>
    /// Recursively finds the node with the specified name.
    /// </summary>
    /// <typeparam name="T">The type of node to find.</typeparam>
    /// <param name="self">The node.</param>
    /// <param name="name">The name of the node to find.</param>
    /// <returns>The node with the specified name or null if no node with the specified name was found.</returns>
    public static T? FindChild<T>(this Node3D self, string name) where T : Node3D
    {
        if (self is T tNode && self.Name == name) return tNode;

        for (var i = 0; i < self.Children.Count; i++)
        {
            var child = self.Children[i];
            var found = child.FindChild<T>(name);
            if (found != null) return found;
        }

        return null;
    }

    /// <summary>
    /// Recursively finds the node with the specified name or fails with an exception if the node is not found.
    /// </summary>
    /// <typeparam name="T">The type of node to find.</typeparam>
    /// <param name="self">The node.</param>
    /// <param name="name">The name of the node to find.</param>
    /// <returns>The node with the specified name.</returns>
    /// <exception cref="InvalidOperationException">No node with the specified name was not found.</exception>
    public static T FindChildOrFail<T>(this Node3D self, string name) where T : Node3D
    {
        return self.FindChild<T>(name) ?? throw new InvalidOperationException($"Child with name '{name}' not found.");
    }

    /// <summary>
    /// Returns the direct child node with the specified name or null if no child with the specified name was found.
    /// Only direct children are searched.
    /// </summary>
    /// <typeparam name="T">The type of node to find.</typeparam>
    /// <param name="self">The node.</param>
    /// <param name="name">The name of the node to find.</param>
    /// <returns>The node with the specified name or null if no node with the specified name was found.</returns>
    public static T? GetDirectChildByName<T>(this Node3D self, string name) where T : Node3D
    {
        for (var i = 0; i < self.Children.Count; i++)
        {
            var child = self.Children[i];
            if (child.Name == name && child is T tChild)
            {
                return tChild;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the direct child node with the specified name or null if no child with the specified name was found.
    /// </summary>
    /// <param name="self">The node.</param>
    /// <param name="name">The name of the node to find.</param>
    /// <returns>The node with the specified name or null if no node with the specified name was found.</returns>
    public static Node3D? GetDirectChildByName(this Node3D self, string name)
    {
        var children = (SwapPopList<Node3D>)self.Children; // Prevents allocation of IEnumerable
        foreach (var child in children)
        {
            if (child.Name == name) return child;
        }

        return null;
    }

    /// <summary>
    /// Finds the node with the specified path. The path is a relative path to a node (example: "Armature/Hips/Spine1/Spine2/Head").
    /// </summary>
    /// <typeparam name="T">The type of node to find.</typeparam>
    /// <param name="self">The node.</param>
    /// <param name="path">The path to the node to find.</param>
    /// <returns>The node with the specified path or null if no node with the specified path was found.</returns>
    public static T? FindPath<T>(this Node3D self, string path) where T : Node3D
    {
        if (string.IsNullOrEmpty(path)) return null;

        var pathParts = path.Split('/');
        var currentNode = self;
        foreach (var pathPart in pathParts)
        {
            currentNode = currentNode.GetDirectChildByName(pathPart);
            if (currentNode == null) return null;
        }

        return currentNode as T;
    }

    /// <summary>
    /// Finds the node with the specified path or fails with an exception if the node is not found.
    /// The path is a relative path to a node (example: "Armature/Hips/Spine1/Spine2/Head").
    /// </summary>
    /// <typeparam name="T">The type of node to find.</typeparam>
    /// <param name="self">The node.</param>
    /// <param name="path">The path to the node to find.</param>
    /// <returns>The node with the specified path.</returns>
    /// <exception cref="InvalidOperationException">No node with the specified path was not found.</exception>
    public static T FindPathOrFail<T>(this Node3D self, string path) where T : Node3D
    {
        return self.FindPath<T>(path) ?? throw new InvalidOperationException($"Path '{path}' not found.");
    }



    /// <summary>
    /// Executes the specified action for each node in the scene graph, in a depth-first manner.
    /// </summary>
    /// <typeparam name="T">The type of node to find.</typeparam>
    /// <param name="self">The node.</param>
    /// <param name="action">The action to execute for each node.</param>
    public static void ForEachRecursive<T>(this Node3D self, Action<T> action)
    {
        if (self is T node)
        {
            action(node);
        }

        for (var i = 0; i < self.Children.Count; i++)
        {
            self.Children[i].ForEachRecursive(action);
        }
    }

    /// <summary>
    /// Returns all children of the specified node that are of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of node to find.</typeparam>
    /// <param name="self">The node.</param>
    /// <returns></returns>
    public static IEnumerable<T> GetChildrenOfType<T>(this Node3D self)
    {
        if (self is T tNode)
        {
            yield return tNode;
        }

        foreach (var child in self.Children)
        {
            foreach (var grandChild in child.GetChildrenOfType<T>())
            {
                yield return grandChild;
            }
        }
    }

    /// <summary>
    /// Sets the specified material to all renderable nodes starting from the specified node.
    /// </summary>
    /// <param name="self">The node.</param>
    /// <param name="material">The material to set.</param>
    public static void SetMaterial(this Node3D self, Material material)
    {
        self.ForEachRecursive<RenderNode3D>((node) =>
        {
            node.Material = material;
        });
    }

    /// <summary>
    /// Sets the specified material to all renderable nodes starting from the specified node.
    /// </summary>
    /// <param name="self">The node.</param>
    /// <param name="material">The material to set.</param>
    /// <param name="textureST">The texture size and offset.</param>
    public static void SetMaterial(this Node3D self, Material material, Vector4 textureST)
    {
        self.ForEachRecursive<RenderNode3D>((node) =>
        {
            node.Material = material;
            node.TextureST = textureST;
        });
    }

    /// <summary>
    /// Renames the node with the specified old name to the new name.
    /// </summary>
    /// <param name="self">The root node.</param>
    /// <param name="oldName">The node name to rename.</param>
    /// <param name="newName">The new name for the node.</param>
    /// <returns>True if the node was renamed, false otherwise.</returns>
    public static bool RenameNode(this Node3D self, string oldName, string newName)
    {
        if (self.Name == oldName)
        {
            self.Name = newName;
            return true;
        }

        for (var i = 0; i < self.Children.Count; i++)
        {
            if (self.Children[i].RenameNode(oldName, newName)) return true;
        }

        return false;
    }

    private static void PrintHierarchyToConsoleCore(Node3D node, string indent, bool isLast, string? format = null)
    {
        var label = node.ToString(format);
        var c = indent.Length == 0 ? '─' : (isLast ? '└' : '├');
        Console.WriteLine($"{indent}{c}╴{label}");
        for (var i = 0; i < node.Children.Count; i++)
        {
            var child = node.Children[i];
            var isLastChild = i == node.Children.Count - 1;
            PrintHierarchyToConsoleCore(child, indent + (isLast ? "  " : "│ "), isLastChild, format);
        }
    }

    /// <summary>
    /// Prints the hierarchy of the specified node to the console.
    /// </summary>
    /// <param name="self">The node.</param>
    /// <param name="format">The format string to use for each node.</param>
    public static void PrintHierarchyToConsole(this Node3D self, string? format = null)
    {
        PrintHierarchyToConsoleCore(self, "", true, format);
    }

    /// <summary>
    /// Tests whether the given ray intersects this node.
    /// </summary>
    /// <param name="self">The render node.</param>
    /// <param name="ray">The ray to test.</param>
    /// <param name="hitInfo">The hit info if the ray intersects this node.</param>
    /// <returns>True if the ray intersects this node, false otherwise.</returns>
    public static bool RayCast(this RenderNode3D self, Ray ray, out HitInfo hitInfo)
    {
        hitInfo = default;
        var mesh = self.Mesh;
        if (mesh is null)
        {
            return false;
        }

        // FIXME: If the world matrix is not updated, this will not work
        Matrix4x4.Invert(self.WorldMatrix, out var invWorld);
        var localRay = Ray.Transform(ray, invWorld);

        // Fast check for bounding box intersection
        if (!localRay.Intersects(mesh.BoundingBox))
        {
            return false;
        }

        if (mesh.MeshData.RayCast(localRay, out hitInfo))
        {
            hitInfo.Position = Vector3.Transform(hitInfo.Position, self.WorldMatrix);
            hitInfo.Normal = Vector3.TransformNormal(hitInfo.Normal, self.WorldMatrix);
            hitInfo.Distance = Vector3.Distance(ray.Origin, hitInfo.Position);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Applies the given <see cref="PackedTexture"/> to all renderable nodes starting from the specified node.
    /// If a renderable node has no material, it will be ignored.
    /// </summary>
    /// <param name="self">The node.</param>
    /// <param name="packedTexture">The texture to apply.</param>
    public static void SetPackedTexture(this Node3D self, PackedTexture packedTexture)
    {
        self.ForEachRecursive<RenderNode3D>((node) =>
        {
            if (node.Material is not StandardMaterial material) return;
            material.Texture = packedTexture.Texture;
            node.TextureST = packedTexture.GetTextureST();
        });
    }

    /// <summary>
    /// Computes the bounding box of the node and all its children.
    /// </summary>
    /// <param name="self">The node.</param>
    /// <returns>The bounding box of the node and all its children, or null if the node has no mesh.</returns>
    public static BoundingBox? GetBoundingBox(this Node3D self)
    {
        BoundingBox? result = null;
        self.ForEachRecursive<RenderNode3D>((node) =>
        {
            if (node.Mesh is null) return;

            if (result is null)
            {
                result = BoundingBox.Transform(node.Mesh.BoundingBox, node.WorldMatrix);
                return;
            }

            BoundingBox bbox = BoundingBox.Transform(node.Mesh.BoundingBox, node.WorldMatrix);
            result = BoundingBox.Combine(result.Value, bbox);
        });

        return result;
    }

    /// <summary>
    /// Computes the tight bounding box of the node and all its children.
    /// </summary>
    /// <param name="self">The node.</param>
    /// <param name="viewProjection">The view-projection matrix to use for the projection.</param>
    /// <returns>The tight bounding box of the node and all its children, or null if the node has no mesh.</returns>
    /// <remarks>
    /// This method is slower than <see cref="GetBoundingBox(Node3D)"/> but it computes a tighter bounding box.
    /// This is done by iterating over all vertices of all the meshes. So it is not recommended to use this method
    /// for a large number of meshes.
    /// </remarks>
    public static Rect? GetTightProjectedRect(this Node3D self, ref Matrix4x4 viewProjection)
    {
        bool first = true;
        Rect result = default;
        var renderables = self.GetChildrenOfType<RenderNode3D>();

        foreach (var renderable in renderables)
        {
            if (renderable.Mesh is null) continue;
            var positions = renderable.Mesh.MeshData.Positions;

            for (var i = 0; i < positions.Length; i++)
            {
                var matrix = renderable.WorldMatrix * viewProjection;
                var projected = Vector3.Transform(positions[i], matrix);

                if (first)
                {
                    result = new Rect(projected.X, projected.Y, 0, 0);
                    first = false;
                }
                else
                {
                    result = Rect.Union(result, new Rect(projected.X, projected.Y, 0, 0));
                }
            }
        }

        return first ? null : result;
    }

    /// <summary>
    /// Gets the projected rectangle of the bounding box of the node and all its children.
    /// </summary>
    /// <param name="self">The node.</param>
    /// <param name="viewProjection">The view-projection matrix to use for the projection.</param>
    /// <returns>The projected rectangle of the bounding box of the node and all its children, or null if the node has no mesh.</returns>
    public static Rect? GetProjectedRect(this Node3D self, ref Matrix4x4 viewProjection)
    {
        var bbox = self.GetBoundingBox();
        if (bbox is null) return null;

        var projectedBB = BoundingBox.Transform(bbox.Value, viewProjection);

        var center = projectedBB.GetCenter();
        var size = projectedBB.GetDimensions();

        return new Rect(center.X - size.X / 2, center.Y - size.Y / 2, size.X, size.Y);
    }

    /// <summary>
    /// Swaps the child of the specified node.
    /// </summary>
    /// <param name="self">The parent node.</param>
    /// <param name="current">The current child node.</param>
    /// <param name="next">The new value for the child node.</param>
    /// <param name="dispose">If true, the current child node will be disposed.</param>
    /// <returns>True if the child was swapped, false otherwise.</returns>
    public static bool SwapChild(this Node3D self, ref Node3D? current, Node3D? next, bool dispose = true)
    {
        if (current == next) return false;

        if (current != null)
        {
            self.RemoveChild(current, dispose);
        }

        if (next != null)
        {
            self.AddChild(next);
        }

        current = next;
        return true;
    }
}
