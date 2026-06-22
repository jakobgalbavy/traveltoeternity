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

            CompletedMissions.Add(mission);
            OnMissionArrived?.Invoke(mission, payout);
        }

        public Mission LaunchMission(string shipName, string destinationId, CargoManifest cargo, List<string> contractorIds)
        {
            var gm = GameManager.Instance;
            long start = gm?.TimeManager?.ElapsedGameMinutes ?? 0;
            DestinationSO dest = gm?.StarMapManager?.GetById(destinationId);

            var mission = new Mission
            {
                Id = Guid.NewGuid().ToString("N")[..8],
                ShipName = shipName,
                DestinationId = destinationId,
                DurationMinutes = dest?.VoyageMinutes ?? TimeManager.HoursToGameMinutes(4),
                Cargo = cargo,
                AssignedContractorIds = contractorIds ?? new List<string>(),
            };
            mission.Launch(start);
            ActiveMissions.Add(mission);
            OnMissionLaunched?.Invoke(mission);
            return mission;
        }

        public void SetEventManager(EventManager em) => _eventManager = em;
    }
}
