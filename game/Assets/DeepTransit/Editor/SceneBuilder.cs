#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DeepTransit.Core;
using DeepTransit.UI;
using DeepTransit.Destinations;
using DeepTransit.Contractors;
using DeepTransit.Events;
using DeepTransit.Ships;

namespace DeepTransit.Editor
{
    public static class SceneBuilder
    {
        // ── Palette ──────────────────────────────────────────────────────────
        static readonly Color C_BG     = new Color(0.08f, 0.10f, 0.13f);
        static readonly Color C_PANEL  = new Color(0.13f, 0.16f, 0.21f);
        static readonly Color C_ACCENT = new Color(0.22f, 0.60f, 0.95f);
        static readonly Color C_TEXT   = new Color(0.88f, 0.90f, 0.94f);
        static readonly Color C_DIM    = new Color(0.50f, 0.55f, 0.62f);
        static readonly Color C_WARN   = new Color(0.95f, 0.30f, 0.30f);
        static readonly Color C_GREEN  = new Color(0.30f, 0.85f, 0.45f);

        const string ScenePath  = "Assets/DeepTransit/Scenes/Bootstrap.unity";
        const string PrefabRoot = "Assets/DeepTransit/Prefabs/UI";

        static Font _font;

        // ── Entry point ──────────────────────────────────────────────────────

        [MenuItem("Tools/Deep Transit/Build Scene")]
        public static void Build()
        {
            if (UnityEditor.EditorApplication.isPlaying)
            {
                Debug.LogError("[DTA] Stop Play mode before running Build Scene.");
                return;
            }

            if (!AssetDatabase.IsValidFolder("Assets/Resources/Modules"))
            {
                Debug.LogError("[DTA] Run 'Tools → Deep Transit → Generate Starter Data' first.");
                return;
            }

            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            EnsureFolder(PrefabRoot);

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            // Idempotent — clear previous build
            foreach (var name in new[] { "Bootstrap", "Canvas", "EventSystem" })
            {
                var existing = GameObject.Find(name);
                if (existing != null) Object.DestroyImmediate(existing);
            }

            // Build prefabs before screens (screens hold references to them)
            var prefabContractorRow = MakeContractorRowPrefab();
            var prefabDestRow       = MakeDestRowPrefab();
            var prefabModuleRow     = MakeModuleRowPrefab();
            var prefabOptionBtn     = MakeOptionBtnPrefab();

            // Bootstrap GameObject
            var bootstrapGo = new GameObject("Bootstrap");
            var bootstrap   = bootstrapGo.AddComponent<Bootstrap>();
            WireBootstrap(bootstrap);
            EditorUtility.SetDirty(bootstrapGo);

            // EventSystem — required for all UI button/touch input
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();

            // Canvas
            var canvas = BuildCanvas();

            // Screens — all deactivated; UIManager.Start() activates Hub
            BuildHub(canvas.transform);
            BuildFleet(canvas.transform, prefabModuleRow);
            BuildMissionConfig(canvas.transform, prefabDestRow);
            BuildContractors(canvas.transform, prefabContractorRow);
            BuildEventCard(canvas.transform, prefabOptionBtn);
            BuildDebrief(canvas.transform);

            foreach (Transform child in canvas.transform)
                child.gameObject.SetActive(false);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[DTA] Scene built. Open Bootstrap.unity and hit Play.");
        }

        // ── Bootstrap wiring ─────────────────────────────────────────────────

        static void WireBootstrap(Bootstrap b)
        {
            b.StarMap         = Load<StarMapSO>("Assets/DeepTransit/ScriptableObjects/StarMap.asset");
            b.StarterBlueprint = Load<ShipBlueprintSO>("Assets/Resources/Blueprints/Blueprint_StarterFreighter.asset");

            b.StarterModules = new[]
            {
                Load<ShipModuleSO>("Assets/Resources/Modules/Module_Hull_Basic.asset"),
                Load<ShipModuleSO>("Assets/Resources/Modules/Module_Engine_Basic.asset"),
                Load<ShipModuleSO>("Assets/Resources/Modules/Module_LifeSupport.asset"),
                Load<ShipModuleSO>("Assets/Resources/Modules/Module_GrowBay.asset"),
                Load<ShipModuleSO>("Assets/Resources/Modules/Module_MedBay.asset"),
                Load<ShipModuleSO>("Assets/Resources/Modules/Module_Berths.asset"),
                Load<ShipModuleSO>("Assets/Resources/Modules/Module_CargoHold.asset"),
                Load<ShipModuleSO>("Assets/Resources/Modules/Module_Navigation.asset"),
                Load<ShipModuleSO>("Assets/Resources/Modules/Module_Security.asset"),
            };

            b.ContractorDefinitions = FindAll<ContractorSO>("Assets/Resources/Contractors");
            b.AllGameEvents         = FindAll<GameEventSO>("Assets/DeepTransit/ScriptableObjects/Events");
        }

