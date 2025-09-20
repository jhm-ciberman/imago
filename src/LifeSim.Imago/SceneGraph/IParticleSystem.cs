using System.Collections.Generic;
using System.Numerics;
using LifeSim.Imago.Textures;

namespace LifeSim.Imago.SceneGraph;

/// <summary>
/// Defines a contract for particle systems that can be rendered in the scene.
/// </summary>
public interface IParticleSystem
{
    /// <summary>
    /// Gets the collection of particles in this system.
    /// </summary>
    public IReadOnlyList<Particle> Particles { get; }

    /// <summary>
    /// Gets the texture used for rendering the particles.
    /// </summary>
    public ITexture Texture { get; }

    /// <summary>
    /// Sorts the particles based on their distance from the camera for proper depth-based rendering.
    /// </summary>
    /// <param name="cameraPosition">The world position of the camera.</param>
    public void SortParticles(Vector3 cameraPosition);
}
