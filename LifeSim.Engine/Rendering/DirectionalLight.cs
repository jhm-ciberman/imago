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

        public Vector3 Direction { get; set; } = new Vector3(30, 30, 30);

        public ColorF Color { get; set; } = ColorF.White;

        private bool _snap = false;

        public float ShadowsDistance { get; set; } = 200f;

        public Matrix4x4 GetShadowMapMatrixOldMode(ICamera mainCamera)
        {
            return Matrix4x4.CreateLookAt(mainCamera.Position + this.Direction, mainCamera.Position, Vector3.UnitY)
                * Matrix4x4.CreateOrthographic(30, 30, 0.1f, 200f);
        }

        public Matrix4x4 GetShadowMapMatrix(ICamera mainCamera)
        {
            //return this.GetShadowMapMatrixOldMode(mainCamera);

            if (Input.GetKeyDown(Veldrid.Key.F))
            {
                this._snap = !this._snap;
            }

            BoundingFrustum mainCameraFrustum = new BoundingFrustum(mainCamera.ViewProjectionMatrix);
            FrustumCorners corners = mainCameraFrustum.GetCorners();

            float sphereDiameter = MathF.Max(
                Vector3.Distance(corners.FarBottomLeft, corners.FarTopRight),
                Vector3.Distance(corners.NearBottomLeft, corners.FarTopRight)
            );

            sphereDiameter = MathF.Round(sphereDiameter * 16) / 16;

            Matrix4x4 lightViewMatrix = Matrix4x4.CreateLookAt(Vector3.Normalize(this.Direction), Vector3.Zero, Vector3.UnitY);
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

            Vector3 centerLS = new Vector3((maxLS.X + minLS.X) / 2f, (maxLS.Y + minLS.Y) / 2f, maxLS.Z);
            if (this._snap)
            {
                centerLS.X = MathF.Round(centerLS.X / f) * f;
                centerLS.Y = MathF.Round(centerLS.Y / f) * f;
            }
            Vector3 centerWS = Vector3.Transform(centerLS, lightViewMatrixInverse);

            lightViewMatrix = Matrix4x4.CreateLookAt(centerWS, centerWS - Vector3.Normalize(this.Direction), Vector3.UnitY);

            Matrix4x4 lightProjectionMatrix = Matrix4x4.CreateOrthographic(sphereDiameter, sphereDiameter, 0, maxLS.Z - minLS.Z);
            return lightViewMatrix * lightProjectionMatrix;
        }
    }
}