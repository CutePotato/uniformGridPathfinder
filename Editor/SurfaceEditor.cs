using UniformGridPathfinder.Navigation;
using UnityEditor;
using UnityEngine;

namespace UniformGridPathfinder.Editor
{
    [CustomEditor(typeof(Surface))]
    public class SurfaceEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            var surface = (Surface)target;
            
            if (GUILayout.Button("Update Obstacles"))
            {
                surface.UpdateObstacles();
            }
        }

        private void OnSceneGUI()
        {
            var cluster = (Surface)target;
            var pos = cluster.transform.position;
            var data = new Data(pos, cluster.size);
            var size = data.Bounds.size;
            Handles.DrawWireCube(pos, new Vector3(size.x, 0, size.z));
        }
    }
}
