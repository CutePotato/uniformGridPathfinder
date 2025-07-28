using HierarchicalJPS.Utilities;
using UnityEngine;

namespace HierarchicalJPS.Samples.Assets.Scripts.Utilities
{
    public class CameraHelper
    {
        public static Vector3? ScreenPointToCell()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) return null;
            return MathHelper.FloorToInt(hit.point) + new Vector3(0.5f, 0, 0.5f);
        }
    }
}