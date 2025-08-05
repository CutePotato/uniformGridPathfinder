using UnityEngine;

namespace UniformGridPathfinder.Utilities
{
    public class SingletonMonobehaviour<T> : MonoBehaviour where T : SingletonMonobehaviour<T>
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<T>();
                }
                return _instance;
            }
        }
    }
}