        // ── Canvas ───────────────────────────────────────────────────────────

        static GameObject BuildCanvas()
        {
            var go = new GameObject("Canvas");
            var c  = go.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode       = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight  = 1f;  // match height — correct for portrait

            go.AddComponent<GraphicRaycaster>();
            go.AddComponent<Image>().color = C_BG;
            return go;
        }

        // ── Hub ──────────────────────────────────────────────────────────────

        static void BuildHub(Transform parent)
        {
            var root = Pnl("Hub", parent, C_BG);
            var hub  = root.AddComponent<HubScreen>();

            // Top currency bar
            var bar = Pnl("CurrencyBar", root.transform, C_PANEL);
            Anchors(bar, 0, 0.92f, 1, 1, 10, 5, -10, -5);
            var barL = bar.AddComponent<HorizontalLayoutGroup>();
            barL.childAlignment = TextAnchor.MiddleCenter;
            barL.spacing = 40; barL.padding = new RectOffset(20, 20, 0, 0);

            hub.SoftCurrencyText = Txt("SoftText", bar.transform, "¤ 5,000",  32, C_GREEN,  TextAnchor.MiddleCenter);
            hub.HardCurrencyText = Txt("HardText", bar.transform, "★ 0",      32, C_ACCENT, TextAnchor.MiddleCenter);
            hub.ReputationText   = Txt("RepText",  bar.transform, "Rep 0",    28, C_DIM,    TextAnchor.MiddleCenter);

            // Mission card
            var card = Pnl("MissionCard", root.transform, C_PANEL);
            Anchors(card, 0.04f, 0.24f, 0.96f, 0.90f, 0, 0, 0, 0);
            var cardL = card.AddComponent<VerticalLayoutGroup>();
            cardL.childAlignment = TextAnchor.UpperLeft;
            cardL.spacing = 12; cardL.padding = new RectOffset(25, 25, 20, 20);
            cardL.childForceExpandWidth = true; cardL.childForceExpandHeight = false;

            hub.MissionCardRoot = card;
            card.SetActive(false); // no active missions on first launch; RefreshMissionCard shows it when needed
            hub.MissionShipNameText    = Txt("ShipName",    card.transform, "ISV Pathfinder",        44, C_TEXT,   TextAnchor.MiddleLeft);
            hub.MissionDestinationText = Txt("Destination", card.transform, "→ Proxima Centauri b",  30, C_DIM,    TextAnchor.MiddleLeft);
            hub.MissionProgressSlider  = SliderEl("Progress", card.transform);
            SzEl(hub.MissionProgressSlider.gameObject, preferredHeight: 36);
            hub.MissionStatusText      = Txt("Status",      card.transform, "0%",                    28, C_ACCENT, TextAnchor.MiddleCenter);

            var statRow = HRow("StatRow", card.transform, 10);
            SzEl(statRow, preferredHeight: 40);
            hub.MissionHullText   = Txt("HullText",   statRow.transform, "Hull 100%",   26, C_TEXT, TextAnchor.MiddleLeft);
            hub.MissionMoraleText = Txt("MoraleText", statRow.transform, "Morale 100%", 26, C_TEXT, TextAnchor.MiddleRight);

            // No-mission panel
            var noMis = Pnl("NoMission", root.transform, C_PANEL);
            Anchors(noMis, 0.04f, 0.24f, 0.96f, 0.90f, 0, 0, 0, 0);
            var noMisL = noMis.AddComponent<VerticalLayoutGroup>();
            noMisL.childAlignment = TextAnchor.MiddleCenter;
            noMisL.childForceExpandWidth = true; noMisL.childForceExpandHeight = false;
            hub.NoMissionRoot = noMis;
            Txt("Label", noMis.transform, "No active mission",       40, C_DIM, TextAnchor.MiddleCenter);
            Txt("Sub",   noMis.transform, "Launch one to begin.",    28, C_DIM, TextAnchor.MiddleCenter);

            // Nav bar — explicit thirds, no LayoutGroup (LayoutGroup + stretch-anchored Btn() = zero-size)
            var nav = Pnl("NavBar", root.transform, C_PANEL);
            Anchors(nav, 0, 0, 1, 0.16f, 0, 0, 0, 0);

            hub.NavFleet = Btn("FleetBtn", nav.transform, "Fleet", C_PANEL, C_TEXT);
            Anchors(hub.NavFleet.gameObject, 0f, 0f, 1f / 3f, 1f, 6, 8, -3, -8);

            hub.NavMissionConfig = Btn("LaunchBtn", nav.transform, "Launch\nMission", C_ACCENT, Color.white);
            Anchors(hub.NavMissionConfig.gameObject, 1f / 3f, 0f, 2f / 3f, 1f, 3, 8, -3, -8);

            hub.NavContractors = Btn("CrewBtn", nav.transform, "Contractors", C_PANEL, C_TEXT);
            Anchors(hub.NavContractors.gameObject, 2f / 3f, 0f, 1f, 1f, 3, 8, -6, -8);

            foreach (var b in new[] { hub.NavFleet, hub.NavMissionConfig, hub.NavContractors })
            {
                var t = b.GetComponentInChildren<Text>();
                t.fontSize = 42;
                t.horizontalOverflow = HorizontalWrapMode.Wrap;
            }

            EditorUtility.SetDirty(root);
        }

