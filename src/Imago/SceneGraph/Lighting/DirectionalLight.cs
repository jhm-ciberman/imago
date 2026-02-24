using System;
using System.Numerics;
using CommunityToolkit.Diagnostics;
using Imago.Support.Drawing;

namespace Imago.SceneGraph.Lighting;

/// <summary>
/// Represents a directional light source that illuminates the scene from a specific direction.
/// </summary>
public class DirectionalLight
{
    private Vector3 _direction = Vector3.Normalize(new Vector3(-1, 1, -1));

    /// <summary>
    /// Gets or sets the light direction.
    /// </summary>
    /// <exception cref="ArgumentException">The direction cannot be zero.</exception>
    public Vector3 Direction
    {
        get => this._direction;
        set
        {
            Guard.IsNotEqualTo(value, Vector3.Zero);
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
