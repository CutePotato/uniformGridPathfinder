using System;using System.Collections.Generic;using System.Linq;using System.Threading;using System.Threading.Tasks;
using UnityEngine;using UniformGridPathfinder.Navigation;using UniformGridPathfinder.HPA;
class Program
{
 static int passed;static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
 static void Test(string name,Action test){test();passed++;Console.WriteLine("PASS "+name);}
 static Node[,] Grid(int width,int height,Random random=null)
 {
  var nodes=new Node[width,height];for(int x=0;x<width;x++)for(int z=0;z<height;z++)nodes[x,z]=new Node(new Vector3(x,0,z)){obstacle=random!=null&&random.NextDouble()<.25};
  for(int x=0;x<width;x++)for(int z=0;z<height;z++)for(int dx=-1;dx<=1;dx++)for(int dz=-1;dz<=1;dz++)
   if((dx!=0||dz!=0)&&x+dx>=0&&z+dz>=0&&x+dx<width&&z+dz<height)nodes[x,z].edges.Add(new Edge(nodes[x,z],nodes[x+dx,z+dz],EdgeType.INTER,dx!=0&&dz!=0?(float)Math.Sqrt(2):1));
  return nodes;
 }
 static float Oracle(Node[,] grid,int sx,int sz,int gx,int gz)
 {
  var distances=new Dictionary<Node,float>{{grid[sx,sz],0}};var open=new HashSet<Node>{grid[sx,sz]};
  while(open.Count>0)
  {
   var n=open.OrderBy(x=>distances[x]).First();open.Remove(n);int x=(int)n.pos.x,z=(int)n.pos.z;if(x==gx&&z==gz)return distances[n];
   for(int dx=-1;dx<=1;dx++)for(int dz=-1;dz<=1;dz++)
   {
    int xx=x+dx,zz=z+dz;if(dx==0&&dz==0||xx<0||zz<0||xx>=grid.GetLength(0)||zz>=grid.GetLength(1)||grid[xx,zz].obstacle)continue;
    if(dx!=0&&dz!=0&&(grid[x+dx,z].obstacle||grid[x,z+dz].obstacle))continue;
    var end=grid[xx,zz];float g=distances[n]+(dx!=0&&dz!=0?(float)Math.Sqrt(2):1);
    if(!distances.TryGetValue(end,out var best)||g<best){distances[end]=g;open.Add(end);}
   }
  }return float.PositiveInfinity;
 }
 static SurfaceBase Surface(float x,float y,float z,int width=6,int depth=6)
 {var s=new SurfaceBase{size=new Vector3(width,0,depth)};s.transform.position=new Vector3(x,y,z);s.Bake();return s;}
 static Node Cell(SurfaceBase s,int x,int z)=>s.Nodes.Values.First(n=>n.cellX==x&&n.cellZ==z);
 static Map Map(params SurfaceBase[] surfaces)=>new Map{surfaces=surfaces};
 static void Main()
 {
  Test("straight jumps retain full cost and origin",()=>{var g=Grid(10,1);var p=Pathfinder.FindPathJPS(g[0,0],g[9,0]);Check(p.Count==2&&p.Last.Value.start==g[0,0]&&Math.Abs(p.Sum(e=>e.weight)-9)<.001,"Bad macro edge");});
  Test("diagonal cannot cross blocked corner",()=>{var g=Grid(2,2);g[1,0].obstacle=g[0,1].obstacle=true;Check(Pathfinder.FindPathJPS(g[0,0],g[1,1]).Count==0,"Corner cut");});
  Test("5000 seeded JPS queries agree with independent Dijkstra",()=>
  {
   var random=new Random(1979);for(int i=0;i<5000;i++)
   {
    int w=random.Next(3,13),h=random.Next(3,13);var g=Grid(w,h,random);int sx=random.Next(w),sz=random.Next(h),gx=random.Next(w),gz=random.Next(h);
    g[sx,sz].obstacle=g[gx,gz].obstacle=false;float optimal=Oracle(g,sx,sz,gx,gz);var p=Pathfinder.FindPathJPS(g[sx,sz],g[gx,gz]);
    float cost=sx==gx&&sz==gz?0:p.Count==0?float.PositiveInfinity:p.Sum(e=>e.weight);
    Check(float.IsPositiveInfinity(cost)&&float.IsPositiveInfinity(optimal)||Math.Abs(cost-optimal)<.001,"JPS differs at case "+i+" cost="+cost+" oracle="+optimal);
    var previous=g[sx,sz];foreach(var edge in p){if(edge.start==null)continue;Check(edge.start==previous,"Broken edge chain");previous=edge.end;}
   }
  });
  Test("mixed-cost graph uses admissible zero heuristic",()=>{var s=new Node(new Vector3(0,0,0));var a=new Node(new Vector3(0,0,10));var g=new Node(new Vector3(10,0,0));s.edges.Add(new Edge(s,g,EdgeType.INTRA,5));s.edges.Add(new Edge(s,a,EdgeType.INTRA,1));a.edges.Add(new Edge(a,g,EdgeType.INTRA,1));Check(Pathfinder.FindPath(s,g).Sum(e=>e.weight)==2,"Suboptimal cheap link");});
  Test("reverse path reverses edge endpoints",()=>{var g=Grid(3,1);var p=Pathfinder.FindPath(g[0,0],g[2,0]);var reverse=Pathfinder.ReversePath(p);Check(reverse.First.Value.start==g[2,0]&&reverse.Last.Value.end==g[0,0],"Wrong reversal");});
  Test("long jumps are iterative and cancellable",()=>{var g=Grid(20000,1);Check(Pathfinder.FindPathJPS(g[0,0],g[19999,0]).Sum(e=>e.weight)==19999,"Long corridor");var cancel=new CancellationToken(true);bool threw=false;try{Pathfinder.FindPathJPS(g[0,0],g[19999,0],cancel);}catch(OperationCanceledException){threw=true;}Check(threw,"Cancellation ignored");});
  Test("fractional floor and overlapping layers remain distinct",()=>{var a=Surface(0,.25f,0);var b=Surface(0,2.25f,0);var m=Map(a,b);Check(m.QueryPath(Cell(a,0,0).pos,Cell(a,5,5).pos).Status==PathStatus.Success,"Fractional floor failed");Check(m.QueryPath(Cell(a,0,0).pos,Cell(b,0,0).pos).Status==PathStatus.NoPath,"Floors connected implicitly");});
  Test("directed cheap stairs preserve cost and waypoints",()=>{var a=Surface(0,0,0);var b=Surface(0,2,0);var m=Map(a,b);var start=Cell(a,2,2).pos;var end=Cell(b,2,2).pos;m.links.Add(new Link{activated=true,startPoint=start,endPoint=end,costModifier=.5f,bidirectional=false,waypoints=new[]{start+new Vector3(0,1,1)}});var p=m.QueryPath(start,end);Check(p.Status==PathStatus.Success&&Math.Abs(p.TotalCost-.5f)<.001&&p.Path.Any(e=>e.IsTransition)&&p.Path.Count==3,"Bad directed stair path");Check(m.QueryPath(end,start).Status==PathStatus.NoPath,"One-way reversed");});
  Test("moving obstacle and destroyed wall invalidate local paths",()=>{var s=Surface(0,0,0,8,1);var m=Map(s);var a=Cell(s,0,0).pos;var b=Cell(s,7,0).pos;Check(m.QueryPath(a,b).Status==PathStatus.Success,"Initial path");s.SetObstacle(Cell(s,4,0).pos,true);Check(m.QueryPath(a,b).Status==PathStatus.NoPath,"Stale obstacle cache");s.SetObstacle(Cell(s,4,0).pos,false);Check(m.QueryPath(a,b).Status==PathStatus.Success,"New shortcut not found");});
  Test("floor destruction removes support and disables links",()=>{var a=Surface(0,0,0,5,1);var b=Surface(0,2,0,5,1);var m=Map(a,b);m.links.Add(new Link{activated=true,startPoint=Cell(a,4,0).pos,endPoint=Cell(b,0,0).pos,costModifier=1});a.SetSupported(Cell(a,2,0).pos,false);Check(m.QueryPath(Cell(a,0,0).pos,Cell(b,4,0).pos).Status==PathStatus.NoPath,"Crossed missing floor");a.SetSupported(Cell(a,2,0).pos,true);Check(m.QueryPath(Cell(a,0,0).pos,Cell(b,4,0).pos).Status==PathStatus.Success,"Repaired support failed");b.SetSupported(Cell(b,0,0).pos,false);Check(m.QueryPath(Cell(a,0,0).pos,Cell(b,4,0).pos).Status==PathStatus.NoPath,"Link used unsupported endpoint");});
  Test("weighted cells use Astar and choose a detour",()=>{var s=Surface(0,0,0,5,3);var m=Map(s);s.SetTraversalCost(Cell(s,2,1).pos,20);var result=m.QueryPath(Cell(s,0,1).pos,Cell(s,4,1).pos);Check(result.Status==PathStatus.Success&&result.TotalCost<10,"Weighted route used expensive cell");});
  Test("bounded Dijkstra shares terrain and transition costs",()=>{var s=Surface(0,0,0,8,1);var m=Map(s);var r=m.GetReachable(Cell(s,0,0).pos,3);Check(r.Costs.Count==4&&r.Costs.Values.Max()==3,"Wrong range");});
  Test("invalid endpoints and cancellation have explicit status",()=>{var s=Surface(0,0,0);var m=Map(s);Check(m.QueryPath(new Vector3(100,0,100),Cell(s,0,0).pos).Status==PathStatus.InvalidStart,"Bad start");Check(m.QueryPath(Cell(s,0,0).pos,Cell(s,4,4).pos,new CancellationToken(true)).Status==PathStatus.Cancelled,"Bad cancellation status");});
  Test("captured snapshots survive later terrain changes",()=>{var s=Surface(0,0,0,5,1);var m=Map(s);var snapshot=m.CaptureSnapshot();s.SetObstacle(Cell(s,2,0).pos,true);Check(snapshot.FindPath(Cell(s,0,0).pos,Cell(s,4,0).pos).Status==PathStatus.Success,"Old snapshot changed");Check(m.QueryPath(Cell(s,0,0).pos,Cell(s,4,0).pos).Status==PathStatus.NoPath,"New snapshot stale");});
  Test("parallel captured queries leave source edges unchanged",()=>{var s=Surface(0,0,0,10,10);var m=Map(s);int edges=s.Nodes.Values.Sum(n=>n.edges.Count);var snapshot=m.CaptureSnapshot();Parallel.For(0,32,i=>Check(snapshot.FindPath(Cell(s,0,0).pos,Cell(s,9,9).pos).Status==PathStatus.Success,"Concurrent query failed"));Check(edges==s.Nodes.Values.Sum(n=>n.edges.Count),"Query mutated source");});
  Test("inactive and null links are safely ignored",()=>{var link=new Link{activated=true};link.UpdateLink();Check(!link.activated,"Null link remained enabled");});
  Test("only changed regions rebuild and changed links advance version",()=>
  {
   var a=Surface(0,0,0);var b=Surface(10,0,0);var m=Map(a,b);m.CaptureSnapshot();long builds=m.RegionBuildCount;
   a.SetObstacle(Cell(a,2,2).pos,true);m.CaptureSnapshot();Check(m.RegionBuildCount==builds+1,"Unchanged region rebuilt");
   long version=m.Version;m.links.Add(new Link{activated=true,startPoint=Cell(a,0,0).pos,endPoint=Cell(b,0,0).pos,costModifier=1});
   m.CaptureSnapshot();Check(m.Version>version,"Link addition did not advance version");
  });
  Test("occupied start can use a directed stair and same-cell query",()=>
  {
   var a=Surface(0,0,0);var b=Surface(0,2,0);var m=Map(a,b);var start=Cell(a,0,0);var goal=Cell(b,0,0);
   m.links.Add(new Link{activated=true,startPoint=start.pos,endPoint=goal.pos,costModifier=1,bidirectional=false});start.obstacle=true;
   Check(m.QueryPath(start.pos,start.pos).Status==PathStatus.Success,"Own occupied cell not resolved");
   Check(m.QueryPath(start.pos,goal.pos).Status==PathStatus.Success&&m.GetReachable(start.pos,1).Costs.ContainsKey(goal),"Own occupied portal blocked actor");
  });
  Test("route validation rejects newly blocked cells and deleted support",()=>
  {
   var s=Surface(0,0,0,8,1);var m=Map(s);var path=m.QueryPath(Cell(s,0,0).pos,Cell(s,7,0).pos).Path.Select(e=>e.end.pos).ToArray();
   Check(m.CaptureSnapshot().IsRouteValid(path),"Valid compressed route rejected");
   s.SetObstacle(Cell(s,4,0).pos,true);Check(!m.CaptureSnapshot().IsRouteValid(path),"New blocker ignored");
   s.SetObstacle(Cell(s,4,0).pos,false);s.SetSupported(Cell(s,3,0).pos,false);Check(!m.CaptureSnapshot().IsRouteValid(path),"Hole ignored");
  });
  Console.WriteLine("ALL "+passed+" TEST GROUPS PASSED");
 }
}
