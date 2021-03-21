namespace LifeSim
{
    public interface INodeVisitor<TNode> where TNode : struct
    {
        void VisitNode(TNode node);
    }
}