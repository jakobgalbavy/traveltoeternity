using UnityEngine;
using DeepTransit.Cargo;
using DeepTransit.Missions;
using DeepTransit.Destinations;

namespace DeepTransit.Economy
{
    public static class PayoutCalculator
    {
        public const int   BasePassengerRate    = 200;
        public const int   BasePackageRate      = 50;
        public const float BaseReputationGain   = 5f;

        public const float FuelCostPerMinute       = 0.5f;
        public const int   SupplyCostPerPassenger   = 40;
        public const int   HandlingCostPerPackage   = 15;
        // Multiplier applied to deferred crew/supply costs at payout time.
        public const float DeferredPayPenalty = 1.30f;

        // Fuel is always paid upfront — can't fly without it.
        public static long FuelCost(DestinationSO destination) =>
            destination == null ? 0 : Mathf.RoundToInt(destination.VoyageMinutes * FuelCostPerMinute);

        // Crew supply + handling — can be deferred to payout time with a penalty.
        public static long DeferrableCost(CargoManifest cargo) =>
            (cargo?.PassengerCount ?? 0) * SupplyCostPerPassenger
            + (cargo?.PackageCount  ?? 0) * HandlingCostPerPackage;

        public static long CalculateLaunchCost(DestinationSO destination, CargoManifest cargo, bool deferCrewPay = false)
        {
            if (destination == null) return 0;
            long fuel = FuelCost(destination);
            return deferCrewPay ? fuel : fuel + DeferrableCost(cargo);
        }

        // Deferred crew cost settled at mission end (with 30% penalty).
        public static long DeferredSettlement(CargoManifest cargo) =>
            Mathf.RoundToInt(DeferrableCost(cargo) * DeferredPayPenalty);

        public static PayoutResult Calculate(Mission mission, DestinationSO destination)
        {
            if (destination == null || mission.Cargo == null)
                return new PayoutResult();

            float passengerPayout = mission.Cargo.PassengerCount
                * BasePassengerRate
                * Mathf.Lerp(0.3f, 1f, mission.CrewMorale)
                * destination.PayoutMultiplier;

            float packagePayout = mission.Cargo.PackageCount
                * BasePackageRate
                * mission.CargoIntegrity
                * destination.PayoutMultiplier;

            float hullPenalty = mission.HullIntegrity < 0.5f
                ? Mathf.Lerp(0.3f, 0f, mission.HullIntegrity / 0.5f)
                : 0f;

            long gross = Mathf.RoundToInt((passengerPayout + packagePayout) * (1f - hullPenalty));

            long deferredDeduction = 0;
            if (mission.DeferredCrewPay)
            {
                deferredDeduction = DeferredSettlement(mission.Cargo);
                gross = System.Math.Max(0L, gross - deferredDeduction);
            }

            float repGain = BaseReputationGain * destination.PayoutMultiplier
                * (mission.HullIntegrity > 0.7f ? 1.2f : 1f);

            return new PayoutResult
            {
                GrossPayout          = gross,
                ReputationGain       = repGain,
                PassengerRevenue     = Mathf.RoundToInt(passengerPayout),
                PackageRevenue       = Mathf.RoundToInt(packagePayout),
                HullPenaltyPct       = hullPenalty,
                MoraleFactor         = Mathf.Lerp(0.3f, 1f, mission.CrewMorale),
                CargoIntegrity       = mission.CargoIntegrity,
                DeferredPayDeduction = deferredDeduction,
            };
        }
    }

    public struct PayoutResult
    {
        public long  GrossPayout;
        public float ReputationGain;
        public int   PassengerRevenue;
        public int   PackageRevenue;
        public float HullPenaltyPct;
        public float MoraleFactor;
        public float CargoIntegrity;
        public long  DeferredPayDeduction;
    }
}
