using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UniformGridPathfinder.HPA;
using UniformGridPathfinder.Attribute;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UniformGridPathfinder.Navigation
{
    public class Map : MonoBehaviour
    {
        public List<Link> links=new List<Link>();
        public SurfaceBase[] surfaces;
        [ReadOnly] public bool baked;
        public LineRenderer linePath;
        public PlayerInput playerInput;
        public long Version {get;private set;}
        public long RegionBuildCount {get;private set;}
        private List<NavigationSnapshot.Transit> previousLinks;
        public int CachedRegionCount {get {lock(gate)return regions.Count;}}
        private readonly object gate=new object();
        private readonly Dictionary<SurfaceBase,NavigationSnapshot.Region> regions=new Dictionary<SurfaceBase,NavigationSnapshot.Region>();
#if UNITY_EDITOR
        public Vector3 startPosition=new Vector3(4.5f,0,-4.5f),endPosition=new Vector3(-10.5f,2,10.5f);
#endif
        protected virtual void Start(){StartBakingSurfaces();}
        protected virtual void LateUpdate()
        {
            lock(gate)foreach(var link in links)
            {
                if(link==null)continue;var start=link.startPoint;var end=link.endPoint;bool active=link.activated;
                link.UpdateLink();if(start!=link.startPoint||end!=link.endPoint||active!=link.activated)Version++;
            }
        }
        protected virtual void StartBakingSurfaces()
        {
            baked=false;foreach(var surface in surfaces)surface.Bake();
            foreach(var link in links)link?.UpdateLink();CreateSurfacesLinkNodes();baked=true;
        }
        protected virtual void CreateSurfacesLinkNodes(){lock(gate){regions.Clear();Version++;}}
        // Compatibility hook: local connectivity is now cached lazily per region, never injected into live cells.
        protected void BuildSurfacesConnections(){foreach(var surface in surfaces)MarkDirty(surface);}
        private sealed class UpdateScope : IDisposable
        {
            private object sync;
            public UpdateScope(object value){sync=value;Monitor.Enter(value);}
            public void Dispose(){var value=Interlocked.Exchange(ref sync,null);if(value!=null)Monitor.Exit(value);}
        }
        /// <summary>Batch a terrain/occupancy edit atomically with respect to snapshot capture. Dispose on the same thread.</summary>
        public IDisposable BeginUpdate()=>new UpdateScope(gate);
        public void MarkDirty(SurfaceBase surface){lock(gate){if(surface!=null)regions.Remove(surface);Version++;}}
        public void UpdateSurfacesLinkNodes()
        {
            foreach(var link in links)link?.UpdateLink();
            // Link updates do not invalidate unchanged local region routes.
            lock(gate)Version++;
        }
        public bool TryGetNode(Vector3 position,out Node node)
        {
            node=null;float distance=float.PositiveInfinity;
            foreach(var surface in surfaces)
                if(surface.TryGetNode(position,out var candidate)&&Mathf.Abs(candidate.pos.y-position.y)<distance)
                {distance=Mathf.Abs(candidate.pos.y-position.y);node=candidate;}
            return node!=null;
        }
        /// <summary>Capture on the Unity thread, then safely search this stable state on a worker.</summary>
        public NavigationSnapshot CaptureSnapshot()
        {
            lock(gate)
            {
                var captured=new List<NavigationSnapshot.Region>();
                foreach(var surface in surfaces)
                {
                    if(!regions.TryGetValue(surface,out var region)||!region.Matches(surface))
                    {region=new NavigationSnapshot.Region(surface);regions[surface]=region;RegionBuildCount++;Version++;}
                    captured.Add(region);
                }
                var transitions=new List<NavigationSnapshot.Transit>();
                foreach(var link in links)
                    if(link!=null&&link.activated)
                    {
                        if(float.IsNaN(link.costModifier)||float.IsInfinity(link.costModifier)||link.costModifier<0)
                            throw new InvalidOperationException("Link cost must be finite and non-negative.");
                        transitions.Add(new NavigationSnapshot.Transit(link.startPoint,link.endPoint,link.costModifier,
                            link.bidirectional,link.useDistanceCost,link.waypoints));
                    }
                bool changed=previousLinks==null||previousLinks.Count!=transitions.Count;
                if(!changed)for(int i=0;i<transitions.Count;i++)
                {
                    var a=previousLinks[i];var b=transitions[i];
                    if(a.Start!=b.Start||a.End!=b.End||a.Cost!=b.Cost||a.Both!=b.Both||!a.Points.SequenceEqual(b.Points)){changed=true;break;}
                }
                if(changed){previousLinks=transitions;Version++;}
                return new NavigationSnapshot(captured,transitions,Version);
            }
        }
        public PathResult QueryPath(Vector3 start,Vector3 goal,CancellationToken token=default)=>CaptureSnapshot().FindPath(start,goal,token);
        /// <summary>Legacy path includes the start marker expected by existing waypoint consumers.</summary>
        public LinkedList<Edge> GetPath(Vector3 start,Vector3 goal)=>QueryPath(start,goal).Path;
        public ReachabilityResult GetReachable(Vector3 start,float budget,CancellationToken token=default)=>CaptureSnapshot().Reachable(start,budget,token);
        [Obsolete("Queries no longer mutate the graph; use QueryPath or GetPath.")]
        public void InsertNodes(Vector3 start,Vector3 goal,out Node nStart,out Node nGoal)
        {
            var path=GetPath(start,goal);
            nStart=path.First?.Value.end;nGoal=path.Last?.Value.end;
            foreach(var edge in path)if(edge.start!=null)edge.start.edges.Add(edge);
        }
        [Obsolete("Queries no longer insert temporary nodes.")] public void RemoveAddedNodes(){}
    }
}
