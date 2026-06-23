using System;
using UnityEngine;
using DeepTransit.Contractors;

namespace DeepTransit.Events
{
    public enum EventSeverity { Minor, Moderate, Critical }

    public enum EventPrecondition { None, HasPassengers, HasPackages, LowHull, LowMorale, LowFood }

    [CreateAssetMenu(menuName = "DeepTransit/Game Event", fileName = "Event_New")]
    public class GameEventSO : ScriptableObject
    {
        public string Id;
        public string Title;
        [TextArea] public string Description;
        public EventSeverity Severity;

        [Tooltip("Chance per game-hour that this event fires (0–1).")]
        [Range(0f, 1f)] public float ChancePerHour;

        [Tooltip("Only fires if this condition is met on the mission.")]
        public EventPrecondition Precondition;

        public EventOption[] Options;

        [Tooltip("Event that replaces this one if left unresolved past the deadline.")]
        public GameEventSO Escalation;

        [Tooltip("Game-minutes before escalation triggers.")]
        public long EscalationMinutes = 120;
    }

    [Serializable]
    public class EventOption
    {
        public string Label;
        public ContractorRole RequiredRole;

        [Tooltip("Base success chance (0–1) without a matching contractor.")]
        [Range(0f, 1f)] public float BaseSuccessChance;

        [Tooltip("Added to success chance when the required contractor is assigned.")]
        [Range(0f, 1f)] public float ContractorBonus;

        [Tooltip("Success extends the escalation deadline rather than closing the event. Models a temporary patch.")]
        public bool IsPartialFix;

        public EventOutcome OnSuccess;
        public EventOutcome OnFailure;
    }

    [Serializable]
    public struct EventOutcome
    {
        [Range(-1f, 0f)] public float HullDamage;        // negative = damage
        [Range(-1f, 1f)] public float MoraleDelta;
        [Range(-1f, 0f)] public float CargoDamage;
        [Range(-1f, 0f)] public float FoodDelta;
        public string LogMessage;
    }
}
