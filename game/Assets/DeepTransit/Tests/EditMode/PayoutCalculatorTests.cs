using NUnit.Framework;
using UnityEngine;
using DeepTransit.Economy;
using DeepTransit.Missions;
using DeepTransit.Cargo;
using DeepTransit.Destinations;

namespace DeepTransit.Tests
{
    public class PayoutCalculatorTests
    {
        // ── Helpers ───────────────────────────────────────────────────────────

        static DestinationSO MakeDest(float multiplier, long voyageMinutes, int repRequired = 0)
        {
            var d = ScriptableObject.CreateInstance<DestinationSO>();
            d.PayoutMultiplier   = multiplier;
            d.VoyageMinutes      = voyageMinutes;
            d.ReputationRequired = repRequired;
            return d;
        }

        static Mission MakeMission(int passengers, int packages,
            float hull = 1f, float morale = 1f, float cargo = 1f, float food = 1f,
            MissionStatus status = MissionStatus.Arrived)
        {
            return new Mission
            {
                Status         = status,
                HullIntegrity  = hull,
                CrewMorale     = morale,
                CargoIntegrity = cargo,
                FoodSupply     = food,
                Cargo          = new CargoManifest { PassengerCount = passengers, PackageCount = packages },
            };
        }

        // ── Launch cost ───────────────────────────────────────────────────────

        [Test]
        public void LaunchCost_NullDestination_ReturnsZero()
        {
            Assert.AreEqual(0L, PayoutCalculator.CalculateLaunchCost(null, null));
        }

        [Test]
        public void LaunchCost_EmptyCargo_OnlyFuel()
        {
            var dest = MakeDest(1.2f, voyageMinutes: 240);
            long cost = PayoutCalculator.CalculateLaunchCost(dest, new CargoManifest());
            // 240 × 0.5 = ¤120
            Assert.AreEqual(120L, cost);
        }

        [Test]
        public void LaunchCost_PassengersAddSupplyCost()
        {
            var dest = MakeDest(1.2f, voyageMinutes: 240);
            var cargo = new CargoManifest { PassengerCount = 5 };
            long cost = PayoutCalculator.CalculateLaunchCost(dest, cargo);
            // fuel=120  supply=5×40=200  → 320
            Assert.AreEqual(320L, cost);
        }

        [Test]
        public void LaunchCost_PackagesAddHandlingCost()
        {
            var dest = MakeDest(1.2f, voyageMinutes: 240);
            var cargo = new CargoManifest { PackageCount = 10 };
            long cost = PayoutCalculator.CalculateLaunchCost(dest, cargo);
            // fuel=120  handling=10×15=150  → 270
            Assert.AreEqual(270L, cost);
        }

        [Test]
        public void LaunchCost_ScalesWithVoyageLength()
        {
            var shortDest = MakeDest(1.2f, voyageMinutes: 240);
            var longDest  = MakeDest(3.5f, voyageMinutes: 1440);
            var cargo = new CargoManifest();

            long shortCost = PayoutCalculator.CalculateLaunchCost(shortDest, cargo);
            long longCost  = PayoutCalculator.CalculateLaunchCost(longDest, cargo);

            Assert.Greater(longCost, shortCost);
        }

        [Test]
        public void LaunchCost_NullCargo_CountsAsEmpty()
        {
            var dest = MakeDest(1.2f, voyageMinutes: 240);
            long costNullCargo  = PayoutCalculator.CalculateLaunchCost(dest, null);
            long costEmptyCargo = PayoutCalculator.CalculateLaunchCost(dest, new CargoManifest());
            Assert.AreEqual(costEmptyCargo, costNullCargo);
        }

        // ── Payout calculate ──────────────────────────────────────────────────

        [Test]
        public void Calculate_NullDestination_ReturnsZeroPayout()
        {
            var mission = MakeMission(5, 0);
            var result  = PayoutCalculator.Calculate(mission, null);
            Assert.AreEqual(0L, result.GrossPayout);
        }

        [Test]
        public void Calculate_PerfectRun_PassengerRevenue()
        {
            // 5 passengers, morale=1.0, multiplier=1.0 → 5 × 200 × Lerp(0.3,1,1.0) × 1.0 = ¤1000
            var dest    = MakeDest(multiplier: 1.0f, voyageMinutes: 240);
            var mission = MakeMission(passengers: 5, packages: 0, morale: 1f);
            var result  = PayoutCalculator.Calculate(mission, dest);
            Assert.AreEqual(1000, result.PassengerRevenue);
        }

