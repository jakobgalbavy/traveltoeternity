using NUnit.Framework;
using DeepTransit.Core;

namespace DeepTransit.Tests
{
    public class TimeManagerTests
    {
        [Test]
        public void HoursToGameMinutes_4Hours_Returns240()
        {
            Assert.AreEqual(240L, TimeManager.HoursToGameMinutes(4));
        }

        [Test]
        public void DaysToGameMinutes_1Day_Returns1440()
        {
            Assert.AreEqual(1440L, TimeManager.DaysToGameMinutes(1));
        }

        [Test]
        public void HoursToGameMinutes_HalfHour_Returns30()
        {
            Assert.AreEqual(30L, TimeManager.HoursToGameMinutes(0.5f));
        }
    }
}
