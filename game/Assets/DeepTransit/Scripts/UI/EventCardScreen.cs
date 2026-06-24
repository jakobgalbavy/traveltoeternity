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

        [Header("Visual")]
        public Image BackgroundPanel;

        static readonly Color TintMinor    = new Color(0.03f, 0.04f, 0.09f, 0.97f);
        static readonly Color TintModerate = new Color(0.13f, 0.07f, 0.01f, 0.97f);
        static readonly Color TintCritical = new Color(0.15f, 0.02f, 0.02f, 0.97f);

        static readonly Color ColMinor    = new Color(0.42f, 0.47f, 0.58f);
        static readonly Color ColModerate = new Color(0.97f, 0.63f, 0.12f);
        static readonly Color ColCritical = new Color(0.98f, 0.28f, 0.22f);

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

            ApplySeverityTint(ev.Definition.Severity);
            BuildOptions();
        }

        void ApplySeverityTint(EventSeverity severity)
        {
            if (BackgroundPanel != null)
                BackgroundPanel.color = severity switch
                {
                    EventSeverity.Minor    => TintMinor,
                    EventSeverity.Moderate => TintModerate,
                    EventSeverity.Critical => TintCritical,
                    _                      => TintMinor,
                };
            if (SeverityText != null)
                SeverityText.color = severity switch
                {
                    EventSeverity.Minor    => ColMinor,
                    EventSeverity.Moderate => ColModerate,
                    EventSeverity.Critical => ColCritical,
                    _                      => ColMinor,
                };
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

                var contractor = gm?.ContractorManager?.GetByRoleFromAssigned(option.RequiredRole, _mission?.AssignedContractorIds);
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
                UIManager.Instance?.OnEventDismissed();
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
