using System.Collections.Generic;
using UniformGridPathfinder.HPA;
using UnityEngine;

namespace UniformGridPathfinder.JPS
{
    public class BackTrack
    {
        private readonly Dictionary<Vector3, Vector3?> _relationChildParent;
        private readonly Dictionary<Vector3, Edge> _dictionary;
        private readonly LinkedList<Edge> _points;

        public BackTrack()
        {
            _relationChildParent = new Dictionary<Vector3, Vector3?>();
            _points = new LinkedList<Edge>();
            _dictionary = new Dictionary<Vector3, Edge>();
        }

        /// <summary>
        /// Add Edge and relation with predecessor
        /// </summary>
        /// <param name="jumpPoint">Jump position to register</param>
        /// <param name="jumpEdge">His edge</param>
        /// <param name="parent">Parent position</param>
        public void AddEdge(Vector3 jumpPoint, Edge jumpEdge, Vector3? parent)
        {
            _dictionary[jumpPoint] = jumpEdge;
            _relationChildParent[jumpPoint] = parent;
        }

        /// <summary>
        /// Start back tracking path
        /// </summary>
        /// <param name="end">Goal position to start back tracking from</param>
        /// <returns>Return path from start to end</returns>
        public LinkedList<Edge> Start(Vector3 end)
        {
            _points.Clear();
            var seen = new HashSet<Vector3>();
            Vector3? parent = end;
            while (parent.HasValue)
            {
                if (!seen.Add(parent.Value)) throw new System.InvalidOperationException("Cyclic predecessor chain.");
                _points.AddFirst(_dictionary[parent.Value]);
                parent = _relationChildParent[parent.Value];
            }

            return _points;
        }
    }
}
