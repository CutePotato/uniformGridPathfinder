using System.Collections.Generic;
using Assets.Scripts.Skills;
using HierarchicalJPS.HPA;
using HierarchicalJPS.Navigation;
using HierarchicalJPS.Samples.Assets.Scripts.AI.Skills;
using HierarchicalJPS.Samples.Assets.Scripts.UI;
using UnityEngine;

namespace HierarchicalJPS.Samples.Assets.Scripts.AI
{
    public class Agent : MonoBehaviour
    {
        public int speed;
        public bool active = false;

        private Vector3 _currentPos;
        public Vector3 destination;
        public LinkedListNode<Edge> currentEdge = null;
        private Vector3 _lastUpdateTargetPathPos;
        private MapManager _map;
        public LinkedList<Edge> path;
        public Sprite image;
        public List<IAbility> abilities;
        
        private void Start()
        {
            _map = MapManager.Instance;
            abilities = new List<IAbility>
            {
                new Movement(image, this)
            };
            PlayerGUIManager.Instance.AddAbilitiesToPanel(abilities);
        }

        private void Update()
        {
            foreach (var ability in abilities)
            {
                ability.Update();
            }
            
            if (currentEdge != null)
            {
                MoveAgent();
            }
        }

        private void MoveAgent()
        {
            Vector3 direction;
            float distance;

            float distToComplete = Time.deltaTime * speed;
            while (distToComplete > 0)
            {
                direction = destination - transform.localPosition;
                distance = direction.magnitude;
                direction.Normalize();

                if (distance > distToComplete)
                    distance = distToComplete;

                // Move
                transform.localPosition += distance * direction;

                // If arrived at destination
                if (transform.localPosition == destination)
                {
                    _currentPos = currentEdge.Value.end.pos;
                    currentEdge = currentEdge.Next;
                    if (currentEdge == null)
                        // We've arrived
                        return;
                    else
                    {
                        destination = currentEdge.Value.end.pos;
                    }
                }

                // Update distance to complete
                distToComplete -= distance;
            }
        }

        public void SetActive(bool active)
        {
            this.active = active;
        }
    }
}