// Editor-only utility. Run via Tools → Deep Transit → Generate Starter Data.
// Creates all ScriptableObject assets needed for first play.
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using DeepTransit.Destinations;
using DeepTransit.Contractors;
using DeepTransit.Events;
using DeepTransit.Ships;

namespace DeepTransit.Editor
{
    public static class StarterDataGenerator
    {
        const string Root = "Assets/DeepTransit/ScriptableObjects";
        const string ResRoot = "Assets/Resources";

        [MenuItem("Tools/Deep Transit/Generate Starter Data")]
        public static void Generate()
        {
            EnsureDirs();
            CreateDestinations();
            CreateContractors();
            CreateModules();
            CreateBlueprints();
            CreateEvents();
            CreateStarMap();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[DTA] Starter data generated.");
        }

        static void EnsureDirs()
        {
            string[] dirs = {
                $"{Root}/Destinations", $"{Root}/Contractors", $"{Root}/Modules",
                $"{Root}/Ships", $"{Root}/Events",
                $"{ResRoot}/Destinations", $"{ResRoot}/Contractors",
                $"{ResRoot}/Modules", $"{ResRoot}/Blueprints", $"{ResRoot}/Events",
            };
            foreach (var d in dirs)
                EnsureFolder(d);
        }

        // Creates every folder in the path, starting from Assets/, if it doesn't exist.
        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        // ── DESTINATIONS ──────────────────────────────────────────────────────

        static void CreateDestinations()
        {
            var proxima = Make<DestinationSO>($"{ResRoot}/Destinations", "Destination_ProximaB");
            proxima.Id = "proxima-b";
            proxima.DisplayName = "Proxima Centauri b";
            proxima.Description = "Our nearest stellar neighbour. A rocky world in the habitable zone — marginal but viable.";
            proxima.DistanceLY = 4.24f;
            proxima.VoyageMinutes = 240;        // 4h game-time
            proxima.PayoutMultiplier = 1.2f;
            proxima.ReputationRequired = 0;
            proxima.MissionType = MissionType.Basic;
            proxima.MinimumPayout = 100;
            proxima.Hazards = new HazardProfile { MeteoriteRisk = 0.1f, RadiationRisk = 0.2f, MechanicalFailureRisk = 0.15f };

            var kepler = Make<DestinationSO>($"{ResRoot}/Destinations", "Destination_Kepler452b");
            kepler.Id = "kepler-452b";
            kepler.DisplayName = "Kepler-452b";
            kepler.Description = "Earth's cousin. High payout, long voyage, rough radiation belts.";
            kepler.DistanceLY = 1400f;
            kepler.VoyageMinutes = 1440;        // 1 game-day
            kepler.PayoutMultiplier = 3.5f;
            kepler.ReputationRequired = 20;
            kepler.MissionType = MissionType.Basic;
            kepler.MinimumPayout = 300;
            kepler.Hazards = new HazardProfile { MeteoriteRisk = 0.2f, RadiationRisk = 0.45f, MechanicalFailureRisk = 0.3f };
            proxima.UnlocksOnFirstCompletion = new[] { kepler };
            EditorUtility.SetDirty(proxima);

            var trappist = Make<DestinationSO>($"{ResRoot}/Destinations", "Destination_Trappist1d");
            trappist.Id = "trappist-1d";
            trappist.DisplayName = "TRAPPIST-1d";
            trappist.Description = "One of seven worlds around a cool red dwarf. Exotic, dangerous, extremely lucrative.";
            trappist.DistanceLY = 39f;
            trappist.VoyageMinutes = 4320;      // 3 game-days
            trappist.PayoutMultiplier = 7f;
            trappist.ReputationRequired = 60;
            trappist.MissionType = MissionType.Basic;
            trappist.MinimumPayout = 750;
            trappist.Hazards = new HazardProfile { MeteoriteRisk = 0.35f, RadiationRisk = 0.6f, MechanicalFailureRisk = 0.5f, PassengerUnrestRisk = 0.3f };
            kepler.UnlocksOnFirstCompletion = new[] { trappist };
            EditorUtility.SetDirty(kepler);

            var starMap = Make<StarMapSO>($"{Root}", "StarMap");
            starMap.AllDestinations = new[] { proxima, kepler, trappist };
            EditorUtility.SetDirty(starMap);
        }