        // ── Fleet ────────────────────────────────────────────────────────────

        static void BuildFleet(Transform parent, GameObject moduleRowPrefab)
        {
            var root  = Pnl("Fleet", parent, C_BG);
            var fleet = root.AddComponent<FleetScreen>();

            var backBtn = Btn("BackBtn", root.transform, "← Back", C_PANEL, C_TEXT);
            Anchors(backBtn.gameObject, 0, 0.93f, 0.28f, 1, 10, 5, -10, -5);
            fleet.BackButton = backBtn;

            var nameRow = HRow("NameRow", root.transform, 10);
            Anchors(nameRow, 0, 0.83f, 1, 0.93f, 20, 5, -20, -5);
            fleet.ShipNameText  = Txt("ShipName",  nameRow.transform, "ISV Pathfinder",  44, C_TEXT,   TextAnchor.MiddleLeft);
            fleet.ShipNameInput = InputFld("NameInput", nameRow.transform, "New name…");
            SzEl(fleet.ShipNameInput.gameObject, preferredWidth: 350, preferredHeight: 70);
            fleet.RenameButton  = Btn("RenameBtn", nameRow.transform, "Rename", C_ACCENT, Color.white);
            SzEl(fleet.RenameButton.gameObject, preferredWidth: 160);

            var stats = Pnl("Stats", root.transform, C_PANEL);
            Anchors(stats, 0, 0.63f, 1, 0.83f, 10, 5, -10, -5);
            var grid = stats.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(300, 44); grid.spacing = new Vector2(10, 5);
            grid.padding = new RectOffset(15, 15, 10, 10);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;

            fleet.HullStatText     = Txt("Hull",  stats.transform, "Hull: 0",        28, C_TEXT, TextAnchor.MiddleLeft);
            fleet.PassengerCapText = Txt("Pass",  stats.transform, "Passengers: 0",  28, C_TEXT, TextAnchor.MiddleLeft);
            fleet.CargoCapText     = Txt("Cargo", stats.transform, "Cargo: 0",       28, C_TEXT, TextAnchor.MiddleLeft);
            fleet.EngineText       = Txt("Eng",   stats.transform, "Engine: 0",      28, C_TEXT, TextAnchor.MiddleLeft);
            fleet.MedicalText      = Txt("Med",   stats.transform, "Medical: 0",     28, C_TEXT, TextAnchor.MiddleLeft);
            fleet.NavigationText   = Txt("Nav",   stats.transform, "Navigation: 0",  28, C_TEXT, TextAnchor.MiddleLeft);

            var (scroll, content) = ScrollView("ModuleScroll", root.transform);
            Anchors(scroll, 0, 0, 1, 0.63f, 10, 10, -10, -10);
            fleet.ModuleListParent = content.transform;
            fleet.ModuleRowPrefab  = moduleRowPrefab;

            EditorUtility.SetDirty(root);
        }

        // ── Mission Config ────────────────────────────────────────────────────

