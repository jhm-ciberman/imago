namespace LifeSim.Engine.SceneGraph
{
    public interface IStage
    {
        Scene3D currentScene3D { get; }

        Canvas2D currentCanvas2D { get; }

        void Update(float deltaTime);
    }
}