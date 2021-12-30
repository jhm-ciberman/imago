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

        public float ShadowsDistance { get; set; } = 10f;

        public Matrix4x4 GetShadowMapMatrixOldMode(ICamera mainCamera)
        {
            return Matrix4x4.CreateLookAt(mainCamera.Position + this.Direction, mainCamera.Position, Vector3.UnitY)
                * Matrix4x4.CreateOrthographic(30, 30, 0.1f, 200f);
        }


        public struct ShadowCascade
        {
            public Matrix4x4 ViewProjection;
            public float Distance;


        }
    }
}