using System.Collections.ObjectModel;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.Controls;

public abstract class ItemsControl : Control
{
    public ObservableCollection<Control> Items { get; } = new ObservableCollection<Control>();

    protected override void DrawCore(SpriteBatcher spriteBatcher)
    {
        foreach (var child in this.Items)
        {
            child.Draw(spriteBatcher);
        }
    }

}