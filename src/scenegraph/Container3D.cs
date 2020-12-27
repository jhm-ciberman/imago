using System;
using System.Collections.Generic;

namespace LifeSim.SceneGraph
{
    public abstract class Container3D
    {
        public string name = string.Empty;

        private Container3D? _parent = null;
        public Container3D? parent => this._parent;

        private List<Node3D> _children = new List<Node3D>();
        public IReadOnlyList<Node3D> children => this._children;

        public void Add(Node3D node)
        {
            if (this._parent == this) return;
            node.parent?.Remove(node);
            if (! this._children.Contains(node)) {
                this._children.Add(node);
                node._parent = this;
            }
        }

        public void Remove(Node3D node)
        {
            if (node.parent == this) {
                this._children.Remove(node);
                node._parent = null;
            }
        }

        public T? Find<T>() where T : Node3D
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