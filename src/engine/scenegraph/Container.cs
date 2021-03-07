using System.Collections.Generic;

namespace LifeSim.Engine.SceneGraph
{
    public abstract class Container<TChildNode> where TChildNode : Container<TChildNode>
    {
        public event System.Action<TChildNode>? onChildAdded;
        public event System.Action<TChildNode>? onChildRemoved;

        public string name = string.Empty;

        private Container<TChildNode>? _parent = null;
        public Container<TChildNode>? parent => this._parent;

        private readonly List<TChildNode> _children = new List<TChildNode>();
        public IReadOnlyList<TChildNode> children => this._children;

        public void Add(TChildNode node)
        {
            if (node._parent == this) return;
            if (node == this) return;
            
            if (node._parent != null) {
                node._parent.Remove(node);
            }

            this._children.Add(node);
            node._parent = this;
            node.onChildAdded += this._OnNodeAdded;
            node.onChildRemoved += this._OnNodeRemoved;
            this.onChildAdded?.Invoke(node);
        }

        public void Remove(TChildNode node)
        {
            if (node._parent != this) return;

            this._children.Remove(node);
            node._parent = null;
            node.onChildAdded -= this._OnNodeAdded;
            node.onChildRemoved -= this._OnNodeRemoved;
            this.onChildRemoved?.Invoke(node);
        }

        private void _OnNodeAdded(TChildNode node)
        {
            this.onChildAdded?.Invoke(node);
        }

        private void _OnNodeRemoved(TChildNode node)
        {
            this.onChildRemoved?.Invoke(node);
        }

        public T? Find<T>() where T : TChildNode
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

        public T? Find<T>(string name) where T : TChildNode
        {
            if (this is T childT && this.name == name) {
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
            System.Console.WriteLine(indent + "- " + this.GetType().Name + ": " + this.name);
            indent += "  ";
            foreach (var child in this.children) {
                child.PrintHierarchyToConsole(indent);
            }
        }

        public void ForEachRecursive<T>(System.Action<T> action) where T : TChildNode
        {
            if (this is T childT) {
                action(childT);
            }
            foreach (var child in this.children) {
                child.ForEachRecursive<T>(action);
            }
        }


        public T? FindPath<T>(string name) where T : TChildNode
        {
            var arrayPaths = name.Split('/');
            int currentIndex = 0;
            Container<TChildNode> currentNode = this;
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

    }
}