using LifeSim.Simulation;

namespace LifeSim.Generation
{
    public class PlantProbabilityLayer
    {
        public PlantModel model { get; }

        private readonly FastNoiseLite _noise;

        public PlantProbabilityLayer(int seed, PlantModel model)
        {
            this.model = model;

            // We add a deterministic value to the noise seed to prevent getting the same noise values as the provided seed
            // We need to use a different seed for each plant, so we get the hash code of each plant ID, since it is unique 
            // for each plant. 
            seed += model.name.GetHashCode(); 
            this._noise = new FastNoiseLite(seed); 
            this._noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        }

        public float GetProbability(Tile tile)
        {
            var cell = tile.baseCell;
            if (! this.model.CanBePlacedIn(cell)) return 0f;

            var coords = cell.coords;
            return (this._noise.GetNoise(coords.x, coords.y) + 1f) / 2f;
        }
    }
}