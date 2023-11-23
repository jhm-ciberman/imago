using System;
using System.Numerics;
using LifeSim.Support.Drawing;

namespace LifeSim.Imago.SceneGraph.Lighting;

public class DirectionalLight
{
    private Vector3 _direction = Vector3.Normalize(new Vector3(-1, 1, -1));

    /// <summary>
    /// Gets or sets the light direction.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The direction cannot be zero.</exception>
    public Vector3 Direction
    {
        get => this._direction;
        set
        {
            if (value == Vector3.Zero)
                throw new ArgumentOutOfRangeException(nameof(value), "The light direction cannot be zero.");

            this._direction = Vector3.Normalize(value);
        }
    }

    /// <summary>
    /// Gets or sets the color of the light. The alpha component determines the intensity of the light.
    /// </summary>
    public ColorF Color { get; set; } = ColorF.White;


    /// <summary>
    /// Gets the shadow map for this light.
    /// </summary>
    public ShadowMap ShadowMap { get; } = new ShadowMap();

}
