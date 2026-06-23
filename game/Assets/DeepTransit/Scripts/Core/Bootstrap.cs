using UnityEngine;
using DeepTransit.Missions;
using DeepTransit.Ships;
using DeepTransit.Contractors;
using DeepTransit.Destinations;
using DeepTransit.Economy;
using DeepTransit.Events;
using DeepTransit.Data;
using DeepTransit.UI;

namespace DeepTransit.Core
{
    // Attach to a single GameObject in the Bootstrap scene.
    // Assign all ScriptableObject references in the inspector.
    public class Bootstrap : MonoBehaviour
    {
        [Header("Data References")]
        public StarMapSO StarMap;
        public ContractorSO[] ContractorDefinitions;

        [Header("Starting Ship")]
        public ShipBlueprintSO StarterBlueprint;
        public ShipModuleSO[]  StarterModules;

        void Awake()
        {
            var go = new GameObject("GameManager");
            var gm                   = go.AddComponent<GameManager>();
            gm.TimeManager           = go.AddComponent<TimeManager>();
            gm.MissionManager        = go.AddComponent<MissionManager>();
            gm.ShipManager           = go.AddComponent<ShipManager>();
            gm.CurrencyManager       = go.AddComponent<CurrencyManager>();
            gm.SaveManager           = go.AddComponent<SaveManager>();

            var starMap              = go.AddComponent<StarMapManager>();
            starMap.StarMap          = StarMap;
            gm.StarMapManager        = starMap;

            var contractorMgr        = go.AddComponent<ContractorManager>();
            contractorMgr.AllDefinitions = ContractorDefinitions;
            gm.ContractorManager     = contractorMgr;

            var eventMgr             = go.AddComponent<EventManager>();
            gm.EventManager          = eventMgr;

            var uiMgr                = go.AddComponent<UIManager>();

            DontDestroyOnLoad(go);
        }

        void Start()
        {
            HookMissionEvents();
            CreateStarterShipIfNeeded();
        }

        void HookMissionEvents()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            gm.MissionManager.OnMissionArrived += (mission, payout) =>
                UIManager.Instance?.ShowDebrief(mission, payout);

            gm.MissionManager.OnMissionFailed += mission =>
                UIManager.Instance?.ShowDebrief(mission, new PayoutResult());

            gm.EventManager.OnEventFired += (mission, ev) =>
                UIManager.Instance?.ShowEvent(mission, ev);
        }

        void CreateStarterShipIfNeeded()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.ShipManager.Ships.Count > 0)
            {
                Debug.Log($"[Bootstrap] Ships already present ({gm?.ShipManager?.Ships?.Count ?? 0}) — skipping starter ship creation.");
                return;
            }
            if (StarterBlueprint == null)
            {
                Debug.LogError("[Bootstrap] StarterBlueprint is null — assign it in the Bootstrap component inspector.");
                return;
            }

            var ship = gm.ShipManager.CreateShip("ISV Pathfinder", StarterBlueprint);
            int installed = 0;
            for (int i = 0; i < StarterModules?.Length; i++)
                if (ship.InstallModule(i, StarterModules[i])) installed++;
            Debug.Log($"[Bootstrap] Created '{ship.Name}' with {installed}/{StarterModules?.Length ?? 0} modules installed.");
        }
    }
}
