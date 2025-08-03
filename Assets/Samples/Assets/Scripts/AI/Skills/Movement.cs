using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assets.Scripts.Skills;
using HierarchicalJPS.Samples.Assets.Scripts.Navigation;
using HierarchicalJPS.Samples.Assets.Scripts.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;
using Edge = HierarchicalJPS.HPA.Edge;

namespace HierarchicalJPS.Samples.Assets.Scripts.AI.Skills
{
    public class Movement : BaseAbility
    {
        private Agent _agent;
        private MapManager _map;
        private PlayerInput _playerInput;

        private bool _visualizePath = false;
        private Task<LinkedList<Edge>> _pathTask = null;
        private Vector3 _lastUpdateTargetPathPos;
        private IEnumerable<Vector3> _lastUpdatePath = null;
        private Vector3? _lastMousePosition = null;
        public Vector3 offset = Vector3.up * 0.1f;
        private LineRenderer _line;

        public Movement(Sprite image, Agent agent) : base("Movement", image)
        {
            _agent = agent;
            _map = MapManager.Instance;
            _line = _map.linePath;
            _playerInput = _map.playerInput;
        }

        private void Init()
        {
            _visualizePath = true;
            _line.enabled = true;
            _playerInput.actions["mouseLeft"].performed += Move;
            _playerInput.actions["escape"].performed += CancelAction;
        }

        private void Cancel()
        {
            _playerInput.actions["mouseLeft"].performed -= Move;
            _playerInput.actions["escape"].performed -= CancelAction;
            _visualizePath = false;
            _line.enabled = false;
        }

        public override async void Execute()
        {
            _playerInput.SwitchCurrentActionMap("movement");
            _playerInput.currentActionMap.Enable();
            Init();
        }

        public override void Update()
        {
            VisualPath();
        }

        private void VisualPath()
        {
            if (!_visualizePath) return;

            if (_pathTask == null)
            {
                _lastMousePosition = CameraHelper.ScreenPointToCell();
                if (!_lastMousePosition.HasValue || _lastMousePosition.Value == null || _lastUpdateTargetPathPos == _lastMousePosition.Value) return;
                _lastUpdateTargetPathPos = _lastMousePosition.Value;

                if (IsCellSelectable(_lastMousePosition.Value))
                {
                    var actualPos = _agent.transform.position;
                    _pathTask = Task.Run(() => _map.GetPath(actualPos, _lastMousePosition.Value));
                }
            }

            if (_pathTask != null && _pathTask.IsCompleted)
            {
                _lastUpdatePath = _pathTask.Result.Select(edge => edge.end.pos);
                _line.positionCount = _lastUpdatePath.Count();
                _line.SetPositions(_lastUpdatePath.Select(point => point+offset).ToArray());
                _pathTask = null;
            }
        }
        
        public bool IsCellSelectable(Vector3 pos)
        {
            foreach (var surface in _map.surfaces)
            {
                if (surface.IsFreeTile(pos) && surface.Nodes.TryGetValue(pos, out var node) && !node.obstacle) return true;
            }
            
            return false;
        }

        private void Move(InputAction.CallbackContext context)
        {
            var pos = CameraHelper.ScreenPointToCell();
            if (!pos.HasValue) return;

            if (!IsCellSelectable(pos.Value))
            {
                Debug.Log("Movement Picked Not valid: " + pos.Value);
                return;
            }

            _visualizePath = false;
            _line.enabled = false;

            _agent.path = _map.GetPath(_agent.transform.position, pos.Value);
            _agent.currentEdge = _agent.path.First;
            _agent.destination = _agent.currentEdge.Value.end.pos;
            Cancel();
        }

        private void CancelAction(InputAction.CallbackContext context)
        {
            Cancel();
        }
    }
}
