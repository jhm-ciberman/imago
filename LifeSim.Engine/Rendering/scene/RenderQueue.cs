using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.Rendering;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering
{
    public class RenderQueue : IEnumerable<Renderable>, IReadOnlyList<Renderable>, IReadOnlyCollection<Renderable>
    {
        private const int defaultCapacity = 250;

        private readonly List<RenderIndex> _indices = new List<RenderIndex>(defaultCapacity);
        private readonly List<Renderable> _items = new List<Renderable>(defaultCapacity);

        public int Count => this._indices.Count;

        public Renderable this[int index] => this._items[this._indices[index].index];

        public void Sort()
        {
            this._indices.Sort();
        }

        public void AddToRenderQueue(SceneStorage sceneStorage, ref BoundingFrustum frustum, Vector3 cameraPosition, bool useShadowmapShader)
        {
            this._indices.Clear();
            this._items.Clear();
            var renderables = sceneStorage.renderables;
            for (int i = 0; i < renderables.Count; i++) {
                Renderable renderable = renderables[i];

                if (renderable.Cull(ref frustum)) {
                    ulong key = renderable.GetSortKey(cameraPosition);
                    var material = renderable.material;
                    this._indices.Add(new RenderIndex(key, this._items.Count));
                    this._items.Add(renderable);

                    /*
                        mesh: renderable.mesh,
                        shader: useShadowmapShader ? material.shadowmapShader : material.shader,
                        pickingID: renderable.pickingID,
                        transformBufferOffset: renderable.transformDataBlock.offset,
                        instanceBufferOffset: renderable.instanceDataBlock.offset,
                        transformResourceSet: renderable.transformResourceSet,
                        materialResourceSet: material.GetMaterialResourceSet(),
                        instanceResourceSet: renderable.instanceResourceSet
                    */
                }
            }
        }

        public IEnumerator<Renderable> GetEnumerator()
        {
            return new Enumerator(this._indices, this._items);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this._indices, this._items);
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
                this._current = null;
            }

            public Renderable Current => this._current ?? throw new Exception("Cannot call current");
            object? IEnumerator.Current => this.Current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (this._nextItemIndex >= this._indices.Count) {
                    this._current = default(Renderable);
                    return false;
                } else {
                    var currentIndex = this._indices[this._nextItemIndex].index;
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

        public readonly struct RenderIndex : IComparable<RenderIndex>
        {
            public readonly ulong key;
            public readonly int index;

            public RenderIndex(ulong key, int index)
            {
                this.key = key;
                this.index = index;
            }

            public override string ToString()
            {
                return Convert.ToString((long) key, 16).PadLeft(16, '0');
            }

            int IComparable<RenderIndex>.CompareTo(RenderIndex other)
            {
                return (this.key < other.key) ? - 1 : 1;
            }
        }
    }
}