        // ── CONTRACTORS ───────────────────────────────────────────────────────

        static void CreateContractors()
        {
            Contractor("GeneralCrew",       ContractorRole.GeneralCrew,       80,  1.5f, 0.55f, 0.2f,  "Reliable all-rounder. Won't specialize, won't complain.");
            Contractor("Engineer",          ContractorRole.Engineer,          150, 2.0f, 0.65f, 0.25f, "Systems and hull specialist. Keeps the ship breathing.");
            Contractor("Botanist",          ContractorRole.Botanist,          130, 1.8f, 0.7f,  0.25f, "Grow bay management. Half botanist, half miracle worker.");
            Contractor("Medic",             ContractorRole.Medic,             160, 2.2f, 0.7f,  0.25f, "Ship's doctor. Worth every credit when illness hits.");
            Contractor("Navigator",         ContractorRole.Navigator,         180, 2.5f, 0.75f, 0.2f,  "Course corrections and hazard avoidance.");
            Contractor("SecurityOfficer",   ContractorRole.SecurityOfficer,   140, 1.8f, 0.65f, 0.3f,  "Conflict resolution by any means necessary.");
            Contractor("CargoMaster",       ContractorRole.CargoMaster,       120, 1.6f, 0.7f,  0.25f, "Freight integrity and manifest management.");
            Contractor("PassengerLiaison",  ContractorRole.PassengerLiaison,  140, 1.9f, 0.7f,  0.25f, "Keeps colonists calm. Essential on long voyages.");
            Contractor("HazmatSpecialist",  ContractorRole.HazmatSpecialist,  220, 2.8f, 0.75f, 0.2f,  "Required for biological and chemical freight.");
            Contractor("CryoTechnician",    ContractorRole.CryoTechnician,    280, 3.0f, 0.8f,  0.2f,  "Cryo systems maintenance. Rare. Expensive. Necessary.");
        }

        static void Contractor(string assetName, ContractorRole role, int rate, float maxMult,
            float baseChance, float bonus, string bio)
        {
            var so = Make<ContractorSO>($"{ResRoot}/Contractors", $"Contractor_{assetName}");
            so.Id = assetName.ToLower();
            so.DisplayName = assetName.Replace("Officer","Officer").Replace("Master","Master");
            so.Role = role;
            so.BaseDailyRate = rate;
            so.MaxExperienceMultiplier = maxMult;
            so.BaseSuccessChance = baseChance;
            so.SuccessChanceBonus = bonus;
            so.Bio = bio;
            EditorUtility.SetDirty(so);
        }

        // ── SHIP MODULES ──────────────────────────────────────────────────────

