namespace UniformGridPathfinder.FSM
{
    public interface IBaseState
    {
        void Enter();
        void Update(float deltaTime);
        void FixedUpdate();
        void Exit();
    }
}