using System.Collections.Generic;
using UniformGridPathfinder.Navigation;
using UnityEngine;
namespace UniformGridPathfinder.HPA
{
    public static class HierarchicalPathfinder
    {
        public static LinkedList<Edge> FindHierarchicalPath(Map graph,Vector3 start,Vector3 dest)=>graph.GetPath(start,dest);
        public static LinkedList<Edge> GetLayerPathFromHPA(LinkedList<Edge> path)
        {
            var result=new LinkedList<Edge>();
            foreach(var edge in path)
                if(edge.UnderlyingPath!=null)foreach(var child in GetLayerPathFromHPA(edge.UnderlyingPath))result.AddLast(child);
                else result.AddLast(edge);
            return result;
        }
    }
}
