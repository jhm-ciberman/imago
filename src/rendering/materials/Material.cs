using System;
using Veldrid;

namespace LifeSim.Rendering
{
    public class Material
    {
        internal static System.Action<Material>? onRefCountZero;

        private Pass _pass;
        public Pass pass => this._pass;

        private uint _refCount = 0;

        public ResourceSet resourceSet;

        public Material(Pass pass, ResourceSet resourceSet) 
        {
            this._pass = pass;
            this.resourceSet = resourceSet;
        }

        internal void Dispose()
        {
            this.resourceSet.Dispose();
        }

        internal void MarkAsUsed()
        {
            this._refCount++;
        }

        internal void MarkAsUnused()
        {
            this._refCount--;
            if (this._refCount == 0) {
                Material.onRefCountZero?.Invoke(this);
            }
        }
    }
}