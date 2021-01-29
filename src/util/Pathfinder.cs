using System;
using System.Collections.Generic;

namespace LifeSim
{
    public abstract class Pathfinder : IComparer<Vector2Int>
    {
        private Vector2Int _start;

        private Vector2Int _end;

        private Vector2Int _size;

        private readonly HashSet<Vector2Int> _closedSet;

        private readonly PriorityQueue<Vector2Int> _openSet;

        private readonly Dictionary<Vector2Int, Node> _nodes;

        public Pathfinder(Vector2Int size, Vector2Int start, Vector2Int end)
        {
            this._start = start;
            this._end = end;
            this._size = size;
            this._closedSet = new HashSet<Vector2Int>();
            this._openSet = new PriorityQueue<Vector2Int>(this);
            this._nodes = new Dictionary<Vector2Int, Node>();
        }

        public float FindGScore(Vector2Int coord)
        {
            if (this._nodes.TryGetValue(coord, out Node node)) return node.gScore;
            return float.PositiveInfinity;
        }

        public int Compare(Vector2Int x, Vector2Int y) => Math.Sign(this._nodes[x].fScore - this._nodes[y].fScore);

        public IEnumerable<Vector2Int> GetNeighbours(Vector2Int coords)
        {
            if (coords.x - 1 >= 0)
                yield return new Vector2Int(coords.x - 1, coords.y);

            if (coords.x + 1 < this._size.x)
                yield return new Vector2Int(coords.x + 1, coords.y);

            if (coords.y - 1 >= 0)
                yield return new Vector2Int(coords.x, coords.y - 1);

            if (coords.y + 1 < this._size.y)
                yield return new Vector2Int(coords.x, coords.y + 1);
        }

        protected abstract float _HeuristicDistance(Vector2Int start, Vector2Int end);

        protected abstract float _WeightFunction(Vector2Int fromCoord, Vector2Int toCoord, Vector2Int cameFromCoord);

        protected List<Vector2Int> _ReconstructPath(Vector2Int current)
        {
            var path = new List<Vector2Int> { current };

            do {
                current = this._nodes[current].cameFrom;
                path.Add(current);
            } while (current != this._start);

            return path;
        }

        private struct Node : IEquatable<Node>
        {
            public Vector2Int cameFrom;
            public Vector2Int coord;
            public float gScore;
            public float fScore;

            public bool Equals(Node other)
            {
                return this.coord.Equals(other.coord);
            }

            public override int GetHashCode()
            {
                return this.coord.GetHashCode();
            }
        }

        public List<Vector2Int>? Pathfind()
        {
            this._nodes[this._start] = new Node {
                cameFrom = this._start,
                coord = this._start,
                gScore = 0f,
                fScore =  this._HeuristicDistance(this._start, this._end),
            };

            this._openSet.Enqueue(this._start);

            while (this._openSet.count > 0)
            {
                var currentCoord = this._openSet.Dequeue();
                this._closedSet.Add(currentCoord);

                if (currentCoord == this._end)
                {
                    return this._ReconstructPath(currentCoord);
                }

                var currentNode = this._nodes[currentCoord];

                // Expand tile
                foreach (var neighbourCoord in this.GetNeighbours(currentCoord))
                {
                    if (this._closedSet.Contains(neighbourCoord)) continue;

                    float gScoreTentative = currentNode.gScore + this._WeightFunction(currentCoord, neighbourCoord, currentNode.cameFrom);

                    var neighbourGScore = this.FindGScore(neighbourCoord);

                    if (gScoreTentative < neighbourGScore)
                    {
                        // This path to neighbor is better than any previous one. Record it!
                        if (! this._closedSet.Contains(neighbourCoord))
                        {
                            this._nodes[neighbourCoord] = new Node {
                                coord = neighbourCoord,
                                cameFrom = currentCoord,
                                gScore = gScoreTentative,
                                fScore = gScoreTentative + this._HeuristicDistance(neighbourCoord, this._end),
                            };
                            this._openSet.Enqueue(neighbourCoord);
                        }

                    }
                }
            }

            return null; // Failure! 
        }
    }
}