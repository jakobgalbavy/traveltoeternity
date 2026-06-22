using UnityEngine;
using DeepTransit.Missions;
using DeepTransit.Ships;

namespace DeepTransit.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Managers")]
        public MissionManager MissionManager;
        public TimeManager TimeManager;
        public ShipManager ShipManager;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
