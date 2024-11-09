using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using LifeSim.Imago.Rendering.Sprites;

namespace LifeSim.Imago.Controls;

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

    protected override void DrawCore(DrawingContext ctx)
    {
        base.DrawCore(ctx);

        for (var i = 0; i < this.Items.Count; i++)
        {
            this.Items[i].Draw(ctx);
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

    private Dictionary<object, Control>? _itemControls;
    private IDataTemplate? _itemItemplate;

    public IDataTemplate? ItemTemplate
    {
        get => this._itemItemplate;
        set
        {
            if (this._itemItemplate == value) return;

            this._itemItemplate = value;
            this.InvalidateMeasure();
            this.OnItemsSourceChanged();
        }
    }

    private IEnumerable<object>? _itemsSource;
    public IEnumerable<object>? ItemsSource
    {
        get => this._itemsSource;
        set
        {
            if (this._itemsSource == value) return;

            if (this._itemsSource is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= this.ItemsSource_CollectionChanged;
            }

            this._itemsSource = value;

            if (this._itemsSource is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += this.ItemsSource_CollectionChanged;
            }

            this.InvalidateMeasure();
            this.OnItemsSourceChanged();
        }
    }

    private void ItemsSource_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (this.ItemTemplate is null) return;

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
                throw new InvalidOperationException();
        }
    }

    private void OnItemsReset()
    {
        if (this.Items.Count == 0) return;

        foreach (var disposable in this.Items.Cast<IDisposable>())
        {
            disposable.Dispose();
        }

        this.Items.Clear();
        this._itemControls?.Clear();
    }

    private void OnItemsAdded(IList items)
    {
        this._itemControls ??= new();

        foreach (var item in items)
        {
            var control = this.ItemTemplate!.CreateItem(item);
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

        if (this.ItemsSource is null) return;
        if (this.ItemTemplate is null) return;

        var list = this.ItemsSource is IList listSource ? listSource : this.ItemsSource.ToList();
        this.OnItemsAdded(list);
    }
}
