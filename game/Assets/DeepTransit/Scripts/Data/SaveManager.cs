using System.IO;
using UnityEngine;
using DeepTransit.Core;
using DeepTransit.Contractors;
using DeepTransit.Missions;
using DeepTransit.Ships;
using DeepTransit.Cargo;

namespace DeepTransit.Data
{
    public class SaveManager : MonoBehaviour
    {
        const string SaveFileName = "deepTransit_save.json";
        string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        public void Save()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            var state = new GameState
            {
                SoftCurrency        = gm.CurrencyManager.SoftCurrency,
                HardCurrency        = gm.CurrencyManager.HardCurrency,
                Reputation          = gm.CurrencyManager.Reputation,
                ElapsedGameMinutes  = gm.TimeManager.ElapsedGameMinutes,
                UnlockedDestinationIds = gm.StarMapManager.SaveUnlocked(),
            };

            foreach (var mission in gm.MissionManager.ActiveMissions)
                state.ActiveMissions.Add(MissionToSave(mission));

            foreach (var contractor in gm.ContractorManager.Roster)
                state.ContractorRoster.Add(ContractorToSave(contractor));

            foreach (var ship in gm.ShipManager.Ships)
                state.Ships.Add(ShipToSave(ship));

            File.WriteAllText(SavePath, JsonUtility.ToJson(state, true));
            Debug.Log($"[DTA] Game saved to {SavePath}");
        }

        public bool Load()
        {
            if (!File.Exists(SavePath)) return false;
            var state = JsonUtility.FromJson<GameState>(File.ReadAllText(SavePath));
            if (state == null) return false;

            var gm = GameManager.Instance;
            gm.CurrencyManager.Load(state.SoftCurrency, state.HardCurrency, state.Reputation);
            gm.StarMapManager.LoadUnlocked(state.UnlockedDestinationIds);

            foreach (var sd in state.ContractorRoster)
            {
                var def = Resources.Load<Contractors.ContractorSO>($"Contractors/{sd.DefinitionName}");
                if (def == null) continue;
                var c = new ContractorInstance
                {
                    InstanceId  = sd.InstanceId,
                    Definition  = def,
                    DisplayName = sd.DisplayName,
                    Experience  = sd.Experience,
                    IsOnMission = sd.IsOnMission,
                };
                gm.ContractorManager.Roster.Add(c);
            }

            foreach (var sd in state.Ships)
            {
                var bp = Resources.Load<Ships.ShipBlueprintSO>($"Blueprints/{sd.BlueprintName}");
                if (bp == null) continue;
                var ship = gm.ShipManager.CreateShip(sd.Name, bp);
                ship.IsOnMission = sd.IsOnMission;
                foreach (var md in sd.Modules)
                {
                    var mod = Resources.Load<Ships.ShipModuleSO>($"Modules/{md.ModuleName}");
                    if (mod == null) continue;
                    ship.InstallModule(md.SlotIndex, mod);
                    ship.Modules[md.SlotIndex].CurrentTier            = md.CurrentTier;
                    ship.Modules[md.SlotIndex].IsUpgrading            = md.IsUpgrading;
                    ship.Modules[md.SlotIndex].UpgradeCompleteMinute  = md.UpgradeCompleteMinute;
                }
            }

            foreach (var md in state.ActiveMissions)
            {
                var mission = new Mission
                {
                    Id = md.Id, ShipName = md.ShipName, DestinationId = md.DestinationId,
                    DurationMinutes = md.DurationMinutes, LaunchMinute = md.LaunchMinute,
                    HullIntegrity = md.HullIntegrity, CrewMorale = md.CrewMorale,
                    CargoIntegrity = md.CargoIntegrity, FoodSupply = md.FoodSupply,
                    Cargo = new CargoManifest { PassengerCount = md.PassengerCount, PackageCount = md.PackageCount },
                    AssignedContractorIds = md.AssignedContractorIds,
                    Status = MissionStatus.InTransit,
                };
                gm.MissionManager.ActiveMissions.Add(mission);
            }

            return true;
        }

        MissionSaveData MissionToSave(Mission m) => new()
        {
            Id = m.Id, ShipName = m.ShipName, DestinationId = m.DestinationId,
            DurationMinutes = m.DurationMinutes, LaunchMinute = m.LaunchMinute,
            HullIntegrity = m.HullIntegrity, CrewMorale = m.CrewMorale,
            CargoIntegrity = m.CargoIntegrity, FoodSupply = m.FoodSupply,
            PassengerCount = m.Cargo?.PassengerCount ?? 0,
            PackageCount = m.Cargo?.PackageCount ?? 0,
            AssignedContractorIds = m.AssignedContractorIds,
        };

        ContractorSaveData ContractorToSave(ContractorInstance c) => new()
        {
            InstanceId = c.InstanceId, DefinitionName = c.Definition?.name,
            DisplayName = c.DisplayName, Experience = c.Experience, IsOnMission = c.IsOnMission,
        };

        ShipSaveData ShipToSave(ShipInstance ship)
        {
            var sd = new ShipSaveData
            {
                Name = ship.Name, BlueprintName = ship.Blueprint?.name, IsOnMission = ship.IsOnMission,
            };
            if (ship.Modules != null)
                for (int i = 0; i < ship.Modules.Length; i++)
                {
                    var m = ship.Modules[i];
                    if (m == null) continue;
                    sd.Modules.Add(new ModuleSaveData
                    {
                        SlotIndex = i, ModuleName = m.Definition?.name,
                        CurrentTier = m.CurrentTier, IsUpgrading = m.IsUpgrading,
                        UpgradeCompleteMinute = m.UpgradeCompleteMinute,
                    });
                }
            return sd;
        }
    }
}
