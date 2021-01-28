using System.Collections.Generic;
using LifeSim.Simulation;

namespace LifeSim.Generation
{
    public class VillageGenerator : IWorldGenerationStep
    {
        private readonly FastNoiseLite _noise;

        private readonly System.Random _random;

        private readonly HouseGenerator _houseGenerator;

        private readonly IContainer _container;

        public VillageGenerator(IContainer container, int seed)
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

                    var bed = new Furniture(this._container.Get<Furniture.Model>("furniture.beds.bed"), house.GetCell(new Vector2Int(0, 0), 0));
                    world.AddEntity(bed);

                    var table = new Furniture(this._container.Get<Furniture.Model>("furniture.tables.table"), house.GetCell(new Vector2Int(1, 2), 0));
                    world.AddEntity(table);

                    var waterthrough = new Furniture(this._container.Get<Furniture.Model>("furniture.waterthrough"), house.GetCell(new Vector2Int(-1, 0), 0));
                    world.AddEntity(waterthrough);

                    //Character character = new Character(world);
                    //character.position = house.lot.coords * Tile.size + new Vector2(1.5f, 1.5f);
                    //character.direction = 90f;
                    //world.characters.Add(character);
                }
            }

            VoxelLight.InitWorldLight(world);
            VoxelLight.UpdateLights();
        }
    }
}