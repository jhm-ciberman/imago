namespace LifeSim.Engine.SceneGraph
{
    public enum EventType
    {
        ChildAdded,
        ChildRemoved,
        TransformDirty,
    }

    public struct Event<TNode>
    {
        public TNode node;
        public EventType type;

        public Event(TNode node, EventType type)
        {
            this.node = node;
            this.type = type;
        }
    }
}