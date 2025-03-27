using System.Collections.Generic;
using System.Numerics;
using LifeSim.Imago.Textures;

namespace LifeSim.Imago.SceneGraph;

public interface IParticleSystem
{
    public IReadOnlyList<Particle> Particles { get; }

    public ITexture Texture { get; }
    public void SortParticles(Vector3 cameraPosition);

}
