using System;
using System.Collections.Generic;
using UnityEngine;
using DeepTransit.Core;

namespace DeepTransit.Contractors
{
    public class ContractorManager : MonoBehaviour
    {
        [Tooltip("All contractor type definitions. Assign in inspector.")]
        public ContractorSO[] AllDefinitions;

        [Tooltip("How many contractors appear in the hire pool at once.")]
        public int PoolSize = 8;

        [Tooltip("Game-minutes between hire pool refreshes.")]
        public long PoolRefreshMinutes = 1440; // 1 game-day

        public List<ContractorInstance> Roster   { get; private set; } = new();
        public List<ContractorInstance> HirePool { get; private set; } = new();

        private long _nextRefreshMinute;
        private int _instanceCounter;

        void OnEnable()  => TimeManager.OnGameMinuteTick += Tick;
        void OnDisable() => TimeManager.OnGameMinuteTick -= Tick;

        void Start() => RefreshPool();

        void Tick(long gameMinute)
        {
            if (gameMinute >= _nextRefreshMinute)
                RefreshPool();
        }

        public void RefreshPool()
        {
            HirePool.Clear();
            if (AllDefinitions == null || AllDefinitions.Length == 0) return;

            for (int i = 0; i < PoolSize; i++)
            {
                var def = AllDefinitions[UnityEngine.Random.Range(0, AllDefinitions.Length)];
                HirePool.Add(GenerateContractor(def));
            }
            _nextRefreshMinute += PoolRefreshMinutes;
        }

        ContractorInstance GenerateContractor(ContractorSO def)
        {
            _instanceCounter++;
            return new ContractorInstance
            {
                InstanceId  = $"c_{_instanceCounter}",
                Definition  = def,
                DisplayName = def.DisplayName,
                Experience  = UnityEngine.Random.Range(0f, 0.3f),
            };
        }

        public bool Hire(ContractorInstance contractor)
        {
            if (!HirePool.Contains(contractor)) return false;
            HirePool.Remove(contractor);
            Roster.Add(contractor);
            return true;
        }

        public void Fire(ContractorInstance contractor)
        {
            contractor.IsOnMission = false;
            Roster.Remove(contractor);
        }

        public ContractorInstance GetByRole(ContractorRole role)
        {
            foreach (var c in Roster)
                if (!c.IsOnMission && c.Definition?.Role == role) return c;
            return null;
        }

        public List<ContractorInstance> GetAvailable()
        {
            var result = new List<ContractorInstance>();
            foreach (var c in Roster)
                if (!c.IsOnMission) result.Add(c);
            return result;
        }
    }
}
