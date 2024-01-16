using System;
using UnityEngine;

namespace HierarchicalJPS.Navigation
{
    [Serializable]
    public struct Link
    {
        public bool activated;
        public bool autoUpdate;
        public int agentTypeID;
        public Transform startTransform;
        public Vector3 startPoint;
        public Transform endTransform;
        public Vector3 endPoint;
        public bool bidirectional;
        public float costModifier;


        public bool Occupied =>
            Physics.Raycast(startPoint + Vector3.up * 2, Vector3.down, 2f, agentTypeID) ||
            Physics.Raycast(endPoint + Vector3.up * 2, Vector3.down, 2f, agentTypeID);

        public float lenght;

        public void UpdateLink()
        {
            if (!activated) return;
            if (startTransform == null || endTransform == null) activated = false;
            if (!autoUpdate) return;
            startPoint = startTransform.position;
            endPoint = endTransform.position;
        }
    }
}