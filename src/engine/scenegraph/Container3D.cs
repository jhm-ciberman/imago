using System.Collections.Generic;

namespace LifeSim.Engine.SceneGraph
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
            if (node.parent == this) return;
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

        public T? Find<T>(string name) where T : Node3D
        {
            if (this is T childT && this.name == name) {
                return childT;
            }
            foreach (var child in this.children) {
                var result = child.Find<T>(name);
                if (result != null) {
                    return result;
                }
            }
            return null;
        }

        public T? FindPath<T>(string name) where T : Node3D
        {
            var arrayPaths = name.Split('/');
            int currentIndex = 0;
            Container3D currentNode = this;
            bool found = true;
            while (found && currentIndex < arrayPaths.Length) {
                var currentNameToFind = arrayPaths[currentIndex];
                foreach (var child in currentNode.children) {
                    if (child.name == currentNameToFind) {
                        currentNode = child;
                        currentIndex++;
                        found = true;
                        break;
                    }
                }
            }
            if (currentIndex < arrayPaths.Length) {
                return null;
            } else if (currentNode is T nodeT) {
                return nodeT;
            } else {
                return null;
            }
        }

        public void ForEachRecursive<T>(System.Action<Node3D> action) where T : Node3D
        {
            if (this is T childT) {
                action(childT);
            }
            foreach (var child in this.children) {
                child.ForEachRecursive<T>(action);
            }
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