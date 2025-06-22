using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Imago.Utilities;

namespace LifeSim.Imago.Rendering;

[Flags]
internal enum RenderQueues : byte
{
    None = 0,
    Opaque = 1 << 0,
    Transparent = 1 << 1,
    OpaqueOrTransparent = Opaque | Transparent,
    ShadowCaster = 1 << 2,
    Picking = 1 << 3,
    All = Opaque | Transparent | ShadowCaster | Picking,
}

/// <summary>
/// Represents a queue of renderable objects, sorted by their distance from the camera.
/// This class is used to efficiently manage and sort the rendering of objects in a scene.
/// The objects are sorted based on their distance from the camera, and can be filtered based on their render flags.
/// </summary>
internal class RenderQueue : IEnumerable<Renderable>, IReadOnlyList<Renderable>, IReadOnlyCollection<Renderable>
{
    private static readonly Comparison<RenderIndex> _frontToBack = (x, y) => x.Key.CompareTo(y.Key);
    private static readonly Comparison<RenderIndex> _backToFront = (x, y) => y.Key.CompareTo(x.Key);

    private readonly List<Renderable> _allRenderables = new List<Renderable>();
    private readonly Dictionary<Renderable, int> _renderableToIndex = new Dictionary<Renderable, int>();
    private readonly List<RenderIndex> _culledIndices = new List<RenderIndex>();
    private readonly List<Renderable> _culledItems = new List<Renderable>();
    private readonly Comparison<RenderIndex> _comparer;

    /// <summary>
    /// Gets the number of <see cref="Renderable"/> objects in this <see cref="RenderQueue"/>.
    /// </summary>
    public int Count => this._culledIndices.Count;

    /// <summary>
    /// Gets the <see cref="Renderable"/> at the specified index.
    /// </summary>
    /// <param name="index">The index of the <see cref="Renderable"/> to get.</param>
    /// <returns>The <see cref="Renderable"/> at the specified index.</returns>
    public Renderable this[int index] => this._culledItems[this._culledIndices[index].Index];

    /// <summary>
    /// Gets or sets the <see cref="RenderQueues"/> that this <see cref="RenderQueue"/> will filter.
    /// </summary>
    public RenderQueues FilterFlags { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderQueue"/> class.
    /// </summary>
    /// <param name="filterFlags">The <see cref="RenderQueues"/> that this <see cref="RenderQueue"/> will filter.</param>
    /// <exception cref="ArgumentException">Thrown when an invalid <see cref="RenderQueues"/> value is specified.</exception>
    public RenderQueue(RenderQueues filterFlags)
    {
        this.FilterFlags = filterFlags;

        this._comparer = filterFlags switch
        {
            RenderQueues.Opaque or RenderQueues.ShadowCaster or RenderQueues.Picking => _frontToBack,
            RenderQueues.Transparent => _backToFront,
            _ => throw new ArgumentException("Invalid filter flags."),
        };
    }


    /// <summary>
    /// Updates the render flags of a given renderable and adds or removes it from the render queue accordingly.
    /// </summary>
    /// <param name="renderable">The renderable to update.</param>
    /// <param name="oldFlags">The old render flags of the renderable.</param>
    /// <param name="newFlags">The new render flags of the renderable.</param>
    /// <remarks>
    /// This method should be called when the renderable render flags change.
    /// </remarks>
    public void UpdateRenderableRenderFlags(Renderable renderable, RenderQueues oldFlags, RenderQueues newFlags)
    {
        if ((newFlags & this.FilterFlags) == (oldFlags & this.FilterFlags))
            return;

        if (newFlags.HasFlag(this.FilterFlags))
        {
            this._renderableToIndex.Add(renderable, this._allRenderables.Count);
            this._allRenderables.Add(renderable);
        }
        else if (oldFlags.HasFlag(this.FilterFlags))
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

    /// <summary>
    /// Updates the render queue by culling renderables outside of the camera frustum and sorting the remaining items by their sort key.
    /// </summary>
    /// <param name="cameraFrustum">The bounding frustum of the camera.</param>
    /// <param name="cameraPosition">The position of the camera.</param>
    public void Update(BoundingFrustum cameraFrustum, Vector3 cameraPosition)
    {
        this._culledIndices.Clear();
        this._culledItems.Clear();
        var renderables = this._allRenderables;
        for (int i = 0; i < renderables.Count; i++)
        {
            Renderable renderable = renderables[i];

            if (!renderable.RenderQueues.HasFlag(this.FilterFlags)) continue;

            if (cameraFrustum.Contains(renderable.BoundingBox) != ContainmentType.Disjoint)
            {
                ulong key = renderable.GetSortKey(cameraPosition);
                this._culledIndices.Add(new RenderIndex(key, this._culledItems.Count));
                this._culledItems.Add(renderable);
            }
        }

        this._culledIndices.Sort(this._comparer);
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

    private readonly struct RenderIndex
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
    }


}
