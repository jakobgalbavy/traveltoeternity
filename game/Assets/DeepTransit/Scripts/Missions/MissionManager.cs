using System;
using System.Collections.Generic;
using UnityEngine;
using DeepTransit.Core;
using DeepTransit.Cargo;
using DeepTransit.Destinations;
using DeepTransit.Economy;
using DeepTransit.Events;

namespace DeepTransit.Missions
{
    public class MissionManager : MonoBehaviour
    {
        public List<Mission> ActiveMissions { get; private set; } = new();
        public List<Mission> CompletedMissions { get; private set; } = new();

        public event Action<Mission> OnMissionLaunched;
        public event Action<Mission, PayoutResult> OnMissionArrived;
        public event Action<Mission> OnMissionFailed;

        [SerializeField] EventManager _eventManager;

        void OnEnable()  => TimeManager.OnGameMinuteTick += Tick;
        void OnDisable() => TimeManager.OnGameMinuteTick -= Tick;

        void Tick(long gameMinute)
        {
            for (int i = ActiveMissions.Count - 1; i >= 0; i--)
            {
                var mission = ActiveMissions[i];
                bool hourCrossed = mission.Tick(gameMinute);

                if (hourCrossed && _eventManager != null)
                    _eventManager.TickHour(mission, gameMinute);

                if (mission.Status == MissionStatus.Arrived)
                {
                    Complete(mission, gameMinute);
                    ActiveMissions.RemoveAt(i);
                }
                else if (mission.Status == MissionStatus.Failed)
                {
                    ReleaseAssets(mission, GameManager.Instance);
                    OnMissionFailed?.Invoke(mission);
                    CompletedMissions.Add(mission);
                    ActiveMissions.RemoveAt(i);
                }
            }
        }

        void Complete(Mission mission, long gameMinute)
        {
            var gm = GameManager.Instance;
            DestinationSO dest = gm?.StarMapManager?.GetById(mission.DestinationId);
            var payout = PayoutCalculator.Calculate(mission, dest);

            gm?.CurrencyManager?.EarnSoft(payout.GrossPayout);
            gm?.CurrencyManager?.AddReputation(payout.ReputationGain);
            gm?.StarMapManager?.OnMissionArrived(dest);

            ReleaseAssets(mission, gm);
            CompletedMissions.Add(mission);
            OnMissionArrived?.Invoke(mission, payout);
        }

        // Engine efficiency at Mk.I (starter tier): blueprint base 1.0 + module Mk.I bonus 1.0 = 2.0.
        // Voyage time scales as baseMinutes × (baseline / efficiency); clamped so no tier gives a penalty.
        public const float BaselineEngineEfficiency = 2f;

        public static long ApplyEngineEfficiency(long baseMinutes, float engineEfficiency)
        {
            float factor = Mathf.Max(BaselineEngineEfficiency, engineEfficiency) / BaselineEngineEfficiency;
            return System.Math.Max(1L, (long)Mathf.RoundToInt(baseMinutes / factor));
        }

        public Mission LaunchMission(string shipName, string destinationId, CargoManifest cargo, List<string> contractorIds, bool deferCrewPay = false)
        {
            var gm = GameManager.Instance;
            long start = gm?.TimeManager?.ElapsedGameMinutes ?? 0;
            DestinationSO dest = gm?.StarMapManager?.GetById(destinationId);

            long baseMinutes = dest?.VoyageMinutes ?? TimeManager.HoursToGameMinutes(4);
            var  ship        = gm?.ShipManager?.GetByName(shipName);
            float efficiency = ship?.GetStat(Ships.ShipStat.EngineEfficiency) ?? BaselineEngineEfficiency;
            long duration    = ApplyEngineEfficiency(baseMinutes, efficiency);

            var mission = new Mission
            {
                Id = Guid.NewGuid().ToString("N")[..8],
                ShipName = shipName,
                DestinationId = destinationId,
                DurationMinutes = duration,
                Cargo = cargo,
                AssignedContractorIds = contractorIds ?? new List<string>(),
                DeferredCrewPay = deferCrewPay,
            };
            mission.Launch(start);
            ActiveMissions.Add(mission);
            OnMissionLaunched?.Invoke(mission);
            return mission;
        }

        public void SetEventManager(EventManager em) => _eventManager = em;

        void ReleaseAssets(Mission mission, Core.GameManager gm)
        {
            // Free the ship
            var ship = gm?.ShipManager?.Ships?.Find(s => s.Name == mission.ShipName);
            if (ship != null) ship.IsOnMission = false;

            // Free all assigned contractors
            if (gm?.ContractorManager == null) return;
            foreach (var id in mission.AssignedContractorIds)
                foreach (var c in gm.ContractorManager.Roster)
                    if (c.InstanceId == id) c.IsOnMission = false;
        }
    }
}
