namespace LifeSim.Engine.Rendering
{
    public interface IMeshFactory
    {
        Mesh CreateMesh(IRenderingResourcesFactory meshFactory);
    }
}