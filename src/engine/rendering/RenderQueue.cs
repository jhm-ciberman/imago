using System.Collections.Generic;
using LifeSim.Engine.SceneGraph;
using Veldrid;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering
{
    public class RenderQueue
    {
        private readonly List<Renderable3D> _shadowsRenderList = new List<Renderable3D>();
        private readonly List<Renderable3D> _regularRenderList = new List<Renderable3D>();

        public RenderQueue()
        {

        }

        public void Update(Scene3D scene, Camera3D camera)
        {
            this._shadowsRenderList.Clear();
            this._regularRenderList.Clear();
            var frustum = new BoundingFrustum(camera.frustumCullingCamera.viewProjectionMatrix);

            foreach (var renderable in scene.renderables) {
                this._AddToRenderQueue(renderable, ref frustum);
            }

            //System.Console.WriteLine(this._regularRenderList.Count + " of " + scene.renderables.Count);
        }

        private void _AddToRenderQueueOld(Node3D node, ref BoundingFrustum frustum)
        {
            if (node is Renderable3D renderable) {
                this._AddToRenderQueue(renderable, ref frustum);
            }

            foreach (var child in node.children) {
                this._AddToRenderQueueOld(child, ref frustum);
            }
        }

        private void _AddToRenderQueue(Renderable3D renderable, ref BoundingFrustum frustum)
        {
            if (renderable.material == null || renderable.mesh == null) return;

            var contains = true; //(frustum.Contains(renderable.mesh.boundingSphere) != ContainmentType.Disjoint);

            if (contains) {
                if (renderable.material.castShadows) {
                    this._shadowsRenderList.Add(renderable);
                }
                this._regularRenderList.Add(renderable);
            }
        }

        public IReadOnlyList<Renderable3D> baseRenderables => this._regularRenderList;
        public IReadOnlyList<Renderable3D> shadowRenderables => this._shadowsRenderList;
    }
}