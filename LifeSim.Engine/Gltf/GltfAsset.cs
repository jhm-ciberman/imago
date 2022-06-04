using System;
using System.Collections.Generic;
using LifeSim.Engine.Anim;

namespace LifeSim.Engine.Gltf;

public class GltfAsset
{
    public IReadOnlyList<IScenePrefab> Scenes { get; } = Array.Empty<IScenePrefab>();
    public IReadOnlyList<Animation> Animations { get; } = Array.Empty<Animation>();

    public GltfAsset(IReadOnlyList<IScenePrefab> scenes, IReadOnlyList<Animation> animations)
    {
        this.Scenes = scenes;
        this.Animations = animations;
    }
}