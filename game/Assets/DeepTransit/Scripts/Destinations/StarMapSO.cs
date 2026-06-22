using UnityEngine;

namespace DeepTransit.Destinations
{
    [CreateAssetMenu(menuName = "DeepTransit/Star Map", fileName = "StarMap")]
    public class StarMapSO : ScriptableObject
    {
        public DestinationSO[] AllDestinations;
    }
}
