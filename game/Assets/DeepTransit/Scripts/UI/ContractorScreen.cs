using UnityEngine;
using UnityEngine.UI;
using DeepTransit.Core;
using DeepTransit.Contractors;

namespace DeepTransit.UI
{
    public class ContractorScreen : MonoBehaviour
    {
        [Header("Roster")]
        public Transform RosterParent;
        public Text RosterHeaderText;

        [Header("Hire Pool")]
        public Transform PoolParent;
        public Button RefreshPoolButton;
        public Text RefreshCostText;

        [Header("Row Prefab")]
        public GameObject ContractorRowPrefab;

        [Header("Nav")]
        public Button BackButton;

        const int PoolRefreshCost = 100;

        void Start()
        {

            BackButton?.onClick.AddListener(() => UIManager.Instance?.Show(Screen.Hub));
            RefreshPoolButton?.onClick.AddListener(OnRefreshPool);
            if (RefreshCostText) RefreshCostText.text = $"¤{PoolRefreshCost}";
        }

        void OnEnable() => Refresh();

        void Refresh()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            BuildList(RosterParent, gm.ContractorManager.Roster, hired: true);
            BuildList(PoolParent,   gm.ContractorManager.HirePool, hired: false);

            if (RosterHeaderText)
                RosterHeaderText.text = $"Roster ({gm.ContractorManager.Roster.Count})";
        }

        void BuildList(Transform parent, System.Collections.Generic.List<ContractorInstance> list, bool hired)
        {
            if (parent == null) return;
            foreach (Transform child in parent) Destroy(child.gameObject);

            foreach (var contractor in list)
            {
                if (ContractorRowPrefab == null) continue;
                var row = Instantiate(ContractorRowPrefab, parent);
                var texts = row.GetComponentsInChildren<Text>();
                var btn   = row.GetComponentInChildren<Button>();

                string roleStr = contractor.Definition?.Role.ToString() ?? "Unknown";
                string expStr  = $"Exp {contractor.Experience * 100f:F0}%";
                string rateStr = $"¤{contractor.DailyRate}/day";
                string status  = contractor.IsOnMission ? " [ON MISSION]" : "";

                if (texts.Length > 0) texts[0].text = $"{contractor.DisplayName} — {roleStr}  {expStr}  {rateStr}{status}";

                var c = contractor;
                if (hired)
                {
                    if (btn) btn.GetComponentInChildren<Text>().text = "Fire";
                    btn?.onClick.AddListener(() =>
                    {
                        GameManager.Instance?.ContractorManager.Fire(c);
                        Refresh();
                    });
                }
                else
                {
                    if (btn) btn.GetComponentInChildren<Text>().text = $"Hire";
                    btn?.onClick.AddListener(() =>
                    {
                        var mgr = GameManager.Instance?.ContractorManager;
                        mgr?.Hire(c);
                        Refresh();
                    });
                }
            }
        }

        void OnRefreshPool()
        {
            var gm = GameManager.Instance;
            if (gm == null || !gm.CurrencyManager.Spend(PoolRefreshCost)) return;
            gm.ContractorManager.RefreshPool();
            Refresh();
        }
    }
}
