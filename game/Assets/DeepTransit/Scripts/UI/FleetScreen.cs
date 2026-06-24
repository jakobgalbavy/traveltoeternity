using UnityEngine;
using UnityEngine.UI;
using DeepTransit.Core;
using DeepTransit.Ships;

namespace DeepTransit.UI
{
    public class FleetScreen : MonoBehaviour
    {
        [Header("Ship Info")]
        public Text ShipNameText;
        public InputField ShipNameInput;
        public Button RenameButton;

        [Header("Stats")]
        public Text HullStatText;
        public Text PassengerCapText;
        public Text CargoCapText;
        public Text EngineText;
        public Text MedicalText;
        public Text NavigationText;

        [Header("Module List")]
        public Transform ModuleListParent;
        public GameObject ModuleRowPrefab;

        [Header("Currency")]
        public Text BalanceText;

        [Header("Nav")]
        public Button BackButton;

        ShipInstance _ship;

        void Start()
        {
            BackButton?.onClick.AddListener(() => UIManager.Instance?.Show(Screen.Hub));
            RenameButton?.onClick.AddListener(OnRename);
        }

        void OnEnable()
        {
            var gm = GameManager.Instance;
            _ship = gm?.ShipManager?.Ships?.Count > 0 ? gm.ShipManager.Ships[0] : null;
            if (gm != null) gm.CurrencyManager.OnChanged += RefreshBalance;
            Refresh();
        }

        void OnDisable()
        {
            var gm = GameManager.Instance;
            if (gm != null) gm.CurrencyManager.OnChanged -= RefreshBalance;
        }

        void RefreshBalance()
        {
            var gm = GameManager.Instance;
            if (BalanceText && gm != null)
                BalanceText.text = $"¤ {gm.CurrencyManager.SoftCurrency:N0}";
        }

        void OnRename()
        {
            if (_ship == null || ShipNameInput == null) return;
            string newName = ShipNameInput.text.Trim();
            if (!string.IsNullOrEmpty(newName))
            {
                _ship.Name = newName;
                Refresh();
            }
        }

        void Refresh()
        {
            RefreshBalance();
            if (_ship == null) return;
            if (ShipNameText)  ShipNameText.text  = _ship.Name;
            if (HullStatText)  HullStatText.text  = $"Hull: {_ship.GetStat(ShipStat.HullIntegrity):F0}";
            if (PassengerCapText) PassengerCapText.text = $"Passengers: {_ship.GetStat(ShipStat.PassengerCapacity):F0}";
            if (CargoCapText)  CargoCapText.text  = $"Cargo: {_ship.GetStat(ShipStat.CargoCapacity):F0}";
            if (EngineText)    EngineText.text     = $"Engine: {_ship.GetStat(ShipStat.EngineEfficiency):F1}";
            if (MedicalText)   MedicalText.text    = $"Medical: {_ship.GetStat(ShipStat.MedicalCapacity):F1}";
            if (NavigationText) NavigationText.text = $"Navigation: {_ship.GetStat(ShipStat.NavigationRating):F1}";

            RefreshModuleList();
        }

        void RefreshModuleList()
        {
            if (ModuleListParent == null || _ship?.Modules == null) return;
            foreach (Transform child in ModuleListParent) Destroy(child.gameObject);

            for (int i = 0; i < _ship.Modules.Length; i++)
            {
                var module = _ship.Modules[i];
                if (ModuleRowPrefab == null) continue;

                var row = Instantiate(ModuleRowPrefab, ModuleListParent);
                var texts = row.GetComponentsInChildren<Text>();
                var buttons = row.GetComponentsInChildren<Button>();

                string slotName = _ship.Blueprint?.Slots[i].SlotName ?? $"Slot {i}";
                string moduleName = module?.Definition?.DisplayName ?? "(empty)";
                string tierStr = module != null ? $"Tier {module.CurrentTier + 1}/{module.Definition.MaxTier + 1}" : "";
                string upgrading = module?.IsUpgrading == true ? " [UPGRADING]" : "";

                if (texts.Length > 0) texts[0].text = $"{slotName}: {moduleName} {tierStr}{upgrading}";

                int slotIndex = i;
                if (buttons.Length > 0 && module != null && !module.IsMaxTier && !module.IsUpgrading)
                {
                    var next = module.NextTierData;
                    if (next.HasValue)
                    {
                        buttons[0].GetComponentInChildren<Text>().text = $"Upgrade ¤{next.Value.CostSoftCurrency}";
                        buttons[0].onClick.AddListener(() => OnUpgrade(slotIndex, next.Value.CostSoftCurrency));
                    }
                }
                else if (buttons.Length > 0)
                {
                    buttons[0].gameObject.SetActive(false);
                }
            }
        }

        void OnUpgrade(int slotIndex, int cost)
        {
            var gm = GameManager.Instance;
            if (gm == null || _ship == null) return;
            if (!gm.CurrencyManager.CanAfford(cost)) return;
            if (_ship.StartUpgrade(slotIndex, gm.TimeManager.ElapsedGameMinutes))
            {
                gm.CurrencyManager.Spend(cost);
                Refresh();
            }
        }
    }
}
