namespace LifeSim.Engine.SceneGraph
{
    public abstract class Stage
    {
        public virtual Scene3D? currentScene3D => null;

        public virtual Canvas2D? currentCanvas2D => null;

        public abstract ILayer[] GetLayers();

        public virtual void Update(float deltaTime)
        {
            // nothing!
        } 
    }
}