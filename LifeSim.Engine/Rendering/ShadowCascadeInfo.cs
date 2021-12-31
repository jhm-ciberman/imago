using System;
using System.Numerics;
using LifeSim.Engine.SceneGraph;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering
{
    public class ShadowCascadeInfo
    {
        /// <summary>
        /// Gets or sets the shadow map view projection matrix.
        /// </summary>
        public Matrix4x4 ShadowMapMatrix { get; set; }

        /// <summary>
        /// Gets or sets the near split distance of the cascade.
        /// </summary>
        public float NearSplitDistance { get; set; }

        /// <summary>
        /// Gets or sets the far split distance of the cascade.
        /// </summary>
        public float FarSplitDistance { get; set; }

        /// <summary>
        /// Gets or sets the camera position
        /// </summary>
        public Vector3 CameraPosition { get; set; }

        public void Update(Camera3D camera, Vector3 lightDirection, float near, float far)
        {
            Matrix4x4 cameraViewProjectionMatrix;

            Matrix4x4 cameraViewMatrix = camera.ViewMatrix;
            Matrix4x4 cameraProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(camera.FieldOfView, camera.AspectRatio, near, far);
            cameraViewProjectionMatrix = cameraViewMatrix * cameraProjectionMatrix;

            BoundingFrustum mainCameraFrustum = new BoundingFrustum(cameraViewProjectionMatrix);

            FrustumCorners corners = mainCameraFrustum.GetCorners();

            float sphereDiameter = MathF.Max(
                Vector3.Distance(corners.FarBottomLeft, corners.FarTopRight),
                Vector3.Distance(corners.NearBottomLeft, corners.FarTopRight)
            );

            sphereDiameter = MathF.Round(sphereDiameter * 16) / 16;

            lightDirection = Vector3.Normalize(lightDirection);

            Matrix4x4 lightViewMatrix = Matrix4x4.CreateLookAt(lightDirection, Vector3.Zero, Vector3.UnitY);
            Matrix4x4.Invert(lightViewMatrix, out Matrix4x4 lightViewMatrixInverse);

            Span<Vector3> frustumCornersWS = stackalloc Vector3[8];
            frustumCornersWS[0] = corners.FarBottomLeft;
            frustumCornersWS[1] = corners.FarBottomRight;
            frustumCornersWS[2] = corners.FarTopLeft;
            frustumCornersWS[3] = corners.FarTopRight;
            frustumCornersWS[4] = corners.NearBottomLeft;
            frustumCornersWS[5] = corners.NearBottomRight;
            frustumCornersWS[6] = corners.NearTopLeft;
            frustumCornersWS[7] = corners.NearTopRight;

            Vector3 minLS = new Vector3(float.MaxValue);
            Vector3 maxLS = new Vector3(float.MinValue);
            for (int i = 0; i < 8; i++)
            {
                Vector3 frustumCornerLS = Vector3.Transform(frustumCornersWS[i], lightViewMatrix);
                minLS = Vector3.Min(minLS, frustumCornerLS);
                maxLS = Vector3.Max(maxLS, frustumCornerLS);
            }

            float shadowMapSize = 512f;
            float f = sphereDiameter / shadowMapSize;

            Vector3 frustumCenterWS = (corners.FarBottomLeft + corners.FarTopRight + corners.FarBottomRight + corners.FarTopLeft
            + corners.NearBottomLeft + corners.NearTopRight + corners.NearBottomRight + corners.NearTopLeft) / 8f;

            Vector3 centerLS = Vector3.Transform(frustumCenterWS, lightViewMatrix);
            centerLS.X = MathF.Round(centerLS.X / f) * f;
            centerLS.Y = MathF.Round(centerLS.Y / f) * f;
            centerLS.Z = maxLS.Z;
            Vector3 centerWS = Vector3.Transform(centerLS, lightViewMatrixInverse);

            //GizmosLayer.Default.DrawWireSphere(frustumCenterWS, sphereDiameter / 2f, LifeSim.Color.Red);
            //GizmosLayer.Default.DrawWireSphere(frustumCenterWS, sphereDiameter / 10f, LifeSim.Color.Cyan);

            lightViewMatrix = Matrix4x4.CreateLookAt(centerWS, centerWS - lightDirection, Vector3.UnitY);

            Matrix4x4 lightProjectionMatrix = Matrix4x4.CreateOrthographic(sphereDiameter, sphereDiameter, 0, maxLS.Z - minLS.Z);

            this.ShadowMapMatrix = lightViewMatrix * lightProjectionMatrix;
            this.CameraPosition = centerWS;
            this.NearSplitDistance = minLS.Z;
            this.FarSplitDistance = maxLS.Z;
        }
    }
}