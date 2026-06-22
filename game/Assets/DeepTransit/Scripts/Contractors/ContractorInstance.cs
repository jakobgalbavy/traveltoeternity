using System;
using UnityEngine;

namespace DeepTransit.Contractors
{
    [Serializable]
    public class ContractorInstance
    {
        public string InstanceId;
        public ContractorSO Definition;
        public string DisplayName;

        [Range(0f, 1f)] public float Experience;  // grows per completed mission
        public bool IsOnMission;

        public int DailyRate => Definition == null ? 0 :
            Mathf.RoundToInt(Definition.BaseDailyRate *
                Mathf.Lerp(1f, Definition.MaxExperienceMultiplier, Experience));

        public float SuccessChance => Definition == null ? 0.5f :
            Mathf.Clamp01(Definition.BaseSuccessChance + Definition.SuccessChanceBonus * Experience);

        public void AddExperience(float amount)
        {
            Experience = Mathf.Clamp01(Experience + amount);
        }
    }
}
