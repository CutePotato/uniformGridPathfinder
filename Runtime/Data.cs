using System;
using UnityEngine;

namespace UniformGridPathfinder
{
    [Serializable]
    public struct Data
    {
        public Vector3 Position { get; set; }
        public Bounds Bounds { get; set; }

        public Data(Vector3 position, Vector3 size)
        {
            Position = position;
            Bounds = new Bounds(position, size);
        }
    }
}