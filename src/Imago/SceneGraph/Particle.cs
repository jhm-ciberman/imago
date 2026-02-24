using System.Numerics;
using Imago.Support.Drawing;

namespace Imago.SceneGraph;

/// <summary>
/// Represents a single particle in a particle system with position, velocity, life, size, and color properties.
/// </summary>
public struct Particle
{
    /// <summary>
    /// Gets or sets the position of the particle in 3D space.
    /// </summary>
    public Vector3 Position;
    /// <summary>
    /// Gets or sets the size of the particle.
    /// </summary>
    public float Size;
    /// <summary>
    /// Gets or sets the velocity of the particle in 3D space.
    /// </summary>
    public Vector3 Velocity;
    /// <summary>
    /// Gets or sets the current life remaining for the particle.
    /// </summary>
    public float Life;
    /// <summary>
    /// Gets or sets the maximum life the particle started with.
    /// </summary>
    public float MaxLife;
    /// <summary>
    /// Gets or sets the color of the particle.
    /// </summary>
    public ColorF Color;
    /// <summary>
    /// Gets or sets the distance from the particle to the camera, used for depth sorting.
    /// </summary>
    public float DistanceToCamera;

    /// <summary>
    /// Initializes a new instance of the <see cref="Particle"/> struct.
    /// </summary>
    /// <param name="position">The initial position of the particle.</param>
    /// <param name="velocity">The initial velocity of the particle.</param>
    /// <param name="life">The initial life of the particle.</param>
    /// <param name="size">The size of the particle.</param>
    /// <param name="color">The color of the particle.</param>
    public Particle(Vector3 position, Vector3 velocity, float life, float size, ColorF color)
    {
        this.Position = position;
        this.Velocity = velocity;
        this.Life = life;
        this.MaxLife = life;
        this.Size = size;
        this.Color = color;
        this.DistanceToCamera = 0;
    }
}