        static void BuildMissionConfig(Transform parent, GameObject destRowPrefab)
        {
            var root   = Pnl("MissionConfig", parent, C_BG);
            var config = root.AddComponent<MissionConfigScreen>();

            var backBtn = Btn("BackBtn", root.transform, "← Back", C_PANEL, C_TEXT);
            Anchors(backBtn.gameObject, 0, 0.93f, 0.28f, 1, 10, 5, -10, -5);
            config.BackButton = backBtn;

            Txt("Header", root.transform, "Select Destination", 40, C_TEXT, TextAnchor.MiddleCenter)
                .Let(t => Anchors(t.gameObject, 0, 0.86f, 1, 0.93f, 20, 0, -20, 0));

            var (destScroll, destContent) = ScrollView("DestScroll", root.transform);
            Anchors(destScroll, 0, 0.56f, 1, 0.86f, 10, 5, -10, -5);
            config.DestinationListParent = destContent.transform;
            config.DestinationRowPrefab  = destRowPrefab;

            config.SelectedDestinationText = Txt("SelectedDest",   root.transform, "No destination selected", 32, C_ACCENT, TextAnchor.MiddleCenter);
            Anchors(config.SelectedDestinationText.gameObject, 0, 0.50f, 1, 0.56f, 20, 0, -20, 0);
            config.VoyageDurationText = Txt("VoyageDuration", root.transform, "", 28, C_DIM, TextAnchor.MiddleCenter);
            Anchors(config.VoyageDurationText.gameObject, 0, 0.45f, 1, 0.50f, 20, 0, -20, 0);

            var cargo = Pnl("CargoSection", root.transform, C_PANEL);
            Anchors(cargo, 0, 0.20f, 1, 0.45f, 10, 5, -10, -5);
            var cargoL = cargo.AddComponent<VerticalLayoutGroup>();
            cargoL.spacing = 10; cargoL.padding = new RectOffset(20, 20, 15, 15);
            cargoL.childForceExpandWidth = true; cargoL.childForceExpandHeight = false;

            var passRow = HRow("PassRow", cargo.transform, 10);
            SzEl(passRow, preferredHeight: 60);
            Txt("PassLabel", passRow.transform, "Passengers:", 30, C_TEXT, TextAnchor.MiddleLeft);
            config.PassengerCountText = Txt("PassCount", passRow.transform, "0", 30, C_ACCENT, TextAnchor.MiddleCenter);
            SzEl(config.PassengerCountText.gameObject, preferredWidth: 60);
            config.PassengerSlider = SliderEl("PassSlider", passRow.transform);
            SzEl(config.PassengerSlider.gameObject, flexibleWidth: 1, preferredHeight: 40);

            var pkgRow = HRow("PkgRow", cargo.transform, 10);
            SzEl(pkgRow, preferredHeight: 60);
            Txt("PkgLabel", pkgRow.transform, "Packages:", 30, C_TEXT, TextAnchor.MiddleLeft);
            config.PackageCountText = Txt("PkgCount", pkgRow.transform, "0", 30, C_ACCENT, TextAnchor.MiddleCenter);
            SzEl(config.PackageCountText.gameObject, preferredWidth: 60);
            config.PackageSlider = SliderEl("PkgSlider", pkgRow.transform);
            SzEl(config.PackageSlider.gameObject, flexibleWidth: 1, preferredHeight: 40);

            config.CapacityWarningText = Txt("Warning",   cargo.transform, "", 26, C_WARN,  TextAnchor.MiddleCenter);
            config.EstimatedPayoutText = Txt("EstPayout", cargo.transform, "Est. payout: ¤0", 28, C_GREEN, TextAnchor.MiddleCenter);

            config.LaunchButton = Btn("LaunchBtn", root.transform, "LAUNCH MISSION", C_ACCENT, Color.white);
            Anchors(config.LaunchButton.gameObject, 0.10f, 0.05f, 0.90f, 0.18f, 0, 0, 0, 0);
            config.LaunchButton.GetComponentInChildren<Text>().fontSize = 38;

            EditorUtility.SetDirty(root);
        }

        // ── Contractors ───────────────────────────────────────────────────────

