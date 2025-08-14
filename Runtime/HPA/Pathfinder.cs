using System.Collections.Generic;
using UniformGridPathfinder.JPS;
using Priority_Queue;
using UnityEngine;

namespace UniformGridPathfinder.HPA
{
    public static class Pathfinder
    {
        /// <summary>
        /// Return all cardinal direction clock wise fashion
        /// </summary>
        public static List<Direction> cardinals = GetCardinalDirections();

        /// <summary>
        /// Gets euclidean distance from one node to another
        /// </summary>
        /// <param name="node1">Start node</param>
        /// <param name="node2">End node</param>
        /// <returns>Return euclidean distance from one node to another</returns>
        public static float EuclidianDistance(Node node1, Node node2)
        {
            return EuclidianDistance(node1.pos, node2.pos);
        }

        /// <summary>
        ///  Gets euclidean distance from one node to another
        /// </summary>
        /// <param name="tile1">Start position</param>
        /// <param name="tile2">End position</param>
        /// <returns>Return euclidean distance from one position to another</returns>
        public static float EuclidianDistance(Vector3 tile1, Vector3 tile2)
        {
            return Mathf.Sqrt(Mathf.Pow(tile2.x - tile1.x, 2) + Mathf.Pow(tile2.y - tile1.y, 2) + Mathf.Pow(tile2.z - tile1.z, 2));
        }

        /// <summary>
        /// A* pathfinding between nodes and surfaces
        /// </summary>
        /// <param name="start">Start node</param>
        /// <param name="dest">Final node</param>
        /// <returns>Return a path from the start node to final node</returns>
        public static LinkedList<Edge> FindPath(Node start, Node dest)
        {
            HashSet<Vector3> Visited = new HashSet<Vector3>();
            Dictionary<Vector3, Edge> Parent = new Dictionary<Vector3, Edge>();
            Dictionary<Vector3, float> gScore = new Dictionary<Vector3, float>();

            SimplePriorityQueue<Node, float> pq = new SimplePriorityQueue<Node, float>();

            float temp_gCost, prev_gCost;

            gScore[start.pos] = 0;
            pq.Enqueue(start, EuclidianDistance(start, dest));
            Node current;

            while (pq.Count > 0)
            {
                current = pq.Dequeue();

                if (current.pos.Equals(dest.pos))
                    //Rebuild path and return it
                    return RebuildPath(Parent, current);


                Visited.Add(current.pos);

                //Visit all neighbours through edges going out of node
                for (int i = 0; i < current.edges.Count; i++)
                {
                    Edge e = current.edges[i];

                    //Check if we visited the outer end of the edge
                    if (Visited.Contains(e.end.pos))
                        continue;

                    temp_gCost = gScore[current.pos] + e.weight;

                    //If new value is not better then do nothing
                    if (gScore.TryGetValue(e.end.pos, out prev_gCost) && temp_gCost >= prev_gCost)
                        continue;

                    //Otherwise store the new value and add the destination into the queue
                    Parent[e.end.pos] = e;
                    gScore[e.end.pos] = temp_gCost;

                    pq.Enqueue(e.end, temp_gCost + EuclidianDistance(e.end, dest));
                }
            }

            return new LinkedList<Edge>();
        }

        /// <summary>
        /// Jump point search pathfinding
        /// </summary>
        /// <param name="start">Start node</param>
        /// <param name="goal">Final node</param>
        /// <returns>Returns path from the start node to goal node</returns>
        public static LinkedList<Edge> FindPathJPS(Node start, Node goal)
        {
            HashSet<Vector3> Visited = new();
            Dictionary<Vector3, float> gScore = new();

            SimplePriorityQueue<Edge, float> pq = new();
            var trackDFS = new BackTrack();

            gScore[start.pos] = 0;
            var startEdge = new Edge(null, start, EdgeType.INTRA, 0);
            trackDFS.AddEdge(start.pos, startEdge, null);
            pq.Enqueue(startEdge, EuclidianDistance(start, goal));
            Edge current;

            while (pq.Count > 0)
            {
                current = pq.Dequeue();

                if (current.end.pos.Equals(goal.pos))
                {
                    //Rebuild path and return it
                    var path = RebuildPathJPS(trackDFS, current.end.pos);
                    return path;
                }

                Visited.Add(current.end.pos);

                var neighbours = current.PrunedNeighbors();
                foreach (var neighbour in neighbours)
                {
                    if (Visited.Contains(neighbour.end.pos))
                    {
                        continue;
                    }

                    Edge jumpNode = Jump(neighbour, goal);
                    if (jumpNode != null)
                    {
                        trackDFS.AddEdge(jumpNode.end.pos, jumpNode, current.end.pos);
                        var score = gScore[current.end.pos] + EuclidianDistance(current.end, jumpNode.end);
                        gScore[jumpNode.end.pos] = score;
                        pq.Enqueue(jumpNode, score + EuclidianDistance(jumpNode.end, goal));
                    }
                }
            }

            return new LinkedList<Edge>();
        }

