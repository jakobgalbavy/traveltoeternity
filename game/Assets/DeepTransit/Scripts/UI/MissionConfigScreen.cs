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
        public Transform  DestinationListParent;
        public GameObject DestinationRowPrefab;

        [Header("Cargo")]
        public Slider PassengerSlider;
        public Text   PassengerCountText;
        public Slider PackageSlider;
        public Text   PackageCountText;
        public Text   CapacityWarningText;

        [Header("Summary")]
        public Text   SelectedDestinationText;
        public Text   EstimatedPayoutText;
        public Text   LaunchCostText;
        public Text   VoyageDurationText;

        [Header("Defer Pay")]
        public Button DeferPayButton;
        public Text   DeferPayText;

        [Header("Actions")]
        public Button LaunchButton;
        public Button BackButton;

        DestinationSO _selectedDest;
        bool          _deferCrewPay;

        void Start()
        {
            BackButton?.onClick.AddListener(() => UIManager.Instance?.Show(Screen.Hub));
            LaunchButton?.onClick.AddListener(OnLaunch);
            PassengerSlider?.onValueChanged.AddListener(_ => OnCargoChanged());
            PackageSlider?.onValueChanged.AddListener(_ => OnCargoChanged());
            DeferPayButton?.onClick.AddListener(ToggleDeferPay);
        }

        void OnEnable()
        {
            _deferCrewPay = false;
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

        void ToggleDeferPay()
        {
            _deferCrewPay = !_deferCrewPay;
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
                RefreshDeferButton(manifest, canAfford: true);
                return;
            }

            long fullCost  = Economy.PayoutCalculator.CalculateLaunchCost(_selectedDest, manifest, deferCrewPay: false);
            long deferCost = Economy.PayoutCalculator.CalculateLaunchCost(_selectedDest, manifest, deferCrewPay: true);
            long activeCost = _deferCrewPay ? deferCost : fullCost;
            bool canAfford  = gm.CurrencyManager.CanAfford(activeCost);

            RefreshDeferButton(manifest, canAfford: gm.CurrencyManager.CanAfford(fullCost));

            if (!canAfford)
            {
                bool couldAffordWithDefer = gm.CurrencyManager.CanAfford(deferCost);
                string hint = !_deferCrewPay && couldAffordWithDefer
                    ? " Enable deferred crew pay to launch with fuel-only cost."
                    : "";
                SetStatus($"Insufficient funds. Need ¤{activeCost:N0} (have ¤{gm.CurrencyManager.SoftCurrency:N0}).{hint}", blocked: true);
                return;
            }

            SetStatus("", blocked: false);

            if (LaunchCostText)
            {
                LaunchCostText.text = _deferCrewPay
                    ? $"Launch cost: ¤{deferCost:N0} (fuel only — crew deducted at arrival)"
                    : $"Launch cost: ¤{fullCost:N0}";
            }

            if (EstimatedPayoutText)
            {
                float grossEst = (passengers * Economy.PayoutCalculator.BasePassengerRate
                               + packages   * Economy.PayoutCalculator.BasePackageRate)
                               * _selectedDest.PayoutMultiplier;
                if (_deferCrewPay)
                {
                    long settlement = Economy.PayoutCalculator.DeferredSettlement(manifest);
                    EstimatedPayoutText.text = $"Est. payout: ¤{Mathf.Max(0f, grossEst - settlement):N0}  (−¤{settlement:N0} crew)";
                }
                else
                {
                    EstimatedPayoutText.text = $"Est. payout: ¤{grossEst:N0}";
                }
            }
        }

        void RefreshDeferButton(CargoManifest manifest, bool canAfford)
        {
            if (DeferPayText == null) return;
            if (_deferCrewPay)
            {
                DeferPayText.text  = "✓ Crew pay deferred (+30%)";
                DeferPayText.color = new Color(1f, 0.75f, 0.2f);
            }
            else
            {
                DeferPayText.text  = canAfford
                    ? "Defer crew pay (+30% penalty)"
                    : "⚠ Defer crew pay to afford launch";
                DeferPayText.color = canAfford
                    ? new Color(0.6f, 0.6f, 0.6f)
                    : new Color(1f, 0.75f, 0.2f);
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

            long launchCost = Economy.PayoutCalculator.CalculateLaunchCost(_selectedDest, manifest, _deferCrewPay);
            if (!gm.CurrencyManager.Spend(launchCost))
            {
                Debug.LogError($"[MissionConfig] OnLaunch: can't afford launch cost ¤{launchCost}.");
                SetStatus($"Insufficient funds (¤{launchCost:N0} required).", blocked: true);
                return;
            }

            Debug.Log($"[MissionConfig] Launching → {_selectedDest.DisplayName}  passengers={manifest.PassengerCount}  packages={manifest.PackageCount}  cost=¤{launchCost}  deferred={_deferCrewPay}");

            var assignedIds = new List<string>();
            foreach (var c in gm.ContractorManager.Roster)
            {
                c.IsOnMission = true;
                assignedIds.Add(c.InstanceId);
            }

            ship.IsOnMission = true;
            gm.MissionManager.LaunchMission(ship.Name, _selectedDest.Id, manifest, assignedIds, _deferCrewPay);
            UIManager.Instance?.Show(Screen.Hub);
        }
    }
}
