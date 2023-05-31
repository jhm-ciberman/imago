using System;
using System.Collections.Generic;
using Imago.Motion;

namespace Imago.Gltf;

public class GltfAsset
{
    public IReadOnlyList<GltfNode> Scenes { get; } = Array.Empty<GltfNode>();
    public IReadOnlyList<Animation> Animations { get; } = Array.Empty<Animation>();

    public GltfAsset(IReadOnlyList<GltfNode> scenes, IReadOnlyList<Animation> animations)
    {
        this.Scenes = scenes;
        this.Animations = animations;
    }

    public GltfNode Scene => this.Scenes[0];

    public Animation Animation => this.Animations[0];
}
