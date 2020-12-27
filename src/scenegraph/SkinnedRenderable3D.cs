using System.Numerics;
using LifeSim.Anim;
using LifeSim.Rendering;

namespace LifeSim.SceneGraph
{
    public class SkinnedRenderable3D : Renderable3D
    {
        private BindedSkin _skin;

        public SkinnedRenderable3D(GPUMesh mesh, Material material, BindedSkin skin) : base(mesh, material)
        {
            this._skin = skin;
        }

        public void CopyMatricesToBuffer(ref BonesInfo buffer, ref Matrix4x4 meshWorldMatrix)
        {
            var joints = this._skin.joints;
            for (int i = 0; i < joints.Count; i++) {
                buffer.bonesMatrices[i] = this._skin.inverseBindMatrices[i] * joints[i].worldMatrix * meshWorldMatrix;
            }
        }
    }
}