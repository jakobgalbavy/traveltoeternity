using System;

namespace DeepTransit.Events
{
    [Serializable]
    public class MissionEvent
    {
        public string EventId;           // references GameEventSO.Id
        public GameEventSO Definition;
        public long FiredAtMinute;
        public long EscalatesAtMinute;
        public bool IsResolved;
        public bool IsEscalated;
        public bool IsPartiallyResolved;
        public int  PartialFixCount;
        public string ResolutionLog;

        public bool IsOverdue(long gameMinute) =>
            !IsResolved && !IsEscalated && gameMinute >= EscalatesAtMinute;

        public long MinutesUntilEscalation(long gameMinute) =>
            IsResolved || IsEscalated ? 0 : Math.Max(0L, EscalatesAtMinute - gameMinute);
    }
}
