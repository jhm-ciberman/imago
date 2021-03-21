namespace LifeSim
{
    public interface INavigator<TNode> where TNode : struct
    {
        float HeuristicDistance(TNode start, TNode end);
        float WeightFunction(TNode fromCoord, TNode toCoord, TNode cameFromCoord);
        void VisitNodeNeighbours(INodeVisitor<TNode> nodeVisitor, TNode node);
    }
}