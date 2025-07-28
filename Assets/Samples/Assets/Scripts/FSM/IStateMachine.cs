namespace HierarchicalJPS.Samples.Assets.Scripts.FSM
{
    public interface IStateMachine
    {
        void SetState(IBaseState state);
    }
}