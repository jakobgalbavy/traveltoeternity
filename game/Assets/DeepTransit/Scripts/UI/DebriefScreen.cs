using UnityEngine;
using UnityEngine.UI;
using DeepTransit.Economy;
using DeepTransit.Missions;

namespace DeepTransit.UI
{
    public class DebriefScreen : MonoBehaviour
    {
        [Header("Header")]
        public Text ShipNameText;
        public Text DestinationText;
        public Text OutcomeText;

        [Header("Breakdown")]
        public Text PassengerRevenueText;
        public Text PackageRevenueText;
        public Text HullPenaltyText;
        public Text MoraleFactorText;
        public Text TotalPayoutText;
        public Text ReputationGainText;

        [Header("Ship Condition")]
        public Text FinalHullText;
        public Text FinalMoraleText;

        [Header("Actions")]
        public Button ContinueButton;

        void Start()
        {
            UIManager.Instance?.Register(this);
            ContinueButton?.onClick.AddListener(() => UIManager.Instance?.Show(Screen.Hub));
        }

        public void Populate(Mission mission, PayoutResult payout)
        {
            if (ShipNameText)    ShipNameText.text    = mission.ShipName;
            if (DestinationText) DestinationText.text = mission.DestinationId;
            if (OutcomeText)     OutcomeText.text      = mission.Status == MissionStatus.Arrived
                ? "MISSION COMPLETE" : "MISSION FAILED";

            if (PassengerRevenueText) PassengerRevenueText.text = $"Passenger revenue:  ¤{payout.PassengerRevenue:N0}";
            if (PackageRevenueText)   PackageRevenueText.text   = $"Package revenue:    ¤{payout.PackageRevenue:N0}";
            if (HullPenaltyText)      HullPenaltyText.text      = $"Hull penalty:       -{payout.HullPenaltyPct * 100f:F0}%";
            if (MoraleFactorText)     MoraleFactorText.text      = $"Morale factor:      ×{payout.MoraleFactor:F2}";
            if (TotalPayoutText)      TotalPayoutText.text       = $"TOTAL:  ¤{payout.GrossPayout:N0}";
            if (ReputationGainText)   ReputationGainText.text    = $"+{payout.ReputationGain:F1} Reputation";

            if (FinalHullText)   FinalHullText.text   = $"Final hull: {mission.HullIntegrity * 100f:F0}%";
            if (FinalMoraleText) FinalMoraleText.text = $"Final morale: {mission.CrewMorale * 100f:F0}%";
        }
    }
}
