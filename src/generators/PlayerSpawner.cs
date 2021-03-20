using LifeSim.Simulation;

namespace LifeSim.Generation
{
    public class PlayerSpawner : IWorldGenerationStep
    {
        private readonly System.Random _random;

        public PlayerSpawner(int seed)
        {
            this._random = new System.Random(seed);
        }

        public void Handle(World world)
        {
            Tile tile = this._GetRandomEmptyTile(world);
            var player = new Character(tile.baseCell);
            world.AddEntity(player);
            world.player = player;

            Character character = new Character(tile.baseCell);
            world.AddEntity(character);
        }

        private Tile _GetRandomEmptyTile(World world)
        {
            Tile tile = world.GetTileAt(world.size / 2);
            
            int maxIter = 1000;

            int rx, ry;
            while (! tile.isEmpty && maxIter > 0)
            {
                var size = world.size; 
                rx = (int) (size.x * this._random.NextDouble());
                ry = (int) (size.y * this._random.NextDouble());
                tile = world.GetTileAt(rx, ry);
                maxIter--;
            }

            return tile;
        }
    }
}