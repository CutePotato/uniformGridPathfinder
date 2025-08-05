using UniformGridPathfinder.Utilities;
using UnityEngine;

namespace UniformGridPathfinder.FSM
{
    public abstract class AbstractGameStateManager : SingletonMonobehaviour<AbstractGameStateManager>, IStateMachine
    {
        protected IBaseState runningState;

        private void Update()
        {
            if(runningState == null)
            {
                GetInitialState();
            }
            runningState.Update(Time.deltaTime);
        }

        private void LateUpdate()
        {
            if (runningState == null) return;
            runningState.FixedUpdate();
        }

        protected abstract void GetInitialState();

        public void SetState(IBaseState state)
        {
            if (runningState == state) return;
            runningState?.Exit();
            runningState = state;
            runningState.Enter();
        }
    }
}