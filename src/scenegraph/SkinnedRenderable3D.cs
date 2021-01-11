using System.Numerics;
using LifeSim.Anim;
using LifeSim.Rendering;

namespace LifeSim.SceneGraph
{
    public class SkinnedRenderable3D : Renderable3D
    {
        private BindedSkin? _skin = null;

        public SkinnedRenderable3D(GPUMesh mesh, SurfaceMaterial material, BindedSkin skin) : base(mesh, material)
        {
            this._skin = skin;
        }

        public SkinnedRenderable3D() : base()
        {

        }

        public void CopyMatricesToBuffer(ref BonesInfo buffer)
        {
            if (this._skin == null) return; // TODO: write identity matrices to the buffer
            
            Matrix4x4.Invert(this.worldMatrix, out Matrix4x4 inverseMeshWorldMatrix);
            var joints = this._skin.joints;
            for (int i = 0; i < joints.Count; i++) {
                buffer.bonesMatrices[i] = this._skin.inverseBindMatrices[i] * joints[i].worldMatrix * inverseMeshWorldMatrix;
            }
        }

        public override string[] GetShaderKeywords()
        {
            return new string[] { "USE_SKINNED_MESH" };
        }
    }
}