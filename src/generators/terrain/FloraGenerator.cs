using System.Collections.Generic;
using LifeSim.Simulation;

namespace LifeSim.Generation
{
    public class FloraGenerator : IWorldGenerationStep
    {
        private readonly System.Random _random;

        private readonly int _seed;

        private readonly float _floraDensity;

        private readonly Container _container; 

        public FloraGenerator(Container container, int seed, float floraDensity)
        {
            this._container = container;
            this._seed = seed;
            this._random = new System.Random(seed);
            this._floraDensity = floraDensity;
        }

        public void Handle(World world) 
        {
            if (this._floraDensity == 0f) return;

            var plantsModels = new PlantModel[] {
                this._container.Get<PlantModel>("plant.bush"),
                this._container.Get<PlantModel>("plant.spruce"),
                this._container.Get<PlantModel>("plant.pine"),
            };

            List<PlantProbabilityLayer> plantProbabilityLayers = new List<PlantProbabilityLayer>();
            foreach (PlantModel plant in plantsModels) {
                plantProbabilityLayers.Add(new PlantProbabilityLayer(this._seed, plant));
            }

            var wr = new WeightedRandom<PlantModel>(this._random);

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
                    wr.Next().Create(tile.baseCell);
                    c++;
                }
            }
        }
    }
}