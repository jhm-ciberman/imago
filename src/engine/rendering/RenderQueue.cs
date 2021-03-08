using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using LifeSim.Engine.SceneGraph;
using Veldrid;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering
{
    public class RenderQueue : IEnumerable<Renderable3D>, IComparer<RenderQueue.RenderItem>
    {
        private const int defaultCapacity = 250;

        private struct RenderItem
        {
            public ulong key;
            public Renderable3D renderable;
        }

        private readonly List<RenderItem> _renderList = new List<RenderItem>(defaultCapacity);

        public int count => this._renderList.Count;

        public void Update(Scene3D scene, Camera3D camera)
        {
            this._renderList.Clear();
            var frustum = camera.occlusionFrustum;
            foreach (var renderable in scene.renderables) {
                this._AddToRenderQueue(renderable, ref frustum, camera.position);
            }
        }

        public void Update(Scene3D scene, DirectionalLight light, Camera3D camera)
        {
            this._renderList.Clear();
            var matrix = light.GetShadowMapMatrix(camera.position);
            var frustum = new BoundingFrustum(matrix);
            foreach (var renderable in scene.renderables) {
                this._AddToRenderQueue(renderable, ref frustum, camera.position);
            }
        }

        public void Sort()
        {
            this._renderList.Sort(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void _AddToRenderQueue(Renderable3D renderable, ref BoundingFrustum frustum, Vector3 cameraPosition)
        {
            if (renderable.material == null || renderable.mesh == null) return;

            if (renderable.Cull(ref frustum)) {

                float cameraDistance = Vector3.DistanceSquared(renderable.worldSpaceCenter, cameraPosition);
                ulong cameraDistanceInt = (ulong) Math.Min(uint.MaxValue, (cameraDistance * 1000f));
                ulong materialHash = (ulong) renderable.material.GetHashCode(); 
                ulong key = (materialHash << 32) | cameraDistanceInt;
                //System.Console.WriteLine(Convert.ToString((long) key, 2));
                this._renderList.Add(new RenderItem() {
                    key = key,
                    renderable = renderable
                });
            }
        }

        public IEnumerator<Renderable3D> GetEnumerator()
        {
            return new Enumerator(this._renderList);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this._renderList);
        }

        int IComparer<RenderItem>.Compare(RenderItem x, RenderItem y)
        {
            return (int) (x.key - y.key);
        }

        private struct Enumerator : IEnumerator<Renderable3D>
        {
            private readonly List<RenderItem> _renderItems;
            private int _nextItemIndex;
            private Renderable3D? _current;

            public Enumerator(List<RenderItem> renderItems)
            {
                this._renderItems = renderItems;
                this._nextItemIndex = 0;
                this._current = null;
            }

            public Renderable3D Current => this._current ?? throw new System.Exception("Current should not be used in the current state");
            object? IEnumerator.Current => this.Current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (this._nextItemIndex >= this._renderItems.Count) {
                    this._current = null;
                    return false;
                } else {
                    this._current = this._renderItems[this._nextItemIndex].renderable;
                    this._nextItemIndex += 1;
                    return true;
                }
            }

            public void Reset()
            {
                this._nextItemIndex = 0;
            }
        }
    }
}