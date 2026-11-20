using System;
using System.Collections.Generic;
using System.Threading;
using Priority_Queue;
using UniformGridPathfinder.JPS;
using UnityEngine;

namespace UniformGridPathfinder.HPA
{
    /// <summary>Search state is query-local. Grid movement forbids corner cutting.</summary>
    public static class Pathfinder
    {
        public static List<Direction> cardinals = new List<Direction> { Direction.North, Direction.East, Direction.South, Direction.West };
        private sealed class Entry { public Node Node; public float G; public Entry(Node n,float g) { Node=n; G=g; } }
        public static float EuclidianDistance(Node a,Node b) => EuclidianDistance(a.pos,b.pos);
        public static float EuclidianDistance(Vector3 a,Vector3 b) { var d=b-a; return Mathf.Sqrt(d.x*d.x+d.y*d.y+d.z*d.z); }
        public static bool IsCardinal(Direction d) { int n=(int)d; return n!=0 && (n&(n-1))==0; }
        public static Direction[] SplitDiagonalDirection(Direction d)
        {
            int n=(int)d,first=n&-n; return new[] { (Direction)first,(Direction)(n-first) };
        }
        public static Edge Step(Edge edge,Direction direction) => edge?.end?.edges.Find(e=>!e.IsTransition && e.direction==direction);
        public static bool Walkable(Node n) => n!=null && n.supported && !n.obstacle;
        private static bool Finite(float n) => !float.IsNaN(n) && !float.IsInfinity(n);
        private static Node Raw(Node n,int dx,int dz)
        {
            if(n==null)return null;
            foreach(var e in n.edges)
            {
                if(e.IsTransition || e.type!=EdgeType.INTER || e.end.surface!=n.surface || Math.Abs(e.end.pos.y-n.pos.y)>.0001f)continue;
                var delta=e.end.pos-n.pos;
                if(Math.Sign(delta.x)==dx && Math.Sign(delta.z)==dz)return e.end;
            }
            return null;
        }
        private static bool Open(Node n,int dx,int dz)=>Walkable(Raw(n,dx,dz));
        public static bool CanTraverse(Edge e)
        {
            if(e==null || !Walkable(e.end) || e.start!=null&&!e.start.supported)return false;
            if(!Finite(e.weight) || e.weight<0)throw new ArgumentOutOfRangeException(nameof(e.weight),"Navigation costs must be finite and non-negative.");
            if(e.IsTransition || e.type==EdgeType.INTRA || e.start==null)return true;
            var d=e.end.pos-e.start.pos;int dx=Math.Sign(d.x),dz=Math.Sign(d.z);
            return dx==0 || dz==0 || Open(e.start,dx,0)&&Open(e.start,0,dz);
        }
        private static LinkedList<Edge> Rebuild(Node start,Node goal,Dictionary<Node,Edge> parents)
        {
            var result=new LinkedList<Edge>();var seen=new HashSet<Node>();var current=goal;
            while(current!=start)
            {
                if(!seen.Add(current) || !parents.TryGetValue(current,out var edge))throw new InvalidOperationException("Invalid predecessor chain.");
                result.AddFirst(edge);current=edge.start;
            }
            return result;
        }
        public static LinkedList<Edge> FindPath(Node start,Node goal) => FindPath(start,goal,CancellationToken.None);
        public static LinkedList<Edge> FindPath(Node start,Node goal,CancellationToken token,Func<Node,IEnumerable<Edge>> neighbours=null)
        {
            if(start==null || goal==null || !start.supported || !Walkable(goal))return new LinkedList<Edge>();
            // h=0 is intentional: transit costs may be lower than their geometric distance.
            var costs=new Dictionary<Node,float>{{start,0}};var parents=new Dictionary<Node,Edge>();var queue=new SimplePriorityQueue<Entry,float>();
            queue.Enqueue(new Entry(start,0),0);
            while(queue.Count>0)
            {
                token.ThrowIfCancellationRequested();var entry=queue.Dequeue();var n=entry.Node;
                if(entry.G>costs[n])continue;
                if(n==goal)return Rebuild(start,goal,parents);
                foreach(var edge in neighbours!=null?neighbours(n):n.edges)
                {
                    if(!CanTraverse(edge))continue;float g=entry.G+edge.weight;
                    if(costs.TryGetValue(edge.end,out float best)&&g>=best)continue;
                    costs[edge.end]=g;parents[edge.end]=edge;queue.Enqueue(new Entry(edge.end,g),g);
                }
            }
            return new LinkedList<Edge>();
        }
        public static Dictionary<Node,float> FindReachable(Node start,float budget,CancellationToken token=default,Func<Node,IEnumerable<Edge>> neighbours=null)
        {
            var costs=new Dictionary<Node,float>();if(start==null||!start.supported||budget<0)return costs;
            var queue=new SimplePriorityQueue<Entry,float>();costs[start]=0;queue.Enqueue(new Entry(start,0),0);
            while(queue.Count>0)
            {
                token.ThrowIfCancellationRequested();var entry=queue.Dequeue();if(entry.G>costs[entry.Node])continue;
                foreach(var edge in neighbours!=null?neighbours(entry.Node):entry.Node.edges)
                {
                    if(!CanTraverse(edge))continue;float g=entry.G+edge.weight;
                    if(g>budget+.00001f || costs.TryGetValue(edge.end,out float best)&&g>=best)continue;
                    costs[edge.end]=g;queue.Enqueue(new Entry(edge.end,g),g);
                }
            }
            return costs;
        }
        private static Node Next(Node n,int dx,int dz)
        {
            var next=Raw(n,dx,dz);return Walkable(next)&&(dx==0||dz==0||Open(n,dx,0)&&Open(n,0,dz))?next:null;
        }
        private static Node Jump(Node origin,int dx,int dz,Node goal,CancellationToken token)
        {
            var n=Next(origin,dx,dz);
            while(n!=null)
            {
                token.ThrowIfCancellationRequested();if(n==goal)return n;
                if(dx!=0&&dz!=0)
                { if(Jump(n,dx,0,goal,token)!=null||Jump(n,0,dz,goal,token)!=null)return n; }
                else if(dx!=0)
                { if(Open(n,0,-1)&&!Open(n,-dx,-1)||Open(n,0,1)&&!Open(n,-dx,1))return n; }
                else
                { if(Open(n,-1,0)&&!Open(n,-1,-dz)||Open(n,1,0)&&!Open(n,1,-dz))return n; }
                n=Next(n,dx,dz);
            }
            return null;
        }
        private static IEnumerable<Node> Pruned(Node n,Node parent)
        {
            if(parent==null)
            { for(int x=-1;x<=1;x++)for(int z=-1;z<=1;z++)if(x!=0||z!=0){var p=Next(n,x,z);if(p!=null)yield return p;}yield break; }
            int dx=Math.Sign(n.pos.x-parent.pos.x),dz=Math.Sign(n.pos.z-parent.pos.z);
            if(dx!=0&&dz!=0)
            {
                var a=Next(n,dx,0);if(a!=null)yield return a;
                var b=Next(n,0,dz);if(b!=null)yield return b;
                var c=Next(n,dx,dz);if(c!=null)yield return c;
            }
            else
            {
                var forward=Next(n,dx,dz);if(forward!=null)yield return forward;
                for(int sign=-1;sign<=1;sign+=2)
                {
                    int sx=dx==0?sign:0,sz=dz==0?sign:0;
                    var side=Next(n,sx,sz);if(side!=null)yield return side;
                    if(forward!=null){var diagonal=Next(n,dx+sx,dz+sz);if(diagonal!=null)yield return diagonal;}
                }
            }
        }
        /// <summary>Online JPS for regular, horizontal, uniform-distance grids. Use FindPath for weighted/transit graphs.</summary>
        public static LinkedList<Edge> FindPathJPS(Node start,Node goal) => FindPathJPS(start,goal,CancellationToken.None);
        public static LinkedList<Edge> FindPathJPS(Node start,Node goal,CancellationToken token)
        {
            var path=FindGridPath(start,goal,token);
            if(start!=null&&goal!=null&&(start==goal||path.Count>0))path.AddFirst(new Edge(null,start,EdgeType.INTRA,0));
            return path;
        }
        internal static LinkedList<Edge> FindGridPath(Node start,Node goal,CancellationToken token)
        {
            if(start==null||goal==null||!start.supported||!Walkable(goal))return new LinkedList<Edge>();
            var costs=new Dictionary<Node,float>{{start,0}};var parents=new Dictionary<Node,Edge>();var queue=new SimplePriorityQueue<Entry,float>();
            queue.Enqueue(new Entry(start,0),EuclidianDistance(start,goal));
            while(queue.Count>0)
            {
                token.ThrowIfCancellationRequested();var entry=queue.Dequeue();var n=entry.Node;
                if(entry.G>costs[n])continue;if(n==goal)return Rebuild(start,goal,parents);
                parents.TryGetValue(n,out var predecessor);
                foreach(var neighbour in Pruned(n,predecessor?.start))
                {
                    int dx=Math.Sign(neighbour.pos.x-n.pos.x),dz=Math.Sign(neighbour.pos.z-n.pos.z);
                    var jump=Jump(n,dx,dz,goal,token);if(jump==null)continue;
                    float distance=EuclidianDistance(n,jump),g=entry.G+distance;
                    if(costs.TryGetValue(jump,out float best)&&g>=best)continue;
                    costs[jump]=g;parents[jump]=new Edge(n,jump,EdgeType.INTRA,distance);
                    queue.Enqueue(new Entry(jump,g),g+EuclidianDistance(jump,goal));
                }
            }
            return new LinkedList<Edge>();
        }
        public static LinkedList<Edge> ReversePath(IEnumerable<Edge> path)
        {
            var result=new LinkedList<Edge>();foreach(var e in path)
            { if(e.start!=null)result.AddFirst(new Edge(e.end,e.start,e.type,e.weight){IsTransition=e.IsTransition}); }return result;
        }
    }
}
