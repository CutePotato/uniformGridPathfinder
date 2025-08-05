using System;
using HierarchicalJPS.Attribute;
using UnityEngine;

namespace HierarchicalJPS.Navigation
{
    [Serializable]
    public struct Link
    {
        public bool activated;
        public int agentTypeID;
        public Transform startTransform;
        [ReadOnly] public Vector3 startPoint;
        public Transform endTransform;
        [ReadOnly] public Vector3 endPoint;
        public float costModifier;


        public bool Occupied =>
            Physics.Raycast(startPoint + Vector3.up * 2, Vector3.down, 2f, agentTypeID) ||
            Physics.Raycast(endPoint + Vector3.up * 2, Vector3.down, 2f, agentTypeID);

        public float lenght;

        public void UpdateLink()
        {
            if (!activated) return;
            if (startTransform == null || endTransform == null) activated = false;
            startPoint = startTransform.position;
            endPoint = endTransform.position;
        }
    }
}