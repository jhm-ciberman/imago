using System.Collections.Generic;
using LifeSim.Core;
using LifeSim.Rendering;

namespace LifeSim.Engine.SceneGraph
{
    public class Canvas2D
    {
        public Viewport viewport;

        private SwapPopList<ICanvasItem> _items = new SwapPopList<ICanvasItem>();
        public IReadOnlyList<ICanvasItem> items => this._items;
        
        private Node2D _root = new Node2D();
        public Node2D root => this._root;
        
        public Canvas2D(Viewport viewport)
        {
            this.viewport = viewport;
        }

        public void Add(Node2D node)
        {
            this._root.Add(node);
            this._AddNodeToRecursive(node);
        } 

        public void Remove(Node2D node)
        {
            this._root.Remove(node);
            this._RemoveNodeRecursive(node);
        } 

        public void AddCanvasItem(ICanvasItem canvasItem)
        {
            this._items.Add(canvasItem);
        }

        public void RemoveCanvasItem(ICanvasItem canvasItem)
        {
            this._items.Remove(canvasItem);
        }

        internal void _AddNodeToRecursive(Node2D node)
        {
            if (node is ICanvasItem canvasItem) {
                this.AddCanvasItem(canvasItem);
            }
            for (int i = 0; i < node.children.Count; i++) {
                this._AddNodeToRecursive(node.children[i]);
            }
        }

        internal void _RemoveNodeRecursive(Node2D node)
        {
            if (node is ICanvasItem canvasItem) {
                this.RemoveCanvasItem(canvasItem);
            }
            for (int i = 0; i < node.children.Count; i++) {
                this._RemoveNodeRecursive(node.children[i]);
            }
        }

        public void UpdateWorldMatrices()
        {
            foreach (var child in this._root.children) {
                child.UpdateWorldMatrix();
            }
        }
    }
}