        static void BuildContractors(Transform parent, GameObject contractorRowPrefab)
        {
            var root   = Pnl("Contractors", parent, C_BG);
            var screen = root.AddComponent<ContractorScreen>();

            var backBtn = Btn("BackBtn", root.transform, "← Back", C_PANEL, C_TEXT);
            Anchors(backBtn.gameObject, 0, 0.93f, 0.28f, 1, 10, 5, -10, -5);
            screen.BackButton = backBtn;

            screen.RosterHeaderText = Txt("RosterHeader", root.transform, "Roster (0)", 38, C_TEXT, TextAnchor.MiddleCenter);
            Anchors(screen.RosterHeaderText.gameObject, 0, 0.86f, 1, 0.93f, 20, 0, -20, 0);

            var (rosterScroll, rosterContent) = ScrollView("RosterScroll", root.transform);
            Anchors(rosterScroll, 0, 0.55f, 1, 0.86f, 10, 5, -10, -5);
            screen.RosterParent = rosterContent.transform;

            var hireRow = HRow("HireHeader", root.transform, 10);
            Anchors(hireRow, 0, 0.48f, 1, 0.55f, 20, 5, -20, -5);
            Txt("HireLabel", hireRow.transform, "Available to Hire", 32, C_TEXT, TextAnchor.MiddleLeft);
            screen.RefreshPoolButton = Btn("RefreshBtn",  hireRow.transform, "Refresh Pool", C_PANEL, C_DIM);
            screen.RefreshCostText   = Txt("RefreshCost", hireRow.transform, "¤100",         28,      C_WARN, TextAnchor.MiddleRight);

            var (poolScroll, poolContent) = ScrollView("PoolScroll", root.transform);
            Anchors(poolScroll, 0, 0.05f, 1, 0.48f, 10, 5, -10, -5);
            screen.PoolParent = poolContent.transform;

            screen.ContractorRowPrefab = contractorRowPrefab;

            EditorUtility.SetDirty(root);
        }

        // ── Event Card ────────────────────────────────────────────────────────

        static void BuildEventCard(Transform parent, GameObject optionBtnPrefab)
        {
            var root   = Pnl("EventCard", parent, new Color(0.03f, 0.04f, 0.07f, 0.97f));
            var screen = root.AddComponent<EventCardScreen>();

            screen.SeverityText    = Txt("Severity",    root.transform, "CRITICAL",             42, C_WARN,  TextAnchor.UpperCenter);
            screen.TitleText       = Txt("Title",       root.transform, "Event Title",           52, C_TEXT,  TextAnchor.UpperCenter);
            screen.ShipNameText    = Txt("ShipName",    root.transform, "ISV Pathfinder",        30, C_DIM,   TextAnchor.UpperCenter);
            screen.DescriptionText = Txt("Description", root.transform, "Event description.",    30, C_TEXT,  TextAnchor.UpperLeft);

            Anchors(screen.SeverityText.gameObject,    0, 0.76f, 1, 0.85f, 20, 0, -20, 0);
            Anchors(screen.TitleText.gameObject,       0, 0.66f, 1, 0.76f, 20, 0, -20, 0);
            Anchors(screen.ShipNameText.gameObject,    0, 0.61f, 1, 0.66f, 20, 0, -20, 0);
            Anchors(screen.DescriptionText.gameObject, 0, 0.48f, 1, 0.61f, 20, 0, -20, 0);

            screen.CountdownText = Txt("Countdown", root.transform, "", 28, C_WARN, TextAnchor.MiddleCenter);
            Anchors(screen.CountdownText.gameObject, 0, 0.86f, 1, 0.92f, 20, 0, -20, 0);

            var statRow = HRow("StatRow", root.transform, 15);
            Anchors(statRow, 0, 0.47f, 1, 0.54f, 20, 5, -20, -5);
            screen.HullText   = Txt("Hull",   statRow.transform, "Hull 100%",   24, C_TEXT,  TextAnchor.MiddleCenter);
            screen.MoraleText = Txt("Morale", statRow.transform, "Morale 100%", 24, C_TEXT,  TextAnchor.MiddleCenter);
            screen.CargoText  = Txt("Cargo",  statRow.transform, "Cargo 100%",  24, C_TEXT,  TextAnchor.MiddleCenter);
            screen.FoodText   = Txt("Food",   statRow.transform, "Food 100%",   24, C_TEXT,  TextAnchor.MiddleCenter);

            var optPanel = Pnl("OptionsPanel", root.transform, Color.clear);
            Anchors(optPanel, 0, 0.05f, 1, 0.47f, 20, 10, -20, -10);
            var optL = optPanel.AddComponent<VerticalLayoutGroup>();
            optL.spacing = 15; optL.childForceExpandWidth = true; optL.childForceExpandHeight = false;
            screen.OptionsParent      = optPanel.transform;
            screen.OptionButtonPrefab = optionBtnPrefab;

            EditorUtility.SetDirty(root);
        }

