using System.Collections.Generic;
using System.Numerics;

namespace LifeSim.Engine.SceneGraph 
{
    public class Node3D
    {
        public event System.Action<Event<Node3D>>? onEvent;

        public string name { get; set; } = string.Empty;

        private Node3D? _parent = null;
        public Node3D? parent => this._parent;

        private readonly List<Node3D> _children = new List<Node3D>();
        public IReadOnlyList<Node3D> children => this._children;

        private Vector3    _position = Vector3.Zero;
        private Quaternion _rotation = Quaternion.Identity;
        private Vector3    _scale = Vector3.One;

        public Vector3    position { get => this._position; set { this._position = value; this._OnTransformDirty(); } }
        public Quaternion rotation { get => this._rotation; set { this._rotation = value; this._OnTransformDirty(); } }
        public Vector3    scale    { get => this._scale;    set { this._scale = value;    this._OnTransformDirty(); } }

        private Matrix4x4 _localMatrix = Matrix4x4.Identity;
        private bool _localMatrixDirty = false;

        private Matrix4x4 _worldMatrix;
        public ref Matrix4x4 worldMatrix => ref this._worldMatrix;

        public bool transformIsDirty => this._localMatrixDirty;

        public Vector3 worldPosition => Vector3.Transform(Vector3.Zero, this._worldMatrix);
        public Vector3 worldScale => Vector3.Transform(this._scale, this._worldMatrix);

        public void Add(Node3D node)
        {
            if (node._parent == this) return;
            if (node == this) return;
            
            if (node._parent != null) {
                node._parent.Remove(node);
            }

            this._children.Add(node);
            node._parent = this;
            node.onEvent += this._OnNotified;
            this._Notify(node, EventType.ChildAdded);
        }

        public void Remove(Node3D node)
        {
            if (node._parent != this) return;

            this._children.Remove(node);
            node._parent = null;
            node.onEvent -= this._OnNotified;
            this._Notify(node, EventType.ChildRemoved);
        }

        private void _OnNotified(Event<Node3D> e)
        {
            this.onEvent?.Invoke(e);
        }

        protected void _Notify(Node3D node, EventType eventType)
        {
            this.onEvent?.Invoke(new Event<Node3D>(node, eventType));
        }

        private void _OnTransformDirty()
        {
            if (this._localMatrixDirty) return;
            this._localMatrixDirty = true;
            this._Notify(this, EventType.TransformDirty);
        }

        public void UpdateWorldMatrix()
        {
            this._worldMatrix = this.GetLocalMatrix();
            if (this._parent != null) {
                this._worldMatrix *= this._parent._localMatrix;
            }
            for(int i = 0; i < this.children.Count; i++) {
                this.children[i]._UpdateWorldMatrix(ref this._worldMatrix);
            }
        }

        private void _UpdateWorldMatrix(ref Matrix4x4 parentMatrix)
        {
            this._worldMatrix = this.GetLocalMatrix() * parentMatrix;
            for(int i = 0; i < this.children.Count; i++) {
                this.children[i]._UpdateWorldMatrix(ref this._worldMatrix);
            }
        }

        public ref Matrix4x4 GetLocalMatrix()
        {
            if (this._localMatrixDirty) {
                this._localMatrix = Matrix4x4.CreateScale(this._scale)
                    * Matrix4x4.CreateFromQuaternion(this._rotation)
                    * Matrix4x4.CreateTranslation(this._position);
                this._localMatrixDirty = false;
            }
            return ref this._localMatrix;
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

        public void ForEachRecursive<T>(System.Action<T> action) where T : Node3D
        {
            if (this is T childT) {
                action(childT);
            }
            foreach (var child in this.children) {
                child.ForEachRecursive<T>(action);
            }
        }

        public T? FindPath<T>(string name) where T : Node3D
        {
            var arrayPaths = name.Split('/');
            int currentIndex = 0;
            Node3D currentNode = this;
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