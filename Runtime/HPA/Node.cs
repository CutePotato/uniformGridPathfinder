using System.Collections.Generic;
using HierarchicalJPS.JPS;
using HierarchicalJPS.Navigation;
using UnityEngine;

namespace HierarchicalJPS.HPA
{
    public class Node
    {
        public Vector3 pos;
        public List<Edge> edges;
        public Node child;
        public bool obstacle = false;
		public SurfaceBase surface;

        public Node(Vector3 value)
        {
            pos = value;
            edges = new List<Edge>();
        }

		public Node(Vector3 value, SurfaceBase surface)
        {
            pos = value;
			this.surface = surface;
            edges = new List<Edge>();
        }

        public bool HasForcedNodes(Direction fromParentDir)
        {
            var cardinals = Pathfinder.cardinals;
            
            foreach (Edge edge in edges)
            {
                if (edge.end.obstacle && edge.direction != fromParentDir && cardinals.Contains(edge.direction))
                {
                    var n = edge.end.edges.Find(p => p.direction == fromParentDir);
                    if (n != null && !n.end.obstacle)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}