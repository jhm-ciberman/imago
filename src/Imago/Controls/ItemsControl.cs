using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Imago.Rendering.Sprites;

namespace Imago.Controls;

/// <summary>
/// Represents a control that can display a collection of items.
/// </summary>
[ItemsProperty(nameof(Items))]
public abstract class ItemsControl : Control
{
    /// <summary>
    /// Represents a collection of <see cref="Control"/> objects managed by an <see cref="ItemsControl"/>.
    /// </summary>
    public class ItemCollection : Collection<Control>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemCollection"/> class.
        /// </summary>
        /// <param name="owner">The <see cref="ItemsControl"/> that owns this collection.</param>
        public ItemCollection(ItemsControl owner)
        {
            this.Owner = owner;
        }

        /// <summary>
        /// Gets the <see cref="ItemsControl"/> that owns this collection.
        /// </summary>
        public ItemsControl Owner { get; }

        /// <inheritdoc/>
        protected override void InsertItem(int index, Control item)
        {
            base.InsertItem(index, item);
            this.Owner.AddVisualChild(item);
        }

        /// <inheritdoc/>
        protected override void RemoveItem(int index)
        {
            var item = this[index];
            base.RemoveItem(index);
            this.Owner.RemoveVisualChild(item);
        }

        /// <inheritdoc/>
        protected override void SetItem(int index, Control item)
        {
            var oldItem = this[index];
            base.SetItem(index, item);
            this.Owner.RemoveVisualChild(oldItem);
            this.Owner.AddVisualChild(item);
        }

        /// <inheritdoc/>
        protected override void ClearItems()
        {
            foreach (var item in this)
            {
                this.Owner.RemoveVisualChild(item);
            }

            base.ClearItems();
        }

        /// <summary>
        /// Replaces an existing item in the collection with a new item.
        /// </summary>
        /// <param name="oldItem">The item to replace.</param>
        /// <param name="newItem">The new item to insert.</param>
        public void Replace(Control oldItem, Control newItem)
        {
            var index = this.IndexOf(oldItem);
            this[index] = newItem;
        }

        /// <summary>
        /// Adds a range of items to the collection.
        /// </summary>
        /// <param name="items">The items to add.</param>
        public void AddRange(IEnumerable<Control> items)
        {
            foreach (var item in items)
            {
                this.Add(item);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="ItemCollection"/>.
        /// </summary>
        /// <returns>A <see cref="List{T}.Enumerator"/> for the <see cref="ItemCollection"/>.</returns>
        public new List<Control>.Enumerator GetEnumerator()
        {
            // This override is to prevent the use of the base GetEnumerator method which allocates a new enumerator.
            // The List<T>.Enumerator is a struct and does not allocate.
            return ((List<Control>)this.Items).GetEnumerator();
        }
    }

    /// <summary>
    /// Gets the collection of child controls displayed by this <see cref="ItemsControl"/>.
    /// </summary>
    public ItemCollection Items { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemsControl"/> class.
    /// </summary>
    public ItemsControl()
    {
        this.Items = new ItemCollection(this);
    }

    /// <inheritdoc/>
    protected override void DrawCore(DrawingContext ctx)
    {
        base.DrawCore(ctx);

        var children = this.VisualChildren;
        for (var i = 0; i < children.Count; i++)
        {
            children[i].Draw(ctx);
        }
    }

    /// <inheritdoc/>
    public override void Update(float deltaTime)
    {
        for (var i = 0; i < this.Items.Count; i++)
        {
            this.Items[i].Update(deltaTime);
        }

        base.Update(deltaTime);
    }

    private IDataTemplate? _itemItemplate;

    /// <summary>
    /// Gets or sets the data template used to generate a <see cref="Control"/> for each item in the <see cref="ItemsSource"/>.
    /// </summary>
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

    private IEnumerable? _itemsSource;
    /// <summary>
    /// Gets or sets a collection that is used to generate the content of the <see cref="ItemsControl"/>.
    /// </summary>
    public IEnumerable? ItemsSource
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
                this.OnItemsInserted(e.NewStartingIndex, e.NewItems!);
                break;
            case NotifyCollectionChangedAction.Remove:
                this.OnItemsRemovedAt(e.OldStartingIndex, e.OldItems!.Count);
                break;
            case NotifyCollectionChangedAction.Replace:
                this.OnItemsRemovedAt(e.OldStartingIndex, e.OldItems!.Count);
                this.OnItemsInserted(e.NewStartingIndex, e.NewItems!);
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

        foreach (var item in this.Items)
        {
            item.Dispose();
        }

        this.Items.Clear();
    }

    private void OnItemsInserted(int startIndex, IList items)
    {
        for (int i = 0; i < items.Count; i++)
        {
            var control = this.ItemTemplate!.CreateItem(items[i]!);
            this.Items.Insert(startIndex + i, control);
        }
    }

    private void OnItemsRemovedAt(int startIndex, int count)
    {
        for (int i = count - 1; i >= 0; i--)
        {
            var control = this.Items[startIndex + i];
            control.Dispose();
            this.Items.RemoveAt(startIndex + i);
        }
    }

    /// <summary>
    /// Called when the <see cref="ItemsSource"/> property changes. This method resets the current items
    /// and then generates new controls for the items in the new source using the <see cref="ItemTemplate"/>.
    /// </summary>
    protected virtual void OnItemsSourceChanged()
    {
        this.OnItemsReset();

        if (this.ItemsSource is null) return;
        if (this.ItemTemplate is null) return;

        var list = this.ItemsSource is IList listSource ? listSource : this.ItemsSource.Cast<object>().ToList();
        this.OnItemsInserted(0, list);
    }
}
