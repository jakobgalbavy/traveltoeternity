using System;
using NUnit.Framework;
using UnityEngine;
using DeepTransit.Events;
using DeepTransit.Missions;
using DeepTransit.Cargo;

namespace DeepTransit.Tests
{
    public class EventManagerTests
    {
        GameObject   _go;
        EventManager _em;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("EventManager");
            _em = _go.AddComponent<EventManager>();
            // Override whatever Awake loaded from Resources so tests are self-contained.
            _em.AllEvents = Array.Empty<GameEventSO>();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_go);
        }

        // ── MissionEvent pure helpers ─────────────────────────────────────────

        [Test]
        public void MissionEvent_IsOverdue_TrueWhenAtDeadline()
        {
            var ev = new MissionEvent { EscalatesAtMinute = 100 };
            Assert.IsTrue(ev.IsOverdue(100));
        }

        [Test]
        public void MissionEvent_IsOverdue_FalseOneMinuteBeforeDeadline()
        {
            var ev = new MissionEvent { EscalatesAtMinute = 100 };
            Assert.IsFalse(ev.IsOverdue(99));
        }

        [Test]
        public void MissionEvent_IsOverdue_FalseWhenResolved()
        {
            var ev = new MissionEvent { EscalatesAtMinute = 100, IsResolved = true };
            Assert.IsFalse(ev.IsOverdue(200));
        }

        [Test]
        public void MissionEvent_IsOverdue_FalseWhenAlreadyEscalated()
        {
            var ev = new MissionEvent { EscalatesAtMinute = 100, IsEscalated = true };
            Assert.IsFalse(ev.IsOverdue(200));
        }

        [Test]
        public void MissionEvent_MinutesUntilEscalation_CorrectDifference()
        {
            var ev = new MissionEvent { EscalatesAtMinute = 100 };
            Assert.AreEqual(40L, ev.MinutesUntilEscalation(60));
        }

        [Test]
        public void MissionEvent_MinutesUntilEscalation_ClampedAtZeroWhenOverdue()
        {
            var ev = new MissionEvent { EscalatesAtMinute = 100 };
            Assert.AreEqual(0L, ev.MinutesUntilEscalation(150));
        }

        [Test]
        public void MissionEvent_MinutesUntilEscalation_ZeroWhenResolved()
        {
            var ev = new MissionEvent { EscalatesAtMinute = 200, IsResolved = true };
            Assert.AreEqual(0L, ev.MinutesUntilEscalation(0));
        }

        // ── Resolve guard conditions ──────────────────────────────────────────

        [Test]
        public void Resolve_AlreadyResolved_ReturnsFalse()
        {
            var (mission, ev) = MakeResolveFixture(successChance: 1f);
            ev.IsResolved = true;
            Assert.IsFalse(_em.Resolve(mission, ev, 0, null));
        }

        [Test]
        public void Resolve_AlreadyEscalated_ReturnsFalse()
        {
            var (mission, ev) = MakeResolveFixture(successChance: 1f);
            ev.IsEscalated = true;
            Assert.IsFalse(_em.Resolve(mission, ev, 0, null));
        }

        [Test]
        public void Resolve_OutOfRangeOptionIndex_ReturnsFalse()
        {
            var (mission, ev) = MakeResolveFixture(successChance: 1f);
            Assert.IsFalse(_em.Resolve(mission, ev, 99, null));
        }

        // ── Resolve success path ──────────────────────────────────────────────

        [Test]
        public void Resolve_FullChance_MarksEventResolved()
        {
            var (mission, ev) = MakeResolveFixture(successChance: 1f);
            _em.Resolve(mission, ev, 0, null);
            Assert.IsTrue(ev.IsResolved);
        }

        [Test]
        public void Resolve_FullChance_AppliesSuccessOutcome()
        {
            var success = new EventOutcome { HullDamage = -0.1f };
            var (mission, ev) = MakeResolveFixture(successChance: 1f, onSuccess: success);
            _em.Resolve(mission, ev, 0, null);
            Assert.AreEqual(0.9f, mission.HullIntegrity, 0.001f);
        }

        // ── Resolve failure path ──────────────────────────────────────────────

        [Test]
        public void Resolve_ZeroChance_AppliesFailureOutcome()
        {
            var failure = new EventOutcome { MoraleDelta = -0.2f };
            var (mission, ev) = MakeResolveFixture(successChance: 0f, onFailure: failure);
            _em.Resolve(mission, ev, 0, null);
            Assert.AreEqual(0.8f, mission.CrewMorale, 0.001f);
        }

        [Test]
        public void Resolve_ZeroChance_EventStillMarkedResolved()
        {
            var (mission, ev) = MakeResolveFixture(successChance: 0f);
            _em.Resolve(mission, ev, 0, null);
            Assert.IsTrue(ev.IsResolved);
        }

        // ── Resolve partial-fix path ──────────────────────────────────────────

        [Test]
        public void Resolve_PartialFix_EventRemainsActive()
        {
            var (mission, ev) = MakeResolveFixture(successChance: 1f, partialFix: true);
            _em.Resolve(mission, ev, 0, null);
            Assert.IsFalse(ev.IsResolved);
        }

        [Test]
        public void Resolve_PartialFix_IncrementsFixCounter()
        {
            var (mission, ev) = MakeResolveFixture(successChance: 1f, partialFix: true);
            _em.Resolve(mission, ev, 0, null);
            Assert.AreEqual(1, ev.PartialFixCount);
        }

        [Test]
        public void Resolve_PartialFix_ExtendsEscalationDeadline()
        {
            var (mission, ev) = MakeResolveFixture(successChance: 1f, partialFix: true, escalationMins: 60);
            long before = ev.EscalatesAtMinute;
            _em.Resolve(mission, ev, 0, null);
            Assert.Greater(ev.EscalatesAtMinute, before);
        }

        [Test]
        public void Resolve_PartialFixTwice_CounterAccumulates()
        {
            var (mission, ev) = MakeResolveFixture(successChance: 1f, partialFix: true);
            _em.Resolve(mission, ev, 0, null);
            _em.Resolve(mission, ev, 0, null);
            Assert.AreEqual(2, ev.PartialFixCount);
        }

        // ── TickHour escalation ───────────────────────────────────────────────

        [Test]
        public void TickHour_EscalatesOverdueEvent_AndAddsChainedEvent()
        {
            var chainedSO = ScriptableObject.CreateInstance<GameEventSO>();
            chainedSO.Id = "chained-event";
            chainedSO.EscalationMinutes = 60;
            chainedSO.Options = Array.Empty<EventOption>();

            var parentSO = MakeEventSO("parent", successChance: 0f, escalationMins: 10);
            parentSO.Escalation = chainedSO;

            var mission = MakeInTransitMission();
            var activeEv = new MissionEvent
            {
                EventId = parentSO.Id,
                Definition = parentSO,
                FiredAtMinute = 0,
                EscalatesAtMinute = 50,
            };
            mission.ActiveEvents.Add(activeEv);

            _em.TickHour(mission, 100);

            Assert.IsTrue(activeEv.IsEscalated);
            Assert.AreEqual(2, mission.ActiveEvents.Count);
            Assert.AreEqual("chained-event", mission.ActiveEvents[1].EventId);
        }

        [Test]
        public void TickHour_DoesNotDuplicateChainedEvent_IfAlreadyActive()
        {
            var chainedSO = ScriptableObject.CreateInstance<GameEventSO>();
            chainedSO.Id = "chained-event";
            chainedSO.EscalationMinutes = 60;
            chainedSO.Options = Array.Empty<EventOption>();

            var parentSO = MakeEventSO("parent", successChance: 0f, escalationMins: 10);
            parentSO.Escalation = chainedSO;

            var mission = MakeInTransitMission();
            // Add parent (overdue) AND its escalation already active.
            mission.ActiveEvents.Add(new MissionEvent
            {
                EventId = parentSO.Id,
                Definition = parentSO,
                EscalatesAtMinute = 50,
            });
            mission.ActiveEvents.Add(new MissionEvent
            {
                EventId = chainedSO.Id,
                Definition = chainedSO,
                EscalatesAtMinute = 200,
            });

            _em.TickHour(mission, 100);

            Assert.AreEqual(2, mission.ActiveEvents.Count);
        }

        // ── Event cooldown ───────────────────────────────────────────────────

        [Test]
        public void TickHour_SecondEvent_BlockedWithinCooldownWindow()
        {
            var evSO = MakeEventSO("evt", successChance: 0f);
            evSO.ChancePerHour = 1f;
            _em.AllEvents = new[] { evSO };

            var mission = MakeInTransitMission();
            _em.TickHour(mission, 60);     // fires; LastEventFiredMinute = 60
            mission.ActiveEvents.Clear();  // remove it so IsAlreadyActive won't block
            _em.TickHour(mission, 120);    // 60 min gap < 120 min cooldown → blocked

            Assert.AreEqual(0, mission.ActiveEvents.Count);
        }

        [Test]
        public void TickHour_EventFiresAgainAfterCooldownExpires()
        {
            var evSO = MakeEventSO("evt", successChance: 0f);
            evSO.ChancePerHour = 1f;
            _em.AllEvents = new[] { evSO };

            var mission = MakeInTransitMission();
            _em.TickHour(mission, 60);     // fires; LastEventFiredMinute = 60
            mission.ActiveEvents.Clear();
            _em.TickHour(mission, 180);    // 120 min gap == cooldown → allowed

            Assert.AreEqual(1, mission.ActiveEvents.Count);
        }

        [Test]
        public void TickHour_FirstEvent_AlwaysAllowed_NoMatterWhenMissionStarts()
        {
            var evSO = MakeEventSO("evt", successChance: 0f);
            evSO.ChancePerHour = 1f;
            _em.AllEvents = new[] { evSO };

            var mission = MakeInTransitMission(); // LastEventFiredMinute = -1
            _em.TickHour(mission, 60);

            Assert.AreEqual(1, mission.ActiveEvents.Count);
        }

        // ── TickHour preconditions ────────────────────────────────────────────

        [Test]
        public void TickHour_PassengerEvent_NotFired_WhenNoPassengers()
        {
            var evSO = MakeEventSO("passenger-event", successChance: 0f);
            evSO.ChancePerHour = 1f;
            evSO.Precondition  = EventPrecondition.HasPassengers;
            _em.AllEvents      = new[] { evSO };

            var mission = MakeInTransitMission(passengers: 0);
            _em.TickHour(mission, 60);

            Assert.AreEqual(0, mission.ActiveEvents.Count);
        }

        [Test]
        public void TickHour_PassengerEvent_Fires_WhenPassengersOnBoard()
        {
            var evSO = MakeEventSO("passenger-event", successChance: 0f);
            evSO.ChancePerHour = 1f;
            evSO.Precondition  = EventPrecondition.HasPassengers;
            _em.AllEvents      = new[] { evSO };

            var mission = MakeInTransitMission(passengers: 5);
            _em.TickHour(mission, 60);

            Assert.AreEqual(1, mission.ActiveEvents.Count);
        }

        [Test]
        public void TickHour_LowHullEvent_NotFired_WhenHullHealthy()
        {
            var evSO = MakeEventSO("low-hull-event", successChance: 0f);
            evSO.ChancePerHour = 1f;
            evSO.Precondition  = EventPrecondition.LowHull;
            _em.AllEvents      = new[] { evSO };

            var mission = MakeInTransitMission();
            mission.HullIntegrity = 0.9f;
            _em.TickHour(mission, 60);

            Assert.AreEqual(0, mission.ActiveEvents.Count);
        }

        [Test]
        public void TickHour_Skipped_WhenMissionNotInTransit()
        {
            var evSO = MakeEventSO("any", successChance: 0f);
            evSO.ChancePerHour = 1f;
            _em.AllEvents      = new[] { evSO };

            var mission = new Mission
            {
                Status = MissionStatus.Arrived,
                Cargo  = new CargoManifest(),
            };

            _em.TickHour(mission, 60);

            Assert.AreEqual(0, mission.ActiveEvents.Count);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        (Mission mission, MissionEvent ev) MakeResolveFixture(
            float successChance,
            EventOutcome onSuccess   = default,
            EventOutcome onFailure   = default,
            bool  partialFix         = false,
            long  escalationMins     = 120)
        {
            var evSO = MakeEventSO("test", successChance, onSuccess, onFailure, partialFix, escalationMins);
            var mission = MakeInTransitMission();
            var ev = new MissionEvent
            {
                EventId          = evSO.Id,
                Definition       = evSO,
                FiredAtMinute    = 0,
                EscalatesAtMinute = escalationMins,
            };
            mission.ActiveEvents.Add(ev);
            return (mission, ev);
        }

        static GameEventSO MakeEventSO(
            string id,
            float successChance,
            EventOutcome onSuccess   = default,
            EventOutcome onFailure   = default,
            bool  partialFix         = false,
            long  escalationMins     = 120)
        {
            var so = ScriptableObject.CreateInstance<GameEventSO>();
            so.Id                = id;
            so.EscalationMinutes = escalationMins;
            so.ChancePerHour     = 0f;
            so.Options = new[]
            {
                new EventOption
                {
                    BaseSuccessChance = successChance,
                    IsPartialFix      = partialFix,
                    OnSuccess         = onSuccess,
                    OnFailure         = onFailure,
                }
            };
            return so;
        }

        static Mission MakeInTransitMission(int passengers = 0)
        {
            var m = new Mission
            {
                DurationMinutes = 1000,
                HullIntegrity   = 1f,
                CrewMorale      = 1f,
                CargoIntegrity  = 1f,
                FoodSupply      = 1f,
                Cargo           = new CargoManifest { PassengerCount = passengers },
            };
            m.Launch(0);
            return m;
        }
    }
}
