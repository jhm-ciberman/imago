using System;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine
{
    public abstract class Stage
    {
        private ILayer[] _layers = Array.Empty<ILayer>();

        public void SetLayers(ILayer[] layers)
        {
            this._layers = layers;
        }

        public ILayer[] GetLayers()
        {
            return this._layers;
        }

        public abstract void Update(float deltaTime);
    }
}