using System;
using System.Collections.Generic;
using UniformGridPathfinder.HPA;
using UniformGridPathfinder.Attribute;
using UnityEngine;

namespace UniformGridPathfinder.Navigation
{
    public class SurfaceBase : MonoBehaviour
    {
        protected static readonly float Sqrt2=Mathf.Sqrt(2);
        protected static readonly Vector3 Offset=new Vector3(.5f,0,.5f);
        public Data data;
        public Vector3 size;
        [Min(.01f)] public float cellSize=1;
        [SerializeField] private string surfaceId;
        public string SurfaceId=>surfaceId;
        public long Revision {get;private set;}
        public Dictionary<Vector3,Node> Nodes=new Dictionary<Vector3,Node>();
        public Dictionary<Vector3,Node> LinkedNodes=new Dictionary<Vector3,Node>();
        [ReadOnly] public bool baked;
        protected Map _map;
        private readonly Dictionary<string,bool> support=new Dictionary<string,bool>();
        private readonly Dictionary<string,float> costs=new Dictionary<string,float>();
        protected virtual void Awake()
        {
            if(string.IsNullOrEmpty(surfaceId))surfaceId=Guid.NewGuid().ToString("N");
            data=new Data(transform.position,size);_map=GetComponentInParent<Map>();
        }
        public virtual void Bake(){baked=false;data=new Data(transform.position,size);CreateNodes();Revision++;baked=true;}
        protected void CreateNodes()
        {
            if(cellSize<=0||float.IsNaN(cellSize)||float.IsInfinity(cellSize))throw new InvalidOperationException("Cell size must be finite and positive.");
            Nodes=new Dictionary<Vector3,Node>();LinkedNodes.Clear();var min=data.Bounds.min;
            int width=Mathf.FloorToInt((size.x+.0001f)/cellSize),depth=Mathf.FloorToInt((size.z+.0001f)/cellSize);
            for(int x=0;x<width;x++)for(int z=0;z<depth;z++)
            {
                var p=new Vector3(min.x+(x+.5f)*cellSize,data.Position.y,min.z+(z+.5f)*cellSize);
                var n=new Node(p,this){cellX=x,cellZ=z};string key=Key(x,z);
                if(support.TryGetValue(key,out bool supported))n.supported=supported;
                if(costs.TryGetValue(key,out float cost))n.traversalCost=cost;
                Nodes.Add(p,n);
            }
            foreach(var n in Nodes.Values)for(int x=-1;x<=1;x++)for(int z=-1;z<=1;z++)
            {
                if(x==0&&z==0)continue;
                var p=n.pos+new Vector3(x*cellSize,0,z*cellSize);
                if(TryGetNode(p,out var end))n.edges.Add(new Edge(n,end,EdgeType.INTER,cellSize*(x!=0&&z!=0?Sqrt2:1)));
            }
        }
        private static string Key(int x,int z)=>x+":"+z;
        public bool TryGetNode(Vector3 world,out Node node)
        {
            node=null;if(Mathf.Abs(world.y-data.Position.y)>.35f||cellSize<=0)return false;
            var min=data.Bounds.min;int x=Mathf.FloorToInt((world.x-min.x)/cellSize),z=Mathf.FloorToInt((world.z-min.z)/cellSize);
            var p=new Vector3(min.x+(x+.5f)*cellSize,data.Position.y,min.z+(z+.5f)*cellSize);
            return Nodes.TryGetValue(p,out node);
        }
        public bool IsFreeTile(Vector3 world)=>TryGetNode(world,out var n)&&n.supported&&!n.obstacle;
        public IDisposable BeginUpdate()=>_map?.BeginUpdate();
        public bool SetSupported(Vector3 world,bool value)
        {
            using(var update=BeginUpdate())
            {
            if(!TryGetNode(world,out var n)||n.supported==value)return false;
            n.supported=value;support[Key(n.cellX,n.cellZ)]=value;MarkChanged();return true;
            }
        }
        public bool SetObstacle(Vector3 world,bool value)
        {
            using(var update=BeginUpdate())
            {
            if(!TryGetNode(world,out var n)||n.obstacle==value)return false;
            n.obstacle=value;MarkChanged();return true;
            }
        }
        public bool SetTraversalCost(Vector3 world,float value)
        {
            using(var update=BeginUpdate())
            {
            if(value<1||float.IsNaN(value)||float.IsInfinity(value))throw new ArgumentOutOfRangeException(nameof(value),"Cell traversal multiplier must be finite and >= 1.");
            if(!TryGetNode(world,out var n)||n.traversalCost==value)return false;
            n.traversalCost=value;costs[Key(n.cellX,n.cellZ)]=value;MarkChanged();return true;
            }
        }
        public void MarkChanged(){Revision++;_map?.MarkDirty(this);}
    }
}
