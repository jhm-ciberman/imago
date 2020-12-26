using System.Diagnostics;
using System.Numerics;

namespace LifeSim.Rendering
{
    public class SkinnedRenderable3D : Renderable3D
    {
        public Skin skin;

        public SkinnedRenderable3D(GPUMesh mesh, Material material, Skin skin) : base(mesh, material)
        {
            this.skin = skin;
        }

        public void CopyMatricesToBuffer(ref BonesInfo buffer, ref Matrix4x4 inverseMeshWorldMatrix)
        {

            var joints = this.skin.joints;
            for (int i = 0; i < joints.Count; i++) {
                buffer.bonesMatrices[i] = this.skin.inverseBindMatrices[i] * joints[i].worldMatrix;
            }
        }
    }
}