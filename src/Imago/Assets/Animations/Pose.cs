using System.Collections.Generic;
using Imago.SceneGraph.Nodes;

namespace Imago.Assets.Animations;

/// <summary>
/// A snapshot of local <see cref="BoneTransform"/> values keyed by bone name.
/// </summary>
/// <remarks>
/// A pose is an intermediate buffer that decouples animation sampling from the scene graph: channels sample into a
/// pose, and the result is later blended and applied to a <see cref="Node3D"/> hierarchy.
/// </remarks>
public class Pose
{
    private readonly Dictionary<string, BoneTransform> _bones = new();

    /// <summary>
    /// Gets the bone names currently stored in this pose.
    /// </summary>
    public IReadOnlyCollection<string> BoneNames => this._bones.Keys;

    /// <summary>
    /// Tries to retrieve the transform for the given bone.
    /// </summary>
    /// <param name="boneName">The bone name.</param>
    /// <param name="value">The stored transform when this method returns <see langword="true"/>; otherwise undefined.</param>
    /// <returns><see langword="true"/> if an entry exists for <paramref name="boneName"/>; otherwise <see langword="false"/>.</returns>
    public bool TryGet(string boneName, out BoneTransform value)
    {
        return this._bones.TryGetValue(boneName, out value);
    }

    /// <summary>
    /// Returns the transform for the given bone, inserting <see cref="BoneTransform.Identity"/> if no entry exists.
    /// </summary>
    /// <param name="boneName">The bone name.</param>
    /// <returns>The transform for the bone.</returns>
    public BoneTransform GetOrAdd(string boneName)
    {
        if (!this._bones.TryGetValue(boneName, out BoneTransform value))
        {
            value = BoneTransform.Identity;
            this._bones[boneName] = value;
        }

        return value;
    }

    /// <summary>
    /// Sets the transform for the given bone, overwriting any existing entry.
    /// </summary>
    /// <param name="boneName">The bone name.</param>
    /// <param name="value">The transform to store.</param>
    public void Set(string boneName, in BoneTransform value)
    {
        this._bones[boneName] = value;
    }

    /// <summary>
    /// Removes all bone entries from the pose.
    /// </summary>
    public void Clear()
    {
        this._bones.Clear();
    }

    /// <summary>
    /// Replaces this pose's contents with the contents of <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The source pose.</param>
    public void CopyFrom(Pose other)
    {
        this._bones.Clear();
        foreach (var pair in other._bones)
        {
            this._bones[pair.Key] = pair.Value;
        }
    }

    /// <summary>
    /// Captures the current local transform of every named descendant of <paramref name="root"/> (and <paramref name="root"/> itself) into this pose.
    /// </summary>
    /// <param name="root">The root node to walk.</param>
    public void CaptureFrom(Node3D root)
    {
        this._bones.Clear();
        CaptureRecursive(this, root);
    }

    /// <summary>
    /// Writes every entry of this pose to its matching named descendant of <paramref name="root"/> (and <paramref name="root"/> itself).
    /// Entries that do not match a node are silently ignored.
    /// </summary>
    /// <param name="root">The root node to walk.</param>
    public void ApplyTo(Node3D root)
    {
        ApplyRecursive(this, root);
    }

    /// <summary>
    /// Component-wise linear blend of two poses.
    /// </summary>
    /// <remarks>
    /// For each bone present in both inputs, the destination receives <see cref="BoneTransform.Lerp"/> of the two values.
    /// Bones present in only one input pass through unchanged; the destination does not fade unmatched bones toward identity.
    /// </remarks>
    /// <param name="a">The pose at <paramref name="t"/> = 0.</param>
    /// <param name="b">The pose at <paramref name="t"/> = 1.</param>
    /// <param name="t">The interpolation factor in the [0, 1] range.</param>
    /// <param name="dest">The destination pose. Cleared and overwritten.</param>
    public static void Lerp(Pose a, Pose b, float t, Pose dest)
    {
        dest._bones.Clear();

        foreach (var pair in a._bones)
        {
            if (b._bones.TryGetValue(pair.Key, out BoneTransform bValue))
            {
                dest._bones[pair.Key] = BoneTransform.Lerp(pair.Value, bValue, t);
            }
            else
            {
                dest._bones[pair.Key] = pair.Value;
            }
        }

        foreach (var pair in b._bones)
        {
            if (dest._bones.ContainsKey(pair.Key)) continue;
            dest._bones[pair.Key] = pair.Value;
        }
    }

    private static void CaptureRecursive(Pose pose, Node3D node)
    {
        if (!string.IsNullOrEmpty(node.Name))
        {
            pose._bones[node.Name] = new BoneTransform(node.Position, node.Rotation, node.Scale);
        }

        for (int i = 0; i < node.Children.Count; i++)
        {
            CaptureRecursive(pose, node.Children[i]);
        }
    }

    private static void ApplyRecursive(Pose pose, Node3D node)
    {
        if (!string.IsNullOrEmpty(node.Name) && pose._bones.TryGetValue(node.Name, out BoneTransform value))
        {
            node.Position = value.Position;
            node.Rotation = value.Rotation;
            node.Scale = value.Scale;
        }

        for (int i = 0; i < node.Children.Count; i++)
        {
            ApplyRecursive(pose, node.Children[i]);
        }
    }
}
