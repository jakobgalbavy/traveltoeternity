using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DeepTransit.Core;
using DeepTransit.Destinations;
using DeepTransit.Cargo;
using DeepTransit.Ships;

namespace DeepTransit.UI
{
    public class MissionConfigScreen : MonoBehaviour
    {
        [Header("Destination")]
        public Transform DestinationListParent;
        public GameObject DestinationRowPrefab;

        [Header("Cargo")]
        public Slider PassengerSlider;
        public Text   PassengerCountText;
        public Slider PackageSlider;
        public Text   PackageCountText;
        public Text   CapacityWarningText;

        [Header("Summary")]
        public Text SelectedDestinationText;
        public Text EstimatedPayoutText;
        public Text VoyageDurationText;

        [Header("Actions")]
        public Button LaunchButton;
        public Button BackButton;

        DestinationSO _selectedDest;

        void Start()
        {
            BackButton?.onClick.AddListener(() => UIManager.Instance?.Show(Screen.Hub));
            LaunchButton?.onClick.AddListener(OnLaunch);
            PassengerSlider?.onValueChanged.AddListener(_ => OnCargoChanged());
            PackageSlider?.onValueChanged.AddListener(_ => OnCargoChanged());
        }

        void OnEnable()
        {
            SetSliderMaxes();
            PopulateDestinations();
            OnCargoChanged();
        }

        void SetSliderMaxes()
        {
            var gm   = GameManager.Instance;
            var ship = gm?.ShipManager?.Ships?.Count > 0 ? gm.ShipManager.Ships[0] : null;
            if (ship == null)
            {
                Debug.LogError("[MissionConfig] SetSliderMaxes: no ship found — Bootstrap may be missing its StarterBlueprint assignment.");
                return;
            }

            float passCap  = ship.GetStat(ShipStat.PassengerCapacity);
            float cargoCap = ship.GetStat(ShipStat.CargoCapacity);
            Debug.Log($"[MissionConfig] Ship '{ship.Name}' — PassCap={passCap:F0}  CargoCap={cargoCap:F0}  OnMission={ship.IsOnMission}");

            if (PassengerSlider) { PassengerSlider.minValue = 0; PassengerSlider.maxValue = Mathf.Max(1, passCap); }
            if (PackageSlider)   { PackageSlider.minValue   = 0; PackageSlider.maxValue   = Mathf.Max(1, cargoCap); }
        }

        void PopulateDestinations()
        {
            if (DestinationListParent == null)
            {
                Debug.LogError("[MissionConfig] DestinationListParent is null — run Build Scene.");
                return;
            }
            foreach (Transform child in DestinationListParent) Destroy(child.gameObject);

            var gm = GameManager.Instance;
            if (gm == null) { Debug.LogError("[MissionConfig] GameManager not found."); return; }

            float rep       = gm.CurrencyManager.Reputation;
            var   available = gm.StarMapManager.GetAvailable(rep);
            Debug.Log($"[MissionConfig] Destinations available: {available.Count}  (reputation={rep:F1})");

            foreach (var dest in available)
            {
                if (DestinationRowPrefab == null)
                {
                    Debug.LogError("[MissionConfig] DestinationRowPrefab is null — run Build Scene.");
                    break;
                }
                var row   = Instantiate(DestinationRowPrefab, DestinationListParent);
                var texts = row.GetComponentsInChildren<Text>();
                if (texts.Length > 0) texts[0].text = $"{dest.DisplayName}  ×{dest.PayoutMultiplier:F1}  Rep {dest.ReputationRequired}+";

                var d = dest;
                row.GetComponent<Button>()?.onClick.AddListener(() => SelectDestination(d));
            }
        }

        void SelectDestination(DestinationSO dest)
        {
            _selectedDest = dest;
            Debug.Log($"[MissionConfig] Selected: {dest.DisplayName}");
            if (SelectedDestinationText) SelectedDestinationText.text = $"→ {dest.DisplayName}";
            if (VoyageDurationText)
            {
                float hours = dest.VoyageMinutes / 60f;
                VoyageDurationText.text = hours >= 24
                    ? $"Duration: {hours / 24f:F1} days"
                    : $"Duration: {hours:F1}h";
            }
            OnCargoChanged();
        }

        void OnCargoChanged()
        {
            var gm   = GameManager.Instance;
            var ship = gm?.ShipManager?.Ships?.Count > 0 ? gm.ShipManager.Ships[0] : null;

            if (ship == null)
            {
                SetStatus("No ship registered. Run Build Scene and Generate Starter Data.", blocked: true);
                return;
            }
            if (ship.IsOnMission)
            {
                SetStatus("Ship is on an active mission.", blocked: true);
                return;
            }

            int passengers = PassengerSlider ? Mathf.RoundToInt(PassengerSlider.value) : 0;
            int packages   = PackageSlider   ? Mathf.RoundToInt(PackageSlider.value)   : 0;

            if (PassengerCountText) PassengerCountText.text = $"Passengers: {passengers}";
            if (PackageCountText)   PackageCountText.text   = $"Packages: {packages}";

            var  manifest = new CargoManifest { PassengerCount = passengers, PackageCount = packages };
            bool valid    = manifest.Validate(ship, out string error);
            if (!valid)
            {
                SetStatus(error, blocked: true);
                return;
            }

            if (_selectedDest == null)
            {
                SetStatus("Select a destination to continue.", blocked: true);
                return;
            }

            SetStatus("", blocked: false);

            if (EstimatedPayoutText)
            {
                float est = (passengers * Economy.PayoutCalculator.BasePassengerRate
                           + packages   * Economy.PayoutCalculator.BasePackageRate)
                           * _selectedDest.PayoutMultiplier;
                EstimatedPayoutText.text = $"Est. payout: ¤{est:N0}";
            }
        }

        void SetStatus(string message, bool blocked)
        {
            if (CapacityWarningText) CapacityWarningText.text = message;
            if (LaunchButton) LaunchButton.interactable = !blocked;
        }

        void OnLaunch()
        {
            var gm   = GameManager.Instance;
            var ship = gm?.ShipManager?.Ships?.Count > 0 ? gm.ShipManager.Ships[0] : null;

            if (_selectedDest == null)
            {
                Debug.LogWarning("[MissionConfig] OnLaunch: no destination selected.");
                SetStatus("Select a destination to continue.", blocked: true);
                return;
            }
            if (ship == null)
            {
                Debug.LogError("[MissionConfig] OnLaunch: no ship found.");
                SetStatus("No ship registered.", blocked: true);
                return;
            }
            if (ship.IsOnMission)
            {
                Debug.LogWarning("[MissionConfig] OnLaunch: ship is already on a mission.");
                SetStatus("Ship is on an active mission.", blocked: true);
                return;
            }

            var manifest = new CargoManifest
            {
                PassengerCount = PassengerSlider ? Mathf.RoundToInt(PassengerSlider.value) : 0,
                PackageCount   = PackageSlider   ? Mathf.RoundToInt(PackageSlider.value)   : 0,
            };

            Debug.Log($"[MissionConfig] Launching → {_selectedDest.DisplayName}  passengers={manifest.PassengerCount}  packages={manifest.PackageCount}");

            var assignedIds = new List<string>();
            foreach (var c in gm.ContractorManager.Roster)
            {
                c.IsOnMission = true;
                assignedIds.Add(c.InstanceId);
            }

            ship.IsOnMission = true;
            gm.MissionManager.LaunchMission(ship.Name, _selectedDest.Id, manifest, assignedIds);
            UIManager.Instance?.Show(Screen.Hub);
        }
    }
}
