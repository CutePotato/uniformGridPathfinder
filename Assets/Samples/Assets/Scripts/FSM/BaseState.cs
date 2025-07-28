namespace HierarchicalJPS.Samples.Assets.Scripts.FSM
{
    public abstract class BaseState : IBaseState
    {
        protected string name;
        protected IStateMachine machine;

        protected BaseState(string name, IStateMachine machine)
        {
            this.name = name;
            this.machine = machine;
        }

        public abstract void Enter();

        public abstract void Exit();

        public abstract void FixedUpdate();

        public abstract void Update(float deltaTime);
    }
}