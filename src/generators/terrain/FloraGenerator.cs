using System.Collections.Generic;
using LifeSim.Simulation;

namespace LifeSim.Generation
{
    public class FloraGenerator : IWorldGenerationStep
    {
        private System.Random _random;

        private int _seed;

        private float _floraDensity;

        private IContainer _container; 

        public FloraGenerator(IContainer container, int seed, float floraDensity)
        {
            this._container = container;
            this._seed = seed;
            this._random = new System.Random(seed);
            this._floraDensity = floraDensity;
        }

        public void Handle(World world) 
        {
            if (this._floraDensity == 0f) return;

            var plantsModels = new Plant.Model[] {
                this._container.Get<Plant.Model>("plant.bush"),
                this._container.Get<Plant.Model>("plant.spruce"),
                this._container.Get<Plant.Model>("plant.pine"),
            };

            List<PlantProbabilityLayer> plantProbabilityLayers = new List<PlantProbabilityLayer>();
            foreach (Plant.Model plant in plantsModels) {
                plantProbabilityLayers.Add(new PlantProbabilityLayer(this._seed, plant));
            }

            var wr = new WeightedRandom<Plant.Model>(this._random);

            int c = 0;
            foreach (Tile tile in world.tiles)
            {
                //if (! tile.isBuildable) continue;

                double p = (tile.fertility * this._random.NextDouble());
                if (p < 1 - this._floraDensity) continue; 

                
                wr.Clear();
                foreach (var layer in plantProbabilityLayers) {
                    wr.Add(layer.model, layer.GetProbability(tile));
                }

                if (wr.hasNext) {
                    wr.Next().PlantIn(tile);
                    c++;
                }
            }
        }
    }
}