        static void CreateModules()
        {
            Module("Hull_Basic", "Basic Hull Plating", Ships.ModuleType.Hull, Ships.ShipStat.HullIntegrity,
                new[] {("Mk.I", 0, 0L, 100f), ("Mk.II", 500, 60L, 30f), ("Mk.III", 1500, 240L, 50f),
                       ("Mk.IV", 4000, 720L, 80f), ("Mk.V",  10000, 2880L, 120f)});

            Module("Engine_Basic", "Drive Assembly", Ships.ModuleType.Engine, Ships.ShipStat.EngineEfficiency,
                new[] {("Mk.I", 0, 0L, 1f), ("Mk.II", 400, 60L, 0.5f), ("Mk.III", 1200, 300L, 0.8f),
                       ("Mk.IV", 3500, 900L, 1.2f), ("Mk.V", 8000, 3600L, 2f)});

            Module("LifeSupport", "Life Support Array", Ships.ModuleType.LifeSupport, Ships.ShipStat.LifeSupportQuality,
                new[] {("Mk.I", 0, 0L, 1f), ("Mk.II", 600, 90L, 0.5f), ("Mk.III", 1800, 360L, 0.8f),
                       ("Mk.IV", 5000, 1440L, 1.5f)});

            Module("GrowBay", "Grow Bay", Ships.ModuleType.GrowBay, Ships.ShipStat.FoodProduction,
                new[] {("Basic", 0, 0L, 5f), ("Expanded", 700, 120L, 3f), ("Hydroponic", 2000, 480L, 5f),
                       ("Automated", 6000, 2160L, 8f)});

            Module("MedBay", "Medical Bay", Ships.ModuleType.MedicalBay, Ships.ShipStat.MedicalCapacity,
                new[] {("Field Kit", 0, 0L, 1f), ("Infirmary", 800, 150L, 1f), ("Surgical Suite", 2500, 600L, 2f)});

            Module("Berths", "Passenger Berths", Ships.ModuleType.PassengerBerths, Ships.ShipStat.PassengerCapacity,
                new[] {("Spartan ×10", 0, 0L, 10f), ("Standard ×5", 600, 90L, 5f), ("Comfort ×5", 1800, 360L, 5f),
                       ("Premium ×10", 5000, 1800L, 10f)});

            Module("CargoHold", "Cargo Hold", Ships.ModuleType.CargoHold, Ships.ShipStat.CargoCapacity,
                new[] {("Small ×20", 0, 0L, 20f), ("Medium ×20", 500, 60L, 20f), ("Large ×30", 1500, 300L, 30f),
                       ("Reinforced ×50", 4000, 1440L, 50f)});

            Module("Navigation", "Navigation Suite", Ships.ModuleType.Navigation, Ships.ShipStat.NavigationRating,
                new[] {("Basic", 0, 0L, 1f), ("Enhanced", 900, 180L, 1f), ("Deep-Space", 3000, 900L, 2f)});

            Module("Security", "Security Systems", Ships.ModuleType.Security, Ships.ShipStat.SecurityLevel,
                new[] {("Standard", 0, 0L, 1f), ("Reinforced", 700, 120L, 1f), ("Automated", 2200, 600L, 2f)});
        }

        static void Module(string assetName, string displayName, Ships.ModuleType type,
            Ships.ShipStat stat, (string label, int cost, long mins, float bonus)[] tiers)
        {
            var so = Make<ShipModuleSO>($"{ResRoot}/Modules", $"Module_{assetName}");
            so.DisplayName = displayName;
            so.Type = type;
            so.AffectedStat = stat;
            so.Tiers = new UpgradeTier[tiers.Length];
            for (int i = 0; i < tiers.Length; i++)
                so.Tiers[i] = new UpgradeTier
                {
                    Label = tiers[i].label,
                    CostSoftCurrency = tiers[i].cost,
                    UpgradeMinutes = tiers[i].mins,
                    StatBonus = tiers[i].bonus,
                };
            EditorUtility.SetDirty(so);
        }

        // ── SHIP BLUEPRINTS ───────────────────────────────────────────────────

        static void CreateBlueprints()
        {
            var bp = Make<ShipBlueprintSO>($"{ResRoot}/Blueprints", "Blueprint_StarterFreighter");
            bp.DisplayName = "Starter Freighter";
            bp.Description = "A modest multi-purpose vessel. Dependable. Room to grow.";
            bp.Slots = new[]
            {
                new ModuleSlot { SlotName = "Primary Hull",    AcceptedType = ModuleType.Hull },
                new ModuleSlot { SlotName = "Drive Assembly",  AcceptedType = ModuleType.Engine },
                new ModuleSlot { SlotName = "Life Support",    AcceptedType = ModuleType.LifeSupport },
                new ModuleSlot { SlotName = "Grow Bay",        AcceptedType = ModuleType.GrowBay },
                new ModuleSlot { SlotName = "Medical Bay",     AcceptedType = ModuleType.MedicalBay },
                new ModuleSlot { SlotName = "Passenger Berths",AcceptedType = ModuleType.PassengerBerths },
                new ModuleSlot { SlotName = "Cargo Hold",      AcceptedType = ModuleType.CargoHold },
                new ModuleSlot { SlotName = "Navigation",      AcceptedType = ModuleType.Navigation },
                new ModuleSlot { SlotName = "Security",        AcceptedType = ModuleType.Security },
            };
            bp.BaseStats = new[]
            {
                new BaseStatValue { Stat = ShipStat.HullIntegrity, Value = 50f },
                new BaseStatValue { Stat = ShipStat.EngineEfficiency, Value = 1f },
            };
            EditorUtility.SetDirty(bp);
        }

