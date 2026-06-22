using System.Collections.Generic;
using UnityEngine;
using DeepTransit.Core;

namespace DeepTransit.Missions
{
    public class MissionManager : MonoBehaviour
    {
        public List<Mission> ActiveMissions { get; private set; } = new();

        void OnEnable()  => TimeManager.OnGameMinuteTick += Tick;
        void OnDisable() => TimeManager.OnGameMinuteTick -= Tick;

        void Tick(long gameMinutes)
        {
            foreach (var mission in ActiveMissions)
                mission.Tick(gameMinutes);
        }

        public void LaunchMission(Mission mission)
        {
            ActiveMissions.Add(mission);
            var startMinute = GameManager.Instance != null
                ? GameManager.Instance.TimeManager.ElapsedGameMinutes
                : 0;
            mission.Launch(startMinute);
        }
    }
}
