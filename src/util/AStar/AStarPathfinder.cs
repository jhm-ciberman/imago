using System;
using System.Collections.Generic;

namespace LifeSim
{
    public class AStarPathfinder<TNode> : IComparer<TNode>, INodeVisitor<TNode> where TNode : struct
    {
        private TNode _start;

        private TNode _end;

        private PathfinderNode _currentNode;

        private HashSet<TNode> _closedSet;

        private PriorityQueue<TNode> _openSet;

        private Dictionary<TNode, PathfinderNode> _nodes;

        private INavigator<TNode> _navigator;

        public AStarPathfinder(INavigator<TNode> navigator)
        {
            this._navigator = navigator;
            this._closedSet = new HashSet<TNode>();
            this._openSet = new PriorityQueue<TNode>(this);
            this._nodes = new Dictionary<TNode, PathfinderNode>();
        }

        public float _GetNodeGScore(TNode coord)
        {
            if (this._nodes.TryGetValue(coord, out PathfinderNode node)) return node.gScore;
            return float.PositiveInfinity;
        }

        int IComparer<TNode>.Compare(TNode x, TNode y)
        {
            return Math.Sign(this._nodes[x].fScore - this._nodes[y].fScore);
        }

        protected List<TNode> _ReconstructPath(TNode current)
        {
            var path = new List<TNode>();
            
            path.Add(current);
            do {
                current = this._nodes[current].cameFrom;
                path.Add(current);
            } while (! current.Equals(this._start));

            path.Reverse();

            return path;
        }

        public List<TNode>? Pathfind(TNode start, TNode end)
        {
            return this.Pathfind(start, end, start);    
        }

        public List<TNode>? Pathfind(TNode start, TNode end, TNode cameFrom)
        {
            this._nodes.Clear();
            this._closedSet.Clear();
            this._openSet.Clear();
            
            this._start = start;
            this._end = end;

            float hCost = this._navigator.HeuristicDistance(this._start, this._end);
            this._nodes[this._start] = new PathfinderNode(this._start, cameFrom, 0f, hCost);
            this._openSet.Enqueue(this._start);
            while (this._openSet.Count > 0) {
                var coords = this._openSet.Dequeue();
                this._closedSet.Add(coords);

                if (coords.Equals(this._end)) {
                    return this._ReconstructPath(coords);
                }

                this._currentNode = this._nodes[coords];
                // Expand tile
                this._navigator.VisitNodeNeighbours(this, coords);
            }

            return null; // Failure! 
        }

        void INodeVisitor<TNode>.VisitNode(TNode neighbourCoord)
        {
            if (this._closedSet.Contains(neighbourCoord)) return;

            float dist = this._navigator.WeightFunction(this._currentNode.coord, neighbourCoord, this._currentNode.cameFrom);
            float gScoreTentative = this._currentNode.gScore + dist;

            var neighbourGScore = this._GetNodeGScore(neighbourCoord);

            if (gScoreTentative < neighbourGScore) {
                // This path to neighbor is better than any previous one. Record it!
                float h = this._navigator.HeuristicDistance(neighbourCoord, this._end);
                float f = gScoreTentative + h;
                this._nodes[neighbourCoord] = new PathfinderNode(neighbourCoord, this._currentNode.coord, gScoreTentative, f);

                if (! this._openSet.Contains(neighbourCoord)) {
                    this._openSet.Enqueue(neighbourCoord);
                }
            }
        }

        struct PathfinderNode : IEquatable<PathfinderNode>
        {
            public readonly TNode coord;
            public readonly TNode cameFrom;
            public readonly float gScore;
            public readonly float fScore;

            public PathfinderNode(TNode coord, TNode cameFrom, float gScore, float fScore)
            {
                this.coord = coord;
                this.cameFrom = cameFrom;
                this.gScore = gScore;
                this.fScore = fScore;
            }

            public bool Equals(PathfinderNode other)
            {
                return this.coord.Equals(other.coord);
            }

            public override int GetHashCode()
            {
                return this.coord.GetHashCode();
            }
        }
    }
}