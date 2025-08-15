using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniformGridPathfinder.HPA;
using UniformGridPathfinder.Attribute;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UniformGridPathfinder.Navigation
{
    public class Map : MonoBehaviour
    {
        private static readonly Vector3 Offset = new(.5f, 0, .5f);
        
        public List<Link> links = new();
        public SurfaceBase[] surfaces;
        private List<Node> _addedNodes;
        [ReadOnly] public bool baked = false;
        public LineRenderer linePath;
        public PlayerInput playerInput;
#if UNITY_EDITOR
        public Vector3 startPosition = new Vector3(4.5f, 0, -4.5f);
        public Vector3 endPosition = new Vector3(-10.5f, 2, 10.5f);
#endif

        protected virtual void Start()
        {
            _addedNodes = new List<Node>();
            StartBakingSurfaces();
        }

        protected virtual void StartBakingSurfaces()
        {
            baked = false;
            
            Parallel.ForEach(surfaces, surface => surface.Bake());
            
            CreateSurfacesLinkNodes();
            
            baked = true;
        }

        protected virtual void CreateSurfacesLinkNodes()
        {
            foreach(var link in links)
            {
                var g1 = link.startPoint;
                var g2 = link.endPoint;

                var c1 = surfaces.First(cluster => cluster.Nodes.ContainsKey(g1));
                var c2 = surfaces.First(cluster => cluster.Nodes.ContainsKey(g2));

                if (!c1.LinkedNodes.TryGetValue(g1, out var n1))
                {
                    n1 = new Node(g1);
                    c1.LinkedNodes.Add(g1, n1);
                    n1.child = c1.Nodes[g1];
                }

                if (!c2.LinkedNodes.TryGetValue(g2, out var n2))
                {
                    n2 = new Node(g2);
                    c2.LinkedNodes.Add(g2, n2);
                    n2.child = c2.Nodes[g2];
                }

                n1.edges.Add(new Edge(n1, n2, EdgeType.INTRA, link.costModifier));
                n2.edges.Add(new Edge(n2, n1, EdgeType.INTRA, link.costModifier));
            }
            
            BuildSurfacesConnections();
        }

        public void UpdateSurfacesLinkNodes()
        {
            baked = false;

            links.ForEach(link => link.UpdateLink());
            
            foreach (var surface in surfaces)
            {
                surface.LinkedNodes.Clear();
            }
            
            CreateSurfacesLinkNodes();

            baked = true;
        }

        protected void BuildSurfacesConnections()
        {
            //Add Intra edges for every border nodes and pathfind between them
            for (int i = 0; i < surfaces.Length; ++i)
                GenerateIntraEdges(surfaces[i]);
        }

        ///Intra edges are edges that lives inside surfaces(linkedNodes)
        private void GenerateIntraEdges(SurfaceBase c)
        {
            int i, j;
            Node n1, n2;

            /* We do this so that we can iterate through pairs once, 
             * by keeping the second index always higher than the first */
            var nodes = new List<Node>(c.LinkedNodes.Values);

            for (i = 0; i < nodes.Count; ++i)
            {
                n1 = nodes[i];
                for (j = i + 1; j < nodes.Count; ++j)
                {
                    n2 = nodes[j];

                    ConnectNodes(n1, n2);
                }
            }
        }

        /// <summary>
        /// Connect two nodes by pathfinding between them. 
        /// </summary>
        /// <remarks>We assume they are different nodes. If the path returned is 0, then there is no path that connects them.</remarks>
        private bool ConnectNodes(Node n1, Node n2)
        {
            LinkedList<Edge> path;
            Edge e1, e2;
            
            path = Pathfinder.FindPathJPS(n1.child, n2.child);

            if (path.Count > 0)
            {
                e1 = new Edge(n1, n2, EdgeType.INTRA, path);

                e2 = new Edge(n2, n1, EdgeType.INTRA, new LinkedList<Edge>(path.Reverse()));

                float weight = 0;
                foreach (var edge in path)
                {
                    weight += edge.weight;
                }

                //Update weights
                e1.weight = weight;
                e2.weight = weight;

                n1.edges.Add(e1);
                n2.edges.Add(e2);

                return true;
            }
            else
            {
                //No path, return false
                return false;
            }
        }
        
        public LinkedList<Edge> GetPath(Vector3 start, Vector3 dest)
        {
            var path = HierarchicalPathfinder.FindHierarchicalPath(this, start, dest);
            return HierarchicalPathfinder.GetLayerPathFromHPA(path);
        }

        /// <summary>
        /// Insert start and dest nodes in graph in all layers
        /// </summary>
        public void InsertNodes(Vector3 start, Vector3 dest, out Node nStart, out Node nDest)
        {
            SurfaceBase cStart, cDest;
            Node newStart, newDest;
            var floorStart = Vector3Int.FloorToInt(start) + Offset;
            var floorDest = Vector3Int.FloorToInt(dest) + Offset;
            
            bool isConnected;
            _addedNodes.Clear();

            cStart = null;
            nStart = null;
            cDest = null;
            nDest = null;
            isConnected = false;

            foreach (var c in surfaces)
            {
                if (c.Nodes.ContainsKey(floorStart))
                {
                    cStart = c;
                    nStart = c.Nodes[floorStart];
                }

                if (c.Nodes.ContainsKey(floorDest))
                {
                    cDest = c;
                    nDest = c.Nodes[floorDest];
                }

                if (cStart != null && cDest != null)
                    break;
            }

            if (cStart == cDest)
            {
                newStart = new Node(floorStart) { child = nStart };
                newDest = new Node(floorDest) { child = nDest };

                isConnected = ConnectNodes(newStart, newDest);

                if (isConnected)
                {
                    nStart = newStart;
                    nDest = newDest;
                }
            }

            if (!isConnected)
            {
                nStart = ConnectToBorder(floorStart, cStart, nStart);
                nDest = ConnectToBorder(floorDest, cDest, nDest);
            }
        }

        /// <summary>
        /// Remove nodes from the graph, including all underlying edges
        /// </summary>
        public void RemoveAddedNodes()
        {
            foreach (Node n in _addedNodes)
            {
                foreach (Edge e in n.edges)
                {
                    //Find an edge in current.end that points to this node
                    e.end.edges.RemoveAll((ee) => ee.end == n);
                }
            }
        }

        /// <summary>
        /// Connect the grid tile to borders by creating a new node
        /// </summary>
        /// <returns>The node created</returns>
        private Node ConnectToBorder(Vector3 pos, SurfaceBase c, Node child)
        {
            Node newNode;

            //If the position is an actual border node, then return it
            if (c.LinkedNodes.TryGetValue(pos, out newNode))
                return newNode;

            //Otherwise create a node and pathfind through border nodes
            newNode = new Node(pos) { child = child };

            foreach (KeyValuePair<Vector3, Node> n in c.LinkedNodes)
            {
                ConnectNodes(newNode, n.Value);
            }

            //Since this node is not part of the graph, we keep track of it to remove it later
            _addedNodes.Add(newNode);

            return newNode;
        }
    }
}
