using System.Collections.Generic;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.SceneGraph
{
    public class CanvasLayer
    {
        public Viewport Viewport;
        private readonly SwapPopList<ICanvasItem> _items = new SwapPopList<ICanvasItem>();

        public IReadOnlyList<ICanvasItem> Items => this._items;

        private readonly Node2D _root = new Node2D();

        public CanvasLayer(Viewport viewport)
        {
            this.Viewport = viewport;
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
            if (node is ICanvasItem canvasItem)
            {
                this.AddCanvasItem(canvasItem);
            }
            for (int i = 0; i < node.Children.Count; i++)
            {
                this._AddNodeToRecursive(node.Children[i]);
            }
        }

        internal void _RemoveNodeRecursive(Node2D node)
        {
            if (node is ICanvasItem canvasItem)
            {
                this.RemoveCanvasItem(canvasItem);
            }
            for (int i = 0; i < node.Children.Count; i++)
            {
                this._RemoveNodeRecursive(node.Children[i]);
            }
        }

        public void UpdateWorldMatrices()
        {
            foreach (var child in this._root.Children)
            {
                child.UpdateWorldMatrix();
            }
        }
    }
}