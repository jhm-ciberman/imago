using System;
using System.Collections.Generic;
using LifeSim.Editor;
using LifeSim.Engine.Rendering;
using LifeSim.Simulation;
using LifeSim.View3D;

namespace LifeSim.Assets
{
    public class AssetsContainer
    {
        private Dictionary<string, IAsset> _resources = new Dictionary<string, IAsset>();

        public void Register(string id, IAsset asset)
        {
            this._resources.Add(id, asset);
        }

        internal IEnumerable<string> assetsIDs => this._resources.Keys;

        public T Get<T>(string id) where T : IAsset
        {
            var asset = this._resources[id];
            if (asset is T assetT) {
                return assetT;
            }
            throw new Exception("The asset \"" + id + "\" is not of type " + typeof(T).Name);
        }

        public IAsset Get(string id)
        {
            return this._resources[id];
        }

        public ApertureAsset GetApertureAsset(Aperture aperture)
        {
            return this.Get<ApertureAsset>("asset:" + aperture.id);
        }

        public PlantAsset GetPlantAsset(Plant plant)
        {
            return this.Get<PlantAsset>("asset:" + plant.id);
        }

        public ObjectAsset GetObjectAsset(Simulation.Object obj)
        {
            return this.Get<ObjectAsset>("asset:" + obj.id);
        }

    }
}