        [Test]
        public void Calculate_PackageRevenue_ScalesWithCargoIntegrity()
        {
            // 10 packages, integrity=0.5, multiplier=1.0 → 10 × 50 × 0.5 = ¤250
            var dest    = MakeDest(multiplier: 1.0f, voyageMinutes: 240);
            var mission = MakeMission(passengers: 0, packages: 10, cargo: 0.5f);
            var result  = PayoutCalculator.Calculate(mission, dest);
            Assert.AreEqual(250, result.PackageRevenue);
        }

        [Test]
        public void Calculate_DestinationMultiplierScalesBothStreams()
        {
            var dest1x  = MakeDest(multiplier: 1.0f, voyageMinutes: 240);
            var dest2x  = MakeDest(multiplier: 2.0f, voyageMinutes: 240);
            var mission = MakeMission(passengers: 5, packages: 5);

            var r1 = PayoutCalculator.Calculate(mission, dest1x);
            var r2 = PayoutCalculator.Calculate(mission, dest2x);

            Assert.AreEqual(r1.GrossPayout * 2, r2.GrossPayout, delta: 2);
        }

        [Test]
        public void Calculate_LowHull_AppliesPenalty()
        {
            var dest       = MakeDest(multiplier: 1.0f, voyageMinutes: 240);
            var goodHull   = MakeMission(5, 0, hull: 1.0f);
            var badHull    = MakeMission(5, 0, hull: 0.3f);

            var goodResult = PayoutCalculator.Calculate(goodHull, dest);
            var badResult  = PayoutCalculator.Calculate(badHull, dest);

            Assert.Greater(goodResult.GrossPayout, badResult.GrossPayout);
        }

        [Test]
        public void Calculate_HullAbove50_NoPenalty()
        {
            var dest    = MakeDest(multiplier: 1.0f, voyageMinutes: 240);
            var mission = MakeMission(5, 0, hull: 0.6f);
            var result  = PayoutCalculator.Calculate(mission, dest);
            Assert.AreEqual(0f, result.HullPenaltyPct, 0.001f);
        }

        [Test]
        public void Calculate_LowMorale_ReducesPassengerRevenue()
        {
            var dest       = MakeDest(multiplier: 1.0f, voyageMinutes: 240);
            var highMorale = MakeMission(5, 0, morale: 1.0f);
            var lowMorale  = MakeMission(5, 0, morale: 0.0f);

            var highResult = PayoutCalculator.Calculate(highMorale, dest);
            var lowResult  = PayoutCalculator.Calculate(lowMorale, dest);

            Assert.Greater(highResult.PassengerRevenue, lowResult.PassengerRevenue);
        }

        [Test]
        public void Calculate_GoodHull_BoostsReputation()
        {
            var dest     = MakeDest(multiplier: 1.0f, voyageMinutes: 240);
            var goodHull = MakeMission(5, 0, hull: 0.9f);
            var badHull  = MakeMission(5, 0, hull: 0.5f);

            var goodRep = PayoutCalculator.Calculate(goodHull, dest);
            var badRep  = PayoutCalculator.Calculate(badHull, dest);

            Assert.Greater(goodRep.ReputationGain, badRep.ReputationGain);
        }

        [Test]
        public void Calculate_HigherMultiplier_ScalesReputationGain()
        {
            var shortDest = MakeDest(multiplier: 1.0f, voyageMinutes: 240);
            var longDest  = MakeDest(multiplier: 4.0f, voyageMinutes: 4320);
            var mission   = MakeMission(5, 0);

            var shortResult = PayoutCalculator.Calculate(mission, shortDest);
            var longResult  = PayoutCalculator.Calculate(mission, longDest);

            Assert.Greater(longResult.ReputationGain, shortResult.ReputationGain);
        }

        // ── Profitability sanity checks ───────────────────────────────────────

        [Test]
        public void ProximaB_5Passengers_NetPositive()
        {
            // Verify the starter mission is profitable on a clean run.
            var dest    = MakeDest(multiplier: 1.2f, voyageMinutes: 240);
            var cargo   = new CargoManifest { PassengerCount = 5 };
            var mission = MakeMission(5, 0);

            long cost   = PayoutCalculator.CalculateLaunchCost(dest, cargo);
            var  payout = PayoutCalculator.Calculate(mission, dest);

            Assert.Greater(payout.GrossPayout, cost, "Starter mission must be profitable on a clean run.");
        }

        [Test]
        public void Trappist1d_5Passengers_NetPositive()
        {
            var dest    = MakeDest(multiplier: 7.0f, voyageMinutes: 4320);
            var cargo   = new CargoManifest { PassengerCount = 5 };
            var mission = MakeMission(5, 0);

            long cost   = PayoutCalculator.CalculateLaunchCost(dest, cargo);
            var  payout = PayoutCalculator.Calculate(mission, dest);

            Assert.Greater(payout.GrossPayout, cost, "Long-haul mission must be profitable on a clean run.");
        }
    }
}
