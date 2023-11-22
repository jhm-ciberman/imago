using System.Collections.Generic;
using System.Numerics;
using Imago.Graphics.Particles;
using Imago.Graphics.Textures;

namespace Imago.SceneGraph;

public interface IParticleSystem
{
    public IReadOnlyList<Particle> Particles { get; }

    public ITexture Texture { get; }
    void SortParticles(Vector3 cameraPosition);

}
