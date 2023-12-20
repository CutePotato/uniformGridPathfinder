using System.Collections.Generic;
using System.Linq;
using HierarchicalJPS.HPA;
using UnityEngine;

namespace HierarchicalJPS.JPS
{
    public class BackTrackDFS
    {
        // Array of lists for
        // Adjacency List Representation
        public Dictionary<Vector3, Vector3?> adj;
        public Dictionary<Vector3, Edge> dictionary;
        public Dictionary<Vector3, bool> visited;
        private LinkedList<Edge> points;

        // Constructor
        public BackTrackDFS()
        {
            adj = new Dictionary<Vector3, Vector3?>();
            points = new LinkedList<Edge>();
            dictionary = new Dictionary<Vector3, Edge>();
        }

        public void AddKey(Vector3 start, Edge edge)
        {
            if (!adj.ContainsKey(start))
            {
                adj.Add(start, null);
            }
            if (!dictionary.ContainsKey(start))
            {
                dictionary.Add(start, edge);
            }
        }

        // Function to Add an edge into the graph
        public void AddEdge(Vector3 jumpPoint, Edge jumpEdge, Vector3 parent)
        {
            if (!dictionary.ContainsKey(jumpPoint))
            {
                dictionary.Add(jumpPoint, jumpEdge);
            }
            if (!adj.ContainsKey(jumpPoint))
            {
                adj.Add(jumpPoint, parent);
                return;
            }

            adj[jumpPoint] = parent;
        }

        // A function used by DFS
        protected void DFSUtil(Vector3 node, Vector3 start)
        {
            // Mark the current node as visited
            // and print it
            visited[node] = true;
            points.AddFirst(dictionary[node]);

            var parent = adj[node];
            if (!parent.HasValue) return;
            // Recur for all the vertices
            // adjacent to this vertex
            if (!visited[parent.Value]){
                DFSUtil(parent.Value, start);
            }
        }

        // The function to do DFS traversal.
        // It uses recursive DFSUtil()
        public LinkedList<Edge> DFS(Vector3 end, Vector3 start)
        {
            // Mark all the vertices as not visited
            // (set as false by default in c#)
            visited = adj.ToDictionary(pair => pair.Key, pair => { return false; });

            // Call the recursive helper function
            // to print DFS traversal
            DFSUtil(end, start);

            return points;
        }
    }
}
