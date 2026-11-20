using System.Collections.Generic;
using UnityEngine;
namespace UniformGridPathfinder.Navigation
{
    public class Surface : SurfaceBase
    {
        public bool ignoreObstacle;
        public bool automaticObstacleUpdates=true;
        public List<Transform> obstacles=new List<Transform>();
        private readonly List<Vector3> previous=new List<Vector3>();
        protected override void Awake(){base.Awake();}
        public override void Bake(){base.Bake();UpdateObstacles();}
        protected virtual void LateUpdate()
        {
            if(!baked||!automaticObstacleUpdates)return;
            int count=0;bool changed=false;
            foreach(var obstacle in obstacles)
            {if(obstacle==null)continue;if(count>=previous.Count||previous[count]!=obstacle.position)changed=true;count++;}
            if(changed||count!=previous.Count)UpdateObstacles();
        }
        public void UpdateObstacles()
        {
            using(var update=BeginUpdate())
            {
            foreach(var position in previous)if(TryGetNode(position,out var n))n.obstacle=false;
            previous.Clear();
            if(!ignoreObstacle)foreach(var obstacle in obstacles)
            {
                if(obstacle==null)continue;previous.Add(obstacle.position);
                if(TryGetNode(obstacle.position,out var n))n.obstacle=true;
            }
            MarkChanged();
            }
        }
    }
}
