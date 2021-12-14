using System;
using System.Numerics;
using LifeSim.Engine.SceneGraph;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering
{
    public class DirectionalLight
    {
        public static DirectionalLight? Singleton { get; set; }

        public DirectionalLight()
        {
            //Singleton = this;
        }



        public Vector3 Direction { get; set; } = new Vector3(100, 200, 100);
        public ColorF Color { get; set; } = ColorF.White;
        public float ShadowsDistance { get; set; } = 30f;

        //public Matrix4x4 GetShadowMapMatrix(ICamera mainCamera)
        //{
        //    return Matrix4x4.CreateLookAt(mainCamera.Position + this.Direction, mainCamera.Position, Vector3.UnitY)
        //        * Matrix4x4.CreateOrthographic(20, 20, 0.1f, this.ShadowsDistance);
        //}

        public Matrix4x4 GetShadowMapMatrix(ICamera mainCamera)
        {
            Matrix4x4 lightViewMatrix = Matrix4x4.CreateLookAt(Vector3.Normalize(this.Direction), Vector3.Zero, Vector3.UnitY);
            Matrix4x4.Invert(lightViewMatrix, out Matrix4x4 lightViewMatrixInverse);

            BoundingFrustum mainCameraFrustum = new BoundingFrustum(mainCamera.ViewProjectionMatrix);
            FrustumCorners corners = mainCameraFrustum.GetCorners();

            Span<Vector3> frustumCornersLS = stackalloc Vector3[8];
            frustumCornersLS[0] = Vector3.Transform(corners.FarBottomLeft, lightViewMatrix);
            frustumCornersLS[1] = Vector3.Transform(corners.FarBottomRight, lightViewMatrix);
            frustumCornersLS[2] = Vector3.Transform(corners.FarTopLeft, lightViewMatrix);
            frustumCornersLS[3] = Vector3.Transform(corners.FarTopRight, lightViewMatrix);
            frustumCornersLS[4] = Vector3.Transform(corners.NearBottomLeft, lightViewMatrix);
            frustumCornersLS[5] = Vector3.Transform(corners.NearBottomRight, lightViewMatrix);
            frustumCornersLS[6] = Vector3.Transform(corners.NearTopLeft, lightViewMatrix);
            frustumCornersLS[7] = Vector3.Transform(corners.NearTopRight, lightViewMatrix);

            Vector3 mainCameraCenter = (corners.FarBottomLeft + corners.FarBottomRight + corners.FarTopLeft + corners.FarTopRight
                + corners.NearBottomLeft + corners.NearBottomRight + corners.NearTopLeft + corners.NearTopRight) / 8;

            float shadowCameraDistance = mainCamera.FarPlane - mainCamera.NearPlane;

            Vector3 min = frustumCornersLS[0], max = frustumCornersLS[0];
            for (int i = 1; i < 8; i++)
            {
                min = Vector3.Min(min, frustumCornersLS[i]);
                max = Vector3.Max(max, frustumCornersLS[i]);
            }

            Vector3 shadowCameraPos = new Vector3((max.X + min.X) / 2f, (max.Y + min.Y) / 2f, max.Z);
            shadowCameraPos = Vector3.Transform(shadowCameraPos, lightViewMatrixInverse);

            return Matrix4x4.CreateLookAt(shadowCameraPos, shadowCameraPos - Vector3.Normalize(this.Direction), Vector3.UnitY)
                 * Matrix4x4.CreateOrthographic(max.X - min.X, max.Y - min.Y, 0, max.Z - min.Z);
        }
    }

}