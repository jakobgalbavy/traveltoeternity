using UnityEngine;
using UnityEngine.UI;
using DeepTransit.Contractors;
using DeepTransit.Core;
using DeepTransit.Events;
using DeepTransit.Missions;

namespace DeepTransit.UI
{
    public class EventCardScreen : MonoBehaviour
    {
        [Header("Event Info")]
        public Text TitleText;
        public Text SeverityText;
        public Text DescriptionText;
        public Text ShipNameText;

        [Header("Options")]
        public Transform OptionsParent;
        public GameObject OptionButtonPrefab;

        [Header("Mission Status")]
        public Text HullText;
        public Text MoraleText;
        public Text CargoText;
        public Text FoodText;

        Mission      _mission;
        MissionEvent _event;

        void Start() { }

        public void Populate(Mission mission, MissionEvent ev)
        {
            _mission = mission;
            _event   = ev;

            if (TitleText)       TitleText.text       = ev.Definition.Title;
            if (SeverityText)    SeverityText.text     = ev.Definition.Severity.ToString().ToUpper();
            if (DescriptionText) DescriptionText.text  = ev.Definition.Description;
            if (ShipNameText)    ShipNameText.text      = mission.ShipName;

            if (HullText)   HullText.text   = $"Hull {mission.HullIntegrity * 100f:F0}%";
            if (MoraleText) MoraleText.text = $"Morale {mission.CrewMorale * 100f:F0}%";
            if (CargoText)  CargoText.text  = $"Cargo {mission.CargoIntegrity * 100f:F0}%";
            if (FoodText)   FoodText.text   = $"Food {mission.FoodSupply * 100f:F0}%";

            BuildOptions();
        }

        void BuildOptions()
        {
            if (OptionsParent == null || _event?.Definition?.Options == null) return;
            foreach (Transform child in OptionsParent) Destroy(child.gameObject);

            var gm = GameManager.Instance;
            for (int i = 0; i < _event.Definition.Options.Length; i++)
            {
                var option = _event.Definition.Options[i];
                if (OptionButtonPrefab == null) continue;

                var obj = Instantiate(OptionButtonPrefab, OptionsParent);
                var texts = obj.GetComponentsInChildren<Text>();
                var btn = obj.GetComponent<Button>();

                var contractor = gm?.ContractorManager?.GetByRole(option.RequiredRole);
                float chance = Mathf.Clamp01(option.BaseSuccessChance +
                    (contractor != null ? option.ContractorBonus * contractor.SuccessChance : 0f));

                string contractorStr = contractor != null
                    ? $" [{contractor.DisplayName}]"
                    : $" [No {option.RequiredRole}]";

                if (texts.Length > 0)
                    texts[0].text = $"{option.Label}{contractorStr}  {chance * 100f:F0}%";

                int optIdx = i;
                btn?.onClick.AddListener(() => OnChooseOption(optIdx, contractor));
            }
        }

        void OnChooseOption(int optionIndex, Contractors.ContractorInstance contractor)
        {
            var gm = GameManager.Instance;
            if (gm == null || _mission == null || _event == null) return;

            gm.EventManager.Resolve(_mission, _event, optionIndex, contractor);
            UIManager.Instance?.Show(Screen.Hub);
        }
    }
}
