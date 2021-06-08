using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.SceneGraph 
{
    public class Node3D
    {
        public string name { get; set; } = string.Empty;

        private Node3D? _parent = null;
        public Node3D? parent => this._parent;

        internal Scene3D? _scene = null;
        public Scene3D? scene => this._scene;

        private readonly List<Node3D> _children = new List<Node3D>();
        public IReadOnlyList<Node3D> children => this._children;

        private Vector3    _position = Vector3.Zero;
        private Quaternion _rotation = Quaternion.Identity;
        private Vector3    _scale = Vector3.One;

        public Vector3 position 
        { 
            get => this._position;
            set { var old = this._position; this._position = value; if (old != value) this._OnTransformDirty(); } 
        }

        public Quaternion rotation 
        { 
            get => this._rotation;
            set { var old = this._rotation; this._rotation = value; if (old != value) this._OnTransformDirty(); } 
        }
        
        public Vector3 scale    
        { 
            get => this._scale;
            set { var old = this._scale; this._scale = value; if (old != value) this._OnTransformDirty(); } 
        }

        private Matrix4x4 _localMatrix = Matrix4x4.Identity;

        protected Matrix4x4 _worldMatrix = Matrix4x4.Identity;
        public ref Matrix4x4 worldMatrix => ref this._worldMatrix;

        protected bool _transformIsDirty = false;
        public bool transformIsDirty => this._transformIsDirty;

        public Vector3 worldPosition => Vector3.Transform(Vector3.Zero, this._worldMatrix);
        public Vector3 worldScale => Vector3.Transform(this._scale, this._worldMatrix);

        private Renderable? _renderable = null;
        public Renderable? renderable
        {
            get => this._renderable;
            set
            {
                if (this._renderable == value) return;
                this._renderable = value;
                if (this._renderable != null && ! this._transformIsDirty) {
                    this._renderable.SetTransform(ref this._worldMatrix);
                }
            }
        }

        public Node3D(Renderable? renderable = null)
        {
            this.renderable = renderable;
        }

        public void Add(Node3D node)
        {
            if (node._parent == this) return;
            if (node == this) return;
            
            if (node._parent != null) {
                node._parent.Remove(node);
            }
            this._children.Add(node);
            node._parent = this;
            node._scene = this._scene;
            node._transformIsDirty = true;
            node._scene?._OnChildAdded(node);
        }

        public void Remove(Node3D node)
        {
            if (node._parent != this) return;

            this._children.Remove(node);
            node._parent = null;
            node._scene?._OnChildRemoved(node);
            node._scene = null;
        }

        protected void _OnTransformDirty()
        {
            if (this._transformIsDirty) return;
            this._transformIsDirty = true;
            this._scene?._OnTransformDirty(this);
        }

        public void UpdateWorldMatrix()
        {
            this._worldMatrix = (this._parent != null)
                ? this.GetLocalMatrix() * this._parent._worldMatrix
                : this.GetLocalMatrix();

            this._renderable?.SetTransform(ref this.worldMatrix);

            for(int i = 0; i < this.children.Count; i++) {
                this.children[i]._UpdateWorldMatrix(ref this._worldMatrix);
            }
        }

        private void _UpdateWorldMatrix(ref Matrix4x4 parentMatrix)
        {
            this._worldMatrix = this.GetLocalMatrix() * parentMatrix;

            this._renderable?.SetTransform(ref this.worldMatrix);

            for(int i = 0; i < this.children.Count; i++) {
                this.children[i]._UpdateWorldMatrix(ref this._worldMatrix);
            }
        }

        public ref Matrix4x4 GetLocalMatrix()
        {
            if (this._transformIsDirty) {
                this._localMatrix = Matrix4x4.CreateScale(this._scale)
                    * Matrix4x4.CreateFromQuaternion(this._rotation)
                    * Matrix4x4.CreateTranslation(this._position);
                this._transformIsDirty = false;
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

        public Renderable? FindRenderable(string name)
        {
            return this.Find(name)?.renderable;
        }

        public Renderable? FirstRenderable()
        {
            if (this.renderable != null) {
                return this.renderable;
            }
            foreach (var child in this.children) {
                var result = child.FirstRenderable();
                if (result != null) {
                    return result;
                }
            }
            return null;
        }

        public Node3D? Find(string name)
        {
            if (this.name == name) {
                return this;
            }
            foreach (var child in this.children) {
                var result = child.Find(name);
                if (result != null) {
                    return result;
                }
            }
            return null;
        }

        public void PrintHierarchyToConsole(string indent = "")
        {
            System.Console.WriteLine(indent + "- " + this.GetType().Name + ": " + this.name + "(scale: " + this.scale + ")");
            indent += "  ";
            foreach (var child in this.children) {
                child.PrintHierarchyToConsole(indent);
            }
        }

        public void ForEachRecursive(System.Action<Node3D> action)
        {
            action(this);
            foreach (var child in this.children) {
                child.ForEachRecursive(action);
            }
        }

        public Node3D? FindPath(string name)
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
            } else {
                return currentNode;
            }
        }
    }
}