        /// <summary>
        /// Check if current step edge is forced node or the goal node we search for
        /// </summary>
        /// <param name="step">Step edge we checking</param>
        /// <param name="goal">Searched node</param>
        /// <returns>Return current edge if is forced node or searched node</returns>
        private static Edge Jump(Edge step, Node goal)
        {
            var direction = step.direction;
            var stepNode = step.end;
            
            if (stepNode.obstacle)
            {
                return null;
            }
            if (stepNode.pos.Equals(goal.pos))
            {
                return step;
            }
            if (stepNode.HasForcedNodes(direction))
            {
                return step;
            }
            //Diagonal
            if (!cardinals.Contains(direction))
            {
                foreach ( Direction d in SplitDiagonalDirection(direction))
                {
                    if (Jump(step, d, goal) != null)
                    {
                        return step;
                    }
                }
            }

            return Jump(step, direction, goal);
        }

        /// <summary>
        /// Stepping iteration until find next forced node or goal node
        /// </summary>
        /// <param name="edge">Edge we stepping from</param>
        /// <param name="direction">Direction we stepping to</param>
        /// <param name="goal">Goal node</param>
        /// <returns>Return next forced node, goal node or null of no valid node find in this direction</returns>
        private static Edge Jump(Edge edge, Direction direction, Node goal)
        {
            var step = Step(edge, direction);
            
            if (step == null)
            {
                return null;
            }
            
            var stepNode = step.end;
            if (stepNode.obstacle)
            {
                return null;
            }
            if (stepNode.pos.Equals(goal.pos))
            {
                return step;
            }

            var nextStep = Step(step, direction);
            if (nextStep != null)
            {
                if (!nextStep.end.obstacle && stepNode.HasForcedNodes(direction))
                {
                    return step;
                }
            }
            
            //Diagonal
            if (!cardinals.Contains(direction))
            {
                foreach ( Direction d in SplitDiagonalDirection(direction))
                {
                    if (Jump(step, d, goal) != null)
                    {
                        return step;
                    }
                }
            }

            return Jump(step, direction, goal);
        }
        
        /// <summary>
        /// Get the list of cardinal directions
        /// </summary>
        /// <returns>Return all cardinal direction clock wise fashion</returns>
        private static List<Direction> GetCardinalDirections()
        {
            return new()
            {
                Direction.North,
                Direction.East,
                Direction.South,
                Direction.West,
            };
        }

        /// <summary>
        /// Stepping from this edge to set direction to get next edge 
        /// </summary>
        /// <param name="edge">Edge we stepping from</param>
        /// <param name="dir">Direction we stepping to</param>
        /// <returns>Next edge</returns>
        public static Edge Step(Edge edge, Direction dir)
        {
            return edge.end.edges.Find(e => e.direction == dir);
        }

        /// <summary>
        /// Split diagonal direction in separate directions
        /// </summary>
        /// <param name="dir">Direction</param>
        /// <returns>Array of split directions</returns>
        public static Direction[] SplitDiagonalDirection(Direction dir)
        {
            int dirInt = (int)dir;
            var first = Mathf.Pow(2, Mathf.Floor(Mathf.Log(dirInt, 2)));
            var second = dirInt - first;

            return new Direction[2] { (Direction)first, (Direction)second };
        }
        
        /// <summary>
        /// Rebuild path from dest edge to start edge
        /// </summary>
        /// <param name="Parent">Path dictionary</param>
        /// <param name="dest">Goal node</param>
        /// <returns>Path from start edge to goal edge</returns>
        private static LinkedList<Edge> RebuildPath(Dictionary<Vector3, Edge> Parent, Node dest)
        {
            LinkedList<Edge> res = new LinkedList<Edge>();
            var current = dest.pos;
            Edge e = null;

            while (Parent.TryGetValue(current, out e))
            {
                res.AddFirst(e);
                current = e.start.pos;
            }

            return res;
        }

        /// <summary>
        /// Rebuild path from dest edge to start edge
        /// </summary>
        /// <param name="track">Deep first search instance</param>
        /// <param name="dest">Destination</param>
        /// <returns></returns>
        private static LinkedList<Edge> RebuildPathJPS(BackTrack track, Vector3 dest)
        {
            return track.Start(dest);
        }
    }
}
