using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Imago.Graphics.Sprites;

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

    private IItemSource? _itemsSource;
    private Dictionary<object, Control>? _itemControls;


    public IItemSource? ItemsSource
    {
        get => this._itemsSource;
        set
        {
            if (this._itemsSource == value) return;

            if (this._itemsSource?.Items is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= this.ItemsSource_CollectionChanged;
            }

            this._itemsSource = value;

            if (this._itemsSource?.Items is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += this.ItemsSource_CollectionChanged;
            }

            this.InvalidateMeasure();
            this.OnItemsSourceChanged();
        }
    }

    private void ItemsSource_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                this.OnItemsAdded(e.NewItems!);
                break;
            case NotifyCollectionChangedAction.Remove:
                this.OnItemsRemoved(e.OldItems!);
                break;
            case NotifyCollectionChangedAction.Replace:
                this.OnItemsRemoved(e.OldItems!);
                this.OnItemsAdded(e.NewItems!);
                break;
            case NotifyCollectionChangedAction.Move:
                break;
            case NotifyCollectionChangedAction.Reset:
                this.OnItemsReset();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnItemsReset()
    {
        this.Items.Clear();
        this._itemControls?.Clear();
    }

    private void OnItemsAdded(IList items)
    {
        this._itemControls ??= new();

        foreach (var item in items)
        {
            var control = this.ItemsSource!.CreateItem(item);
            this.Items.Add(control);
            this._itemControls.Add(item, control);
        }
    }

    private void OnItemsRemoved(IList items)
    {
        foreach (var item in items)
        {
            var control = this._itemControls![item];
            this.Items.Remove(control);
            this._itemControls.Remove(item);
        }
    }

    protected virtual void OnItemsSourceChanged()
    {
        this.OnItemsReset();

        if (this.ItemsSource is not null)
        {
            var list = this.ItemsSource.Items.Cast<object>().ToList();
            this.OnItemsAdded(list);
        }
    }
}

public class ItemsSource<T> : IItemSource
{
    public ItemsSource(IEnumerable<T> items, Func<T, Control> itemTemplate)
    {
        this.Items = items;
        this.ItemTemplate = itemTemplate;
    }

    public IEnumerable<T> Items { get; }
    public Func<T, Control> ItemTemplate { get; }

    IEnumerable IItemSource.Items => this.Items;

    Control IItemSource.CreateItem(object item)
    {
        return this.ItemTemplate((T)item);
    }
}

public interface IItemSource
{
    IEnumerable Items { get; }
    Control CreateItem(object item);
}
