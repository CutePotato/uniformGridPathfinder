using HierarchicalJPS.FSM;
using HierarchicalJPS.Samples.Assets.Scripts.AI;
using HierarchicalJPS.Samples.Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HierarchicalJPS.Samples.Assets.Scripts.States
{
    public class PlayerTurn : BaseState
    {
        private PlayerGUIManager _gui;
        private Agent _player;
        private PlayerInput _playerInput;

        public PlayerTurn(IStateMachine machine, Agent player, PlayerInput playerInput) : base("PlayerTurn", machine)
        {
            _gui = PlayerGUIManager.Instance;
            _player = player;
            _playerInput = playerInput;
        }

        public override void Enter()
        {
            Debug.Log(name + " " + "Enter");
            
            _player.SetActive(true);

            _gui.panel.gameObject.SetActive(true);
        }

        public override void Exit()
        {
            Debug.Log(name + " " + "Exit");
            _player.SetActive(false);

            _playerInput.currentActionMap.Disable();
            _gui.panel.gameObject.SetActive(false);
        }

        public override void FixedUpdate()
        {
            
        }

        public override void Update(float deltaTime)
        {
            
        }
    }
}