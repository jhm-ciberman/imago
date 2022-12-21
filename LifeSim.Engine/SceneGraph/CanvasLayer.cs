using System;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.Rendering;
using LifeSim.Utils;

namespace LifeSim.Engine.SceneGraph;

public class CanvasLayer
{
    public Viewport Viewport { get; }
    private readonly SwapPopList<ICanvasItem> _items = new SwapPopList<ICanvasItem>();

    public IReadOnlyList<ICanvasItem> Items => this._items;

    private readonly Node2D _root = new Node2D();

    public Matrix4x4 ViewProjectionMatrix => Matrix4x4.CreateOrthographicOffCenter(0, this.Viewport.Width, this.Viewport.Height, 0, -10f, 100f);

    public CanvasLayer(Viewport viewport)
    {
        this.Viewport = viewport;
    }

    public void Add(Node2D node)
    {
        this._root.Add(node);
        this.AddNodeToRecursive(node);
    }

    public void Remove(Node2D node)
    {
        this._root.Remove(node);
        this.RemoveNodeRecursive(node);
    }

    public void AddCanvasItem(ICanvasItem canvasItem)
    {
        this._items.Add(canvasItem);
    }

    public void RemoveCanvasItem(ICanvasItem canvasItem)
    {
        this._items.Remove(canvasItem);
    }

    internal void AddNodeToRecursive(Node2D node)
    {
        if (node is ICanvasItem canvasItem)
        {
            this.AddCanvasItem(canvasItem);
        }
        for (int i = 0; i < node.Children.Count; i++)
        {
            this.AddNodeToRecursive(node.Children[i]);
        }
    }

    internal void RemoveNodeRecursive(Node2D node)
    {
        if (node is ICanvasItem canvasItem)
        {
            this.RemoveCanvasItem(canvasItem);
        }
        for (int i = 0; i < node.Children.Count; i++)
        {
            this.RemoveNodeRecursive(node.Children[i]);
        }
    }

    public void UpdateTransforms()
    {
        foreach (var child in this._root.Children)
        {
            child.UpdateWorldMatrix();
        }
    }
}