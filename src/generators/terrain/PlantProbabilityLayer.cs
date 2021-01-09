using LifeSim.Simulation;

namespace LifeSim.Generation
{
    public class PlantProbabilityLayer
    {
        private Plant.Model _model;
        public Plant.Model model => this._model;

        private FastNoiseLite _noise;

        public PlantProbabilityLayer(int seed, Plant.Model model)
        {
            this._model = model;

            // We add a deterministic value to the noise seed to prevent getting the same noise values as the provided seed
            // We need to use a different seed for each plant, so we get the hash code of each plant ID, since it is unique 
            // for each plant. 
            seed += model.name.GetHashCode(); 
            this._noise = new FastNoiseLite(seed); 
            this._noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        }

        public float GetProbability(Tile tile)
        {
            var coords = tile.coords;
            if (! this._model.CanBePlantedIn(tile.world, coords)) return 0f;

            return (this._noise.GetNoise(coords.x, coords.y) + 1f) / 2f;
        }
    }
}