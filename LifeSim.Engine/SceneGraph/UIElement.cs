using System.Collections.Generic;
using System.Numerics;

namespace LifeSim.Engine.SceneGraph;

public abstract class UIElement
{
    public string Name { get; set; } = string.Empty;

    public UIElement()
    {
        //
    }

    public UIElement(string name)
    {
        this.Name = name;
    }

    private readonly List<UIElement> _children = new List<UIElement>();
    public IReadOnlyList<UIElement> Children => this._children;

    public Vector2 Position { get; protected set; } = Vector2.Zero;

    public bool Visible { get; protected set; } = true;

    public Vector2 DesiredSize { get; protected set; } = Vector2.Zero;

    public abstract Vector2 Measure(Vector2 availableSize);

    public abstract void Arrange(Rectangle finalRect);
}