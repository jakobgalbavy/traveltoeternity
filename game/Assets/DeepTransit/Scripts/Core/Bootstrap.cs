using UnityEngine;
using DeepTransit.Missions;

namespace DeepTransit.Core
{
    // Attach to a GameObject in the Bootstrap scene.
    // Wires up all managers and fires a test mission so you can
    // watch progress log in the Console while in Play mode.
    public class Bootstrap : MonoBehaviour
    {
        void Awake()
        {
            var go = new GameObject("GameManager");
            var gm = go.AddComponent<GameManager>();
            gm.TimeManager = go.AddComponent<TimeManager>();
            gm.MissionManager = go.AddComponent<MissionManager>();
            DontDestroyOnLoad(go);
        }

        void Start()
        {
            var mission = new Mission
            {
                Id = "TEST-001",
                ShipName = "ISV Pathfinder",
                DestinationId = "proxima-b",
                DurationMinutes = TimeManager.HoursToGameMinutes(4),  // 4h voyage
                PassengerRatio = 0.5f
            };

            GameManager.Instance.MissionManager.LaunchMission(mission);
            Debug.Log($"[DTA] Mission {mission.ShipName} launched. Duration: {mission.DurationMinutes} game-minutes.");
            TimeManager.OnGameMinuteTick += min =>
            {
                if (min % 60 == 0)  // log every in-game hour
                    Debug.Log($"[DTA] {mission.ShipName} progress: {mission.ProgressNormalized * 100f:F0}% | Status: {mission.Status}");
            };
        }
    }
}
