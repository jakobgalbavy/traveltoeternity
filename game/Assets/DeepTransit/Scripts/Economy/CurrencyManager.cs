using System;
using UnityEngine;

namespace DeepTransit.Economy
{
    public class CurrencyManager : MonoBehaviour
    {
        public long SoftCurrency { get; private set; } = 5000;
        public long HardCurrency { get; private set; } = 0;
        public float Reputation   { get; private set; } = 0f;

        public event Action OnChanged;

        public bool CanAfford(long soft, long hard = 0) =>
            SoftCurrency >= soft && HardCurrency >= hard;

        public bool Spend(long soft, long hard = 0)
        {
            if (!CanAfford(soft, hard)) return false;
            SoftCurrency -= soft;
            HardCurrency -= hard;
            OnChanged?.Invoke();
            return true;
        }

        public void EarnSoft(long amount)
        {
            SoftCurrency += amount;
            OnChanged?.Invoke();
        }

        public void EarnHard(long amount)
        {
            HardCurrency += amount;
            OnChanged?.Invoke();
        }

        public void AddReputation(float amount)
        {
            Reputation = Mathf.Max(0f, Reputation + amount);
            OnChanged?.Invoke();
        }

        public void Load(long soft, long hard, float rep)
        {
            SoftCurrency = soft;
            HardCurrency = hard;
            Reputation   = rep;
            OnChanged?.Invoke();
        }
    }
}
