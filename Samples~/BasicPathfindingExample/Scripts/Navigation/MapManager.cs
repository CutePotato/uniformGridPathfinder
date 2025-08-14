using UniformGridPathfinder.Navigation;

namespace UniformGridPathfinder.Samples.Assets.Scripts.Navigation
{
    public class MapManager : Map
    {
        private static MapManager _instance = null;

        public static MapManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<MapManager>();
                }
                return _instance;
            }
        }
    }
}
