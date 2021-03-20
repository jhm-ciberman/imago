using System.Collections.Generic;
using LifeSim.Engine.SceneGraph;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class RenderList
    {
        private readonly List<RenderNode3D> _shadowsRenderList = new List<RenderNode3D>();
        private readonly List<RenderNode3D> _regularRenderList = new List<RenderNode3D>();

        public RenderList()
        {

        }

        public void UpdateRenderList(Node3D node)
        {
            this._shadowsRenderList.Clear();
            this._regularRenderList.Clear();
            this._UpdateRecursive(node);
        }

        private void _UpdateRecursive(Node3D node)
        {
            if (node is RenderNode3D renderable && renderable.material != null) {
                if (renderable.material.castShadows) {
                    this._shadowsRenderList.Add(renderable);
                }
                this._regularRenderList.Add(renderable);
            }
            foreach (var child in node.children) {
                this._UpdateRecursive(child);
            }
        }

        public IReadOnlyList<RenderNode3D> baseRenderables => this._regularRenderList;
        public IReadOnlyList<RenderNode3D> shadowRenderables => this._shadowsRenderList;
    }
}