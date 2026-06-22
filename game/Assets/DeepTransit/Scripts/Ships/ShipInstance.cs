using System;
using UnityEngine;

namespace DeepTransit.Ships
{
    [Serializable]
    public class ShipInstance
    {
        public string Name;               // player-given, e.g. "ISV Horizon"
        public ShipBlueprintSO Blueprint;

        // Parallel to Blueprint.Slots — null means slot is empty.
        public ShipModuleInstance[] Modules;

        public bool IsOnMission;

        public void InitSlots()
        {
            if (Blueprint == null) return;
            Modules = new ShipModuleInstance[Blueprint.Slots.Length];
        }

        // Install a module into a slot. Returns false if the slot type doesn't match.
        public bool InstallModule(int slotIndex, ShipModuleSO definition)
        {
            if (Blueprint == null || slotIndex >= Blueprint.Slots.Length) return false;
            if (Blueprint.Slots[slotIndex].AcceptedType != definition.Type) return false;

            Modules[slotIndex] = new ShipModuleInstance { Definition = definition, CurrentTier = 0 };
            return true;
        }

        // Returns the total value of a stat: base + all installed module bonuses.
        public float GetStat(ShipStat stat)
        {
            float total = Blueprint != null ? Blueprint.GetBaseStat(stat) : 0f;
            if (Modules == null) return total;
            foreach (var module in Modules)
            {
                if (module?.Definition == null) continue;
                if (module.Definition.AffectedStat == stat)
                    total += module.TotalStatBonus();
            }
            return total;
        }

        // Begin upgrading a module. Returns false if not eligible.
        public bool StartUpgrade(int slotIndex, long currentMinute)
        {
            var module = Modules?[slotIndex];
            if (module == null || module.IsUpgrading || module.IsMaxTier) return false;

            var next = module.NextTierData;
            if (next == null) return false;

            module.IsUpgrading = true;
            module.UpgradeCompleteMinute = currentMinute + next.Value.UpgradeMinutes;
            return true;
        }

        // Called every game-minute tick; completes any finished upgrades.
        public void TickUpgrades(long gameMinute)
        {
            if (Modules == null) return;
            foreach (var module in Modules)
                module?.TryCompleteUpgrade(gameMinute);
        }
    }
}
