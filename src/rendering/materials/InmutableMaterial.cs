using Veldrid;

namespace LifeSim.Rendering
{
    public class InmutableMaterial : IMaterial
    {
        private Pass _pass;
        public Pass pass => this._pass;

        private ResourceSet _resourceSet;
        public ResourceSet GetResourceSet() => this._resourceSet;

        public InmutableMaterial(Pass pass, ResourceSet resourceSet) 
        {
            this._pass = pass;
            this._resourceSet = resourceSet;
        }

        public void Dispose()
        {
            this._resourceSet.Dispose();
        }
    }
}