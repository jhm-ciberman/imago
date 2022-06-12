using System;

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

    // path is a relative path to a node (example: "Armature/Hips/Spine1/Spine2/Head")

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
}