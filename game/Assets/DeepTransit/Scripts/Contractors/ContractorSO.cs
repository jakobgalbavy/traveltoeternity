using UnityEngine;

namespace DeepTransit.Contractors
{
    [CreateAssetMenu(menuName = "DeepTransit/Contractor Type", fileName = "Contractor_New")]
    public class ContractorSO : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public ContractorRole Role;
        [TextArea] public string Bio;

        [Tooltip("Base daily rate in soft currency at experience 0.")]
        public int BaseDailyRate;

        [Tooltip("Multiplier applied to daily rate at max experience. E.g. 2.5 = 2.5× base.")]
        public float MaxExperienceMultiplier = 2f;

        // Base probability of resolving an event successfully (0–1) with no experience.
        [Range(0f, 1f)] public float BaseSuccessChance = 0.6f;

        // How much experience boosts success chance (added linearly with experience 0→1).
        [Range(0f, 1f)] public float SuccessChanceBonus = 0.3f;
    }
}
