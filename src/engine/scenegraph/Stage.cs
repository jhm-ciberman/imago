namespace LifeSim.Engine.SceneGraph
{
    public abstract class Stage
    {
        public abstract ILayer[] GetLayers();

        public virtual void Update(float deltaTime)
        {
            // nothing!
        } 
    }
}