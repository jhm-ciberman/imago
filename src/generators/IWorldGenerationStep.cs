using LifeSim.Simulation;

namespace LifeSim.Generation
{
    public interface IWorldGenerationStep
    {
        void Handle(World world);
    }
}