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

        [Header("Urgency")]
        public Text CountdownText;

        Mission      _mission;
        MissionEvent _event;

        void Start() { }

        void Update()
        {
            if (_event == null || CountdownText == null) return;
            var gm = GameManager.Instance;
            if (gm == null) return;

            long minsLeft = _event.MinutesUntilEscalation(gm.TimeManager.ElapsedGameMinutes);
            if (_event.Definition?.Escalation != null)
            {
                string urgency = minsLeft < 30 ? "CRITICAL — " : minsLeft < 60 ? "URGENT — " : "";
                CountdownText.text = minsLeft > 0
                    ? $"{urgency}Escalates in {FormatMinutes(minsLeft)}"
                    : "Overdue";
            }
            else
            {
                CountdownText.text = string.Empty;
            }
        }

        public void Populate(Mission mission, MissionEvent ev)
        {
            _mission = mission;
            _event   = ev;

            if (TitleText)       TitleText.text       = ev.Definition.Title;
            if (SeverityText)    SeverityText.text     = ev.Definition.Severity.ToString().ToUpper();
            if (DescriptionText) DescriptionText.text  = BuildDescription(ev);
            if (ShipNameText)    ShipNameText.text      = mission.ShipName;

            if (HullText)   HullText.text   = $"Hull {mission.HullIntegrity * 100f:F0}%";
            if (MoraleText) MoraleText.text = $"Morale {mission.CrewMorale * 100f:F0}%";
            if (CargoText)  CargoText.text  = $"Cargo {mission.CargoIntegrity * 100f:F0}%";
            if (FoodText)   FoodText.text   = $"Food {mission.FoodSupply * 100f:F0}%";

            BuildOptions();
        }

        string BuildDescription(MissionEvent ev)
        {
            string desc = ev.Definition.Description;
            if (ev.IsPartiallyResolved && ev.PartialFixCount > 0)
                desc += $"\n\n[Patched ×{ev.PartialFixCount} — root cause unresolved]";
            return desc;
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

                string partialTag = option.IsPartialFix ? " (Temp Fix)" : "";

                if (texts.Length > 0)
                    texts[0].text = $"{option.Label}{contractorStr}{partialTag}  {chance * 100f:F0}%";

                int optIdx = i;
                btn?.onClick.AddListener(() => OnChooseOption(optIdx, contractor));
            }
        }

        void OnChooseOption(int optionIndex, Contractors.ContractorInstance contractor)
        {
            var gm = GameManager.Instance;
            if (gm == null || _mission == null || _event == null) return;

            bool resolved = gm.EventManager.Resolve(_mission, _event, optionIndex, contractor);

            // If the option was a partial fix, stay on the card so the player can see updated state.
            bool isPartialOption = optionIndex < (_event.Definition?.Options?.Length ?? 0)
                && _event.Definition.Options[optionIndex].IsPartialFix;

            if (resolved && isPartialOption)
                Populate(_mission, _event);
            else
                UIManager.Instance?.Show(Screen.Hub);
        }

        static string FormatMinutes(long minutes)
        {
            if (minutes < 60) return $"{minutes}m";
            long h = minutes / 60;
            long m = minutes % 60;
            return m > 0 ? $"{h}h {m}m" : $"{h}h";
        }
    }
}
