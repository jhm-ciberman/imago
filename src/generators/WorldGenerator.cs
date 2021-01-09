using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LifeSim.Simulation;

namespace LifeSim.Generation
{
    public class WorldGenerator
    {
        private List<IWorldGenerationStep> _pipeline = new List<IWorldGenerationStep>();

        private Vector2Int _size;

        public struct ProgressEvent
        {
            public int currentStep;
            public int totalSteps;
            public string currentStepName;
        }

        public WorldGenerator(Vector2Int size)
        {
            this._size = size;
        }

        public Task<World> GenerateWorld(IProgress<ProgressEvent>? progress = null)
        {
            return Task.Run(() => {
                // Create a new world and run it through the pipeline
                World world = new World(this._size);

                int currentStep = 0;
                foreach (var step in this._pipeline)
                {
                    System.Console.WriteLine("GOTOOO: " + step.GetType().Name);
                    Benchmark.Run(step.GetType().Name, () => step.Handle(world));
                    currentStep += 1;

                    int total = this._pipeline.Count;
                    var name = currentStep < total ? this._pipeline[currentStep].GetType().ToString() : "";

                    progress?.Report(new ProgressEvent { currentStep = currentStep, totalSteps = total, currentStepName = name});
                }
                
                foreach (var chunk in world.chunks) chunk.SetAsNoDirty();

                return world;
            });
        }

        public void Add(IWorldGenerationStep step)
        {
            this._pipeline.Add(step);
        }

    }
}