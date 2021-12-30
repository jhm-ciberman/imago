using System;
using System.Numerics;
using LifeSim.Engine.SceneGraph;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering
{
    public static class ShadowCascadesHelper
    {
        public static ShadowCascadeInfo GetShadowMapMatrix(ICamera mainCamera, DirectionalLight light)
        {
            Matrix4x4 cameraViewProjectionMatrix;
            if (mainCamera is Camera3D camera3D)
            {
                Matrix4x4 cameraViewMatrix = camera3D.ViewMatrix;
                float near = camera3D.NearPlane;
                float far = MathF.Min(camera3D.FarPlane, light.ShadowsDistance);
                Matrix4x4 cameraProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(camera3D.FieldOfView, camera3D.AspectRatio, near, far);
                cameraViewProjectionMatrix = cameraViewMatrix * cameraProjectionMatrix;
            }
            else
            {
                cameraViewProjectionMatrix = mainCamera.ViewProjectionMatrix;
            }
            BoundingFrustum mainCameraFrustum = new BoundingFrustum(cameraViewProjectionMatrix);

            FrustumCorners corners = mainCameraFrustum.GetCorners();

            float sphereDiameter = MathF.Max(
                Vector3.Distance(corners.FarBottomLeft, corners.FarTopRight),
                Vector3.Distance(corners.NearBottomLeft, corners.FarTopRight)
            );

            sphereDiameter = MathF.Round(sphereDiameter * 16) / 16;

            Vector3 lightDirectionNormalized = Vector3.Normalize(light.Direction);

            Matrix4x4 lightViewMatrix = Matrix4x4.CreateLookAt(lightDirectionNormalized, Vector3.Zero, Vector3.UnitY);
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

            lightViewMatrix = Matrix4x4.CreateLookAt(centerWS, centerWS - Vector3.Normalize(light.Direction), Vector3.UnitY);

            Matrix4x4 lightProjectionMatrix = Matrix4x4.CreateOrthographic(sphereDiameter, sphereDiameter, 0, maxLS.Z - minLS.Z);

            return new ShadowCascadeInfo
            {
                ShadowMapMatrix = lightViewMatrix * lightProjectionMatrix,
                CameraPosition = centerWS,
                SplitDistance = minLS.Z,
            };
        }

        private enum SplitMode
        {
            Uniform,
            Logarithmic,
            Practical,
        }

        /*
        private SplitMode _splitMode = SplitMode.Uniform;

        private readonly float[] _cascadesSplits = new float[4]; // The number of cascades is customizable.

        private void _GetSplits(Camera3D camera, DirectionalLight light)
        {
            float near = camera.NearPlane;
            float far = MathF.Min(camera.FarPlane, light.ShadowsDistance);

            switch (this._splitMode)
            {

                case SplitMode.Uniform:
                    this._UniformSplit(this._cascadesSplits, near, far);
                    break;
                case SplitMode.Logarithmic:
                    this._LogarithmicSplit(this._cascadesSplits, near, far);
                    break;
                case SplitMode.Practical:
                    this._PracticalSplit(this._cascadesSplits, near, far, 0.5f);
                    break;
            }
        }

        private void _UniformSplit(float[] splits, float near, float far)
        {
            int count = splits.Length;
            float range = far - near;
            float step = range / (count - 1);
            for (int i = 0; i < count; i++)
            {
                splits[i] = near + step * i;
            }
        }

        private void _LogarithmicSplit(float[] splits, float near, float far)
        {
            int count = splits.Length;
            for (int i = 0; i < count; i++)
            {
                splits[i] = MathF.Pow(near * (far / near), i / count);
            }
        }

        private void _PracticalSplit(float[] splits, float near, float far, float lamda)
        {
            int count = splits.Length;
            float range = far - near;
            float step = range / (count - 1);
            for (int i = 0; i < count; i++)
            {
                var a = near + step * i;
                var b = MathF.Pow(near * (far / near), i / count);
                splits[i] = lamda * a + (1 - lamda) * b; // Interpolate the two
            }
        }
        */
    }
}