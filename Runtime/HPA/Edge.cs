using System.Collections.Generic;
using System.Linq;
using HierarchicalJPS.JPS;
using HierarchicalJPS.Utilities;
using UnityEngine;

namespace HierarchicalJPS.HPA
{
    public class Edge
    {
        public Node start;
        public Node end;
        public EdgeType type;
        public float weight;
        public Direction direction;

        public LinkedList<Edge> UnderlyingPath;

        public Edge(Node n1, Node n2, EdgeType type, LinkedList<Edge> path)
        {
            start = n1;
            end = n2;
            this.type = type;
            UnderlyingPath = path;
            SetDirection();
        }

        public Edge(Node n1, Node n2, EdgeType type, float weight)
        {
            start = n1;
            end = n2;
            this.type = type;
            this.weight = weight;
            SetDirection();
        }

        public void SetDirection() {
            if (start == null || end == null) return;
            Vector3Int vectorDir = MathHelper.FloorToInt(end.pos - start.pos);
            if (vectorDir.x >= 1 && vectorDir.z >= 1){
                direction = Direction.NorthEast;
                return;
            }
            if (vectorDir.x >= 1 && vectorDir.z <= -1){
                direction = Direction.SouthEast;
                return;
            }
            if (vectorDir.x <= -1 && vectorDir.z >= 1){
                direction = Direction.NorthWest;
                return;
            }
            if (vectorDir.x <= -1 && vectorDir.z <= -1){
                direction = Direction.SouthWest;
                return;
            }
            if (vectorDir.x >= 1){
                direction = Direction.East;
                return;
            }
            if (vectorDir.x <= -1){
                direction = Direction.West;
                return;
            }
            if (vectorDir.z >= 1){
                direction = Direction.North;
                return;
            }
            if (vectorDir.z <= -1){
                direction = Direction.South;
                return;
            }
        }
        
        public IEnumerable<Edge> PrunedNeighbors()
        {
            var forcedNodes = new List<Edge>();
            
            if (start == null) return end.edges.Where(e => e.end.obstacle == false);

            var obstacles = GetCardinalObstaclesExcludingSameDirection();

            if (obstacles.Any())
            {
                ForcedNodes(obstacles, ref forcedNodes);
            }

            NaturalNodes(ref forcedNodes);
            
            return forcedNodes;
        }

        public void NaturalNodes(ref List<Edge> forcedNodes)
        {
            var validEdges = end.edges.Where(e => (e.direction & direction) == e.direction && e.end.obstacle == false);
            forcedNodes.AddRange(validEdges);
        }

        private void ForcedNodes(IEnumerable<Edge> obstacles, ref List<Edge> forcedNodes)
        {
            var dirs = Pathfinder.SplitDiagonalDirection(direction);
            foreach (Edge obstacle in obstacles)
            {
                foreach (var dir in dirs)
                {
                    var forcedNodeDirection = dir | obstacle.direction;
                    if (dir == Direction.None || dir == obstacle.direction || (int)forcedNodeDirection % 5 == 0) continue;
                    forcedNodes.AddRange(end.edges.Where(e => e.direction == forcedNodeDirection));
                }
            }
        }

        private IEnumerable<Edge> GetCardinalObstaclesExcludingSameDirection()
        {
            var cardinals = Pathfinder.cardinals;
            return end.edges.Where(e =>
                e.end.obstacle &&
                cardinals.Contains(e.direction) &&
                e.direction != direction
            );
        }

        public float GetUnderlyingPathCost()
        {
            float cost = 0;
            foreach (var edge in UnderlyingPath)
            {
                cost += edge.weight;
            }
            return cost;
        }
    }
}
