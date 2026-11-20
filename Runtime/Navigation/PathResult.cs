using System.Collections.Generic;
using UniformGridPathfinder.HPA;
using UnityEngine;
namespace UniformGridPathfinder.Navigation
{
    public enum PathStatus { Success,NoPath,InvalidStart,InvalidGoal,Cancelled }
    public sealed class PathResult
    {
        public PathStatus Status {get;internal set;}
        public float TotalCost {get;internal set;}
        public long Version {get;internal set;}
        public LinkedList<Edge> Path {get;internal set;}=new LinkedList<Edge>();
    }
    public sealed class ReachabilityResult
    {
        public long Version {get;internal set;}
        public Dictionary<Node,float> Costs {get;internal set;}=new Dictionary<Node,float>();
    }
}
