using System;
using LifeSim.Simulation;

namespace LifeSim.Generation
{
    public class TerrainGenerator : IWorldGenerationStep
    {
        private readonly float _elevationAmount;

        private readonly IValueGenerator _valueGenerator;

        public TerrainGenerator(float elevationAmount, IValueGenerator valueGenerator)
        {
            this._elevationAmount = elevationAmount;

            this._valueGenerator = valueGenerator;
        }

        public void Handle(World world) 
        {
            var size = world.size;
            var squareMeters = MathF.Sqrt(size.x * size.y);
            int maxHeight = (int) MathF.Ceiling(squareMeters * this._elevationAmount / 12f);
            
            foreach (Chunk chunk in world.chunks)
            {
                Vector2Int offset = chunk.coords * Chunk.SIZE;
                float[,] heightmap = this._GenerateHeightMap(maxHeight, this._valueGenerator, offset, chunk.minMaxHeight);
                this._GenerateChunkHeightData(chunk, heightmap);
                world.minMaxHeight.Value(chunk.minMaxHeight.min);
                world.minMaxHeight.Value(chunk.minMaxHeight.max);
            }
        }

        private float[,] _GenerateHeightMap(float maxHeight, IValueGenerator valueGenerator, Vector2Int offset, MinMaxValue minMax)
        {
            float[,] data = new float[Chunk.SIZE + 1, Chunk.SIZE + 1];

            for (int y = 0; y <= Chunk.SIZE; y++) 
            {
                for (int x = 0; x <= Chunk.SIZE; x++) 
                {
                    float regularHeight = maxHeight * valueGenerator.CalculateHeight(offset.x + x, offset.y + y);
                    float height = MathF.Round(regularHeight / Tile.SNAP_INCREMENT) * Tile.SNAP_INCREMENT;
                    //float height = regularHeight * 5f;
                    data[x, y] = minMax.Value(height);
                }
            }

            return data;
        }

        private void _GenerateChunkHeightData(Chunk chunk, float[,] heightmap) 
        {
            for (int y = 0; y <= Chunk.SIZE; y++)
            {
                for (int x = 0; x <= Chunk.SIZE; x++)
                {
                    Tile tile = chunk.GetTile(x, y);
                    float value = heightmap[x, y];

                    if (y < Chunk.SIZE) 
                    {
                        if (x < Chunk.SIZE) 
                        { // Top left (0)
                            tile.SetHeight0(value);
                        }

                        if (x > 0) 
                        { // Top right (1)
                            chunk.GetTile(x - 1, y).SetHeight1(value);
                        }
                    }

                    if (y > 0) 
                    {
                        if (x < Chunk.SIZE)
                        { // Bottom left (2)
                            chunk.GetTile(x, y - 1).SetHeight2(value);
                        } 

                        if (x > 0) 
                        { // Bottom right (3)
                            chunk.GetTile(x - 1, y - 1).SetHeight3(value);
                        } 
                    }
                }
            }
        }


    }
}