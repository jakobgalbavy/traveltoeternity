using System;
using System.Collections.Generic;
using UnityEngine;
using DeepTransit.Contractors;
using DeepTransit.Missions;

namespace DeepTransit.Events
{
    public class EventManager : MonoBehaviour
    {
        public GameEventSO[] AllEvents;

        public event Action<Mission, MissionEvent> OnEventFired;
        public event Action<Mission, MissionEvent, bool> OnEventResolved;

        private readonly System.Random _rng = new();

        // Called every game-hour by MissionManager.
        public void TickHour(Mission mission, long gameMinute)
        {
            if (mission.Status != Missions.MissionStatus.InTransit) return;

            // Collect overdue events first to avoid modifying the list mid-iteration.
            var overdue = new List<MissionEvent>();
            foreach (var ev in mission.ActiveEvents)
            {
                if (ev.IsOverdue(gameMinute))
                    overdue.Add(ev);
            }
            foreach (var ev in overdue)
                Escalate(mission, ev, gameMinute);

            var fired = TryFireEvent(mission, gameMinute);
            if (fired != null)
                OnEventFired?.Invoke(mission, fired);
        }

        MissionEvent TryFireEvent(Mission mission, long gameMinute)
        {
            if (AllEvents == null) return null;

            var candidates = new List<GameEventSO>(AllEvents);
            candidates.Sort((_, __) => _rng.Next(-1, 2));

            foreach (var evSO in candidates)
            {
                if (evSO.ChancePerHour <= 0f) continue;
                if (!MeetsPrecondition(evSO, mission)) continue;
                if (_rng.NextDouble() > evSO.ChancePerHour) continue;
                if (IsAlreadyActive(mission, evSO.Id)) continue;

                var ev = new MissionEvent
                {
                    EventId = evSO.Id,
                    Definition = evSO,
                    FiredAtMinute = gameMinute,
                    EscalatesAtMinute = gameMinute + evSO.EscalationMinutes,
                };
                mission.ActiveEvents.Add(ev);
                return ev;
            }
            return null;
        }

        // Player resolves an event by picking an option and assigning a contractor (can be null).
        public bool Resolve(Mission mission, MissionEvent ev, int optionIndex, ContractorInstance contractor)
        {
            if (ev.IsResolved || ev.IsEscalated || optionIndex >= ev.Definition.Options.Length) return false;

            var option = ev.Definition.Options[optionIndex];
            float chance = option.BaseSuccessChance;
            if (contractor != null && contractor.Definition?.Role == option.RequiredRole)
                chance += option.ContractorBonus * contractor.SuccessChance;

            bool success = (float)_rng.NextDouble() <= Mathf.Clamp01(chance);

            if (success && option.IsPartialFix)
            {
                // Partial fix: apply reduced outcome, extend the deadline, stay active.
                ApplyOutcome(mission, option.OnSuccess);
                ev.IsPartiallyResolved = true;
                ev.PartialFixCount++;
                ev.EscalatesAtMinute += ev.Definition.EscalationMinutes;

                if (contractor != null)
                    contractor.AddExperience(0.02f);

                OnEventResolved?.Invoke(mission, ev, true);
                return true;
            }

            var outcome = success ? option.OnSuccess : option.OnFailure;
            ApplyOutcome(mission, outcome);
            ev.IsResolved = true;
            ev.ResolutionLog = outcome.LogMessage;

            if (contractor != null)
                contractor.AddExperience(success ? 0.05f : 0.02f);

            OnEventResolved?.Invoke(mission, ev, success);
            return success;
        }

        void Escalate(Mission mission, MissionEvent ev, long gameMinute)
        {
            ev.IsEscalated = true;
            if (ev.Definition.Escalation == null) return;

            var next = ev.Definition.Escalation;
            if (IsAlreadyActive(mission, next.Id)) return;

            var escalated = new MissionEvent
            {
                EventId = next.Id,
                Definition = next,
                FiredAtMinute = gameMinute,
                EscalatesAtMinute = gameMinute + next.EscalationMinutes,
            };
            mission.ActiveEvents.Add(escalated);
            OnEventFired?.Invoke(mission, escalated);
        }

        void ApplyOutcome(Mission mission, EventOutcome outcome)
        {
            mission.HullIntegrity  = Mathf.Clamp01(mission.HullIntegrity  + outcome.HullDamage);
            mission.CrewMorale     = Mathf.Clamp01(mission.CrewMorale     + outcome.MoraleDelta);
            mission.CargoIntegrity = Mathf.Clamp01(mission.CargoIntegrity + outcome.CargoDamage);
            mission.FoodSupply     = Mathf.Clamp01(mission.FoodSupply     + outcome.FoodDelta);

            if (mission.HullIntegrity <= 0f)
                mission.Status = Missions.MissionStatus.Failed;
        }

        bool MeetsPrecondition(GameEventSO evSO, Mission mission)
        {
            return evSO.Precondition switch
            {
                EventPrecondition.HasPassengers => mission.Cargo?.PassengerCount > 0,
                EventPrecondition.HasPackages   => mission.Cargo?.PackageCount > 0,
                EventPrecondition.LowHull       => mission.HullIntegrity < 0.3f,
                EventPrecondition.LowMorale     => mission.CrewMorale < 0.3f,
                EventPrecondition.LowFood       => mission.FoodSupply < 0.2f,
                _                               => true,
            };
        }

        bool IsAlreadyActive(Mission mission, string eventId)
        {
            foreach (var ev in mission.ActiveEvents)
                if (!ev.IsResolved && !ev.IsEscalated && ev.EventId == eventId)
                    return true;
            return false;
        }
    }
}
