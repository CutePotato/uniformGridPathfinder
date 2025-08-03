using System.Collections.Generic;
using HierarchicalJPS.Attribute;
using HierarchicalJPS.HPA;
using UnityEngine;

namespace HierarchicalJPS.Navigation
{
    public class SurfaceBase : MonoBehaviour
    {
        protected static readonly float Sqrt2 = Mathf.Sqrt(2.0f);
        protected static readonly Vector3 Offset = new Vector3(.5f, 0, .5f);
        public Data data;
        public Vector3 size;
        public Dictionary<Vector3, Node> Nodes = new();
        public Dictionary<Vector3, Node> LinkedNodes = new();
        [ReadOnly] public bool baked = false;
        protected Map _map;

        protected virtual void Awake()
        {
            data = new Data(transform.position, size);
            _map = GetComponentInParent<Map>();
        }

        public virtual void Bake()
        {
            baked = false;
            
            CreateNodes();

            baked = true;
        }

        protected void CreateNodes()
        {
            Nodes = new Dictionary<Vector3, Node>();

            //1. Create all nodes necessary
            var bounds = data.Bounds;
            var leftBottomCorner = bounds.min;

            for (int i = 0; i < bounds.size.x; ++i)
            {
                for (int j = 0; j < bounds.size.z; ++j)
                {
                    var step = new Vector3(i, 0, j);
                    var cornerLeftCell = leftBottomCorner + step;
                    Vector3 pos = cornerLeftCell + Offset;
                    var node = new Node(pos, this);
                    Nodes.Add(pos, node);
                }
            }

            CreateNodesEdges();
        }

        private void CreateNodesEdges()
        {
            //2. Create all possible edges
            foreach (Node n in Nodes.Values)
            {
                //Look for straight edges
                for (int i = -1; i < 2; i += 2)
                {
                    SearchMapEdge(Nodes, n, n.pos.x + i, n.pos.z, false);

                    SearchMapEdge(Nodes, n, n.pos.x, n.pos.z + i, false);
                }

                //Look for diagonal edges
                for (int i = -1; i < 2; i += 2)
                for (int j = -1; j < 2; j += 2) 
                {
                    SearchMapEdge(Nodes, n, n.pos.x + i, n.pos.z + j, true);
                }
            }
        }

        private void SearchMapEdge(Dictionary<Vector3, Node> mapNodes, Node n, float x, float z, bool diagonal)
        {
            var weight = diagonal ? Sqrt2 : 1f;
            Vector3 gridTile = Vector3.zero + Vector3.up * n.pos.y;

            //Don't let diagonal movement occur when an obstacle is crossing the edge
            if (diagonal)
            {
                gridTile.x = n.pos.x;
                gridTile.z = z;
                if (!IsFreeTile(gridTile))
                    return;

                gridTile.x = x;
                gridTile.z = n.pos.z;
                if (!IsFreeTile(gridTile))
                    return;
            }

            gridTile.x = x;
            gridTile.z = z;
            if (!IsFreeTile(gridTile))
                return;

            var n2 = mapNodes[gridTile];
            //Edge is valid, add it to the node
            n.edges.Add(new Edge(n, n2, EdgeType.INTER, weight));
        }
        
        public bool IsFreeTile(Vector3 tile)
        {
            Node node;
            var bounds = data.Bounds;
            var min = bounds.min;
            var max = bounds.max;
            Nodes.TryGetValue(tile, out node);
            return tile.x >= min.x && tile.x <= max.x &&
                   tile.y >= min.y && tile.y <= max.y &&
                   tile.z >= min.z && tile.z <= max.z &&
                   node != null;
        }
    }
}
