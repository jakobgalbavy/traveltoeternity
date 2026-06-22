using System;
using UnityEngine;

namespace DeepTransit.Ships
{
    [Serializable]
    public class ShipModuleInstance
    {
        public ShipModuleSO Definition;
        public int CurrentTier;          // index into Definition.Tiers
        public bool IsUpgrading;
        public long UpgradeCompleteMinute;

        public bool IsMaxTier => Definition == null || CurrentTier >= Definition.MaxTier;

        public UpgradeTier? NextTierData =>
            !IsMaxTier ? Definition.Tiers[CurrentTier + 1] : (UpgradeTier?)null;

        // Sum of all StatBonus values from tier 0 up to and including CurrentTier.
        public float TotalStatBonus()
        {
            if (Definition?.Tiers == null) return 0f;
            float total = 0f;
            for (int i = 0; i <= CurrentTier && i < Definition.Tiers.Length; i++)
                total += Definition.Tiers[i].StatBonus;
            return total;
        }

        public bool TryCompleteUpgrade(long gameMinute)
        {
            if (!IsUpgrading || gameMinute < UpgradeCompleteMinute) return false;
            CurrentTier++;
            IsUpgrading = false;
            return true;
        }
    }
}
