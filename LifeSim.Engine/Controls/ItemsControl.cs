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
            item.Parent = this.Owner;
            item.Root = this.Owner.Root;
        }

        protected override void RemoveItem(int index)
        {
            var item = this[index];
            item.Parent = null;
            item.Root = null;
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, Control item)
        {
            var oldItem = this[index];
            oldItem.Parent = null;
            oldItem.Root = null;
            base.SetItem(index, item);
            item.Parent = this.Owner;
            item.Root = this.Owner.Root;
        }
    }

    public ItemCollection Items { get; }

    public ItemsControl()
    {
        this.Items = new ItemCollection(this);
    }

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

        base.Update(deltaTime);
    }

}