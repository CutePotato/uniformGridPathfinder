using UniformGridPathfinder.Samples.Assets.Scripts.AI;
using UniformGridPathfinder.FSM;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UniformGridPathfinder.Samples.Assets.Scripts.States
{
    public class GameStateManager : AbstractGameStateManager
    {
        private MapInit _mapInit;
        public PlayerTurn playerTurn;
        public PlayerInput playerInput;
        public Agent player;

        private void Start()
        {
            _mapInit = new MapInit(this);
            playerTurn = new PlayerTurn(this, player, playerInput);
        }

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

        protected override void GetInitialState()
        {
            SetState(_mapInit);
        }
    }
}