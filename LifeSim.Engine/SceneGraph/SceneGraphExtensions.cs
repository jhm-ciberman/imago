using System;
using System.Numerics;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.Resources;
using Veldrid.Utilities;

namespace LifeSim.Engine.SceneGraph;

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

        foreach (var child in self.Children)
        {
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
        var found = self.FindChild<T>(name);
        if (found == null) throw new InvalidOperationException($"Child with name '{name}' not found.");
        return found;
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
        foreach (var child in self.Children)
        {
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
        foreach (var child in self.Children)
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
        var found = self.FindPath<T>(path);
        if (found == null) throw new InvalidOperationException($"Path '{path}' not found.");
        return found;
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
        foreach (var child in self.Children)
        {
            child.ForEachRecursive(action);
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

    private static void PrintHierarchyToConsoleCore(Node3D node, string indent, bool isLast, Func<Node3D, string>? toString = null)
    {
        var label = node.ToString();
        var c = indent.Length == 0 ? '─' : (isLast ? '└' : '├');
        string extra = toString?.Invoke(node) ?? "";
        Console.WriteLine($"{indent}{c}╴{label} {extra}");
        for (var i = 0; i < node.Children.Count; i++)
        {
            var child = node.Children[i];
            var isLastChild = i == node.Children.Count - 1;
            PrintHierarchyToConsoleCore(child, indent + (isLast ? "  " : "│ "), isLastChild, toString);
        }
    }

    /// <summary>
    /// Prints the hierarchy of the specified node to the console.
    /// </summary>
    /// <param name="self">The node.</param>
    public static void PrintHierarchyToConsole(this Node3D self, Func<Node3D, string>? toString = null)
    {
        PrintHierarchyToConsoleCore(self, "", true, toString);
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
            if (node.Material is null) return;
            node.Material.Texture = packedTexture.Texture;
            node.TextureST = packedTexture.GetTextureST();
        });
    }
}
