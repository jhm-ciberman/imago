using System;
using System.Numerics;

namespace LifeSim.Engine.Rendering;

public class ShadowMap
{
    private float _maximumShadowDistance = 50f;

    /// <summary>
    /// Gets or sets the maximum distance that the light will cast shadows. 
    /// The value is measured in world units.
    /// </summary>
    /// <throws cref="ArgumentOutOfRangeException">
    /// Thrown if the value is less than 0.
    /// </throws>
    public float MaximumShadowsDistance
    {
        get => this._maximumShadowDistance;
        set
        {
            if (this._maximumShadowDistance < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "The maximum shadow distance cannot be negative.");
            }

            this._maximumShadowDistance = value;
        }
    }

    private float _splitLambda = 0.15f;

    /// <summary>
    /// Gets or sets the split lamda used to calculate the cascade splits.
    /// A value of zero will make the shadow maps to be distributed evenly and 
    /// a value of 1 will make the shadow maps to be distributed with a 
    /// logarithmic distribution. A value between 0 and 1 will 
    /// use a mix of the two methods.
    /// </summary>
    /// <throws cref="ArgumentOutOfRangeException">
    /// Thrown if the value is less than zero or greater than one.
    /// </throws>
    public float SplitLambda
    {
        get => this._splitLambda;
        set
        {
            if (value < 0 || value > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Split lambda must be between 0 and 1.");
            }

            this._splitLambda = value;
        }
    }

    private uint _cascadesCount = 4;

    /// <summary>
    /// Gets or sets the number of cascades used by the light. The number must be between 1 and 4.
    /// A value of 1 means that the light will use a single shadow map with no splits.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value is less than 1 or greater than 4.
    /// </exception>
    public uint CascadesCount
    {
        get => this._cascadesCount;
        set
        {
            if (value < 1 || value > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "The number of cascades must be between 1 and 4.");
            }

            this._cascadesCount = value;
        }
    }

    private uint _size = 2048;

    /// <summary>
    /// Gets or sets the size of the shadow map texture.
    /// It must be a power of two. This size will be used for each cascade.
    /// So if the value is 1024, the first cascade will be 1024x1024, the second 1024x1024, etc.
    /// The cascades are stored as an array texture.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The shadow map size must be a power of two.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The shadow map size must be greater than zero.
    /// </exception>
    public uint Size
    {
        get => this._size;
        set
        {
            if ((value & (value - 1)) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "The shadow map size must be a power of two.");
            }

            if (value == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "The shadow map size cannot be zero.");
            }

            this._size = value;
        }
    }

    /// <summary>
    /// Gets or sets the padding added to the back of the shadow map 
    /// when culling the geometry so the geometry that is behind the camera
    /// is rendered.
    /// </summary>
    public float CullingZPadding { get; set; } = 5f;

    /// <summary>
    /// Gets or sets the depth bias used when rendering the shadow map. 
    /// The units are in texels so a value of 0.1f will bias the depth by 0.1 texels.
    /// This way the same value works independent of the shadow map resolution.
    /// </summary>
    public float DepthBias { get; set; } = 5.0f;

    /// <summary>
    /// Gets or sets the normal offset used when rendering the shadow map. 
    /// The units are in texels so a value of 0.1f will offset the shadowmap coordinates 
    /// in the direction of the normal by 0.1 texels.
    /// This way the same value works independent of the shadow map resolution.
    /// </summary>
    public float NormalOffset { get; set; } = 0.0f;

    /// <summary>
    /// Gets or set the shadow color.
    /// </summary>
    public ColorF Color { get; set; } = new ColorF(0.0f, 0.0f, 0.0f, .8f);
}