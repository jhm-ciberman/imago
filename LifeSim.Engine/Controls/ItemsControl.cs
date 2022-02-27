using System.Collections.Generic;
using System.Collections.ObjectModel;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.Controls;

public abstract class ItemsControl : Control
{
    public ObservableCollection<Control> Items { get; } = new ObservableCollection<Control>();

    public override IEnumerable<Control> VisualChildren => this.Items;

    protected override void DrawCore(SpriteBatcher spriteBatcher)
    {
        foreach (var child in this.Items)
        {
            child.Draw(spriteBatcher);
        }
    }

    public override void Update(float deltaTime)
    {
        foreach (var child in this.Items)
        {
            child.Update(deltaTime);
        }
    }

}