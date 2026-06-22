using System;
using System.Collections.Generic;
using DeepTransit.Missions;
using DeepTransit.Contractors;
using DeepTransit.Ships;

namespace DeepTransit.Data
{
    [Serializable]
    public class GameState
    {
        public long SoftCurrency;
        public long HardCurrency;
        public float Reputation;

        public List<string> UnlockedDestinationIds = new();
        public List<MissionSaveData> ActiveMissions = new();
        public List<ContractorSaveData> ContractorRoster = new();
        public List<ShipSaveData> Ships = new();
        public long ElapsedGameMinutes;
    }

    // Lightweight save proxies (SO references become string IDs loaded from Resources).

    [Serializable]
    public class MissionSaveData
    {
        public string Id;
        public string ShipName;
        public string DestinationId;
        public long   DurationMinutes;
        public long   LaunchMinute;
        public float  HullIntegrity;
        public float  CrewMorale;
        public float  CargoIntegrity;
        public float  FoodSupply;
        public int    PassengerCount;
        public int    PackageCount;
        public List<string> AssignedContractorIds = new();
    }

    [Serializable]
    public class ContractorSaveData
    {
        public string InstanceId;
        public string DefinitionName;   // SO asset name in Resources/Contractors/
        public string DisplayName;
        public float  Experience;
        public bool   IsOnMission;
    }

    [Serializable]
    public class ShipSaveData
    {
        public string Name;
        public string BlueprintName;    // SO asset name in Resources/Blueprints/
        public bool   IsOnMission;
        public List<ModuleSaveData> Modules = new();
    }

    [Serializable]
    public class ModuleSaveData
    {
        public int    SlotIndex;
        public string ModuleName;       // SO asset name in Resources/Modules/
        public int    CurrentTier;
        public bool   IsUpgrading;
        public long   UpgradeCompleteMinute;
    }
}
