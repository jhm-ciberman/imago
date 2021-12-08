using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.SceneGraph
{
    public class Node3D : IDisposable
    {
        public string Name { get; set; } = string.Empty;
        public Node3D? Parent { get; private set; } = null;

        private readonly SwapPopList<Node3D> _children = new SwapPopList<Node3D>();
        public IReadOnlyList<Node3D> Children => this._children;

        private Vector3    _position = Vector3.Zero;
        private Quaternion _rotation = Quaternion.Identity;
        private Vector3    _scale = Vector3.One;

        public Vector3 Position
        {
            get => this._position;
            set
            {
                if (this._position != value)
                {
                    this._position = value;
                    this._NotifyTransformDirty();
                }
            }
        }

        public Quaternion Rotation
        {
            get => this._rotation;
            set
            {
                if (this._rotation != value)
                {
                    this._rotation = value;
                    this._NotifyTransformDirty();
                }
            }
        }

        public Vector3 Scale
        {
            get => this._scale;
            set
            {
                if (this._scale != value)
                {
                    this._scale = value;
                    this._NotifyTransformDirty();
                }
            }
        }

        public Scene? Scene { get; protected set; } = null;

        private Matrix4x4 _localMatrix = Matrix4x4.Identity;

        protected Matrix4x4 _worldMatrix = Matrix4x4.Identity;
        public ref Matrix4x4 WorldMatrix => ref this._worldMatrix;

        protected bool _transformIsDirty = true;
        public bool TransformIsDirty => this._transformIsDirty;

        public Vector3 WorldPosition => Vector3.Transform(Vector3.Zero, this._worldMatrix);
        public Vector3 WorldScale => Vector3.Transform(this._scale, this._worldMatrix);

        public Node3D()
        {
            //
        }

        public void Add(Node3D node)
        {
            if (node.Parent != this && node != this)
            {
                if (node.Parent != null)
                {
                    node.Parent.Remove(node);
                }
                this._children.Add(node);
                node.Parent = this;
                node.Scene = this.Scene;
                this.Scene?.NotifyNodeAdded(node);
            }
        }

        public void Remove(Node3D node)
        {
            if (node.Parent != this) return;

            this._children.Remove(node);
            node.Parent = null;
            node.Scene = null;
            this.Scene?.NotifyNodeRemoved(node);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void _NotifyTransformDirty()
        {
            if (this._transformIsDirty) return;
            this._transformIsDirty = true;

            this.Scene?.NotifyTransformDirty(this);
        }

        public ref Matrix4x4 GetLocalMatrix()
        {
            if (this._transformIsDirty)
            {
                this._localMatrix = Matrix4x4.CreateScale(this._scale)
                    * Matrix4x4.CreateFromQuaternion(this._rotation)
                    * Matrix4x4.CreateTranslation(this._position);
                this._transformIsDirty = false;
            }
            return ref this._localMatrix;
        }

        public virtual void UpdateWorldMatrix(ref Matrix4x4 parentMatrix)
        {
            this._worldMatrix = this.GetLocalMatrix() * parentMatrix;

            for (int i = 0; i < this.Children.Count; i++)
            {
                this.Children[i].UpdateWorldMatrix(ref this._worldMatrix);
            }
        }

        public T? Find<T>() where T : Node3D
        {
            if (this is T childT)
            {
                return childT;
            }
            foreach (var child in this.Children)
            {
                var result = child.Find<T>();
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        public virtual Renderable? FirstRenderable()
        {
            foreach (var child in this.Children)
            {
                var result = child.FirstRenderable();
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        public Node3D? Find(string name)
        {
            if (this.Name == name)
            {
                return this;
            }
            foreach (var child in this.Children)
            {
                var result = child.Find(name);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        public virtual void PrintHierarchyToConsole(string indent = "")
        {
            Console.WriteLine(indent + "- " + this.GetType().Name + ": " + this.Name + "(scale: " + this.Scale + ")");
            indent += "  ";
            foreach (var child in this.Children)
            {
                child.PrintHierarchyToConsole(indent);
            }
        }

        public void ForEachRecursive(System.Action<Node3D> action)
        {
            action(this);
            foreach (var child in this.Children)
            {
                child.ForEachRecursive(action);
            }
        }

        public Node3D? FindPath(string name)
        {
            var arrayPaths = name.Split('/');
            int currentIndex = 0;
            Node3D currentNode = this;
            bool found = true;
            while (found && currentIndex < arrayPaths.Length)
            {
                var currentNameToFind = arrayPaths[currentIndex];
                foreach (var child in currentNode.Children)
                {
                    if (child.Name == currentNameToFind)
                    {
                        currentNode = child;
                        currentIndex++;
                        found = true;
                        break;
                    }
                }
            }
            return currentIndex < arrayPaths.Length ? null : currentNode;
        }

        public virtual void Dispose()
        {
            foreach (var child in this.Children)
            {
                child.Dispose();
            }
        }
    }
}