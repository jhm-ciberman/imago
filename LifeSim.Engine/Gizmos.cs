using LifeSim.Engine.SceneGraph;

namespace LifeSim.Rendering
{
    public static class Gizmos
    {
        private static GizmosLayer? _layer = null;
        public static GizmosLayer Layer
        {
            get => Gizmos._layer ?? throw new System.Exception("Layer is not set");
            set => Gizmos._layer = value;
        }


        
    }
}