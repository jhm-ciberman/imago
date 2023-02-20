using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.SceneGraph;

public interface IParticleSystem
{
    public IReadOnlyList<Particle> Particles { get; }

    public ITexture Texture { get; }
    void SortParticles(Vector3 cameraPosition);

}
