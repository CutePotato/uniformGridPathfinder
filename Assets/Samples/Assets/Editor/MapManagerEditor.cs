using System.Collections.Generic;
using HierarchicalJPS.HPA;
using HierarchicalJPS.Samples.Assets.Scripts.Navigation;
using UnityEditor;
using UnityEngine;

namespace HierarchicalJPS.Samples.Assets.Editor
{
    [CustomEditor(typeof(MapManager))]
    public class MapManagerEditor : UnityEditor.Editor
    {
        public float screenSpaceSize = 5.0f;
        public float edgeRadio = .2f;
        private LinkedList<Edge> _path;
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            var map = (MapManager)target;
            
            if (GUILayout.Button("Find Path"))
            {
                _path = map.GetPath(map.startPosition, map.endPosition);
            }

            if (Application.isPlaying)
            {
                if (GUILayout.Button("Update Surfaces Links"))
                {
                    map.UpdateSurfacesLinkNodes();
                }
            }
        }

        private void OnSceneGUI()
        {
            var map = (MapManager)target;

            if (_path?.Count > 0)
            {
                foreach (var edge in _path)
                {
                    Handles.color = Color.magenta;
                    Handles.DrawSolidDisc(edge.end.pos, Vector3.up, edgeRadio);
                }
            }

            foreach(var edge in map.links)
            {
                Handles.color = Color.cyan;
                Handles.DrawDottedLine(edge.startPoint, edge.endPoint, screenSpaceSize);
                Handles.DrawSolidDisc(edge.startPoint, Vector3.up, edgeRadio);
                Handles.DrawSolidDisc(edge.endPoint, Vector3.up, edgeRadio);
            }

            foreach (var cluster in map.surfaces)
            {
                var pos = cluster.transform.position;
                var data = new Data(pos, cluster.size);
                var size = data.Bounds.size;
                Handles.DrawWireCube(pos, new Vector3(size.x, 0, size.z));
            }
        }
    }
}
