using System;
using UnityEngine;

namespace DeepTransit.Ships
{
    [CreateAssetMenu(menuName = "DeepTransit/Ship Blueprint", fileName = "Blueprint_New")]
    public class ShipBlueprintSO : ScriptableObject
    {
        public string DisplayName;
        [TextArea] public string Description;

        // Each slot defines which module type it accepts.
        public ModuleSlot[] Slots;

        // Base stats before any module bonuses.
        public BaseStatValue[] BaseStats;

        public float GetBaseStat(ShipStat stat)
        {
            if (BaseStats == null) return 0f;
            foreach (var entry in BaseStats)
                if (entry.Stat == stat) return entry.Value;
            return 0f;
        }
    }

    [Serializable]
    public struct ModuleSlot
    {
        public string SlotName;       // e.g. "Primary Hull"
        public ModuleType AcceptedType;
    }

    [Serializable]
    public struct BaseStatValue
    {
        public ShipStat Stat;
        public float Value;
    }
}
