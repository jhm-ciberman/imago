using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering;

public class RenderQueue : IEnumerable<Renderable>, IReadOnlyList<Renderable>, IReadOnlyCollection<Renderable>
{
    private const int DEFAULT_CAPACITY = 250;

    private readonly List<Renderable> _allRenderables = new List<Renderable>();
    private readonly Dictionary<Renderable, int> _renderableToIndex = new Dictionary<Renderable, int>();
    private readonly List<RenderIndex> _culledIndices = new List<RenderIndex>(DEFAULT_CAPACITY);
    private readonly List<Renderable> _culledItems = new List<Renderable>(DEFAULT_CAPACITY);

    public int Count => this._culledIndices.Count;

    public Renderable this[int index] => this._culledItems[this._culledIndices[index].Index];

    public RenderQueueFlags FilterFlags { get; set; }

    public RenderQueue(RenderQueueFlags filterFlags)
    {
        this.FilterFlags = filterFlags;

        Renderable.OnRenderQueueFlagsChanged += this.OnRenderQueueFlagsChanged;
    }

    private void OnRenderQueueFlagsChanged(Renderable renderable, RenderQueueFlags oldFlags, RenderQueueFlags newFlags)
    {
        // This event could be called from a different thread. The "_allRenderables" list should be locked.

        if (newFlags.HasFlag(this.FilterFlags))
        {
            lock (this._allRenderables)
            {
                this._renderableToIndex.Add(renderable, this._allRenderables.Count);
                this._allRenderables.Add(renderable);
            }
        }
        else if (oldFlags.HasFlag(this.FilterFlags))
        {
            lock (this._allRenderables)
            {
                if (this._renderableToIndex.TryGetValue(renderable, out int index))
                {
                    // Swap pop algorithm to remove in O(1) time.
                    var lastIndex = this._allRenderables.Count - 1;
                    var lastRenderable = this._allRenderables[lastIndex];
                    this._allRenderables[index] = lastRenderable;
                    this._renderableToIndex[lastRenderable] = index;
                    this._allRenderables.RemoveAt(lastIndex);
                    this._renderableToIndex.Remove(renderable);
                }
            }
        }
    }

    public void Sort()
    {
        this._culledIndices.Sort();
    }

    public void AddToRenderQueue(BoundingFrustum cameraFrustum, Vector3 cameraPosition)
    {
        lock (this._allRenderables)
        {
            this._culledIndices.Clear();
            this._culledItems.Clear();
            var renderables = this._allRenderables;
            for (int i = 0; i < renderables.Count; i++)
            {
                Renderable renderable = renderables[i];

                if (!renderable.RenderQueueFlags.HasFlag(this.FilterFlags)) continue;

                if (cameraFrustum.Contains(renderable.BoundingBox) != ContainmentType.Disjoint)
                {
                    ulong key = renderable.GetSortKey(cameraPosition);
                    this._culledIndices.Add(new RenderIndex(key, this._culledItems.Count));
                    this._culledItems.Add(renderable);
                }
            }
        }
    }

    public IEnumerator<Renderable> GetEnumerator()
    {
        return new Enumerator(this._culledIndices, this._culledItems);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new Enumerator(this._culledIndices, this._culledItems);
    }

    private struct Enumerator : IEnumerator<Renderable>
    {
        private readonly List<RenderIndex> _indices;
        private readonly List<Renderable> _items;
        private int _nextItemIndex;
        private Renderable? _current;

        public Enumerator(List<RenderIndex> indices, List<Renderable> items)
        {
            this._indices = indices;
            this._items = items;
            this._nextItemIndex = 0;
            this._current = default(Renderable);
        }

        public Renderable Current => this._current ?? throw new Exception();
        object? IEnumerator.Current => this.Current;

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if (this._nextItemIndex >= this._indices.Count)
            {
                this._current = default(Renderable);
                return false;
            }
            else
            {
                var currentIndex = this._indices[this._nextItemIndex].Index;
                this._current = this._items[currentIndex];
                this._nextItemIndex += 1;
                return true;
            }
        }

        public void Reset()
        {
            this._nextItemIndex = 0;
        }
    }

    private readonly struct RenderIndex : IComparable<RenderIndex>
    {
        public readonly ulong Key;
        public readonly int Index;

        public RenderIndex(ulong key, int index)
        {
            this.Key = key;
            this.Index = index;
        }

        public override string ToString()
        {
            return Convert.ToString((long)this.Key, 16).PadLeft(16, '0');
        }

        int IComparable<RenderIndex>.CompareTo(RenderIndex other)
        {
            return (this.Key < other.Key) ? -1 : (this.Key == other.Key) ? 0 : 1;
        }
    }
}