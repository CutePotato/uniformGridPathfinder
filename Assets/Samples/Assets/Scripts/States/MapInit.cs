﻿using HierarchicalJPS.Samples.Assets.Scripts.FSM;
using HierarchicalJPS.Samples.Assets.Scripts.Navigation;

namespace HierarchicalJPS.Samples.Assets.Scripts.States
{
    public class MapInit : BaseState
    {
        private MapManager _map;
        
        public MapInit(IStateMachine machine) : base("Map Init", machine)
        {
            _map = MapManager.Instance;
        }

        public override void Enter()
        {
            
        }

        public override void Exit()
        {
            
        }

        public override void FixedUpdate()
        {
            
        }

        public override void Update(float deltaTime)
        {
            if(_map.baked) machine.SetState(((GameStateManager)machine).playerTurn);
        }
    }
}
