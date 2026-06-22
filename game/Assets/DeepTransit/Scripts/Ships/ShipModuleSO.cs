using System;
using UnityEngine;

namespace DeepTransit.Ships
{
    [CreateAssetMenu(menuName = "DeepTransit/Ship Module", fileName = "Module_New")]
    public class ShipModuleSO : ScriptableObject
    {
        public string DisplayName;
        [TextArea] public string Description;
        public ModuleType Type;
        public ShipStat AffectedStat;

        // Tier 0 = base install (free, instant).
        // Each subsequent tier is an upgrade. Index = tier number.
        // Tiers are cumulative: stat at tier N = sum of Tiers[0..N].StatBonus.
        public UpgradeTier[] Tiers;

        public int MaxTier => Tiers != null ? Tiers.Length - 1 : 0;
    }

    [Serializable]
    public struct UpgradeTier
    {
        public string Label;           // e.g. "Reinforced Plating Mk.II"
        public int CostSoftCurrency;
        public long UpgradeMinutes;    // real-time minutes for the upgrade timer
        public float StatBonus;        // incremental bonus added at this tier
    }
}
