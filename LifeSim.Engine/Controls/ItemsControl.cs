using System.Collections.Generic;
using System.Collections.ObjectModel;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.Controls;

public abstract class ItemsControl : Control
{
    public class ItemCollection : Collection<Control>
    {
        public ItemCollection(ItemsControl owner)
        {
            this.Owner = owner;
        }

        public ItemsControl Owner { get; }

        protected override void InsertItem(int index, Control item)
        {
            base.InsertItem(index, item);
            this.Owner.AddVisualChild(item);
        }

        protected override void RemoveItem(int index)
        {
            var item = this[index];
            base.RemoveItem(index);
            this.Owner.RemoveVisualChild(item);
        }

        protected override void SetItem(int index, Control item)
        {
            var oldItem = this[index];
            base.SetItem(index, item);
            this.Owner.RemoveVisualChild(oldItem);
            this.Owner.AddVisualChild(item);
        }
    }

    public ItemCollection Items { get; }

    public ItemsControl()
    {
        this.Items = new ItemCollection(this);
    }

    protected override void DrawCore(SpriteBatcher spriteBatcher)
    {
        base.DrawCore(spriteBatcher);

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

        base.Update(deltaTime);
    }

}