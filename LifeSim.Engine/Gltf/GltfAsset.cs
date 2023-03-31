using System;
using System.Collections.Generic;
using LifeSim.Engine.Anim;

namespace LifeSim.Engine.Gltf;

public class GltfAsset
{
    public IReadOnlyList<GltfPrefab> Scenes { get; } = Array.Empty<GltfPrefab>();
    public IReadOnlyList<Animation> Animations { get; } = Array.Empty<Animation>();

    public GltfAsset(IReadOnlyList<GltfPrefab> scenes, IReadOnlyList<Animation> animations)
    {
        this.Scenes = scenes;
        this.Animations = animations;
    }

    public GltfPrefab Scene => this.Scenes[0];

    public Animation Animation => this.Animations[0];
}
