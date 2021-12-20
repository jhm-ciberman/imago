using System;
using System.Numerics;
using LifeSim.Engine.SceneGraph;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering
{
    public class DirectionalLight
    {
        public DirectionalLight()
        {

        }


        private bool _newMode = false;
        public Vector3 Direction { get; set; } = new Vector3(100, 200, 100);
        public ColorF Color { get; set; } = ColorF.White;
        public float ShadowsDistance { get; set; } = 200f;

        private bool _stabilizeCascades = false;

        public Matrix4x4 GetShadowMapMatrixOldMode(ICamera mainCamera)
        {
            return Matrix4x4.CreateLookAt(mainCamera.Position + this.Direction, mainCamera.Position, Vector3.UnitY)
                * Matrix4x4.CreateOrthographic(30, 30, 0.1f, 200f);
        }

        public unsafe Matrix4x4 GetShadowMapMatrix(ICamera mainCamera)
        {
            if (Input.GetKeyDown(Veldrid.Key.F))
            {
                this._newMode = !this._newMode;
            }
            if (!this._newMode)
            {
                return this.GetShadowMapMatrixOldMode(mainCamera);
            }

            if (Input.GetKeyDown(Veldrid.Key.G))
            {
                this._stabilizeCascades = !this._stabilizeCascades;
            }



            BoundingFrustum mainCameraFrustum = new BoundingFrustum(mainCamera.ViewProjectionMatrix);
            FrustumCorners corners = mainCameraFrustum.GetCorners();

            Span<Vector3> frustumCornersWS = stackalloc Vector3[8];
            frustumCornersWS[0] = corners.FarBottomLeft;
            frustumCornersWS[1] = corners.FarBottomRight;
            frustumCornersWS[2] = corners.FarTopLeft;
            frustumCornersWS[3] = corners.FarTopRight;
            frustumCornersWS[4] = corners.NearBottomLeft;
            frustumCornersWS[5] = corners.NearBottomRight;
            frustumCornersWS[6] = corners.NearTopLeft;
            frustumCornersWS[7] = corners.NearTopRight;

            Vector3 frustumCenter = (corners.FarBottomLeft + corners.FarBottomRight + corners.FarTopLeft + corners.FarTopRight
                + corners.NearBottomLeft + corners.NearBottomRight + corners.NearTopLeft + corners.NearTopRight) / 8;

            float shadowCameraDistance = mainCamera.FarPlane - mainCamera.NearPlane;

            Vector3 min, max;
            if (this._stabilizeCascades)
            {
                float sphereRadius = 0;
                for (int i = 0; i < 8; ++i)
                {
                    float dist = Vector3.Distance(frustumCornersWS[i], frustumCenter);
                    sphereRadius = Math.Max(sphereRadius, dist);
                }

                sphereRadius = (float)Math.Ceiling(sphereRadius * 16) / 16;

                max = new Vector3(sphereRadius, sphereRadius, sphereRadius);
                min = -max;
            }
            else
            {
                Matrix4x4 lightViewMatrix = Matrix4x4.CreateLookAt(frustumCenter, frustumCenter - Vector3.Normalize(this.Direction), Vector3.UnitY);
                Matrix4x4.Invert(lightViewMatrix, out Matrix4x4 lightViewMatrixInverse);

                // Calculate the AABB of the frustum in light space
                min = new Vector3(float.MaxValue);
                max = new Vector3(float.MinValue);
                for (int i = 0; i < 8; i++)
                {
                    Vector3 cornerLS = Vector3.Transform(frustumCornersWS[i], lightViewMatrix);
                    min = Vector3.Min(min, cornerLS);
                    max = Vector3.Max(max, cornerLS);
                }
            }

            Vector3 shadowCameraPos = frustumCenter - Vector3.Normalize(this.Direction) * min.Z;

            GizmosLayer.Default.DrawLine(frustumCenter, shadowCameraPos, LifeSim.Color.Red);

            var matrix = Matrix4x4.CreateLookAt(shadowCameraPos, frustumCenter, Vector3.UnitY)
                 * Matrix4x4.CreateOrthographic(max.X - min.X, max.Y - min.Y, 0f, max.Z - min.Z);

            var frustum = new BoundingFrustum(matrix);
            GizmosLayer.Default.DrawFrustum(ref frustum, LifeSim.Color.Red);

            return matrix;
        }
    }

}