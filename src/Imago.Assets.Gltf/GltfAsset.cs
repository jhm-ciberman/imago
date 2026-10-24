using System;
using System.Collections.Generic;
using Imago.Assets.Animations;

namespace Imago.Assets.Gltf;

/// <summary>
/// Represents the contents of a loaded glTF file, including scenes and animations.
/// </summary>
public class GltfAsset
{
    /// <summary>
    /// Gets a read-only list of all scenes defined in the glTF file.
    /// </summary>
    public IReadOnlyList<GltfNode> Scenes { get; } = Array.Empty<GltfNode>();

    /// <summary>
    /// Gets a read-only list of all animations defined in the glTF file.
    /// </summary>
    public IReadOnlyList<Animation> Animations { get; } = Array.Empty<Animation>();

    /// <summary>
    /// Initializes a new instance of the <see cref="GltfAsset"/> class.
    /// </summary>
    /// <param name="scenes">The list of scenes.</param>
    /// <param name="animations">The list of animations.</param>
    public GltfAsset(IReadOnlyList<GltfNode> scenes, IReadOnlyList<Animation> animations)
    {
        this.Scenes = scenes;
        this.Animations = animations;
    }

    /// <summary>
    /// Gets the default scene from the glTF file (usually the first one).
    /// </summary>
    public GltfNode Scene => this.Scenes[0];

    /// <summary>
    /// Gets the default animation from the glTF file (usually the first one).
    /// </summary>
    public Animation Animation => this.Animations[0];
}
