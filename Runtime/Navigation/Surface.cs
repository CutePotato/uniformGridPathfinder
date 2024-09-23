using System.Collections.Generic;
using System.Linq;
using HierarchicalJPS.Attribute;
using HierarchicalJPS.HPA;
using UnityEngine;

namespace HierarchicalJPS.Navigation
{
    public class Surface : SurfaceBase
    {
        public bool ignoreObstacle;
        public List<Transform> obstacles = new();
        private Vector3[] _initObstacles;

        protected override void Awake()
        {
            base.Awake();
			InitObstacles();
        }

        public override void Bake()
        {
            baked = false;
            
            CreateNodes();
            if (!ignoreObstacle)
            {
                SetObstacles();
            }

            baked = true;
        }

        private void InitObstacles()
        {
            _initObstacles = obstacles.Select(o => o.position).ToArray();
        }

        private void SetObstacles()
        {
            foreach (var obstacle in _initObstacles)
            {
                if(!Nodes.TryGetValue(obstacle, out var node)) continue;
                node.obstacle = true;
            }
        }

        public void UpdateObstacles()
        {
            foreach (var obstacle in _initObstacles)
            {
                if(!Nodes.TryGetValue(obstacle, out var node)) continue;
                node.obstacle = false;
            }
            InitObstacles();
            SetObstacles();
            _map.UpdateSurfacesLinkNodes();
        }
    }
}
