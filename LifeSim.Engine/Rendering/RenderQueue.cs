using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.SceneGraph;
using Veldrid;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering
{
    public class RenderQueue : IEnumerable<RenderNode3D>, IComparer<RenderQueue.RenderItem>
    {
        private const int defaultCapacity = 250;

        private struct RenderItem
        {
            public ulong key;
            public RenderNode3D renderable;

            public override string ToString()
            {
                return Convert.ToString((long) key, 16).PadLeft(16, '0');
            }
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
            if (Input.GetKey(Key.Space)) {
                this._renderList.Sort(this);
            }
        }

        private void _AddToRenderQueue(RenderNode3D renderable, ref BoundingFrustum frustum, Vector3 cameraPosition)
        {
            if (renderable.material == null || renderable.mesh == null) return;

            if (renderable.Cull(ref frustum)) {

                float cameraDistance = Vector3.DistanceSquared(renderable.worldSpaceCenter, cameraPosition);

                ulong distanceHash = (ulong) Math.Min(uint.MaxValue, (cameraDistance * 1000f)) & 0xFFFFFFFF;
                ulong materialHash = (ulong) (renderable.material.GetHashCode() & 0xFFFF);
                ulong meshHash     = (ulong) (renderable.mesh.GetHashCode() & 0xFFFF); 
                
                ulong key = (materialHash << 48) | (meshHash << 32) | (distanceHash << 0);

                this._renderList.Add(new RenderItem() {
                    key = key,
                    renderable = renderable
                });
            }
        }

        public IEnumerator<RenderNode3D> GetEnumerator()
        {
            return new Enumerator(this._renderList);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this._renderList);
        }

        int IComparer<RenderItem>.Compare(RenderItem x, RenderItem y)
        {
            return (x.key > y.key) ? - 1 : 1;
        }

        private struct Enumerator : IEnumerator<RenderNode3D>
        {
            private readonly List<RenderItem> _renderItems;
            private int _nextItemIndex;
            private RenderNode3D? _current;

            public Enumerator(List<RenderItem> renderItems)
            {
                this._renderItems = renderItems;
                this._nextItemIndex = 0;
                this._current = null;
            }

            public RenderNode3D Current => this._current ?? throw new System.Exception("Current should not be used in the current state");
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