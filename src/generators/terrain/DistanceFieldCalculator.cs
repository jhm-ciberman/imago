using System;
using LifeSim.Simulation;

namespace LifeSim.Generation
{
    public class DistanceFieldCalculator
    {
        private const int DIAG_DIST = 10;
        private const int LATERAL_DIST = 14;

        public delegate bool HasFeature(World world, int x, int y);

        public static int[,] ComputeDistanceField(World world, HasFeature hasFeatureCallback)
        {
            int[,] field = DistanceFieldCalculator._CreateInitialField(world, hasFeatureCallback);
            DistanceFieldCalculator._ComputeField(field);
            return field;
        }

        private static void _ComputeField(int[,] field)
        {
            int width = field.GetLength(0);
            int height = field.GetLength(1);

            // First pass, from top left to bottom right
            for (int y = 1; y < height; y++) {
                for (int x = 1; x < width; x++) {
                    DistanceFieldCalculator._Min(field, x, y, -1, -1);
                }

                for (int x = width - 2; x >= 0; x--) {
                    DistanceFieldCalculator._Min(field, x, y, +1, -1);
                }
            }

            // Second pass, from bottom right to top left
            for (int y = height - 2; y >= 0; y--) {
                for (int x = width - 2; x >= 0; x--) {
                    DistanceFieldCalculator._Min(field, x, y, +1, +1);
                }

                for (int x = 1; x < width; x++) {
                    DistanceFieldCalculator._Min(field, x, y, -1, +1);
                }
            }
        }

        private static void _Min(int[,] field, int x, int y, int dirX, int dirY)
        {
            field[x, y] = Math.Min(
                Math.Min(
                    field[x, y], 
                    field[x + dirX, y + dirY] + DIAG_DIST
                ),
                Math.Min(
                    field[x + dirX, y] + LATERAL_DIST,
                    field[x, y + dirY] + LATERAL_DIST
                )
            );
        }

        private static int[,] _CreateInitialField(World world, HasFeature hasFeatureCallback)
        {
            int[,] field = new int[world.size.x, world.size.y];

            for (int y = 0; y < world.size.y; y++) {
                for (int x = 0; x < world.size.x; x++) {
                    field[x, y] = hasFeatureCallback(world, x, y) ? 0 : int.MaxValue - 20;
                }
            }

            return field;
        }
    }
}