        // ── Debrief ───────────────────────────────────────────────────────────

        static void BuildDebrief(Transform parent)
        {
            var root   = Pnl("Debrief", parent, C_BG);
            var screen = root.AddComponent<DebriefScreen>();

            var vl = root.AddComponent<VerticalLayoutGroup>();
            vl.childAlignment = TextAnchor.UpperCenter;
            vl.spacing = 14; vl.padding = new RectOffset(40, 40, 80, 40);
            vl.childForceExpandWidth = true; vl.childForceExpandHeight = false;

            screen.ShipNameText    = Txt("ShipName",    root.transform, "ISV Pathfinder",      44, C_TEXT,   TextAnchor.MiddleCenter);
            screen.DestinationText = Txt("Destination", root.transform, "Proxima Centauri b",  30, C_DIM,    TextAnchor.MiddleCenter);
            screen.OutcomeText     = Txt("Outcome",     root.transform, "MISSION COMPLETE",    54, C_GREEN,  TextAnchor.MiddleCenter);

            SzEl(screen.ShipNameText.gameObject,    preferredHeight: 60);
            SzEl(screen.DestinationText.gameObject, preferredHeight: 40);
            SzEl(screen.OutcomeText.gameObject,     preferredHeight: 70);

            var bd = Pnl("Breakdown", root.transform, C_PANEL);
            SzEl(bd, preferredHeight: 370);
            var bdL = bd.AddComponent<VerticalLayoutGroup>();
            bdL.spacing = 8; bdL.padding = new RectOffset(25, 25, 15, 15);
            bdL.childForceExpandWidth = true; bdL.childForceExpandHeight = false;

            screen.PassengerRevenueText = Txt("PassRev",  bd.transform, "Passenger revenue:  ¤0",   30, C_TEXT,   TextAnchor.MiddleLeft);
            screen.PackageRevenueText   = Txt("PkgRev",   bd.transform, "Package revenue:    ¤0",   30, C_TEXT,   TextAnchor.MiddleLeft);
            screen.HullPenaltyText      = Txt("HullPen",  bd.transform, "Hull penalty:       -0%",  28, C_WARN,   TextAnchor.MiddleLeft);
            screen.MoraleFactorText     = Txt("Morale",   bd.transform, "Morale factor:      ×1.00",28, C_DIM,    TextAnchor.MiddleLeft);
            screen.TotalPayoutText      = Txt("Total",    bd.transform, "TOTAL:  ¤0",               42, C_GREEN,  TextAnchor.MiddleLeft);
            screen.ReputationGainText   = Txt("Rep",      bd.transform, "+0 Reputation",            30, C_ACCENT, TextAnchor.MiddleLeft);

            foreach (Transform t in bd.transform) SzEl(t.gameObject, preferredHeight: 44);

            var condRow = HRow("CondRow", root.transform, 20);
            SzEl(condRow, preferredHeight: 40);
            screen.FinalHullText   = Txt("FinalHull",   condRow.transform, "Final hull: 100%",   28, C_DIM, TextAnchor.MiddleLeft);
            screen.FinalMoraleText = Txt("FinalMorale", condRow.transform, "Final morale: 100%", 28, C_DIM, TextAnchor.MiddleRight);

            screen.ContinueButton = Btn("ContinueBtn", root.transform, "CONTINUE", C_ACCENT, Color.white);
            screen.ContinueButton.GetComponentInChildren<Text>().fontSize = 40;
            SzEl(screen.ContinueButton.gameObject, preferredHeight: 100);

            EditorUtility.SetDirty(root);
        }

        // ── Prefabs ───────────────────────────────────────────────────────────

        static GameObject MakeContractorRowPrefab()
        {
            var go = new GameObject("ContractorRowPrefab");
            var hl = go.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 10; hl.childForceExpandHeight = true;
            SzEl(go, preferredHeight: 72);

            Txt("NameText", go.transform, "Name — Role  Exp  Rate/day", 26, C_TEXT, TextAnchor.MiddleLeft)
                .Let(t => SzEl(t.gameObject, flexibleWidth: 1));
            Btn("ActionButton", go.transform, "Hire", C_ACCENT, Color.white)
                .Let(b => { SzEl(b.gameObject, preferredWidth: 130, preferredHeight: 60); b.GetComponentInChildren<Text>().fontSize = 26; });

            return SavePrefab(go, "ContractorRowPrefab");
        }