        // ── EVENTS ────────────────────────────────────────────────────────────

        static void CreateEvents()
        {
            // ── HULL FAILURE CHAIN ─────────────────────────────────────────────
            // Leak → Breach → OxygenLoss → CrewHypoxia → CropBlight
            // Each link escalates to the next if left unresolved past its deadline.

            var cropBlight = Event("Event_CropBlight", "Crop Blight", EventSeverity.Moderate,
                "A fungal infection has spread through the grow bay. The crew is too impaired to tend it.",
                0.06f, EventPrecondition.None,
                new[]
                {
                    Option("Treat Infection", ContractorRole.Botanist, 0.6f, 0.3f,
                        Outcome(food: 0f, morale: 0.05f, msg: "Blight contained. Crops recovering."),
                        Outcome(food: -0.2f, morale: -0.15f, msg: "Blight spread. Significant crop loss.")),
                    Option("Emergency Harvest", ContractorRole.GeneralCrew, 0.8f, 0.1f,
                        Outcome(food: -0.05f, morale: 0f, msg: "Early harvest completed. Some loss."),
                        Outcome(food: -0.15f, morale: -0.05f, msg: "Early harvest botched. Heavy spoilage.")),
                }, escalation: null, escalationMins: 180);

            // ChancePerHour = 0 → only reachable via chain escalation, never fires randomly.
            var crewHypoxia = Event("Event_CrewHypoxia", "Crew Hypoxia", EventSeverity.Critical,
                "CO₂ saturation is dangerously high. Crew are incapacitated and unable to maintain ship systems.",
                0f, EventPrecondition.None,
                new[]
                {
                    Option("Emergency Scrubber Override", ContractorRole.Engineer, 0.55f, 0.35f,
                        Outcome(morale: -0.1f, msg: "Scrubbers cleared. Crew recovering slowly."),
                        Outcome(morale: -0.3f, hullDmg: -0.05f, msg: "Override failed. Crew incapacitation deepens.")),
                    Option("Administer Stimulants", ContractorRole.Medic, 0.7f, 0.25f,
                        Outcome(morale: -0.05f, msg: "Stimulants buying time. Oxygen situation still critical."),
                        Outcome(morale: -0.2f, msg: "Stimulants ineffective. Crew deteriorating.")),
                }, escalation: cropBlight, escalationMins: 60);

            var oxygenLoss = Event("Event_OxygenLoss", "Oxygen Depletion", EventSeverity.Critical,
                "Cabin pressure is falling. The breach has compromised the oxygen recycler loop.",
                0f, EventPrecondition.None,
                new[]
                {
                    Option("Reroute Life Support", ContractorRole.Engineer, 0.6f, 0.3f,
                        Outcome(morale: -0.1f, msg: "Life support rerouted. O₂ levels stabilising."),
                        Outcome(morale: -0.2f, hullDmg: -0.05f, msg: "Reroute failed. Pressure still dropping.")),
                    Option("Emergency O₂ Canisters", ContractorRole.GeneralCrew, 0.85f, 0.1f,
                        Outcome(morale: 0f, msg: "Canisters deployed. Buys time but won't last."),
                        Outcome(morale: -0.1f, msg: "Canister deployment too slow. CO₂ rising.")),
                }, escalation: crewHypoxia, escalationMins: 45);

            var breach = Event("Event_HullBreach", "Hull Breach", EventSeverity.Critical,
                "A section of hull has failed. Decompression in progress — oxygen is venting.",
                0.02f, EventPrecondition.LowHull,
                new[]
                {
                    Option("Emergency Seal", ContractorRole.Engineer, 0.5f, 0.35f,
                        Outcome(hullDmg: -0.15f, morale: -0.1f, msg: "Breach sealed. Structural integrity reduced."),
                        Outcome(hullDmg: -0.35f, morale: -0.25f, msg: "Seal failed. Major decompression event.")),
                }, escalation: oxygenLoss, escalationMins: 60);

            Event("Event_HullLeak", "Hull Micro-Leak", EventSeverity.Minor,
                "A hairline fracture has been detected in the outer hull. Pressure is slowly dropping.",
                0.08f, EventPrecondition.None,
                new[]
                {
                    Option("Repair Crew", ContractorRole.Engineer, 0.7f, 0.2f,
                        Outcome(hullDmg: 0f, morale: 0.05f, msg: "Leak patched. No further damage."),
                        Outcome(hullDmg: -0.1f, morale: -0.05f, msg: "Repair failed. Leak worsened.")),
                    Option("Temporary Sealant", ContractorRole.GeneralCrew, 0.5f, 0.1f,
                        Outcome(hullDmg: -0.02f, morale: 0f, msg: "Sealant applied. Buys time but the leak persists."),
                        Outcome(hullDmg: -0.08f, morale: -0.1f, msg: "Sealant failed. Leak spreading."),
                        isPartialFix: true),
                }, escalation: breach, escalationMins: 120);

            // ── STANDALONE EVENTS ──────────────────────────────────────────────

            Event("Event_PassengerConflict", "Passenger Conflict", EventSeverity.Moderate,
                "A fight has broken out between colonist factions. Morale on deck is deteriorating.",
                0.07f, EventPrecondition.HasPassengers,
                new[]
                {
                    Option("Mediate", ContractorRole.PassengerLiaison, 0.65f, 0.3f,
                        Outcome(morale: 0.1f, msg: "Conflict de-escalated. Tensions remain but manageable."),
                        Outcome(morale: -0.2f, msg: "Mediation failed. Situation escalated.")),
                    Option("Security Intervention", ContractorRole.SecurityOfficer, 0.7f, 0.25f,
                        Outcome(morale: -0.05f, msg: "Parties separated. Uneasy calm restored."),
                        Outcome(morale: -0.15f, msg: "Security overwhelmed. Brawl spread.")),
                }, escalation: null);

            Event("Event_MedicalEmergency", "Medical Emergency", EventSeverity.Moderate,
                "Multiple crew members have reported severe symptoms. Source unknown.",
                0.05f, EventPrecondition.None,
                new[]
                {
                    Option("Treat Patients", ContractorRole.Medic, 0.55f, 0.4f,
                        Outcome(morale: 0.05f, msg: "Illness contained. Full recovery expected."),
                        Outcome(morale: -0.2f, food: -0.05f, msg: "Illness spread. Quarantine implemented.")),
                }, escalation: null);

            Event("Event_MeteoriteStrike", "Meteorite Strike", EventSeverity.Critical,
                "A meteorite has impacted the port side. Hull integrity is compromised.",
                0.03f, EventPrecondition.None,
                new[]
                {
                    Option("Emergency Repair", ContractorRole.Engineer, 0.6f, 0.3f,
                        Outcome(hullDmg: -0.1f, msg: "Damage contained. Hull stable."),
                        Outcome(hullDmg: -0.3f, morale: -0.2f, msg: "Repair incomplete. Structural damage critical.")),
                }, escalation: breach);

            Event("Event_CargoShift", "Cargo Shift", EventSeverity.Minor,
                "Unsecured cargo has shifted during a correction burn. Potential damage to freight.",
                0.07f, EventPrecondition.HasPackages,
                new[]
                {
                    Option("Secure Cargo", ContractorRole.CargoMaster, 0.75f, 0.2f,
                        Outcome(cargoDmg: -0.02f, msg: "Cargo re-secured. Minimal loss."),
                        Outcome(cargoDmg: -0.15f, morale: -0.05f, msg: "Securing failed. Significant cargo damaged.")),
                }, escalation: null);

            // ── NEW EVENTS ─────────────────────────────────────────────────────

            Event("Event_EngineMalfunction", "Engine Malfunction", EventSeverity.Moderate,
                "Irregular drive output detected. Thrust is degrading and structural stress is mounting. Uncorrected, the vibrations will crack the hull.",
                0.10f, EventPrecondition.None,
                new[]
                {
                    Option("Diagnostic Repair", ContractorRole.Engineer, 0.60f, 0.30f,
                        Outcome(morale: 0.03f, msg: "Drive repaired. Output nominal."),
                        Outcome(hullDmg: -0.10f, morale: -0.05f, msg: "Repair failed. Vibrations worsening.")),
                    Option("Emergency Shutdown", ContractorRole.GeneralCrew, 0.78f, 0.10f,
                        Outcome(morale: -0.05f, msg: "Drive shut down safely. Speed reduced but ship is stable."),
                        Outcome(hullDmg: -0.08f, morale: -0.10f, msg: "Shutdown botched. Surge before cutoff.")),
                }, escalation: breach, escalationMins: 90);

            Event("Event_RadiationSurge", "Solar Radiation Surge", EventSeverity.Moderate,
                "An unexpected solar flare is flooding the ship with hard radiation. Crew exposure is rising and sensitive cargo is at risk.",
                0.06f, EventPrecondition.None,
                new[]
                {
                    Option("Deploy Shielding", ContractorRole.HazmatSpecialist, 0.65f, 0.30f,
                        Outcome(morale: 0.02f, msg: "Radiation baffles deployed. Exposure controlled."),
                        Outcome(morale: -0.10f, cargoDmg: -0.05f, msg: "Shielding deployment failed. Exposure spreading.")),
                    Option("Crew Shelter Protocol", ContractorRole.Medic, 0.75f, 0.20f,
                        Outcome(morale: -0.05f, msg: "Crew sheltered. Radiation exposure minimal."),
                        Outcome(morale: -0.15f, hullDmg: -0.05f, msg: "Shelter protocol too slow. Crew exposed.")),
                }, escalation: null, escalationMins: 60);

            Event("Event_NavigationDrift", "Navigation Drift", EventSeverity.Minor,
                "Calibration error in the nav array has allowed course drift to accumulate. A correction burn is needed or we'll miss the arrival window.",
                0.09f, EventPrecondition.None,
                new[]
                {
                    Option("Recalibrate Array", ContractorRole.Navigator, 0.70f, 0.25f,
                        Outcome(morale: 0.02f, msg: "Array recalibrated. Back on course."),
                        Outcome(cargoDmg: -0.08f, morale: -0.05f, msg: "Calibration failed. Rough correction burn required.")),
                    Option("Manual Override", ContractorRole.GeneralCrew, 0.48f, 0.05f,
                        Outcome(morale: -0.03f, msg: "Manual correction complete. Slightly behind schedule."),
                        Outcome(cargoDmg: -0.12f, morale: -0.10f, msg: "Manual correction overshoot. Cargo rattled.")),
                }, escalation: null, escalationMins: 90);

            Event("Event_CryoFault", "Cryo Bay Fault", EventSeverity.Critical,
                "Temperature variance detected in Cryo Bay 2. Occupants are at risk of premature thaw if the fault is not isolated immediately.",
                0.05f, EventPrecondition.HasPassengers,
                new[]
                {
                    Option("Cryo Tech Intervention", ContractorRole.CryoTechnician, 0.72f, 0.28f,
                        Outcome(morale: 0.05f, msg: "Cryo systems stabilised. All occupants safe."),
                        Outcome(morale: -0.20f, cargoDmg: -0.05f, msg: "Fault cascaded. Multiple premature thaws.")),
                    Option("Emergency Isolation", ContractorRole.Engineer, 0.55f, 0.25f,
                        Outcome(morale: -0.05f, msg: "Bay isolated. Occupants transferred to contingency berths."),
                        Outcome(morale: -0.15f, hullDmg: -0.03f, msg: "Isolation failed. Thermal runaway in bay systems.")),
                }, escalation: null, escalationMins: 60);

            Event("Event_RefrigerationFailure", "Refrigeration Failure", EventSeverity.Moderate,
                "Cold storage units have malfunctioned. Provisions are warming rapidly. If not addressed, spoilage will cascade to the grow bay.",
                0.08f, EventPrecondition.None,
                new[]
                {
                    Option("Repair Cold Storage", ContractorRole.RefrigerationTech, 0.65f, 0.30f,
                        Outcome(food: 0.03f, msg: "Cold storage repaired. Provisions saved."),
                        Outcome(food: -0.15f, morale: -0.08f, msg: "Repair failed. Significant spoilage.")),
                    Option("Emergency Rationing", ContractorRole.GeneralCrew, 0.82f, 0.08f,
                        Outcome(food: -0.07f, morale: -0.05f, msg: "Rationing implemented. Most provisions preserved."),
                        Outcome(food: -0.20f, morale: -0.12f, msg: "Rationing too late. Heavy spoilage.")),
                }, escalation: cropBlight, escalationMins: 75);

            Event("Event_HazardousCargo", "Hazardous Cargo Leak", EventSeverity.Moderate,
                "Atmospheric sensors detect contamination from improperly stored freight. Crew health and hull integrity are at risk from exposure.",
                0.06f, EventPrecondition.HasPackages,
                new[]
                {
                    Option("Containment Protocol", ContractorRole.HazmatSpecialist, 0.65f, 0.35f,
                        Outcome(morale: 0.02f, msg: "Leak contained. Atmosphere scrubbed."),
                        Outcome(morale: -0.12f, hullDmg: -0.05f, msg: "Containment failed. Corrosive spread to hull.")),
                    Option("Jettison Affected Cargo", ContractorRole.CargoMaster, 0.82f, 0.18f,
                        Outcome(cargoDmg: -0.08f, msg: "Affected cargo jettisoned. Crew safe."),
                        Outcome(cargoDmg: -0.18f, morale: -0.08f, hullDmg: -0.03f, msg: "Jettison malfunction. Leak spread.")),
                }, escalation: null, escalationMins: 75);
        }

