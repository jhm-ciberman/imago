using System.Collections.Generic;
using LifeSim.SceneGraph;
using Veldrid;

namespace LifeSim.Rendering
{
    public class RenderList
    {
        private List<Renderable3D> _renderList = new List<Renderable3D>();

        public RenderList()
        {

        }

        public void UpdateRenderList(Container3D node)
        {
            this._renderList.Clear();
            this._UpdateRecursive(node);
        }

        private void _UpdateRecursive(Container3D node)
        {
            if (node is Renderable3D renderable) {
                this._renderList.Add(renderable);
            }
            foreach (var child in node.children) {
                this._UpdateRecursive(child);
            }
        }

        public IReadOnlyList<Renderable3D> renderables => this._renderList;
    }
}