        static GameObject MakeDestRowPrefab()
        {
            var btn = Btn("DestinationRowPrefab", null, "Destination  ×1.0  Rep 0+", C_PANEL, C_TEXT);
            btn.GetComponentInChildren<Text>().Let(t => { t.alignment = TextAnchor.MiddleLeft; t.fontSize = 28; });
            SzEl(btn.gameObject, preferredHeight: 72);
            return SavePrefab(btn.gameObject, "DestinationRowPrefab");
        }

        static GameObject MakeModuleRowPrefab()
        {
            var go = new GameObject("ModuleRowPrefab");
            var hl = go.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 10; hl.childForceExpandHeight = true;
            SzEl(go, preferredHeight: 72);

            Txt("ModuleText", go.transform, "Slot: Module  Tier", 26, C_TEXT, TextAnchor.MiddleLeft)
                .Let(t => SzEl(t.gameObject, flexibleWidth: 1));
            Btn("UpgradeButton", go.transform, "Upgrade ¤0", C_ACCENT, Color.white)
                .Let(b => { SzEl(b.gameObject, preferredWidth: 175, preferredHeight: 60); b.GetComponentInChildren<Text>().fontSize = 24; });

            return SavePrefab(go, "ModuleRowPrefab");
        }

        static GameObject MakeOptionBtnPrefab()
        {
            var btn = Btn("OptionButtonPrefab", null, "Option  [Role]  70%", C_PANEL, C_TEXT);
            btn.GetComponentInChildren<Text>().Let(t => { t.alignment = TextAnchor.MiddleLeft; t.fontSize = 30; });
            SzEl(btn.gameObject, preferredHeight: 95);
            return SavePrefab(btn.gameObject, "OptionButtonPrefab");
        }

        // ── UI primitives ─────────────────────────────────────────────────────

        static GameObject Pnl(string name, Transform parent, Color color)
        {
            var go  = new GameObject(name);
            if (parent) go.transform.SetParent(parent, false);
            go.AddComponent<Image>().color = color;
            Full(go.GetComponent<RectTransform>());
            return go;
        }

        static Text Txt(string name, Transform parent, string content, int size, Color color, TextAnchor anchor)
        {
            var go = new GameObject(name);
            if (parent) go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.text = content; t.fontSize = size; t.color = color;
            t.alignment = anchor; t.font = _font;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow   = VerticalWrapMode.Overflow;
            Full(go.GetComponent<RectTransform>());
            return t;
        }

        static Button Btn(string name, Transform parent, string label, Color bg, Color fg)
        {
            var go = new GameObject(name);
            if (parent) go.transform.SetParent(parent, false);
            go.AddComponent<Image>().color = bg;
            var btn = go.AddComponent<Button>();
            var cols = btn.colors;
            cols.highlightedColor = bg * 1.25f; cols.pressedColor = bg * 0.75f;
            btn.colors = cols;
            Full(go.GetComponent<RectTransform>());

            var tGo = new GameObject("Text");
            tGo.transform.SetParent(go.transform, false);
            var txt = tGo.AddComponent<Text>();
            txt.text = label; txt.fontSize = 30; txt.color = fg;
            txt.alignment = TextAnchor.MiddleCenter; txt.font = _font;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow   = VerticalWrapMode.Overflow;
            Full(tGo.GetComponent<RectTransform>());
            return btn;
        }

        static Slider SliderEl(string name, Transform parent)
        {
            var go = new GameObject(name);
            if (parent) go.transform.SetParent(parent, false);
            // AddComponent<Slider> via [RequireComponent] creates the RectTransform first
            var slider = go.AddComponent<UnityEngine.UI.Slider>();
            Full(go.GetComponent<RectTransform>());

            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(go.transform, false);
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = C_PANEL; bgImg.raycastTarget = false;
            Full(bgGo.GetComponent<RectTransform>());

            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(go.transform, false);
            var faImg = fillArea.AddComponent<Image>(); // forces RectTransform
            faImg.color = Color.clear; faImg.raycastTarget = false;
            Full(fillArea.GetComponent<RectTransform>());

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = C_ACCENT; fillImg.raycastTarget = false;
            Full(fill.GetComponent<RectTransform>());

            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.targetGraphic = bgImg;
            slider.minValue = 0; slider.maxValue = 10; slider.wholeNumbers = true;
            return slider;
        }