        static GameEventSO Event(string assetName, string title, EventSeverity severity,
            string desc, float chancePerHour, EventPrecondition precondition,
            EventOption[] options, GameEventSO escalation = null, long escalationMins = 120)
        {
            var so = Make<GameEventSO>($"{ResRoot}/Events", assetName);
            so.Id = assetName;
            so.Title = title;
            so.Severity = severity;
            so.Description = desc;
            so.ChancePerHour = chancePerHour;
            so.Precondition = precondition;
            so.Options = options;
            so.Escalation = escalation;
            so.EscalationMinutes = escalationMins;
            EditorUtility.SetDirty(so);
            return so;
        }

        static EventOption Option(string label, ContractorRole role, float baseChance, float bonus,
            EventOutcome onSuccess, EventOutcome onFailure, bool isPartialFix = false) =>
            new EventOption
            {
                Label = label, RequiredRole = role,
                BaseSuccessChance = baseChance, ContractorBonus = bonus,
                IsPartialFix = isPartialFix,
                OnSuccess = onSuccess, OnFailure = onFailure,
            };

        static EventOutcome Outcome(float hullDmg = 0f, float morale = 0f,
            float cargoDmg = 0f, float food = 0f, string msg = "") =>
            new EventOutcome { HullDamage = hullDmg, MoraleDelta = morale,
                               CargoDamage = cargoDmg, FoodDelta = food, LogMessage = msg };

        // ── STAR MAP ──────────────────────────────────────────────────────────

        static void CreateStarMap()
        {
            // StarMap asset already created in CreateDestinations; nothing extra needed.
        }

        // ── UTILITY ───────────────────────────────────────────────────────────

        static T Make<T>(string folder, string assetName) where T : ScriptableObject
        {
            string path = $"{folder}/{assetName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;
            var so = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(so, path);
            return so;
        }
    }
}
#endif
