using NUnit.Framework;
using DeepTransit.Missions;

namespace DeepTransit.Tests
{
    public class MissionTests
    {
        [Test]
        public void Mission_NotStarted_ProgressIsZero()
        {
            var m = new Mission { DurationMinutes = 100 };
            Assert.AreEqual(0f, m.ProgressNormalized);
        }

        [Test]
        public void Mission_HalfwayThrough_ProgressIsHalf()
        {
            var m = new Mission { DurationMinutes = 100 };
            m.Launch(0);
            m.Tick(50);
            Assert.AreEqual(0.5f, m.ProgressNormalized, 0.001f);
        }

        [Test]
        public void Mission_TickPastDuration_StatusIsArrived()
        {
            var m = new Mission { DurationMinutes = 60 };
            m.Launch(0);
            m.Tick(60);
            Assert.AreEqual(MissionStatus.Arrived, m.Status);
        }

        [Test]
        public void Mission_TickBeforeDuration_StatusIsInTransit()
        {
            var m = new Mission { DurationMinutes = 60 };
            m.Launch(0);
            m.Tick(59);
            Assert.AreEqual(MissionStatus.InTransit, m.Status);
        }

        [Test]
        public void Mission_LaunchWithOffset_TracksRelativeTime()
        {
            var m = new Mission { DurationMinutes = 60 };
            m.Launch(1000);   // launched at game-minute 1000
            m.Tick(1030);     // 30 minutes elapsed
            Assert.AreEqual(0.5f, m.ProgressNormalized, 0.001f);
        }
    }
}