        static InputField InputFld(string name, Transform parent, string placeholder)
        {
            var go = new GameObject(name);
            if (parent) go.transform.SetParent(parent, false);
            go.AddComponent<Image>().color = C_PANEL;
            Full(go.GetComponent<RectTransform>());

            var ph = new GameObject("Placeholder");
            ph.transform.SetParent(go.transform, false);
            var phT = ph.AddComponent<Text>();
            phT.text = placeholder; phT.color = C_DIM; phT.fontSize = 28; phT.font = _font;
            phT.alignment = TextAnchor.MiddleLeft;
            Full(ph.GetComponent<RectTransform>());

            var tGo = new GameObject("Text");
            tGo.transform.SetParent(go.transform, false);
            var txt = tGo.AddComponent<Text>();
            txt.color = C_TEXT; txt.fontSize = 28; txt.font = _font;
            txt.alignment = TextAnchor.MiddleLeft;
            Full(tGo.GetComponent<RectTransform>());

            var inp = go.AddComponent<InputField>();
            inp.textComponent = txt; inp.placeholder = phT; inp.targetGraphic = go.GetComponent<Image>();
            return inp;
        }

        static (GameObject scroll, GameObject content) ScrollView(string name, Transform parent)
        {
            var root = new GameObject(name);
            if (parent) root.transform.SetParent(parent, false);
            root.AddComponent<Image>().color = Color.clear;
            Full(root.GetComponent<RectTransform>());

            var vp = new GameObject("Viewport");
            vp.transform.SetParent(root.transform, false);
            vp.AddComponent<Image>().color = new Color(0, 0, 0, 0.01f);
            vp.AddComponent<Mask>().showMaskGraphic = false;
            Full(vp.GetComponent<RectTransform>());

            var content = new GameObject("Content");
            content.transform.SetParent(vp.transform, false);
            // VerticalLayoutGroup has [RequireComponent(RectTransform)] — add before reading RT
            var vl = content.AddComponent<VerticalLayoutGroup>();
            var cRT = content.GetComponent<RectTransform>();
            cRT.anchorMin = new Vector2(0, 1); cRT.anchorMax = new Vector2(1, 1);
            cRT.pivot = new Vector2(0.5f, 1); cRT.offsetMin = cRT.offsetMax = Vector2.zero;

            vl.childForceExpandWidth = true; vl.childForceExpandHeight = false; vl.spacing = 5;
            var csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var sr = root.AddComponent<ScrollRect>();
            sr.content = cRT; sr.viewport = vp.GetComponent<RectTransform>();
            sr.horizontal = false; sr.vertical = true;

            return (root, content);
        }

        static GameObject HRow(string name, Transform parent, float spacing)
        {
            var go = new GameObject(name);
            if (parent) go.transform.SetParent(parent, false);
            var hl = go.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = spacing; hl.childForceExpandHeight = true; hl.childForceExpandWidth = false;
            Full(go.GetComponent<RectTransform>());
            return go;
        }

        // ── Layout / anchor helpers ───────────────────────────────────────────

        static void Full(RectTransform rt)
        { rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero; }

        static void Anchors(GameObject go, float x0, float y0, float x1, float y1,
            float l = 0, float b = 0, float r = 0, float t = 0)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(x0, y0); rt.anchorMax = new Vector2(x1, y1);
            rt.offsetMin = new Vector2(l, b);   rt.offsetMax = new Vector2(r, t);
        }

        static void SzEl(GameObject go, float preferredWidth = -1, float preferredHeight = -1, float flexibleWidth = -1)
        {
            var le = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
            if (preferredWidth  >= 0) le.preferredWidth  = preferredWidth;
            if (preferredHeight >= 0) le.preferredHeight = preferredHeight;
            if (flexibleWidth   >= 0) le.flexibleWidth   = flexibleWidth;
        }

        // ── Prefab / asset helpers ────────────────────────────────────────────

        static GameObject SavePrefab(GameObject go, string name)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabRoot}/{name}.prefab");
            Object.DestroyImmediate(go);
            return prefab;
        }

        static T Load<T>(string path) where T : Object
        {
            var a = AssetDatabase.LoadAssetAtPath<T>(path);
            if (a == null) Debug.LogWarning($"[DTA] Missing asset: {path}");
            return a;
        }

        static T[] FindAll<T>(string folder) where T : Object =>
            AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folder })
                .Select(g => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null).ToArray();

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parts = path.Split('/');
            string cur = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = cur + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }
    }

    // Minimal fluent helper so we can chain without extra variables
    static class FluentExt
    {
        public static T Let<T>(this T obj, System.Action<T> action) { action(obj); return obj; }
    }
}
#endif
