using System;
using UnityEngine;
using UniformGridPathfinder.Attribute;
namespace UniformGridPathfinder.Navigation
{
    [Serializable] public class Link
    {
        public bool activated;
        public int agentTypeID;
        public Transform startTransform,endTransform;
        [ReadOnly] public Vector3 startPoint,endPoint;
        [Min(0)] public float costModifier=1;
        public bool bidirectional=true;
        public bool useDistanceCost;
        public Vector3[] waypoints=Array.Empty<Vector3>();
        public float lenght;
        public bool Occupied=>Physics.Raycast(startPoint+Vector3.up*2,Vector3.down,2,agentTypeID)||Physics.Raycast(endPoint+Vector3.up*2,Vector3.down,2,agentTypeID);
        public void UpdateLink()
        {
            if(!activated)return;
            if(startTransform==null||endTransform==null){activated=false;return;}
            startPoint=startTransform.position;endPoint=endTransform.position;
        }
    }
}
