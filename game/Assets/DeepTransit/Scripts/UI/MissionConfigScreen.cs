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
            var ship = GameManager.Instance?.ShipManager?.Ships?.Count > 0
                ? GameManager.Instance.ShipManager.Ships[0] : null;
            if (ship == null) return;
            if (PassengerSlider)
            {
                PassengerSlider.minValue = 0;
                PassengerSlider.maxValue = Mathf.Max(1, ship.GetStat(ShipStat.PassengerCapacity));
            }
            if (PackageSlider)
            {
                PackageSlider.minValue = 0;
                PackageSlider.maxValue = Mathf.Max(1, ship.GetStat(ShipStat.CargoCapacity));
            }
        }

        void PopulateDestinations()
        {
            if (DestinationListParent == null) return;
            foreach (Transform child in DestinationListParent) Destroy(child.gameObject);

            var gm = GameManager.Instance;
            if (gm == null) return;
            var available = gm.StarMapManager.GetAvailable(gm.CurrencyManager.Reputation);

            foreach (var dest in available)
            {
                if (DestinationRowPrefab == null) continue;
                var row = Instantiate(DestinationRowPrefab, DestinationListParent);
                var texts = row.GetComponentsInChildren<Text>();
                if (texts.Length > 0) texts[0].text = $"{dest.DisplayName}  ×{dest.PayoutMultiplier:F1}  Rep {dest.ReputationRequired}+";

                var d = dest;
                row.GetComponent<Button>()?.onClick.AddListener(() => SelectDestination(d));
            }
        }

        void SelectDestination(DestinationSO dest)
        {
            _selectedDest = dest;
            if (SelectedDestinationText)
                SelectedDestinationText.text = $"→ {dest.DisplayName}";
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
            var gm = GameManager.Instance;
            var ship = gm?.ShipManager?.Ships?.Count > 0 ? gm.ShipManager.Ships[0] : null;
            if (ship == null) return;

            int passengers = PassengerSlider ? Mathf.RoundToInt(PassengerSlider.value) : 0;
            int packages   = PackageSlider   ? Mathf.RoundToInt(PackageSlider.value)   : 0;

            if (PassengerCountText) PassengerCountText.text = $"Passengers: {passengers}";
            if (PackageCountText)   PackageCountText.text   = $"Packages: {packages}";

            var manifest = new CargoManifest { PassengerCount = passengers, PackageCount = packages };
            bool valid = manifest.Validate(ship, out string error);
            if (CapacityWarningText) CapacityWarningText.text = valid ? "" : error;
            if (LaunchButton) LaunchButton.interactable = valid && _selectedDest != null;

            if (_selectedDest != null && EstimatedPayoutText)
            {
                float est = (passengers * Economy.PayoutCalculator.BasePassengerRate
                           + packages  * Economy.PayoutCalculator.BasePackageRate)
                           * _selectedDest.PayoutMultiplier;
                EstimatedPayoutText.text = $"Est. payout: ¤{est:N0}";
            }
        }

        void OnLaunch()
        {
            if (_selectedDest == null) return;
            var gm = GameManager.Instance;
            var ship = gm?.ShipManager?.Ships?.Count > 0 ? gm.ShipManager.Ships[0] : null;
            if (ship == null || ship.IsOnMission) return;

            var manifest = new CargoManifest
            {
                PassengerCount = PassengerSlider ? Mathf.RoundToInt(PassengerSlider.value) : 0,
                PackageCount   = PackageSlider   ? Mathf.RoundToInt(PackageSlider.value)   : 0,
            };

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
