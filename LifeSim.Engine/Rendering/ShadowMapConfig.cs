using System;
using System.Numerics;

namespace LifeSim.Engine.Rendering
{
    public class ShadowMapConfig
    {
        public event Action<uint>? OnShadowMapSizeChanged;

        public event Action<uint>? OnCascadeCountChanged;

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

                if (this._cascadesCount != value)
                {
                    this._cascadesCount = value;
                    this.OnCascadeCountChanged?.Invoke(value);
                }
            }
        }

        private uint _shadowMapSize = 2048;

        /// <summary>
        /// Gets or sets the size of the shadow map texture.
        /// It must be a power of two.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The shadow map size must be a power of two.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The shadow map size must be greater than zero.
        /// </exception>
        public uint ShadowMapResolution
        {
            get => this._shadowMapSize;
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

                if (value != this._shadowMapSize)
                {
                    this._shadowMapSize = value;
                    this.OnShadowMapSizeChanged?.Invoke(value);
                }
            }
        }

        private float _cullingZPadding = 5f;

        /// <summary>
        /// Gets or sets the padding added to the back of the shadow map 
        /// when culling the geometry so the geometry that is behind the camera
        /// is rendered.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The padding must be greater or equal to zero.
        /// </exception>
        public float CullingZPadding
        {
            get => this._cullingZPadding;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "The padding must be greater or equal to zero.");
                }

                this._cullingZPadding = value;
            }
        }

        private float _depthBias = 0.25f;

        /// <summary>
        /// Gets or sets the depth bias used when rendering the shadow map.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The depth bias must be greater or equal to zero.
        /// </exception>
        public float DepthBias
        {
            get => this._depthBias;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "The depth bias must be greater or equal to zero.");
                }

                this._depthBias = value;
            }
        }

        private float _normalBias = 0.1f;

        /// <summary>
        /// Gets or sets the normal bias used when rendering the shadow map.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The normal bias must be greater or equal to zero.
        /// </exception>
        public float NormalBias
        {
            get => this._normalBias;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "The normal bias must be greater or equal to zero.");
                }

                this._normalBias = value;
            }
        }

    }
}