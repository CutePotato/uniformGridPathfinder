using HierarchicalJPS.Navigation;

namespace HierarchicalJPS.Samples.Assets.Scripts.Navigation
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
