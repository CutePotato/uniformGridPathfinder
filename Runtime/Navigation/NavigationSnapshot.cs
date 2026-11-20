using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using UniformGridPathfinder.HPA;
using UnityEngine;
namespace UniformGridPathfinder.Navigation
{
    /// <summary>Query-local graphs over immutable captured cells. Published paths contain detached output nodes.</summary>
    public sealed class NavigationSnapshot
    {
        internal sealed class Region
        {
            internal readonly SurfaceBase Surface;
            internal readonly Dictionary<Vector3,Node> Cells=new Dictionary<Vector3,Node>();
            internal readonly Dictionary<Node,Node> Sources=new Dictionary<Node,Node>();
            internal readonly bool Uniform;
            internal readonly Vector3 Minimum; internal readonly float Height,CellSize;
            private readonly long revision;
            private readonly ConcurrentDictionary<Tuple<Node,Node>,LinkedList<Edge>> routes=new ConcurrentDictionary<Tuple<Node,Node>,LinkedList<Edge>>();
            internal Region(SurfaceBase surface)
            {
                Surface=surface;revision=surface.Revision;Uniform=true;Minimum=surface.data.Bounds.min;Height=surface.data.Position.y;CellSize=surface.cellSize;
                var clones=new Dictionary<Node,Node>();
                foreach(var source in surface.Nodes.Values)
                {
                    var n=new Node(source.pos,surface){obstacle=source.obstacle,supported=source.supported,traversalCost=source.traversalCost,cellX=source.cellX,cellZ=source.cellZ};
                    if(n.traversalCost!=1)Uniform=false;
                    Cells[n.pos]=n;clones[source]=n;Sources[n]=source;
                }
                foreach(var pair in clones)foreach(var edge in pair.Key.edges)
                    if(!edge.IsTransition&&edge.type==EdgeType.INTER&&clones.TryGetValue(edge.end,out var end))
                        {
                        pair.Value.edges.Add(new Edge(pair.Value,end,EdgeType.INTER,edge.weight*end.traversalCost));
                        if(Math.Abs(edge.weight-Pathfinder.EuclidianDistance(pair.Value,end))>.0001f)Uniform=false;
                    }
            }
            internal bool Matches(SurfaceBase surface)
            {
                if(revision!=surface.Revision||Cells.Count!=surface.Nodes.Count)return false;
                foreach(var pair in Cells)
                    if(!surface.Nodes.TryGetValue(pair.Key,out var live)||pair.Value.obstacle!=live.obstacle||pair.Value.supported!=live.supported||pair.Value.traversalCost!=live.traversalCost)return false;
                return true;
            }
            internal LinkedList<Edge> Local(Node a,Node b,CancellationToken token,bool cache)
            {
                if(a==b)return new LinkedList<Edge>();
                if(!Pathfinder.Walkable(a)||!Pathfinder.Walkable(b))return new LinkedList<Edge>();
                var key=Tuple.Create(a,b);
                if(cache&&routes.TryGetValue(key,out var found))return found;
                var path=Uniform?Pathfinder.FindGridPath(a,b,token):Pathfinder.FindPath(a,b,token);
                if(cache)routes.TryAdd(key,path);return path;
            }
        }
        internal sealed class Transit
        {
            internal readonly Vector3 Start,End;internal readonly float Cost;internal readonly bool Both;
            internal readonly Vector3[] Points;
            internal Transit(Vector3 start,Vector3 end,float cost,bool both,bool distance,Vector3[] middle)
            {
                Start=start;End=end;Both=both;
                var points=new List<Vector3>{start};if(middle!=null)points.AddRange(middle);points.Add(end);Points=points.ToArray();
                float length=0;for(int i=1;i<Points.Length;i++)length+=Pathfinder.EuclidianDistance(Points[i-1],Points[i]);
                Cost=distance?length*cost:cost;
            }
        }
        private readonly List<Region> regions;private readonly List<Transit> links;
        public long Version {get;}
        internal NavigationSnapshot(List<Region> regions,List<Transit> links,long version){this.regions=regions;this.links=links;Version=version;}
        private Node Resolve(Vector3 world,out Region region)
        {
            Node best=null;region=null;float height=.35001f;
            foreach(var candidate in regions)
            {
                // Snap X/Z relative to each lattice; never round away the floor height.
                var min=candidate.Minimum;float cell=candidate.CellSize;
                var p=new Vector3(min.x+(Mathf.FloorToInt((world.x-min.x)/cell)+.5f)*cell,candidate.Height,
                    min.z+(Mathf.FloorToInt((world.z-min.z)/cell)+.5f)*cell);
                if(Mathf.Abs(p.y-world.y)<height&&candidate.Cells.TryGetValue(p,out var node))
                {best=node;region=candidate;height=Mathf.Abs(p.y-world.y);}
            }
            return best;
        }
        private static List<Edge> TransitPath(Node start,Node end,Transit transit,bool reverse)
        {
            var points=reverse?transit.Points.Reverse().ToArray():transit.Points;
            float length=0;for(int i=1;i<points.Length;i++)length+=Pathfinder.EuclidianDistance(points[i-1],points[i]);
            var edges=new List<Edge>();var previous=start;
            for(int i=1;i<points.Length;i++)
            {
                var next=i==points.Length-1?end:new Node(points[i],start.surface);
                float cost=length>0?transit.Cost*Pathfinder.EuclidianDistance(points[i-1],points[i])/length:0;
                edges.Add(new Edge(previous,next,EdgeType.INTRA,cost){IsTransition=true});previous=next;
            }
            return edges;
        }
        private IEnumerable<Tuple<Node,Node,Transit,bool>> ValidLinks(Node occupiedStart=null)
        {
            foreach(var link in links)
            {
                var start=Resolve(link.Start,out _);var end=Resolve(link.End,out _);
                if(start==null||end==null||!start.supported||!end.supported||(start.obstacle&&start!=occupiedStart)||(end.obstacle&&end!=occupiedStart))continue;
                yield return Tuple.Create(start,end,link,false);
                if(link.Both)yield return Tuple.Create(end,start,link,true);
            }
        }
        private static LinkedList<Edge> Output(Node start,IEnumerable<Edge> path)
        {
            var result=new LinkedList<Edge>();var nodes=new Dictionary<Node,Node>();
            Node Copy(Node n){if(n==null)return null;if(!nodes.TryGetValue(n,out var copy)){copy=new Node(n.pos,n.surface){supported=n.supported,cellX=n.cellX,cellZ=n.cellZ};nodes[n]=copy;}return copy;}
            result.AddLast(new Edge(null,Copy(start),EdgeType.INTRA,0));
            foreach(var edge in path)result.AddLast(new Edge(Copy(edge.start),Copy(edge.end),edge.type,edge.weight){IsTransition=edge.IsTransition});
            return result;
        }
        public PathResult FindPath(Vector3 startWorld,Vector3 goalWorld,CancellationToken token=default)
        {
            var result=new PathResult{Version=Version};
            try
            {
                token.ThrowIfCancellationRequested();var start=Resolve(startWorld,out var startRegion);var goal=Resolve(goalWorld,out var goalRegion);
                if(start==null||!start.supported){result.Status=PathStatus.InvalidStart;return result;}
                if(start==goal){result.Status=PathStatus.Success;result.Path=Output(start,Array.Empty<Edge>());return result;}
                if(!Pathfinder.Walkable(goal)){result.Status=PathStatus.InvalidGoal;return result;}
                // An occupied start is allowed for legacy actor queries, but cannot be used as a blocked portal elsewhere.
                var localStart=start;
                if(start.obstacle)
                {
                    var clone=new Node(start.pos,start.surface){cellX=start.cellX,cellZ=start.cellZ};clone.edges.AddRange(start.edges);localStart=clone;
                }
                var portals=new Dictionary<Node,Node>();var byRegion=new Dictionary<Region,List<Node>>();
                foreach(var region in regions)byRegion[region]=new List<Node>();
                Node Portal(Node cell)
                {
                    if(!portals.TryGetValue(cell,out var p))
                    {p=new Node(cell.pos,cell.surface){child=cell};portals[cell]=p;var region=regions.Find(r=>r.Surface==cell.surface);byRegion[region].Add(cell);}
                    return p;
                }
                var transitions=ValidLinks(start).Select(t=>Tuple.Create(t.Item1==start?localStart:t.Item1,t.Item2==start?localStart:t.Item2,t.Item3,t.Item4)).ToArray();
                foreach(var transit in transitions)
                {
                    var a=Portal(transit.Item1);var b=Portal(transit.Item2);
                    a.edges.Add(new Edge(a,b,EdgeType.INTRA,new LinkedList<Edge>(TransitPath(transit.Item1,transit.Item2,transit.Item3,transit.Item4))){weight=transit.Item3.Cost,IsTransition=true});
                }
                var first=Portal(localStart);var last=Portal(goal);
                foreach(var region in regions)
                {
                    var cells=byRegion[region];
                    for(int i=0;i<cells.Count;i++)for(int j=0;j<cells.Count;j++)
                    {
                        if(i==j)continue;token.ThrowIfCancellationRequested();var a=cells[i];var b=cells[j];
                        bool cache=a!=localStart&&b!=localStart&&a!=goal&&b!=goal;
                        LinkedList<Edge> path;
                        if(a==localStart&&start.obstacle)
                            path=region.Uniform?Pathfinder.FindGridPath(a,b,token):Pathfinder.FindPath(a,b,token);
                        else path=region.Local(a,b,token,cache);
                        if(path.Count==0)continue;
                        portals[a].edges.Add(new Edge(portals[a],portals[b],EdgeType.INTRA,path){weight=path.Sum(e=>e.weight)});
                    }
                }
                var macro=Pathfinder.FindPath(first,last,token);
                if(macro.Count==0){result.Status=PathStatus.NoPath;return result;}
                var expanded=new List<Edge>();foreach(var edge in macro)expanded.AddRange(edge.UnderlyingPath);
                result.Path=Output(localStart,expanded);result.TotalCost=expanded.Sum(e=>e.weight);result.Status=PathStatus.Success;return result;
            }
            catch(OperationCanceledException){result.Status=PathStatus.Cancelled;return result;}
        }
        /// <summary>Revalidate remaining compressed segments against current support, blockers and directed transit routes.</summary>
        public bool IsRouteValid(IReadOnlyList<Vector3> points,int firstSegment=0)
        {
            if(points==null||points.Count==0)return false;
            var occupiedStart=Resolve(points[0],out _);
            for(int i=Math.Max(1,firstSegment+1);i<points.Count;i++)
            {
                var a=Resolve(points[i-1],out var regionA);var b=Resolve(points[i],out var regionB);
                if(a!=null&&b!=null&&regionA==regionB&&Math.Abs(points[i].y-points[i-1].y)<.001f)
                {
                    if(!a.supported||!Pathfinder.Walkable(b))return false;
                    int dx=Math.Sign(b.pos.x-a.pos.x),dz=Math.Sign(b.pos.z-a.pos.z);
                    float x=Math.Abs(b.pos.x-a.pos.x),z=Math.Abs(b.pos.z-a.pos.z);
                    if(dx!=0&&dz!=0&&Math.Abs(x-z)>.001f)return false;
                    var current=a;int remaining=regionA.Cells.Count;
                    while(current!=b&&remaining-->0)
                    {
                        var step=current.edges.Find(e=>!e.IsTransition&&Math.Sign(e.end.pos.x-current.pos.x)==dx&&Math.Sign(e.end.pos.z-current.pos.z)==dz);
                        if(step==null||!Pathfinder.CanTraverse(step))return false;current=step.end;
                    }
                    if(current!=b)return false;
                }
                else
                {
                    bool found=false;
                    foreach(var link in ValidLinks(occupiedStart))
                    {
                        var route=link.Item4?link.Item3.Points.Reverse().ToArray():link.Item3.Points;
                        for(int j=1;j<route.Length;j++)if(route[j-1]==points[i-1]&&route[j]==points[i]){found=true;break;}
                        if(found)break;
                    }
                    if(!found)return false;
                }
            }
            return true;
        }
        public ReachabilityResult Reachable(Vector3 origin,float budget,CancellationToken token=default)
        {
            token.ThrowIfCancellationRequested();var result=new ReachabilityResult{Version=Version};var start=Resolve(origin,out _);
            if(start==null||!start.supported)return result;
            var overlay=new Dictionary<Node,List<Edge>>();
            foreach(var link in ValidLinks(start))
            {
                if(!overlay.TryGetValue(link.Item1,out var edges))overlay[link.Item1]=edges=new List<Edge>();
                edges.Add(new Edge(link.Item1,link.Item2,EdgeType.INTRA,link.Item3.Cost){IsTransition=true});
            }
            IEnumerable<Edge> Neighbours(Node n)
            {foreach(var edge in n.edges)yield return edge;if(overlay.TryGetValue(n,out var edges))foreach(var edge in edges)yield return edge;}
            var costs=Pathfinder.FindReachable(start,budget,token,Neighbours);
            foreach(var pair in costs)
            {var region=regions.Find(r=>r.Surface==pair.Key.surface);result.Costs[region.Sources[pair.Key]]=pair.Value;}
            return result;
        }
    }
}
