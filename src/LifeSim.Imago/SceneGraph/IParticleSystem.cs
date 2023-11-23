using System.Collections.Generic;
using System.Numerics;
using LifeSim.Imago.Graphics.Textures;

namespace LifeSim.Imago.SceneGraph;

public interface IParticleSystem
{
    public IReadOnlyList<Particle> Particles { get; }

    public ITexture Texture { get; }
    void SortParticles(Vector3 cameraPosition);

}
