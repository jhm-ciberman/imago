using System;
using Veldrid;

namespace LifeSim.Rendering
{
    public class Material
    {
        private Pass _pass;
        public Pass pass => this._pass;

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
    }
}