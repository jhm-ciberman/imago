using System;
using System.Collections.Generic;

namespace LifeSim.Engine.SceneGraph
{
    public abstract class Container2D
    {
        public string name = string.Empty;

        private Container2D? _parent = null;
        public Container2D? parent => this._parent;

        private List<Node2D> _children = new List<Node2D>();
        public IReadOnlyList<Node2D> children => this._children;

        public void Add(Node2D node)
        {
            if (this._parent == this) return;
            node.parent?.Remove(node);
            if (! this._children.Contains(node)) {
                this._children.Add(node);
                node._parent = this;
            }
        }

        public void Remove(Node2D node)
        {
            if (node.parent == this) {
                this._children.Remove(node);
                node._parent = null;
            }
        }

        public T? Find<T>() where T : Node2D
        {
            if (this is T childT) {
                return childT;
            }
            foreach (var child in this.children) {
                var result = child.Find<T>();
                if (result != null) {
                    return result;
                }
            }
            return null;
        }

        public void PrintHierarchyToConsole(string indent = "")
        {
            System.Console.WriteLine(indent + "- " + this.GetType().Name + ": " +this.name);
            indent += "  ";
            foreach (var child in this.children) {
                child.PrintHierarchyToConsole(indent);
            }
        }
    }
}