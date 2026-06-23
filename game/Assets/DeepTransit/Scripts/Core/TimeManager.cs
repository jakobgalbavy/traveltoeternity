using System;
using UnityEngine;

namespace DeepTransit.Core
{
    // Translates real elapsed seconds into in-game mission time.
    // All voyage timers and event clocks read from here.
    public class TimeManager : MonoBehaviour
    {
        // 1 real second = 1 in-game minute (60x compression).
        // A 4-hour real-world short mission takes ~14 real seconds of active time.
        // Adjust RealSecondsPerGameMinute to tune pacing.
        public const float RealSecondsPerGameMinute = 1f;

        public static event Action<long> OnGameMinuteTick;

        public long ElapsedGameMinutes { get; private set; }
        public int  SpeedMultiplier    { get; set; } = 1;
        public bool Paused             { get; set; } = false;

        private float _accumulator;

        void Update()
        {
            if (Paused) return;
            _accumulator += Time.deltaTime * SpeedMultiplier;
            while (_accumulator >= RealSecondsPerGameMinute)
            {
                _accumulator -= RealSecondsPerGameMinute;
                ElapsedGameMinutes++;
                OnGameMinuteTick?.Invoke(ElapsedGameMinutes);
            }
        }

        public static long HoursToGameMinutes(float hours) => (long)(hours * 60);
        public static long DaysToGameMinutes(float days) => (long)(days * 1440);
    }
}
