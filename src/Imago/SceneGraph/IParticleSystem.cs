using System.Collections.Generic;
using System.Numerics;
using Imago.Rendering;
using Imago.Rendering.Particles;

namespace Imago.SceneGraph;

public interface IParticleSystem
{
    public IReadOnlyList<Particle> Particles { get; }

    public ITexture Texture { get; }
    void SortParticles(Vector3 cameraPosition);

}
