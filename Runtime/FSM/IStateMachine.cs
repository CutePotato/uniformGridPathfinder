namespace HierarchicalJPS.FSM
{
    public interface IStateMachine
    {
        void SetState(IBaseState state);
    }
}