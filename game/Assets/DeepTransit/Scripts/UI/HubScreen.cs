using UnityEngine;
using UnityEngine.UI;
using DeepTransit.Core;
using DeepTransit.Missions;

namespace DeepTransit.UI
{
    public class HubScreen : MonoBehaviour
    {
        [Header("Currency Bar")]
        public Text SoftCurrencyText;
        public Text HardCurrencyText;
        public Text ReputationText;

        [Header("Active Mission Card")]
        public GameObject MissionCardRoot;
        public Text MissionShipNameText;
        public Text MissionDestinationText;
        public Slider MissionProgressSlider;
        public Text MissionStatusText;
        public Text MissionHullText;
        public Text MissionMoraleText;

        [Header("No Mission")]
        public GameObject NoMissionRoot;

        [Header("Nav Buttons")]
        public Button NavFleet;
        public Button NavMissionConfig;
        public Button NavContractors;

        void OnEnable()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            gm.CurrencyManager.OnChanged += Refresh;
            Refresh();
        }

        void OnDisable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.CurrencyManager.OnChanged -= Refresh;
        }

        void Start()
        {
            UIManager.Instance?.Register(this);
            NavFleet?.onClick.AddListener(() => UIManager.Instance?.Show(Screen.Fleet));
            NavMissionConfig?.onClick.AddListener(() => UIManager.Instance?.Show(Screen.MissionConfig));
            NavContractors?.onClick.AddListener(() => UIManager.Instance?.Show(Screen.Contractors));
        }

        void Update() => RefreshMissionCard();

        void Refresh()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            if (SoftCurrencyText) SoftCurrencyText.text = $"¤ {gm.CurrencyManager.SoftCurrency:N0}";
            if (HardCurrencyText) HardCurrencyText.text = $"★ {gm.CurrencyManager.HardCurrency:N0}";
            if (ReputationText)   ReputationText.text   = $"Rep {gm.CurrencyManager.Reputation:F0}";
        }

        void RefreshMissionCard()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            var missions = gm.MissionManager.ActiveMissions;
            bool hasMission = missions.Count > 0;
            MissionCardRoot?.SetActive(hasMission);
            NoMissionRoot?.SetActive(!hasMission);

            if (!hasMission || missions.Count == 0) return;
            var m = missions[0];

            if (MissionShipNameText)   MissionShipNameText.text   = m.ShipName;
            if (MissionDestinationText) MissionDestinationText.text = m.DestinationId;
            if (MissionProgressSlider) MissionProgressSlider.value = m.ProgressNormalized;
            if (MissionStatusText)     MissionStatusText.text      = $"{m.ProgressNormalized * 100f:F0}%";
            if (MissionHullText)       MissionHullText.text        = $"Hull {m.HullIntegrity * 100f:F0}%";
            if (MissionMoraleText)     MissionMoraleText.text      = $"Morale {m.CrewMorale * 100f:F0}%";
        }
    }
}
