using UnityEngine;
using DeepTransit.Missions;
using DeepTransit.Events;
using DeepTransit.Economy;

namespace DeepTransit.UI
{
    public enum Screen { Hub, Fleet, MissionConfig, Contractors, Event, Debrief }

    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        HubScreen         _hub;
        FleetScreen       _fleet;
        MissionConfigScreen _missionConfig;
        ContractorScreen  _contractors;
        EventCardScreen   _eventCard;
        DebriefScreen     _debrief;

        Screen _current = Screen.Hub;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            // Discover screens directly — they may be inactive so can't self-register via Start()
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[UIManager] No Canvas found. Run Tools → Deep Transit → Build Scene.");
                return;
            }

            _hub           = canvas.GetComponentInChildren<HubScreen>(true);
            _fleet         = canvas.GetComponentInChildren<FleetScreen>(true);
            _missionConfig = canvas.GetComponentInChildren<MissionConfigScreen>(true);
            _contractors   = canvas.GetComponentInChildren<ContractorScreen>(true);
            _eventCard     = canvas.GetComponentInChildren<EventCardScreen>(true);
            _debrief       = canvas.GetComponentInChildren<DebriefScreen>(true);

            Show(Screen.Hub);
        }

        public void Show(Screen screen)
        {
            _current = screen;
            _hub?.gameObject.SetActive(screen == Screen.Hub);
            _fleet?.gameObject.SetActive(screen == Screen.Fleet);
            _missionConfig?.gameObject.SetActive(screen == Screen.MissionConfig);
            _contractors?.gameObject.SetActive(screen == Screen.Contractors);
            _eventCard?.gameObject.SetActive(screen == Screen.Event);
            _debrief?.gameObject.SetActive(screen == Screen.Debrief);
        }

        public void ShowEvent(Mission mission, MissionEvent ev)
        {
            _eventCard?.Populate(mission, ev);
            Show(Screen.Event);
        }

        public void ShowDebrief(Mission mission, PayoutResult payout)
        {
            _debrief?.Populate(mission, payout);
            Show(Screen.Debrief);
        }
    }
}
