using System.Collections.Generic;
using Assets.Scripts.Skills;
using HierarchicalJPS.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HierarchicalJPS.Samples.Assets.Scripts.UI
{
    public class PlayerGUIManager : SingletonMonobehaviour<PlayerGUIManager>
    {
        public Canvas canvas;
        public GridLayoutGroup panel;
        public GameObject button;

        public void AddAbilitiesToPanel(List<IAbility> abilities)
        {
            foreach (Transform transform in panel.transform)
            {
                Destroy(transform.gameObject);
            }

            foreach(IAbility ability in abilities)
            {
                var go = Instantiate(button, panel.transform);
                go.GetComponentInChildren<TextMeshProUGUI>().text = ability.GetName();
                go.GetComponent<Image>().sprite = ability.GetImage();
                go.GetComponent<Button>().onClick.AddListener(ability.Execute);
                
            }
        }
    }
}