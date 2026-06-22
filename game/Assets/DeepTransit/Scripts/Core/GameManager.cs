using UnityEngine;
using DeepTransit.Missions;
using DeepTransit.Ships;
using DeepTransit.Contractors;
using DeepTransit.Destinations;
using DeepTransit.Economy;
using DeepTransit.Events;
using DeepTransit.Data;

namespace DeepTransit.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Managers")]
        public TimeManager       TimeManager;
        public MissionManager    MissionManager;
        public ShipManager       ShipManager;
        public ContractorManager ContractorManager;
        public StarMapManager    StarMapManager;
        public CurrencyManager   CurrencyManager;
        public EventManager      EventManager;
        public SaveManager       SaveManager;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            MissionManager.SetEventManager(EventManager);
            bool loaded = SaveManager.Load();
            if (!loaded)
                OnFirstLaunch();
        }

        void OnFirstLaunch()
        {
            StarMapManager.UnlockDestination("proxima-b");
        }

        void OnApplicationPause(bool paused)
        {
            if (paused) SaveManager.Save();
        }

        void OnApplicationQuit() => SaveManager.Save();
    }
}
