using System;
using System.Collections.Generic;
using UnityEngine;
using DeepTransit.Cargo;
using DeepTransit.Events;

namespace DeepTransit.Missions
{
    public enum MissionStatus { Preparing, InTransit, Arrived, Failed }

    [Serializable]
    public class Mission
    {
        // Identity
        public string Id;
        public string ShipName;
        public string DestinationId;
        public MissionStatus Status;

        // Timing
        public long DurationMinutes;
        public long LaunchMinute;

        // Cargo
        public CargoManifest Cargo;

        // Contractors assigned (instance IDs)
        public List<string> AssignedContractorIds = new();

        // Crew agreed to defer pay; settlement deducted from payout at mission end.
        public bool DeferredCrewPay;

        // Live state (0–1 normalised)
        public float HullIntegrity   = 1f;
        public float CrewMorale      = 1f;
        public float CargoIntegrity  = 1f;
        public float FoodSupply      = 1f;

        // Events
        public List<MissionEvent> ActiveEvents = new();

        public List<MissionEvent> PendingEvents
        {
            get
            {
                var pending = new List<MissionEvent>();
                foreach (var e in ActiveEvents)
                    if (!e.IsResolved && !e.IsEscalated) pending.Add(e);
                return pending;
            }
        }

        public float ProgressNormalized =>
            Status == MissionStatus.InTransit && DurationMinutes > 0
                ? Mathf.Clamp01((float)(_currentMinute - LaunchMinute) / DurationMinutes)
                : 0f;

        private long _currentMinute;
        private long _lastHourTick = -1;

        public event Action<long> OnHourTick;  // fires every in-game hour

        public void Launch(long startMinute)
        {
            LaunchMinute = startMinute;
            _currentMinute = startMinute;
            Status = MissionStatus.InTransit;
        }

        // Returns true if an hour boundary was crossed (for event checks).
        public bool Tick(long gameMinutes)
        {
            if (Status != MissionStatus.InTransit) return false;
            _currentMinute = gameMinutes;

            long elapsedHours = (gameMinutes - LaunchMinute) / 60;
            if (elapsedHours > _lastHourTick)
            {
                _lastHourTick = elapsedHours;
                ConsumeFoodTick();
                OnHourTick?.Invoke(gameMinutes);
            }

            if (gameMinutes - LaunchMinute >= DurationMinutes)
                Status = MissionStatus.Arrived;

            return elapsedHours > _lastHourTick - 1;
        }

        void ConsumeFoodTick()
        {
            // Food depletes faster with more passengers; reaches ~0 after 4× voyage duration without resupply.
            int headcount = (Cargo?.PassengerCount ?? 0) + 2; // +2 for base crew
            float depletionPerHour = headcount * 0.001f;
            FoodSupply = Mathf.Clamp01(FoodSupply - depletionPerHour);
            if (FoodSupply <= 0f)
                CrewMorale = Mathf.Clamp01(CrewMorale - 0.05f);
        }
    }
}
