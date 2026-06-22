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
        public string ResolutionLog;

        public bool IsOverdue(long gameMinute) =>
            !IsResolved && !IsEscalated && gameMinute >= EscalatesAtMinute;
    }
}
