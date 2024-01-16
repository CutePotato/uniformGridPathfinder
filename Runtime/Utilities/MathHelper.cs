using UnityEngine;

namespace HierarchicalJPS.Utilities
{
    public static class MathHelper
    {
        public static readonly Vector3 forward = new Vector3(0, 0, 1f);
        public static readonly Vector3 back = new Vector3(0, 0, -1f);
        public static readonly Vector3 left = new Vector3(-1f, 0, 0);
        public static readonly Vector3 right = new Vector3(1f, 0, 0);
        public static readonly Vector3 up = new Vector3(0, 1f, 0);
        public static readonly Vector3 down = new Vector3(0, -1f, 0);

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