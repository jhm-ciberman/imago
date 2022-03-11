using System;
using System.Collections.Generic;
using LifeSim.Engine.Anim;

namespace LifeSim.Engine.Gltf;

public class GltfAsset
{
    public IReadOnlyList<IScenePrefab> Scenes { get; } = Array.Empty<IScenePrefab>();
    public IScenePrefab? Scene => this.Scenes.Count > 0 ? this.Scenes[0] : null;
    public IReadOnlyList<Animation> Animations { get; } = Array.Empty<Animation>();

    public GltfAsset(IReadOnlyList<IScenePrefab> scenes, IReadOnlyList<Animation> animations)
    {
        this.Scenes = scenes;
        this.Animations = animations;
    }

    public IScenePrefab GetScene(string name)
    {
        foreach (var scene in this.Scenes)
        {
            if (scene.Name == name)
            {
                return scene;
            }
        }

        throw new ArgumentException($"Scene with name {name} not found.");
    }

    public Animation GetAnimation(string name)
    {
        foreach (var animation in this.Animations)
        {
            if (animation.Name == name)
            {
                return animation;
            }
        }

        throw new ArgumentException($"Animation with name {name} not found.");
    }
}