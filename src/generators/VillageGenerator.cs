using System.Collections.Generic;
using LifeSim.Simulation;

namespace LifeSim.Generation
{
    public class VillageGenerator : IWorldGenerationStep
    {
        private readonly FastNoiseLite _noise;

        private readonly System.Random _random;

        private readonly HouseGenerator _houseGenerator;

        private readonly Container _container;

        public VillageGenerator(Container container, int seed)
        {
            this._container = container;
            this._random = new System.Random(seed);

            this._houseGenerator = new HouseGenerator(container, this._random);

            this._noise = new FastNoiseLite(seed + 2);
            this._noise.SetFrequency(1f / 50f);
        }

        public void Handle(World world)
        {

            FlatSurfaceFinder flatSurfaceFinder = new FlatSurfaceFinder(world);
            
            HouseLotFinder houseLotFinder = new HouseLotFinder();

            List<HouseLot> houseLots = houseLotFinder.Find(flatSurfaceFinder.surfaces);



            foreach (HouseLot houseLot in houseLots) 
            {
                Vector2Int center = houseLot.center;
                float dp = world.GetTileAt(houseLot.center).distanceToWater / 600;
                float sp = (this._noise.GetNoise(center.x, center.y) + 1f) / 2f;
                float p = dp * 0.8f + sp * 0.1f;

                if (this._random.NextDouble() < p)
                {
                    House house = this._houseGenerator.Generate(houseLot);
                    house.AttachToWorld();

                    var bed = this._container.Get<ObjectModel>("furniture.beds.bed").Create(house.GetCell(new Vector2Int(0, 0), 0));
                    world.AddEntity(bed);

                    var table = this._container.Get<ObjectModel>("furniture.tables.table").Create(house.GetCell(new Vector2Int(1, 2), 0));
                    world.AddEntity(table);

                    var waterthrough = this._container.Get<ObjectModel>("furniture.waterthrough").Create(house.GetCell(new Vector2Int(-1, 0), 0));
                    world.AddEntity(waterthrough);
                }
            }

            VoxelLight.InitWorldLight(world);
            VoxelLight.UpdateLights();
        }
    }
}