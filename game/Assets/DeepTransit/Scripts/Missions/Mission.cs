using System;
using UnityEngine;
using DeepTransit.Ships;

namespace DeepTransit.Missions
{
    public enum MissionStatus { Preparing, InTransit, Arrived, Failed }

    [Serializable]
    public class Mission
    {
        public string Id;
        public string ShipName;
        public string DestinationId;
        public long DurationMinutes;      // total voyage length in game-minutes
        public long LaunchMinute;
        public MissionStatus Status;

        // Cargo split: 0.0 = all packages, 1.0 = all passengers
        [Range(0f, 1f)] public float PassengerRatio;

        public float ProgressNormalized =>
            Status == MissionStatus.InTransit && DurationMinutes > 0
                ? Mathf.Clamp01((float)(_currentMinute - LaunchMinute) / DurationMinutes)
                : 0f;

        private long _currentMinute;

        public void Launch(long startMinute)
        {
            LaunchMinute = startMinute;
            Status = MissionStatus.InTransit;
        }

        public void Tick(long gameMinutes)
        {
            if (Status != MissionStatus.InTransit) return;
            _currentMinute = gameMinutes;
            if (gameMinutes - LaunchMinute >= DurationMinutes)
                Status = MissionStatus.Arrived;
        }
    }
}
