using NUnit.Framework;
using UnityEngine;
using DeepTransit.Ships;
using DeepTransit.Missions;

namespace DeepTransit.Tests
{
    public class ShipTests
    {
        ShipModuleSO MakeModule(ModuleType type, ShipStat stat, params float[] tierBonuses)
        {
            var so = ScriptableObject.CreateInstance<ShipModuleSO>();
            so.Type = type;
            so.AffectedStat = stat;
            so.Tiers = new UpgradeTier[tierBonuses.Length];
            for (int i = 0; i < tierBonuses.Length; i++)
                so.Tiers[i] = new UpgradeTier { StatBonus = tierBonuses[i], UpgradeMinutes = 60 };
            return so;
        }

        ShipBlueprintSO MakeBlueprint(params ModuleType[] slotTypes)
        {
            var bp = ScriptableObject.CreateInstance<ShipBlueprintSO>();
            bp.Slots = new ModuleSlot[slotTypes.Length];
            for (int i = 0; i < slotTypes.Length; i++)
                bp.Slots[i] = new ModuleSlot { AcceptedType = slotTypes[i] };
            bp.BaseStats = new BaseStatValue[]
            {
                new BaseStatValue { Stat = ShipStat.HullIntegrity, Value = 100f }
            };
            return bp;
        }

        [Test]
        public void GetStat_BaseStatReturnedWithNoModules()
        {
            var ship = new ShipInstance { Blueprint = MakeBlueprint(ModuleType.Hull) };
            ship.InitSlots();
            Assert.AreEqual(100f, ship.GetStat(ShipStat.HullIntegrity));
        }

        [Test]
        public void GetStat_InstalledModuleAddsBonus()
        {
            var bp = MakeBlueprint(ModuleType.Hull);
            var ship = new ShipInstance { Blueprint = bp };
            ship.InitSlots();
            ship.InstallModule(0, MakeModule(ModuleType.Hull, ShipStat.HullIntegrity, 50f));
            Assert.AreEqual(150f, ship.GetStat(ShipStat.HullIntegrity));
        }

        [Test]
        public void GetStat_UpgradedModuleSumsTiers()
        {
            var bp = MakeBlueprint(ModuleType.Hull);
            var ship = new ShipInstance { Blueprint = bp };
            ship.InitSlots();
            ship.InstallModule(0, MakeModule(ModuleType.Hull, ShipStat.HullIntegrity, 50f, 30f, 20f));

            ship.Modules[0].CurrentTier = 2;
            // 100 base + 50 + 30 + 20 = 200
            Assert.AreEqual(200f, ship.GetStat(ShipStat.HullIntegrity), 0.001f);
        }

        [Test]
        public void InstallModule_WrongType_ReturnsFalse()
        {
            var bp = MakeBlueprint(ModuleType.Hull);
            var ship = new ShipInstance { Blueprint = bp };
            ship.InitSlots();
            var engineModule = MakeModule(ModuleType.Engine, ShipStat.EngineEfficiency, 10f);
            Assert.IsFalse(ship.InstallModule(0, engineModule));
        }

        [Test]
        public void StartUpgrade_CompletesAfterTimer()
        {
            var bp = MakeBlueprint(ModuleType.Hull);
            var ship = new ShipInstance { Blueprint = bp };
            ship.InitSlots();
            ship.InstallModule(0, MakeModule(ModuleType.Hull, ShipStat.HullIntegrity, 50f, 30f));

            ship.StartUpgrade(0, currentMinute: 0);
            Assert.IsTrue(ship.Modules[0].IsUpgrading);

            ship.TickUpgrades(59);   // not done yet
            Assert.AreEqual(0, ship.Modules[0].CurrentTier);

            ship.TickUpgrades(60);   // done
            Assert.AreEqual(1, ship.Modules[0].CurrentTier);
            Assert.IsFalse(ship.Modules[0].IsUpgrading);
        }

        [Test]
        public void StartUpgrade_AtMaxTier_ReturnsFalse()
        {
            var bp = MakeBlueprint(ModuleType.Hull);
            var ship = new ShipInstance { Blueprint = bp };
            ship.InitSlots();
            ship.InstallModule(0, MakeModule(ModuleType.Hull, ShipStat.HullIntegrity, 50f));
            // Only 1 tier (index 0), already at max
            Assert.IsFalse(ship.StartUpgrade(0, 0));
        }

        // ── Engine efficiency / voyage time ──────────────────────────────────

        [Test]
        public void ApplyEngineEfficiency_AtBaseline_NoChange()
        {
            // Mk.I installed = efficiency 2.0 (blueprint 1.0 + Mk.I 1.0) = baseline
            long result = MissionManager.ApplyEngineEfficiency(240, MissionManager.BaselineEngineEfficiency);
            Assert.AreEqual(240L, result);
        }

        [Test]
        public void ApplyEngineEfficiency_BelowBaseline_NoSpeedPenalty()
        {
            // Ship with no engine module (efficiency = blueprint base 1.0 only)
            long result = MissionManager.ApplyEngineEfficiency(240, 1f);
            Assert.AreEqual(240L, result); // clamped — no penalty
        }

        [Test]
        public void ApplyEngineEfficiency_AboveBaseline_ReducesDuration()
        {
            // Mk.II total = 2.5 → factor = 2.5/2.0 = 1.25 → 240/1.25 = 192
            long result = MissionManager.ApplyEngineEfficiency(240, 2.5f);
            Assert.AreEqual(192L, result);
        }

        [Test]
        public void ApplyEngineEfficiency_MaxTier_SignificantReduction()
        {
            // Mk.V total = 6.5 → 240 / (6.5/2.0) = 240 / 3.25 ≈ 74
            long result = MissionManager.ApplyEngineEfficiency(240, 6.5f);
            Assert.Less(result, 100L);
            Assert.Greater(result, 0L);
        }

        [Test]
        public void ApplyEngineEfficiency_HigherEfficiency_AlwaysFaster()
        {
            long tier1 = MissionManager.ApplyEngineEfficiency(1440, 2.5f);
            long tier4 = MissionManager.ApplyEngineEfficiency(1440, 6.5f);
            Assert.Less(tier4, tier1);
        }
    }
}
