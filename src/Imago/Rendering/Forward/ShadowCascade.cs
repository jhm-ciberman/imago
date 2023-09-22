using System;
using System.Numerics;
using Imago.SceneGraph;
using Veldrid.Utilities;

namespace Imago.Rendering.Forward;

internal class ShadowCascade
{
    public float SplitNear { get; set; }
    public float SplitFar { get; set; }

    public float DepthBias { get; set; }
    public float NormalOffset { get; set; }
    public Matrix4x4 ViewProjectionMatrix { get; set; }

    public void UpdateCascadeMatrix(int cascadeIndex, Camera camera, Vector3 lightDirection, float near, float far, ShadowMap config)
    {
        Matrix4x4 cameraProjectionMatrix = camera.GetShadowCascadeViewProjectionMatrix(near, far);
        Matrix4x4 cameraViewProjectionMatrix = camera.ViewMatrix * cameraProjectionMatrix;

        BoundingFrustum mainCameraFrustum = new BoundingFrustum(cameraViewProjectionMatrix);

        FrustumCorners corners = mainCameraFrustum.GetCorners();

        float sphereDiameter = MathF.Max(
                Vector3.Distance(corners.FarBottomLeft, corners.FarTopRight),
                Vector3.Distance(corners.NearBottomLeft, corners.FarTopRight)
            );

        sphereDiameter = MathF.Round(sphereDiameter * 16) / 16;

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

        float unitsPerTexel = sphereDiameter / config.Size;

        Vector3 frustumCenterWS = (corners.FarBottomLeft + corners.FarTopRight + corners.FarBottomRight + corners.FarTopLeft
            + corners.NearBottomLeft + corners.NearTopRight + corners.NearBottomRight + corners.NearTopLeft) / 8f;

        Vector3 centerLS = Vector3.Transform(frustumCenterWS, lightViewMatrix);
        centerLS.X = MathF.Round(centerLS.X / unitsPerTexel) * unitsPerTexel;
        centerLS.Y = MathF.Round(centerLS.Y / unitsPerTexel) * unitsPerTexel;
        centerLS.Z = maxLS.Z + config.CullingZPadding;
        Vector3 centerWS = Vector3.Transform(centerLS, lightViewMatrixInverse);

        float orthoDepth = maxLS.Z - minLS.Z + config.CullingZPadding;
        lightViewMatrix = Matrix4x4.CreateLookAt(centerWS, centerWS - lightDirection, Vector3.UnitY);

        Matrix4x4 lightProjectionMatrix = Matrix4x4.CreateOrthographic(sphereDiameter, sphereDiameter, 0.0f, orthoDepth);

        this.ViewProjectionMatrix = lightViewMatrix * lightProjectionMatrix;
        this.SplitNear = near;
        this.SplitFar = far;

        this.DepthBias = -config.DepthBias * unitsPerTexel;
        this.NormalOffset = -config.NormalOffset * unitsPerTexel;
    }
}
