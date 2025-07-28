using UnityEngine;

namespace HierarchicalJPS.Utilities
{
    public static class MathHelper
    {
        public static Vector3Int FloorToInt(Vector3 vector)
        {
            int x, y, z;

            x = AlmostZero(vector.x) ? 0 : Mathf.FloorToInt(vector.x);
            y = AlmostZero(vector.y) ? 0 : Mathf.FloorToInt(vector.y);
            z = AlmostZero(vector.z) ? 0 : Mathf.FloorToInt(vector.z);

            return new Vector3Int(x, y, z);
        }

        private static bool AlmostZero(float num)
        {
            if (num < .01f && num > -.01f) return true;
            return false;
        }
    }

}