using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.Rendering
{
    public class DirectionalLight
    {
        public DirectionalLight(int cascadesCount = 4)
        {
            this.CascadesCount = cascadesCount;
        }

        /// <summary>
        /// Gets or sets the light direction.
        /// </summary>
        public Vector3 Direction { get; set; } = new Vector3(30, 30, 30);

        /// <summary>
        /// Gets or sets the color of the light. The alpha component determines the intensity of the light.
        /// </summary>
        public ColorF Color { get; set; } = ColorF.White;

        /// <summary>
        /// Gets or sets the maximum distance that the light will cast shadows. 
        /// The value is measured in world units.
        /// </summary>
        public float ShadowsDistance { get; set; } = 10f;

        public Matrix4x4 GetShadowMapMatrixOldMode(ICamera mainCamera)
        {
            return Matrix4x4.CreateLookAt(mainCamera.Position + this.Direction, mainCamera.Position, Vector3.UnitY)
                * Matrix4x4.CreateOrthographic(30, 30, 0.1f, 200f);
        }

        private ShadowCascadeInfo[] _cascades = null!;

        public IReadOnlyList<ShadowCascadeInfo> Cascades => this._cascades;

        /// <summary>
        /// Gets or sets the split mode used to calculate the cascade splits.
        /// </summary>
        public ShadowCascadeSplitMode SplitMode { get; set; } = ShadowCascadeSplitMode.Practical;

        /// <summary>
        /// Gets or sets the split lamda used to calculate the cascade splits when
        /// using the practical split mode. In any other split mode, this value is ignored.
        /// </summary>
        public float SplitLambda { get; set; } = 0.5f;

        /// <summary>
        /// Gets or sets the number of cascades used by the light. A value of 1 means
        /// that the light will use a single shadow map with no splits.
        /// </summary>
        public int CascadesCount
        {
            get => this._cascades.Length;
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Cascades count must be at least 1.");
                }

                this._cascades = new ShadowCascadeInfo[value];
                for (int i = 0; i < this._cascades.Length; i++)
                {
                    this._cascades[i] = new ShadowCascadeInfo();
                }
            }
        }

        /// <summary>
        /// Updates the shadow map for the light.
        /// </summary>
        public void UpdateShadowMapCascades(Camera3D camera)
        {
            float near = camera.NearPlane;
            float far = MathF.Min(camera.FarPlane, this.ShadowsDistance);

            _ = Parallel.For(0, this.CascadesCount, i =>
            {
                // I know that I'm recalculating the near and far split distances for each cascade,
                // but I think (not sure) that it is more efficient as it's parallelized.
                float nearSplit = this.GetSplitDistance(near, far, i);
                float farSplit = this.GetSplitDistance(near, far, i + 1);
                farSplit = MathF.Min(farSplit, this.ShadowsDistance);
                this._cascades[i].Update(camera, this.Direction, nearSplit, farSplit);
            });
        }

        private float GetSplitDistance(float near, float far, int index)
        {
            return this.SplitMode switch
            {
                ShadowCascadeSplitMode.Uniform => GetUniformSplitDistance(near, far, index, this._cascades.Length),
                ShadowCascadeSplitMode.Logarithmic => GetLogarithmicSplitDistance(near, far, index, this._cascades.Length),
                ShadowCascadeSplitMode.Practical => GetPracticalSplitDistance(near, far, index, this._cascades.Length, this.SplitLambda),
                _ => throw new ArgumentOutOfRangeException(nameof(this.SplitMode), "Invalid split mode.")
            };
        }

        private static float GetUniformSplitDistance(float near, float far, int index, int count)
        {
            return near + ((far - near) * index / count);
        }

        private static float GetLogarithmicSplitDistance(float near, float far, int index, int count)
        {
            return MathF.Pow(near * (far / near), index / count);
        }

        private static float GetPracticalSplitDistance(float near, float far, int index, int count, float lambda)
        {
            // Lerp between uniform and logarithmic split distances.
            // https://developer.nvidia.com/gpugems/gpugems3/part-ii-light-and-shadows/chapter-10-parallel-split-shadow-maps-programmable-gpus

            float uniformDistance = GetUniformSplitDistance(near, far, index, count);
            float logarithmicDistance = GetLogarithmicSplitDistance(near, far, index, count);
            return uniformDistance * lambda + logarithmicDistance * (1 - lambda);
        }

    }
}