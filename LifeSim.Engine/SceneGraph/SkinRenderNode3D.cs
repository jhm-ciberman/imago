using System.Numerics;
using LifeSim.Engine.Anim;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.SceneGraph
{
    public class SkinRenderNode3D : RenderNode3D
    {
        public readonly BindedSkin? skin = null;

        public SkinRenderNode3D(Mesh mesh, SurfaceMaterial material, BindedSkin skin) : base(mesh, material)
        {
            this.skin = skin;
        }

        public SkinRenderNode3D() : base()
        {

        }

        public void Update()
        {
            if (this.skin == null || this._renderable == null) return;

            Matrix4x4.Invert(this.worldMatrix, out Matrix4x4 inverseMeshWorldMatrix);
            var joints = this.skin.joints;
            var invBindMatrices = this.skin.inverseBindMatrices;
            var skeleton = this._renderable.skeleton;
            for (int i = 0; i < joints.Count; i++) {
                skeleton.bonesMatrices[i] = invBindMatrices[i] * joints[i].worldMatrix * inverseMeshWorldMatrix;
            }
        }
    }
}