using System;
using System.Numerics;

namespace LifeSim.Engine.Rendering
{
    public class ShadowMapConfig
    {
        public event Action<uint>? OnShadowMapSizeChanged;

        public event Action<uint>? OnCascadeCountChanged;

        private float _maximumShadowDistance = 10f;

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

        private float _splitLambda = 0.5f;

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

        private uint _cascadesCount = 1;

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
                this.OnCascadeCountChanged?.Invoke(value);
            }
        }

        private uint _shadowMapSize = 1024;

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
        public uint ShadowMapSize
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

                this._shadowMapSize = value;
                this.OnShadowMapSizeChanged?.Invoke(value);
            }
        }
    }
}