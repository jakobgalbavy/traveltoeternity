using UnityEngine;
using DeepTransit.Missions;
using DeepTransit.Destinations;

namespace DeepTransit.Economy
{
    public static class PayoutCalculator
    {
        // Base payout per passenger delivered alive.
        public const int BasePassengerRate = 200;
        // Base payout per package delivered intact.
        public const int BasePackageRate = 50;
        // Reputation gained per successful mission, scaled by distance.
        public const float BaseReputationGain = 5f;

        public static PayoutResult Calculate(Mission mission, DestinationSO destination)
        {
            if (destination == null || mission.Cargo == null)
                return new PayoutResult();

            // Passenger payout: per-head × morale modifier × destination multiplier
            float passengerPayout = mission.Cargo.PassengerCount
                * BasePassengerRate
                * Mathf.Lerp(0.3f, 1f, mission.CrewMorale)
                * destination.PayoutMultiplier;

            // Package payout: per-unit × cargo integrity × destination multiplier
            float packagePayout = mission.Cargo.PackageCount
                * BasePackageRate
                * mission.CargoIntegrity
                * destination.PayoutMultiplier;

            // Hull condition penalty: below 50% hull costs a fine
            float hullPenalty = mission.HullIntegrity < 0.5f
                ? Mathf.Lerp(0.3f, 0f, mission.HullIntegrity / 0.5f)
                : 0f;

            long gross = Mathf.RoundToInt((passengerPayout + packagePayout) * (1f - hullPenalty));
            float repGain = BaseReputationGain * destination.PayoutMultiplier
                * (mission.HullIntegrity > 0.7f ? 1.2f : 1f);

            return new PayoutResult
            {
                GrossPayout       = gross,
                ReputationGain    = repGain,
                PassengerRevenue  = Mathf.RoundToInt(passengerPayout),
                PackageRevenue    = Mathf.RoundToInt(packagePayout),
                HullPenaltyPct    = hullPenalty,
                MoraleFactor      = Mathf.Lerp(0.3f, 1f, mission.CrewMorale),
                CargoIntegrity    = mission.CargoIntegrity,
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
    }
}
