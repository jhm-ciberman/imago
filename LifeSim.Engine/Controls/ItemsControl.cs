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
            if (this.Owner.Root != null)
            {
                item.OnAddedToVisualTree(this.Owner.Root);
            }
            this.Owner.InvalidateMeasure();
        }

        protected override void RemoveItem(int index)
        {
            var item = this[index];
            item.Parent = null;
            if (this.Owner.Root != null)
            {
                item.OnRemovedFromVisualTree(this.Owner.Root);
            }
            this.Owner.InvalidateMeasure();
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, Control item)
        {
            var oldItem = this[index];
            oldItem.Parent = null;
            if (this.Owner.Root != null)
            {
                oldItem.OnRemovedFromVisualTree(this.Owner.Root);
            }
            base.SetItem(index, item);
            item.Parent = this.Owner;
            if (this.Owner.Root != null)
            {
                item.OnAddedToVisualTree(this.Owner.Root);
            }
            this.Owner.InvalidateMeasure();
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