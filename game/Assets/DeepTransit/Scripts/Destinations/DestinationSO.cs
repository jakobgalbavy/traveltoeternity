using System;
using UnityEngine;

namespace DeepTransit.Destinations
{
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
