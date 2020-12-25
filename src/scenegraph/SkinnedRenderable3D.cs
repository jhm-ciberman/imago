using System.Numerics;

namespace LifeSim.Rendering
{
    public class SkinnedRenderable3D : Renderable3D
    {
        public Skin skin;

        public Matrix4x4[] boneMatrices;

        public SkinnedRenderable3D(GPUMesh mesh, Material material, Skin skin) : base(mesh, material)
        {
            this.skin = skin;
            this.boneMatrices = new Matrix4x4[skin.joints.Count];

        }

        public void CopyMatricesToBuffer(ref BonesInfo buffer)
        {
            for (int i = 0; i < this.skin.joints.Count; i++) {
                buffer.bonesMatrices[i] = Matrix4x4.Identity;
            }
        }
    }
}