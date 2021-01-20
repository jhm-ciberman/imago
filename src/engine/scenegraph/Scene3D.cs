using System.Numerics;
using LifeSim.Engine.Rendering;
using Veldrid;

namespace LifeSim.Engine.SceneGraph
{
    public class Scene3D : Container<Node3D>, ILayer
    {
        public Camera3D? activeCamera = null;

        public DirectionalLight mainLight = new DirectionalLight();
        
        public Vector3 ambientColor = new Vector3(.2f, .2f, .2f);

        public RgbaFloat clearColor = new RgbaFloat(0.84f, 0.84f, 0.86f, 1.0f);
        //private RgbaFloat _clearColor = new RgbaFloat(0.04f, 0.04f, 0.06f, 1.0f);

        public Scene3D()
        {
            //
        }

        public void UpdateWorldMatrices()
        {
            foreach (var child in this.children) {
                child.UpdateWorldMatrix();
            }
        }

        void ILayer.Render(GPURenderer renderer)
        {
            renderer.RenderScene3D(this);
        }
    }
}