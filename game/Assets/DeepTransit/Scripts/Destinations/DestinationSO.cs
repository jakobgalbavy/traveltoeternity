using System;
using UnityEngine;

namespace DeepTransit.Destinations
{
    public enum MissionType
    {
        Basic,   // Repeatable contract; always pays out at least MinimumPayout.
        Special, // One-shot or storyline; may pay zero currency and yield a non-monetary reward instead.
    }

    [CreateAssetMenu(menuName = "DeepTransit/Destination", fileName = "Destination_New")]
    public class DestinationSO : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        [TextArea] public string Description;
        public float DistanceLY;
        public long VoyageMinutes;        // base voyage duration in game-minutes
        public float PayoutMultiplier;    // applied on top of base payout
        public int ReputationRequired;    // minimum rep to unlock

        public MissionType MissionType;
        [Tooltip("Basic missions guarantee at least this payout even on a terrible run.")]
        public long MinimumPayout;

        public HazardProfile Hazards;
        public DestinationSO[] UnlocksOnFirstCompletion;
    }

    [Serializable]
    public struct HazardProfile
    {
        [Range(0f, 1f)] public float MeteoriteRisk;
        [Range(0f, 1f)] public float RadiationRisk;
        [Range(0f, 1f)] public float MechanicalFailureRisk;
        [Range(0f, 1f)] public float PassengerUnrestRisk;
    }
}
