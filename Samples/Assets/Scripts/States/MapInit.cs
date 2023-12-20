using FSM;
using HierarchicalJPS.Navigation;

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

        public override void Update()
        {
            if(_map.baked) machine.SetState(((GameStateManager)machine).playerTurn);
        }
    }
}
