using System.Collections.Generic;
using System.Collections.ObjectModel;
using Imago.Rendering.Sprites;

namespace Imago.Controls;

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

        public void Replace(Control oldItem, Control newItem)
        {
            var index = this.IndexOf(oldItem);
            this[index] = newItem;
        }

        public void AddRange(IEnumerable<Control> items)
        {
            foreach (var item in items)
            {
                this.Add(item);
            }
        }

        public new List<Control>.Enumerator GetEnumerator()
        {
            // This override is to prevent the use of the base GetEnumerator method which allocates a new enumerator.
            // The List<T>.Enumerator is a struct and does not allocate.
            return ((List<Control>)this.Items).GetEnumerator();
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

        for (var i = 0; i < this.Items.Count; i++)
        {
            this.Items[i].Draw(spriteBatcher);
        }
    }

    public override void Update(float deltaTime)
    {
        for (var i = 0; i < this.Items.Count; i++)
        {
            this.Items[i].Update(deltaTime);
        }

        base.Update(deltaTime);
    }

}
