using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using Minesweeper.Core;
using Minesweeper.Audio;
using Minesweeper.UI;

namespace Minesweeper.Editor
{
    /// <summary>
    /// Generates the "Cyber HUD Sapper" scene from scratch.
    /// Layout follows the Stitch mockups in
    /// stitch_cyberpunk_minesweeper_ui_system (game screen / leaderboard /
    /// settings / victory / defeat) and the design tokens of DESIGN.md.
    /// </summary>
    public static class CyberMinesweeperSceneBuilder
    {
        /// <summary>
        /// The HUD is laid out for a native 1920x1080 canvas (NFR-001): one canvas unit is one
        /// physical pixel at the target resolution, so the Canvas Scaler factor is exactly 1.0
        /// and nothing is resampled. All literals in this file are authored on a 1600x900
        /// design grid and multiplied by this factor inside the factory helpers.
        /// </summary>
        private const float UI_SCALE = 1.2f;

        // ------------------------------------------------------------------
        // Design tokens (tailwind palette of the mockups + DESIGN.md)
        // ------------------------------------------------------------------
        private static readonly Color ColBackground = Hex("#0B0E17");
        private static readonly Color ColTopBar = Hex("#0D121C");
        private static readonly Color ColBottomBar = Hex("#0A0E16");
        private static readonly Color ColSurfaceLowest = Hex("#070A11");
        private static readonly Color ColSurfaceLow = Hex("#111622");
        private static readonly Color ColSurface = Hex("#181E2B");
        private static readonly Color ColSurfaceHigh = Hex("#222A3A");
        private static readonly Color ColSurfaceHighest = Hex("#2E374A");
        private static readonly Color ColOutline = Hex("#2B3548");
        private static readonly Color ColPrimary = Hex("#E0FDFF");
        private static readonly Color ColPrimaryContainer = Hex("#00F2FE");
        private static readonly Color ColOnPrimaryContainer = Hex("#003B40");
        private static readonly Color ColSecondary = Hex("#38BDF8");
        private static readonly Color ColError = Hex("#F87171");
        private static readonly Color ColOnSurface = Hex("#DFE4F0");
        private static readonly Color ColOnSurfaceVariant = Hex("#929DB2");
        private static readonly Color ColSuccess = Hex("#34D399");
        private static readonly Color ColAmber = Hex("#FBBF24");
        private static readonly Color ColCrimson = Hex("#FF0055");
        private static readonly Color ColRose = Hex("#F43F5E");

        // Deck geometry (design grid is 1600x900; CreateUIObject applies UI_SCALE)
        private const float DeckWidth = 1060f;
        private const float DeckHeight = 800f;
        private const float GridAvailableWidth = 1028f;
        private const float GridAvailableHeight = 604f;

        [MenuItem("Tools/Build Cyber Minesweeper Scene")]
        public static void BuildScene()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                Debug.LogWarning("Exited play mode before building scene. Please run again once stopped.");
                return;
            }

            Debug.Log("Starting Cyber Minesweeper Scene generation (Modern Cyberpunk HUD v2.4 PRO)...");
            Directory.CreateDirectory("Assets/Prefabs");
            Directory.CreateDirectory("Assets/Scenes");

            EnsureSpriteBorders();

            // 1. Load Fonts -------------------------------------------------
            var fontMono = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/JetBrainsMono SDF.asset");
            var fontSans = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/SpaceGrotesk SDF.asset");
            if (fontMono == null) fontMono = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if (fontSans == null) fontSans = fontMono;

            // Load Sprites --------------------------------------------------
            var sCellClosed = Load<Sprite>("Assets/Sprites/cell_closed.png");
            var sCellHover = Load<Sprite>("Assets/Sprites/cell_hover.png");
            var sCellPressed = Load<Sprite>("Assets/Sprites/cell_pressed.png");
            var sCellRevealed = Load<Sprite>("Assets/Sprites/cell_revealed.png");
            var sCellExploded = Load<Sprite>("Assets/Sprites/cell_exploded.png");
            var sIconFlag = Load<Sprite>("Assets/Sprites/icon_flag.png");
            var sIconMine = Load<Sprite>("Assets/Sprites/icon_mine.png");
            var sIconFalseFlag = Load<Sprite>("Assets/Sprites/icon_false_flag.png");

            var sFaceNormal = Load<Sprite>("Assets/Sprites/face_normal.png");
            var sFaceAlert = Load<Sprite>("Assets/Sprites/face_alert.png");
            var sFaceWin = Load<Sprite>("Assets/Sprites/face_win.png");
            var sFaceDefeat = Load<Sprite>("Assets/Sprites/face_defeat.png");

            var sPanelCyber = Load<Sprite>("Assets/Sprites/panel_cyber.png");
            var sPanelDanger = Load<Sprite>("Assets/Sprites/panel_danger.png");
            var sPanelDeck = Load<Sprite>("Assets/Sprites/panel_deck.png");
            var sPanelModal = Load<Sprite>("Assets/Sprites/panel_modal.png");
            var sBtnCyber = Load<Sprite>("Assets/Sprites/button_cyber.png");
            var sBtnDanger = Load<Sprite>("Assets/Sprites/button_danger.png");
            var sBtnAccent = Load<Sprite>("Assets/Sprites/button_accent.png");

            var sIconRestart = Load<Sprite>("Assets/Sprites/icon_restart.png");
            var sIconPause = Load<Sprite>("Assets/Sprites/icon_pause.png");
            var sIconPlay = Load<Sprite>("Assets/Sprites/icon_play.png");
            var sIconHint = Load<Sprite>("Assets/Sprites/icon_hint.png");
            var sIconSoundOn = Load<Sprite>("Assets/Sprites/icon_sound_on.png");
            var sIconSoundOff = Load<Sprite>("Assets/Sprites/icon_sound_off.png");
            var sIconFullscreen = Load<Sprite>("Assets/Sprites/icon_fullscreen.png");
            var sIconSettings = Load<Sprite>("Assets/Sprites/icon_settings.png");
            var sIconGamepad = Load<Sprite>("Assets/Sprites/icon_gamepad.png");
            var sIconTimer = Load<Sprite>("Assets/Sprites/icon_timer.png");
            var sIconChevron = Load<Sprite>("Assets/Sprites/icon_chevron_down.png");
            var sIconGrid = Load<Sprite>("Assets/Sprites/icon_grid.png");
            var sIconBarChart = Load<Sprite>("Assets/Sprites/icon_barchart.png");
            var sIconSliders = Load<Sprite>("Assets/Sprites/icon_sliders.png");
            var sIconAlertCircle = Load<Sprite>("Assets/Sprites/icon_alert_circle.png");
            var sIconTarget = Load<Sprite>("Assets/Sprites/icon_target.png");
            var sRingCyan = Load<Sprite>("Assets/Sprites/ring_cyan.png");
            var sIconClose = Load<Sprite>("Assets/Sprites/icon_close.png");
            var sIconRulesSprite = Load<Sprite>("Assets/Sprites/icon_rules.png");

            // 9-sliced HUD chrome
            var sTabActive = Load<Sprite>("Assets/Sprites/tab_active.png");
            var sCardActive = Load<Sprite>("Assets/Sprites/card_active.png");
            var sSlotCounter = Load<Sprite>("Assets/Sprites/slot_counter.png");
            var sSlotInset = Load<Sprite>("Assets/Sprites/slot_inset.png");
            var sSlotMini = Load<Sprite>("Assets/Sprites/slot_mini.png");
            var sDotSprite = Load<Sprite>("Assets/Sprites/badge_dot.png");
            var sReplaySprite = Load<Sprite>("Assets/Sprites/icon_replay.png");

            // 2. Create Scene ----------------------------------------------
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = ColBackground;
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            camGo.transform.position = new Vector3(0, 0, -10);

            var managersGo = new GameObject("GameManagers", typeof(GameController), typeof(AudioManager));

            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var esGo = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(InputSystemUIInputModule));

            // Backdrop
            var bgGo = CreateUIObject("Background", canvasGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            bgGo.AddComponent<Image>().color = ColBackground;

            // 3. CENTRAL TACTICAL DECK --------------------------------------
            // The deck is created before the navigation bar so the system dropdown
            // menus of the top bar are drawn on top of it.
            var consoleDeck = BuildTacticalDeck(canvasGo.transform, fontMono, fontSans,
                sPanelDeck, sBtnCyber, sBtnAccent, sBtnDanger,
                sIconRestart, sIconPause, sIconPlay, sIconHint, sIconFlag, sIconTimer,
                sIconGrid, sRingCyan, sDotSprite, sSlotCounter, sSlotInset,
                sFaceNormal, sFaceAlert, sFaceWin, sFaceDefeat,
                sCellClosed, sCellHover, sCellPressed, sCellRevealed, sCellExploded,
                sIconFlag, sIconMine, sIconFalseFlag, fontMono,
                out var headerStatusView);

            // 4. TOP NAVIGATION BAR ----------------------------------------
            var topBar = BuildTopBar(canvasGo.transform, fontMono, sIconGamepad, sIconSoundOn, sIconSoundOff,
                sIconFullscreen, sIconSettings, sIconChevron, sBtnCyber, sTabActive, sDotSprite, sPanelModal,
                out var btnGame, out var gameTabBg,
                out var btnRecords, out var recordsTabBg,
                out var btnSettings, out var settingsTabBg,
                out var btnHelp, out var helpTabBg,
                out var btnDiffPill, out var diffPillText,
                out var btnSound, out var soundIconImg,
                out var btnFullscreen, out var btnGear);

            // 5. BOTTOM SYSTEM STATUS BAR ----------------------------------
            var bottomBar = BuildBottomStatusBar(canvasGo.transform, fontMono);

            // 6. MODALS LAYER ----------------------------------------------
            var modalsLayer = CreateUIObject("ModalsLayer", canvasGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var defeatModal = BuildDefeatModal(modalsLayer.transform, fontMono, sPanelModal, sBtnDanger, sBtnCyber, sIconAlertCircle, sSlotMini);
            var victoryModal = BuildVictoryModal(modalsLayer.transform, fontMono, sPanelModal, sBtnCyber, sBtnAccent, sIconTarget, sTabActive, sDotSprite);
            var settingsModal2 = BuildSettingsModal(modalsLayer.transform, fontMono, fontSans, sPanelModal, sBtnCyber, sBtnAccent, sIconSliders, sIconClose, sCardActive);
            var leaderboardsModal2 = BuildLeaderboardsModal(modalsLayer.transform, fontMono, sPanelModal, sBtnCyber, sBtnAccent, sIconBarChart, sIconClose, sReplaySprite, sDotSprite, sTabActive, sBtnCyber);
            var helpModal2 = BuildHelpModal(modalsLayer.transform, fontMono, fontSans, sPanelModal, sBtnCyber, sBtnDanger, sIconRulesSprite, sIconClose, sSlotMini, sTabActive);

            // 7. Wire the top bar ------------------------------------------
            var topMenuComp = topBar.AddComponent<TopMenuBar>();
            SetPrivateField(topMenuComp, "_btnNewGame", btnGame);
            SetPrivateField(topMenuComp, "_btnRecords", btnRecords);
            SetPrivateField(topMenuComp, "_btnSettings", btnSettings);
            SetPrivateField(topMenuComp, "_btnHelp", btnHelp);
            SetPrivateField(topMenuComp, "_gameTabBg", gameTabBg);
            SetPrivateField(topMenuComp, "_recordsTabBg", recordsTabBg);
            SetPrivateField(topMenuComp, "_settingsTabBg", settingsTabBg);
            SetPrivateField(topMenuComp, "_helpTabBg", helpTabBg);
            SetPrivateField(topMenuComp, "_btnDifficultyPill", btnDiffPill);
            SetPrivateField(topMenuComp, "_difficultyPillText", diffPillText);
            SetPrivateField(topMenuComp, "_btnSoundToggle", btnSound);
            SetPrivateField(topMenuComp, "_soundIconImage", soundIconImg);
            SetPrivateField(topMenuComp, "_btnFullscreen", btnFullscreen);
            SetPrivateField(topMenuComp, "_btnSettingsGear", btnGear);
            SetPrivateField(topMenuComp, "_settingsModal", settingsModal2);
            SetPrivateField(topMenuComp, "_leaderboardsModal", leaderboardsModal2);
            SetPrivateField(topMenuComp, "_helpAboutModal", helpModal2);
            topMenuComp.SoundOnSprite = sIconSoundOn;
            topMenuComp.SoundOffSprite = sIconSoundOff;
            topMenuComp.ActiveTabSprite = sTabActive;
            topMenuComp.InactiveTabSprite = sBtnCyber;

            BindAllCustomScriptsInScene();

            string scenePath = "Assets/Scenes/CyberMinesweeper.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"Successfully updated and saved scene to {scenePath}!");

            var buildScenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };
            EditorBuildSettings.scenes = buildScenes;
        }

        // ==================================================================
        // TOP BAR
        // ==================================================================
        private static GameObject BuildTopBar(Transform parent, TMP_FontAsset fontMono,
            Sprite sGamepad, Sprite sSoundOn, Sprite sSoundOff, Sprite sFullscreen, Sprite sSettingsGear, Sprite sChevron,
            Sprite sBtnCyber, Sprite sTabActive, Sprite sDotSprite, Sprite sPanelModal,
            out Button btnGame, out Image gameTabBg,
            out Button btnRecords, out Image recordsTabBg,
            out Button btnSettings, out Image settingsTabBg,
            out Button btnHelp, out Image helpTabBg,
            out Button btnDiffPill, out TextMeshProUGUI diffPillText,
            out Button btnSound, out Image soundIconImg,
            out Button btnFullscreen, out Button btnGear)
        {
            var topBar = CreateUIObject("TopMenuBar", parent, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 56));
            var topRt = topBar.GetComponent<RectTransform>();
            topRt.pivot = new Vector2(0.5f, 1f);
            topRt.anchoredPosition = Vector2.zero;
            topBar.AddComponent<Image>().color = ColTopBar;

            // --- Left cluster: brand + nav tabs (anchored to the left edge) ---
            var leftCluster = CreateUIObject("LeftCluster", topBar.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(660, 34));
            var lcRt = leftCluster.GetComponent<RectTransform>();
            lcRt.pivot = new Vector2(0f, 0.5f);
            lcRt.anchoredPosition = new Vector2(28f * UI_SCALE, 0f * UI_SCALE);
            var lcLe = leftCluster.AddComponent<LayoutElement>();
            lcLe.ignoreLayout = true;

            var hlgTop = leftCluster.AddComponent<HorizontalLayoutGroup>();
            hlgTop.spacing = 18 * UI_SCALE;
            hlgTop.childAlignment = TextAnchor.MiddleLeft;
            hlgTop.childControlWidth = false;
            hlgTop.childControlHeight = false;
            hlgTop.childForceExpandWidth = false;

            // --- Brand group ---
            var brandGo = CreateUIObject("Brand", leftCluster.transform, Vector2.zero, Vector2.zero, new Vector2(210, 32));
            var hlgBrand = brandGo.AddComponent<HorizontalLayoutGroup>();
            hlgBrand.spacing = 8 * UI_SCALE;
            hlgBrand.childAlignment = TextAnchor.MiddleLeft;
            hlgBrand.childControlWidth = false;
            hlgBrand.childControlHeight = false;

            if (sGamepad != null)
            {
                var iconGo = CreateUIObject("GamepadIcon", brandGo.transform, Vector2.zero, Vector2.zero, new Vector2(20, 20));
                var gpImg = iconGo.AddComponent<Image>();
                gpImg.sprite = sGamepad;
                gpImg.color = ColPrimaryContainer;
                gpImg.raycastTarget = false;
            }

            var titleText = CreateText(brandGo.transform, "САПЁР", fontMono, 18, FontStyles.Bold, ColPrimary, tracking: 4);
            titleText.textWrappingMode = TextWrappingModes.NoWrap;
            titleText.GetComponent<RectTransform>().sizeDelta = new Vector2(74 * UI_SCALE, 26 * UI_SCALE);

            var badgeGo = CreateUIObject("BadgePro", brandGo.transform, Vector2.zero, Vector2.zero, new Vector2(70, 22));
            badgeGo.AddComponent<Image>().color = ColSurfaceHigh;
            var badgeText = CreateText(badgeGo.transform, "v2.4 PRO", fontMono, 10, FontStyles.Bold, ColOnSurfaceVariant, tracking: 4);
            badgeText.alignment = TextAlignmentOptions.Center;
            badgeText.textWrappingMode = TextWrappingModes.NoWrap;
            var btRt = badgeText.GetComponent<RectTransform>();
            btRt.anchorMin = Vector2.zero; btRt.anchorMax = Vector2.one; btRt.offsetMin = Vector2.zero; btRt.offsetMax = Vector2.zero;

            // --- Nav tabs ---
            var tabsGroup = CreateUIObject("NavTabs", leftCluster.transform, Vector2.zero, Vector2.zero, new Vector2(400, 34));
            var hlgTabs = tabsGroup.AddComponent<HorizontalLayoutGroup>();
            hlgTabs.spacing = 6 * UI_SCALE;
            hlgTabs.childAlignment = TextAnchor.MiddleLeft;
            hlgTabs.childControlHeight = false;
            hlgTabs.childControlWidth = false;

            btnGame = CreateNavTab(tabsGroup.transform, "Игра", fontMono, sBtnCyber, sTabActive, out gameTabBg, isPrimary: true);
            btnRecords = CreateNavTab(tabsGroup.transform, "Рекорды", fontMono, sBtnCyber, sTabActive, out recordsTabBg);
            btnSettings = CreateNavTab(tabsGroup.transform, "Параметры", fontMono, sBtnCyber, sTabActive, out settingsTabBg);
            btnHelp = CreateNavTab(tabsGroup.transform, "Справка", fontMono, sBtnCyber, sTabActive, out helpTabBg);
            // --- Right controls (anchored to the right edge) ---
            var rightGroup = CreateUIObject("RightControls", topBar.transform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(430, 34));
            var rightRt = rightGroup.GetComponent<RectTransform>();
            rightRt.pivot = new Vector2(1f, 0.5f);
            rightRt.anchoredPosition = new Vector2(-28f * UI_SCALE, 0f * UI_SCALE);
            var rightLe = rightGroup.AddComponent<LayoutElement>();
            rightLe.ignoreLayout = true;
            var hlgRight = rightGroup.AddComponent<HorizontalLayoutGroup>();
            hlgRight.spacing = 10 * UI_SCALE;
            hlgRight.childAlignment = TextAnchor.MiddleRight;
            hlgRight.childControlWidth = false;
            hlgRight.childControlHeight = false;

            // Difficulty pill
            var diffPillGo = CreateUIObject("DifficultyPill", rightGroup.transform, Vector2.zero, Vector2.zero, new Vector2(290, 34));
            diffPillGo.AddComponent<Image>().color = ColSurfaceLow;
            btnDiffPill = diffPillGo.AddComponent<Button>();
            var hlgDP = diffPillGo.AddComponent<HorizontalLayoutGroup>();
            hlgDP.padding = Pad(12, 10, 0, 0);
            hlgDP.spacing = 8 * UI_SCALE;
            hlgDP.childAlignment = TextAnchor.MiddleLeft;
            hlgDP.childControlWidth = false;
            hlgDP.childControlHeight = false;

            CreatePulseDot(diffPillGo.transform, ColSuccess, 8f, sDotSprite);
            diffPillText = CreateText(diffPillGo.transform, "Любитель (16×16) • 40 мин", fontMono, 11, FontStyles.Bold, ColPrimary, tracking: 2);
            diffPillText.textWrappingMode = TextWrappingModes.NoWrap;
            diffPillText.GetComponent<RectTransform>().sizeDelta = new Vector2(228 * UI_SCALE, 22 * UI_SCALE);

            var diffChevronGo = CreateUIObject("Chevron", diffPillGo.transform, Vector2.zero, Vector2.zero, new Vector2(14, 14));
            var diffChevronImg = diffChevronGo.AddComponent<Image>();
            diffChevronImg.sprite = sChevron;
            diffChevronImg.color = ColOnSurfaceVariant;
            diffChevronImg.raycastTarget = false;

            btnSound = CreateIconButton("BtnSound", rightGroup.transform, sSoundOn, out soundIconImg).GetComponent<Button>();
            btnFullscreen = CreateIconButton("BtnFullscreen", rightGroup.transform, sFullscreen, out _).GetComponent<Button>();
            btnGear = CreateIconButton("BtnSettingsGear", rightGroup.transform, sSettingsGear, out _).GetComponent<Button>();

            // Hairline separator under the navigation bar
            var topDivider = CreateUIObject("TopDivider", topBar.transform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1));
            var tdLe = topDivider.AddComponent<LayoutElement>();
            tdLe.ignoreLayout = true;
            topDivider.AddComponent<Image>().color = ColOutline;

            return topBar;
        }

        // ==================================================================
        // BOTTOM SYSTEM STATUS BAR
        // ==================================================================
        private static GameObject BuildBottomStatusBar(Transform parent, TMP_FontAsset fontMono)
        {
            var bar = CreateUIObject("SystemStatusBar", parent, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 30));
            var rt = bar.GetComponent<RectTransform>();
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = Vector2.zero;
            bar.AddComponent<Image>().color = ColBottomBar;

            var left = CreateText(bar.transform, "Сапёр Desktop  •  Режим тактического сканирования", fontMono, 11, FontStyles.Bold, ColOnSurfaceVariant, tracking: 3);
            left.textWrappingMode = TextWrappingModes.NoWrap;
            var leftRt = left.GetComponent<RectTransform>();
            leftRt.anchorMin = new Vector2(0f, 0.5f);
            leftRt.anchorMax = new Vector2(0f, 0.5f);
            leftRt.pivot = new Vector2(0f, 0.5f);
            leftRt.sizeDelta = new Vector2(460 * UI_SCALE, 20 * UI_SCALE);
            leftRt.anchoredPosition = new Vector2(28f * UI_SCALE, 0f * UI_SCALE);

            var right = CreateText(bar.transform, "Движок: CyberSweeper 2.4    FPS: 60", fontMono, 11, FontStyles.Bold, ColOnSurfaceVariant, tracking: 3);
            right.textWrappingMode = TextWrappingModes.NoWrap;
            right.alignment = TextAlignmentOptions.Right;
            var rightRt = right.GetComponent<RectTransform>();
            rightRt.anchorMin = new Vector2(1f, 0.5f);
            rightRt.anchorMax = new Vector2(1f, 0.5f);
            rightRt.pivot = new Vector2(1f, 0.5f);
            rightRt.sizeDelta = new Vector2(320 * UI_SCALE, 20 * UI_SCALE);
            rightRt.anchoredPosition = new Vector2(-28f * UI_SCALE, 0f * UI_SCALE);
            var rightLe = right.gameObject.AddComponent<LayoutElement>();
            rightLe.ignoreLayout = true;

            var statusBar = bar.AddComponent<SystemStatusBar>();
            SetPrivateField(statusBar, "_fpsText", right);
            return bar;
        }

        // ==================================================================
        // TACTICAL DECK
        // ==================================================================
        private static GameObject BuildTacticalDeck(Transform parent, TMP_FontAsset fontMono, TMP_FontAsset fontSans,
            Sprite sPanel, Sprite sBtnCyber, Sprite sBtnAccent, Sprite sBtnDanger,
            Sprite sIconRestart, Sprite sIconPause, Sprite sIconPlay, Sprite sIconHint,
            Sprite sIconFlag, Sprite sIconTimer, Sprite sIconGrid,
            Sprite sRing, Sprite sDot, Sprite sSlotCounter, Sprite sSlotInset,
            Sprite sFaceNormal, Sprite sFaceAlert, Sprite sFaceWin, Sprite sFaceDefeat,
            Sprite sCellClosed, Sprite sCellHover, Sprite sCellPressed, Sprite sCellRevealed, Sprite sCellExploded,
            Sprite sIconFlag2, Sprite sIconMine, Sprite sIconFalseFlag, TMP_FontAsset cellFont,
            out HeaderStatusView headerStatusView)
        {
            var deck = CreateUIObject("TacticalDeck", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(DeckWidth, DeckHeight));
            deck.GetComponent<RectTransform>().anchoredPosition = new Vector2(0 * UI_SCALE, 10 * UI_SCALE);
            var deckImg = deck.AddComponent<Image>();
            deckImg.sprite = sPanel;
            deckImg.type = Image.Type.Sliced;
            deckImg.color = Color.white;

            AddCornerBracket(deck.transform, new Vector2(0, 1), new Vector2(10, -10), ColPrimaryContainer);
            AddCornerBracket(deck.transform, new Vector2(1, 1), new Vector2(-10, -10), ColPrimaryContainer);
            AddCornerBracket(deck.transform, new Vector2(0, 0), new Vector2(10, 10), ColPrimaryContainer);
            AddCornerBracket(deck.transform, new Vector2(1, 0), new Vector2(-10, 10), ColPrimaryContainer);

            var screenShake = deck.AddComponent<ScreenShake>();
            SetPrivateField(screenShake, "_targetTransform", deck.GetComponent<RectTransform>());

            var vlg = deck.AddComponent<VerticalLayoutGroup>();
            vlg.padding = Pad(16, 16, 12, 12);
            vlg.spacing = 8 * UI_SCALE;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // ---------- 1. HEADER STATUS DECK (counters + face) ----------
            var headerDeck = CreateUIObject("HeaderStatusDeck", deck.transform, Vector2.zero, Vector2.zero, new Vector2(0, 78));
            headerDeck.AddComponent<Image>().color = ColSurfaceLow;
            var hsdLe = headerDeck.AddComponent<LayoutElement>();
            hsdLe.preferredHeight = 78 * UI_SCALE;
            hsdLe.minHeight = 78 * UI_SCALE;

            var hlgHsd = headerDeck.AddComponent<HorizontalLayoutGroup>();
            hlgHsd.padding = Pad(16, 16, 0, 0);
            hlgHsd.spacing = 12 * UI_SCALE;
            hlgHsd.childAlignment = TextAnchor.MiddleCenter;
            hlgHsd.childControlWidth = false;
            hlgHsd.childControlHeight = false;

            // Mine counter LED bay (left)
            var mineBay = CreateUIObject("MineBay", headerDeck.transform, Vector2.zero, Vector2.zero, new Vector2(148, 48));
            var mineBayImg = mineBay.AddComponent<Image>();
            if (sSlotCounter != null) { mineBayImg.sprite = sSlotCounter; mineBayImg.type = Image.Type.Sliced; }
            else mineBayImg.color = ColSurfaceLowest;
            var hlgMB = mineBay.AddComponent<HorizontalLayoutGroup>();
            hlgMB.padding = Pad(14, 12, 0, 0);
            hlgMB.spacing = 10 * UI_SCALE;
            hlgMB.childAlignment = TextAnchor.MiddleLeft;
            hlgMB.childControlWidth = false;
            hlgMB.childControlHeight = false;

            if (sIconFlag != null)
            {
                var flagIconGo = CreateUIObject("Icon", mineBay.transform, Vector2.zero, Vector2.zero, new Vector2(20, 20));
                var flagIconImg = flagIconGo.AddComponent<Image>();
                flagIconImg.sprite = sIconFlag;
                flagIconImg.color = ColError;
                flagIconImg.raycastTarget = false;
            }

            var mineCounterText = CreateText(mineBay.transform, "040", fontMono, 26, FontStyles.Bold, ColError, tracking: 7);
            mineCounterText.textWrappingMode = TextWrappingModes.NoWrap;
            mineCounterText.GetComponent<RectTransform>().sizeDelta = new Vector2(96 * UI_SCALE, 32 * UI_SCALE);

            var spacerLeft = CreateUIObject("SpacerLeft", headerDeck.transform, Vector2.zero, Vector2.zero, Vector2.zero);
            spacerLeft.AddComponent<LayoutElement>().flexibleWidth = 1f;

            // Status face button (center)
            var faceBtnGo = CreateUIObject("StatusFaceButton", headerDeck.transform, Vector2.zero, Vector2.zero, new Vector2(64, 64));
            faceBtnGo.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
            var faceBtn = faceBtnGo.AddComponent<Button>();

            var faceIconGo = CreateUIObject("FaceIcon", faceBtnGo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(62, 62));
            var faceIconImg = faceIconGo.AddComponent<Image>();
            faceIconImg.sprite = sFaceNormal;
            faceIconImg.raycastTarget = false;

            if (sDot != null)
            {
                var ledGo = CreateUIObject("StatusLed", faceBtnGo.transform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(10, 10));
                ledGo.GetComponent<RectTransform>().anchoredPosition = new Vector2(2 * UI_SCALE, 2 * UI_SCALE);
                var ledImg = ledGo.AddComponent<Image>();
                ledImg.sprite = sDot;
                ledImg.color = ColPrimaryContainer;
                ledImg.raycastTarget = false;
            }

            var sfb = faceBtnGo.AddComponent<StatusFaceButton>();
            SetPrivateField(sfb, "_faceImage", faceIconImg);
            SetPrivateField(sfb, "_button", faceBtn);
            sfb.FaceNormal = sFaceNormal;
            sfb.FaceAlert = sFaceAlert;
            sfb.FaceWin = sFaceWin;
            sfb.FaceDefeat = sFaceDefeat;

            var spacerRight = CreateUIObject("SpacerRight", headerDeck.transform, Vector2.zero, Vector2.zero, Vector2.zero);
            spacerRight.AddComponent<LayoutElement>().flexibleWidth = 1f;

            // Timer LED bay (right)
            var timerBay = CreateUIObject("TimerBay", headerDeck.transform, Vector2.zero, Vector2.zero, new Vector2(148, 48));
            var timerBayImg = timerBay.AddComponent<Image>();
            if (sSlotCounter != null) { timerBayImg.sprite = sSlotCounter; timerBayImg.type = Image.Type.Sliced; }
            else timerBayImg.color = ColSurfaceLowest;
            var hlgTB = timerBay.AddComponent<HorizontalLayoutGroup>();
            hlgTB.padding = Pad(12, 14, 0, 0);
            hlgTB.spacing = 10 * UI_SCALE;
            hlgTB.childAlignment = TextAnchor.MiddleRight;
            hlgTB.childControlWidth = false;
            hlgTB.childControlHeight = false;

            var timerCounterText = CreateText(timerBay.transform, "045", fontMono, 26, FontStyles.Bold, ColPrimaryContainer, tracking: 7);
            timerCounterText.textWrappingMode = TextWrappingModes.NoWrap;
            timerCounterText.GetComponent<RectTransform>().sizeDelta = new Vector2(96 * UI_SCALE, 32 * UI_SCALE);

            if (sIconTimer != null)
            {
                var timerIconGo = CreateUIObject("Icon", timerBay.transform, Vector2.zero, Vector2.zero, new Vector2(18, 18));
                var timerIconImg = timerIconGo.AddComponent<Image>();
                timerIconImg.sprite = sIconTimer;
                timerIconImg.color = ColPrimaryContainer;
                timerIconImg.raycastTarget = false;
            }

            // ---------- 2. ACTION ROW ----------
            var actionRow = CreateUIObject("ActionRow", deck.transform, Vector2.zero, Vector2.zero, new Vector2(0, 38));
            var arLe = actionRow.AddComponent<LayoutElement>();
            arLe.preferredHeight = 38 * UI_SCALE;
            arLe.minHeight = 38 * UI_SCALE;
            var hlgAction = actionRow.AddComponent<HorizontalLayoutGroup>();
            hlgAction.spacing = 10 * UI_SCALE;
            hlgAction.childAlignment = TextAnchor.MiddleLeft;
            hlgAction.childControlWidth = false;
            hlgAction.childControlHeight = false;

            var btnRestart = CreateActionButtonWithIcon("BtnRestart", actionRow.transform, "Заново", sIconRestart, 58f, fontMono, sBtnCyber).GetComponent<Button>();
            var btnPause = CreateActionButtonWithIcon("BtnPause", actionRow.transform, "Пауза", sIconPause, 52f, fontMono, sBtnCyber, out var pauseText, out var pauseIcon).GetComponent<Button>();

            var actionSpacer = CreateUIObject("Spacer", actionRow.transform, Vector2.zero, Vector2.zero, Vector2.zero);
            actionSpacer.AddComponent<LayoutElement>().flexibleWidth = 1f;

            var mouseHint = CreateText(actionRow.transform, "ЛКМ — Открыть  •  ПКМ — Флаг", fontMono, 11, FontStyles.Normal, ColOnSurfaceVariant, tracking: 3);
            mouseHint.textWrappingMode = TextWrappingModes.NoWrap;
            mouseHint.GetComponent<RectTransform>().sizeDelta = new Vector2(260 * UI_SCALE, 22 * UI_SCALE);

            // ---------- 3. MINEFIELD GRID ----------
            var gridWrapper = CreateUIObject("GridWrapper", deck.transform, Vector2.zero, Vector2.zero, new Vector2(0, 0));
            var gridWrapperImg = gridWrapper.AddComponent<Image>();
            if (sSlotInset != null) { gridWrapperImg.sprite = sSlotInset; gridWrapperImg.type = Image.Type.Sliced; }
            else gridWrapperImg.color = ColSurfaceLowest;
            var gwLe = gridWrapper.AddComponent<LayoutElement>();
            gwLe.preferredHeight = GridAvailableHeight * UI_SCALE;
            gwLe.flexibleHeight = 1;

            var gridContainerGo = CreateUIObject("GridContainer", gridWrapper.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(440, 440));
            var gridLayout = gridContainerGo.AddComponent<GridLayoutGroup>();
            gridLayout.childAlignment = TextAnchor.MiddleCenter;

            var boardView = gridContainerGo.AddComponent<BoardGridView>();
            SetPrivateField(boardView, "_gridContainer", gridContainerGo.GetComponent<RectTransform>());
            SetPrivateField(boardView, "_gridLayout", gridLayout);
            SetPrivateField(boardView, "_consoleFrame", deck.GetComponent<RectTransform>());
            boardView.ConfigureResources(sCellClosed, sCellHover, sCellPressed, sCellRevealed, sCellExploded,
                sIconFlag2, sIconMine, sIconFalseFlag, cellFont);
            SetPrivateField(boardView, "_availableWidth", GridAvailableWidth * UI_SCALE);
            SetPrivateField(boardView, "_availableHeight", GridAvailableHeight * UI_SCALE);

            // ---------- 4. FOOTER STATUS DECK ----------
            var footerDeck = CreateUIObject("FooterDeck", deck.transform, Vector2.zero, Vector2.zero, new Vector2(0, 32));
            footerDeck.AddComponent<Image>().color = ColSurfaceLow;
            var fdLe = footerDeck.AddComponent<LayoutElement>();
            fdLe.preferredHeight = 32 * UI_SCALE;
            fdLe.minHeight = 32 * UI_SCALE;

            var hlgFd = footerDeck.AddComponent<HorizontalLayoutGroup>();
            hlgFd.padding = Pad(16, 16, 0, 0);
            hlgFd.spacing = 10 * UI_SCALE;
            hlgFd.childAlignment = TextAnchor.MiddleLeft;
            hlgFd.childControlWidth = false;
            hlgFd.childControlHeight = false;

            if (sIconFlag != null)
            {
                var fIconGo = CreateUIObject("Icon", footerDeck.transform, Vector2.zero, Vector2.zero, new Vector2(14, 14));
                var fIconImg = fIconGo.AddComponent<Image>();
                fIconImg.sprite = sIconFlag;
                fIconImg.color = ColError;
                fIconImg.raycastTarget = false;
            }

            var flagsCountText = CreateText(footerDeck.transform, "Флаги: 10 / 40", fontMono, 10, FontStyles.Normal, ColOnSurface, tracking: 2);
            flagsCountText.textWrappingMode = TextWrappingModes.NoWrap;
            flagsCountText.GetComponent<RectTransform>().sizeDelta = new Vector2(104 * UI_SCALE, 20 * UI_SCALE);

            var fs1 = CreateUIObject("Spacer1", footerDeck.transform, Vector2.zero, Vector2.zero, Vector2.zero);
            fs1.AddComponent<LayoutElement>().flexibleWidth = 1f;

            if (sIconGrid != null)
            {
                var gIconGo = CreateUIObject("Icon", footerDeck.transform, Vector2.zero, Vector2.zero, new Vector2(14, 14));
                var gIconImg = gIconGo.AddComponent<Image>();
                gIconImg.sprite = sIconGrid;
                gIconImg.color = ColOnSurfaceVariant;
                gIconImg.raycastTarget = false;
            }

            var bottomDiffText = CreateText(footerDeck.transform, "Сложность: 16×16 • 40 мин", fontMono, 10, FontStyles.Normal, ColOnSurfaceVariant, tracking: 2);
            bottomDiffText.textWrappingMode = TextWrappingModes.NoWrap;
            bottomDiffText.GetComponent<RectTransform>().sizeDelta = new Vector2(178 * UI_SCALE, 20 * UI_SCALE);

            var fs2 = CreateUIObject("Spacer2", footerDeck.transform, Vector2.zero, Vector2.zero, Vector2.zero);
            fs2.AddComponent<LayoutElement>().flexibleWidth = 1f;

            var statusDot = CreatePulseDot(footerDeck.transform, ColPrimaryContainer, 8f, sDot).GetComponent<Image>();
            var statusLabel = CreateText(footerDeck.transform, "В процессе", fontMono, 10, FontStyles.Bold, ColOnSurface, tracking: 2);
            statusLabel.textWrappingMode = TextWrappingModes.NoWrap;
            statusLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(86 * UI_SCALE, 20 * UI_SCALE);

            var fs3 = CreateUIObject("Spacer3", footerDeck.transform, Vector2.zero, Vector2.zero, Vector2.zero);
            fs3.AddComponent<LayoutElement>().flexibleWidth = 1f;

            var shortcutText = CreateText(footerDeck.transform, "[Пробел / F2 — Рестарт]", fontMono, 10, FontStyles.Normal, ColOnSurfaceVariant, tracking: 2);
            shortcutText.textWrappingMode = TextWrappingModes.NoWrap;
            shortcutText.GetComponent<RectTransform>().sizeDelta = new Vector2(160 * UI_SCALE, 20 * UI_SCALE);

            // ---------- Wire HeaderStatusView ----------
            headerStatusView = headerDeck.AddComponent<HeaderStatusView>();
            SetPrivateField(headerStatusView, "_mineCounterText", mineCounterText);
            SetPrivateField(headerStatusView, "_timerText", timerCounterText);
            SetPrivateField(headerStatusView, "_btnRestart", btnRestart);
            SetPrivateField(headerStatusView, "_btnPause", btnPause);
            SetPrivateField(headerStatusView, "_pauseText", pauseText);
            SetPrivateField(headerStatusView, "_pauseIcon", pauseIcon);
            SetPrivateField(headerStatusView, "_flagsCountText", flagsCountText);
            SetPrivateField(headerStatusView, "_bottomDifficultyText", bottomDiffText);
            SetPrivateField(headerStatusView, "_statusDot", statusDot);
            SetPrivateField(headerStatusView, "_statusLabel", statusLabel);
            headerStatusView.PauseSprite = sIconPause;
            headerStatusView.PlaySprite = sIconPlay;

            return deck;
        }

        // ==================================================================
        // MODALS
        // ==================================================================
        private static DefeatModal BuildDefeatModal(Transform parent, TMP_FontAsset font, Sprite sPanel, Sprite sBtnDanger, Sprite sBtnCyber, Sprite sIconAlert, Sprite sSlotMini)
        {
            var modalRoot = CreateUIObject("DefeatModalRoot", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var backdrop = CreateModalBackdrop("Backdrop", modalRoot.transform, new Color(0.02f, 0.03f, 0.05f, 0.86f));
            var panel = CreateUIObject("Panel", backdrop.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(560, 470));
            var pImg = panel.AddComponent<Image>();
            pImg.sprite = sPanel;
            pImg.type = Image.Type.Sliced;

            var vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = Pad(28, 28, 22, 22);
            vlg.spacing = 10 * UI_SCALE;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Emblem (centred row so the VLG does not stretch it)
            var emblemRow = CreateUIObject("EmblemRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 62));
            var erLe = emblemRow.AddComponent<LayoutElement>();
            erLe.preferredHeight = 62 * UI_SCALE;
            var hlgEmblem = emblemRow.AddComponent<HorizontalLayoutGroup>();
            hlgEmblem.childAlignment = TextAnchor.MiddleCenter;
            hlgEmblem.childControlWidth = false;
            hlgEmblem.childControlHeight = false;

            var emblemGo = CreateUIObject("Emblem", emblemRow.transform, Vector2.zero, Vector2.zero, new Vector2(58, 58));
            emblemGo.AddComponent<Image>().color = new Color(0.08f, 0.05f, 0.08f, 0.9f);
            if (sIconAlert != null)
            {
                var iconGo = CreateUIObject("Icon", emblemGo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(34, 34));
                var iconImg = iconGo.AddComponent<Image>();
                iconImg.sprite = sIconAlert;
                iconImg.color = ColError;
                iconImg.raycastTarget = false;
            }

            // Title row: "Детонация" + badge
            var titleRow = CreateUIObject("TitleRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 38));
            var trLe = titleRow.AddComponent<LayoutElement>();
            trLe.preferredHeight = 38 * UI_SCALE;
            var hlgTitle = titleRow.AddComponent<HorizontalLayoutGroup>();
            hlgTitle.spacing = 12 * UI_SCALE;
            hlgTitle.childAlignment = TextAnchor.MiddleCenter;
            hlgTitle.childControlWidth = false;
            hlgTitle.childControlHeight = false;

            var title = CreateText(titleRow.transform, "Детонация", font, 30, FontStyles.Bold, ColPrimary, tracking: 2);
            title.textWrappingMode = TextWrappingModes.NoWrap;
            title.GetComponent<RectTransform>().sizeDelta = new Vector2(168 * UI_SCALE, 38 * UI_SCALE);

            var badgeGo = CreateUIObject("Badge", titleRow.transform, Vector2.zero, Vector2.zero, new Vector2(104, 26));
            badgeGo.AddComponent<Image>().color = new Color(0.35f, 0.05f, 0.12f, 0.9f);
            var badge = CreateText(badgeGo.transform, "ПОРАЖЕНИЕ", font, 10, FontStyles.Bold, ColError, tracking: 5);
            badge.alignment = TextAlignmentOptions.Center;
            var bRt = badge.GetComponent<RectTransform>();
            bRt.anchorMin = Vector2.zero; bRt.anchorMax = Vector2.one; bRt.offsetMin = Vector2.zero; bRt.offsetMax = Vector2.zero;

            var subtitle = CreateText(panel.transform, "Сектор подорван на мине", font, 13, FontStyles.Normal, ColOnSurfaceVariant, tracking: 1);
            subtitle.alignment = TextAlignmentOptions.Center;
            var leSub = subtitle.gameObject.AddComponent<LayoutElement>();
            leSub.preferredHeight = 22 * UI_SCALE;

            // Stats
            var statsRow = CreateUIObject("StatsRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 74));
            var leStats = statsRow.AddComponent<LayoutElement>();
            leStats.preferredHeight = 74 * UI_SCALE;
            var hlgStats = statsRow.AddComponent<HorizontalLayoutGroup>();
            hlgStats.spacing = 12 * UI_SCALE;
            hlgStats.childAlignment = TextAnchor.MiddleCenter;
            hlgStats.childControlWidth = true;
            hlgStats.childControlHeight = true;
            hlgStats.childForceExpandWidth = true;

            CreateStatTile(statsRow.transform, "ВРЕМЯ", "02:08", font, ColPrimaryContainer, out var tTime);
            CreateStatTile(statsRow.transform, "ОТКРЫТО", "68%", font, ColPrimaryContainer, out var tOpened);
            CreateStatTile(statsRow.transform, "ОСТАЛОСЬ МИН", "12", font, ColError, out var tMines);

            // Buttons
            var btnRow = CreateUIObject("ButtonRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 44));
            var leBtns = btnRow.AddComponent<LayoutElement>();
            leBtns.preferredHeight = 44 * UI_SCALE;
            var hlg = btnRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 14 * UI_SCALE;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            var btnRetry = CreateActionButton(btnRow.transform, "Попробовать снова", font, sBtnDanger, 220, 44);
            var btnInspect = CreateActionButton(btnRow.transform, "Обзор поля", font, sBtnCyber, 200, 44);

            // Back-to-menu link
            var linkRow = CreateUIObject("LinkRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 24));
            var leLink = linkRow.AddComponent<LayoutElement>();
            leLink.preferredHeight = 24 * UI_SCALE;
            var btnMenu = CreateActionButton(linkRow.transform, "← В главное меню", font, null, 220, 24);
            // The link sits directly under the panel layout group, so its rect has to be
            // pinned to the left edge instead of being centred on the anchor point.
            var menuRt = btnMenu.GetComponent<RectTransform>();
            menuRt.pivot = new Vector2(0f, 0.5f);
            menuRt.anchoredPosition = Vector2.zero;
            var linkText = btnMenu.GetComponentInChildren<TextMeshProUGUI>();
            if (linkText != null)
            {
                linkText.color = ColOnSurfaceVariant;
                linkText.fontStyle = FontStyles.Normal;
            }

            var dm = modalRoot.AddComponent<DefeatModal>();
            SetPrivateField(dm, "_panel", backdrop);
            SetPrivateField(dm, "_sectorSubtitleText", subtitle);
            SetPrivateField(dm, "_timeStatText", tTime);
            SetPrivateField(dm, "_openedPercentText", tOpened);
            SetPrivateField(dm, "_remainingMinesText", tMines);
            SetPrivateField(dm, "_retryButton", btnRetry);
            SetPrivateField(dm, "_inspectButton", btnInspect);
            SetPrivateField(dm, "_menuButton", btnMenu);
            backdrop.SetActive(false);
            return dm;
        }

        private static VictoryModal BuildVictoryModal(Transform parent, TMP_FontAsset font, Sprite sPanel, Sprite sBtnCyber, Sprite sBtnAccent, Sprite sIconTarget, Sprite sTabActive, Sprite sDotSprite)
        {
            var modalRoot = CreateUIObject("VictoryModalRoot", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var backdrop = CreateModalBackdrop("Backdrop", modalRoot.transform, new Color(0.02f, 0.03f, 0.05f, 0.86f));
            var panel = CreateUIObject("Panel", backdrop.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(560, 560));
            var pImg = panel.AddComponent<Image>();
            pImg.sprite = sPanel;
            pImg.type = Image.Type.Sliced;

            var vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = Pad(28, 28, 24, 24);
            vlg.spacing = 10 * UI_SCALE;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Mission badge pill (centred row so the VLG does not stretch it)
            var badgeRow = CreateUIObject("BadgeRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 36));
            var badgeLe = badgeRow.AddComponent<LayoutElement>();
            badgeLe.preferredHeight = 36 * UI_SCALE;
            var hlgBadgeRow = badgeRow.AddComponent<HorizontalLayoutGroup>();
            hlgBadgeRow.childAlignment = TextAnchor.MiddleCenter;
            hlgBadgeRow.childControlWidth = false;
            hlgBadgeRow.childControlHeight = false;

            var badgeGo = CreateUIObject("Badge", badgeRow.transform, Vector2.zero, Vector2.zero, new Vector2(236, 30));
            if (sTabActive != null)
            {
                var badgeImg = badgeGo.AddComponent<Image>();
                badgeImg.sprite = sTabActive;
                badgeImg.type = Image.Type.Sliced;
            }
            else
            {
                badgeGo.AddComponent<Image>().color = new Color(0f, 0.95f, 1f, 0.14f);
            }
            var hlgBadge = badgeGo.AddComponent<HorizontalLayoutGroup>();
            hlgBadge.spacing = 10 * UI_SCALE;
            hlgBadge.childAlignment = TextAnchor.MiddleCenter;
            hlgBadge.childControlWidth = false;
            hlgBadge.childControlHeight = false;
            CreatePulseDot(badgeGo.transform, ColPrimaryContainer, 8f, sDotSprite);
            var badge = CreateText(badgeGo.transform, "МИССИЯ ВЫПОЛНЕНА", font, 11, FontStyles.Bold, ColPrimaryContainer, tracking: 6);
            badge.textWrappingMode = TextWrappingModes.NoWrap;
            badge.alignment = TextAlignmentOptions.Center;
            badge.GetComponent<RectTransform>().sizeDelta = new Vector2(176 * UI_SCALE, 20 * UI_SCALE);

            var title = CreateText(panel.transform, "Победа!", font, 34, FontStyles.Bold, ColPrimary, tracking: 1);
            title.alignment = TextAlignmentOptions.Center;
            var leTitle = title.gameObject.AddComponent<LayoutElement>();
            leTitle.preferredHeight = 44 * UI_SCALE;

            var subtitle = CreateText(panel.transform, "Все мины успешно обезврежены", font, 13, FontStyles.Normal, ColOnSurfaceVariant, tracking: 1);
            subtitle.alignment = TextAlignmentOptions.Center;
            var leSub = subtitle.gameObject.AddComponent<LayoutElement>();
            leSub.preferredHeight = 22 * UI_SCALE;

            // Stats
            var statsRow = CreateUIObject("StatsRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 74));
            var leStats = statsRow.AddComponent<LayoutElement>();
            leStats.preferredHeight = 74 * UI_SCALE;
            var hlgStats = statsRow.AddComponent<HorizontalLayoutGroup>();
            hlgStats.spacing = 12 * UI_SCALE;
            hlgStats.childAlignment = TextAnchor.MiddleCenter;
            hlgStats.childControlWidth = true;
            hlgStats.childControlHeight = true;
            hlgStats.childForceExpandWidth = true;

            CreateStatTile(statsRow.transform, "ВРЕМЯ", "42 сек", font, ColPrimaryContainer, out var tTime);
            CreateStatTile(statsRow.transform, "СЛОЖНОСТЬ", "16×16 • 40м", font, ColPrimaryContainer, out var tDiff);
            CreateStatTile(statsRow.transform, "РЕКОРД", "Новый!", font, ColSuccess, out var tRecord);

            // Callsign
            var nameLabel = CreateText(panel.transform, "Имя игрока", font, 11, FontStyles.Bold, ColOnSurfaceVariant, tracking: 4);
            nameLabel.alignment = TextAlignmentOptions.MidlineLeft;
            nameLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(200 * UI_SCALE, 20 * UI_SCALE);
            var leName = nameLabel.gameObject.AddComponent<LayoutElement>();
            leName.preferredHeight = 20 * UI_SCALE;

            var inputRow = CreateUIObject("InputRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 44));
            var leInput = inputRow.AddComponent<LayoutElement>();
            leInput.preferredHeight = 44 * UI_SCALE;
            var inputGo = CreateUIObject("CallsignInput", inputRow.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            inputGo.AddComponent<Image>().color = ColSurfaceLow;
            var tmpInput = inputGo.AddComponent<TMP_InputField>();

            var textCompGo = CreateUIObject("Text", inputGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var textComp = textCompGo.AddComponent<TextMeshProUGUI>();
            if (font != null) textComp.font = font;
            textComp.fontSize = 14 * UI_SCALE;
            textComp.color = ColPrimaryContainer;
            textComp.alignment = TextAlignmentOptions.MidlineLeft;
            tmpInput.textComponent = textComp;
            tmpInput.text = "Игрок 1";

            // Primary CTA
            var btnSave = CreateActionButton(panel.transform, "Сохранить результат", font, sBtnAccent, 0, 46);
            var saveBtnTxt = btnSave.GetComponentInChildren<TextMeshProUGUI>();
            if (saveBtnTxt != null)
            {
                saveBtnTxt.color = ColOnPrimaryContainer;
                saveBtnTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(0 * UI_SCALE, 46 * UI_SCALE);
            }

            // Secondary actions
            var btnRow = CreateUIObject("ActionsRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 44));
            var leBtns = btnRow.AddComponent<LayoutElement>();
            leBtns.preferredHeight = 44 * UI_SCALE;
            var hlgBtn = btnRow.AddComponent<HorizontalLayoutGroup>();
            hlgBtn.spacing = 14 * UI_SCALE;
            hlgBtn.childAlignment = TextAnchor.MiddleCenter;
            hlgBtn.childControlWidth = true;
            hlgBtn.childControlHeight = true;

            var btnNew = CreateActionButton(btnRow.transform, "Новая игра", font, sBtnCyber, 0, 44);
            var btnMenu = CreateActionButton(btnRow.transform, "В меню", font, sBtnCyber, 0, 44);

            var vm = modalRoot.AddComponent<VictoryModal>();
            SetPrivateField(vm, "_panel", backdrop);
            SetPrivateField(vm, "_timeText", tTime);
            SetPrivateField(vm, "_difficultyText", tDiff);
            SetPrivateField(vm, "_recordStatusText", tRecord);
            SetPrivateField(vm, "_callsignInput", tmpInput);
            SetPrivateField(vm, "_saveRecordButton", btnSave);
            SetPrivateField(vm, "_saveButtonText", saveBtnTxt);
            SetPrivateField(vm, "_newGameButton", btnNew);
            SetPrivateField(vm, "_menuButton", btnMenu);
            backdrop.SetActive(false);
            return vm;
        }

        private static SettingsModal BuildSettingsModal(Transform parent, TMP_FontAsset font, TMP_FontAsset fontSans,
            Sprite sPanel, Sprite sBtnCyber, Sprite sBtnAccent, Sprite sIconSliders, Sprite sIconClose, Sprite sCardActive)
        {
            var modalRoot = CreateUIObject("SettingsModalRoot", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var backdrop = CreateModalBackdrop("Backdrop", modalRoot.transform, new Color(0.02f, 0.03f, 0.05f, 0.86f));
            var panel = CreateUIObject("Panel", backdrop.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(760, 620));
            var pImg = panel.AddComponent<Image>();
            pImg.sprite = sPanel;
            pImg.type = Image.Type.Sliced;

            var vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = Pad(26, 26, 20, 20);
            vlg.spacing = 10 * UI_SCALE;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // ---------- Header ----------
            var headerRow = CreateUIObject("ModalHeader", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 54));
            var leHeader = headerRow.AddComponent<LayoutElement>();
            leHeader.preferredHeight = 54 * UI_SCALE;
            var hlgHeader = headerRow.AddComponent<HorizontalLayoutGroup>();
            hlgHeader.spacing = 14 * UI_SCALE;
            hlgHeader.childAlignment = TextAnchor.MiddleLeft;
            hlgHeader.childControlWidth = false;
            hlgHeader.childControlHeight = false;

            var iconBox = CreateUIObject("IconBox", headerRow.transform, Vector2.zero, Vector2.zero, new Vector2(42, 42));
            iconBox.AddComponent<Image>().color = new Color(0f, 0.95f, 1f, 0.14f);
            if (sIconSliders != null)
            {
                var slGo = CreateUIObject("Icon", iconBox.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(22, 22));
                var slImg = slGo.AddComponent<Image>();
                slImg.sprite = sIconSliders;
                slImg.color = ColPrimaryContainer;
                slImg.raycastTarget = false;
            }

            var headerTextCol = CreateUIObject("Titles", headerRow.transform, Vector2.zero, Vector2.zero, new Vector2(430, 46));
            var vlgTitles = headerTextCol.AddComponent<VerticalLayoutGroup>();
            vlgTitles.spacing = 2 * UI_SCALE;
            vlgTitles.childAlignment = TextAnchor.MiddleLeft;
            vlgTitles.childControlWidth = false;
            vlgTitles.childControlHeight = false;

            var title = CreateText(headerTextCol.transform, "Параметры игры", font, 18, FontStyles.Bold, ColPrimary, tracking: 2);
            title.textWrappingMode = TextWrappingModes.NoWrap;
            title.GetComponent<RectTransform>().sizeDelta = new Vector2(430 * UI_SCALE, 24 * UI_SCALE);

            var subtitle = CreateText(headerTextCol.transform, "Конфигурация игрового поля и зарядов", font, 11, FontStyles.Normal, ColOnSurfaceVariant, tracking: 2);
            subtitle.textWrappingMode = TextWrappingModes.NoWrap;
            subtitle.GetComponent<RectTransform>().sizeDelta = new Vector2(430 * UI_SCALE, 20 * UI_SCALE);

            var headerSpacer = CreateUIObject("Spacer", headerRow.transform, Vector2.zero, Vector2.zero, Vector2.zero);
            headerSpacer.AddComponent<LayoutElement>().flexibleWidth = 1f;

            var btnClose = CreateIconButton("BtnClose", headerRow.transform, sIconClose, out _).GetComponent<Button>();

            // Divider
            var divider = CreateUIObject("Divider", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 1));
            var leDiv = divider.AddComponent<LayoutElement>();
            leDiv.preferredHeight = 1 * UI_SCALE;
            divider.AddComponent<Image>().color = ColOutline;

            // ---------- Section: difficulty ----------
            var secLabel = CreateSectionLabel(panel.transform, "ВЫБЕРИТЕ СЛОЖНОСТЬ", font);
            var presetsRow = CreateUIObject("PresetsRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 74));
            var lePresets = presetsRow.AddComponent<LayoutElement>();
            lePresets.preferredHeight = 74 * UI_SCALE;
            var hlgPresets = presetsRow.AddComponent<HorizontalLayoutGroup>();
            hlgPresets.spacing = 10 * UI_SCALE;
            hlgPresets.childAlignment = TextAnchor.MiddleLeft;
            hlgPresets.childControlWidth = true;
            hlgPresets.childControlHeight = true;
            hlgPresets.childForceExpandWidth = true;

            var btnBeg = CreatePresetCard(presetsRow.transform, "LVL 1", "Новичок", "9 × 9 • 10 мин", font, sBtnCyber, out var indBeg);
            var btnInt = CreatePresetCard(presetsRow.transform, "LVL 2", "Любитель", "16 × 16 • 40 мин", font, sBtnCyber, out var indInt);
            var btnExp = CreatePresetCard(presetsRow.transform, "LVL 3", "Профессионал", "30 × 16 • 99 мин", font, sBtnCyber, out var indExp);
            var btnCust = CreatePresetCard(presetsRow.transform, "ОСОБЫЙ", "Особый", "Настраиваемый", font, sBtnCyber, out var indCust);

            // ---------- Section: custom grid ----------
            var secRow = CreateUIObject("CustomSectionHeader", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 22));
            var leSec = secRow.AddComponent<LayoutElement>();
            leSec.preferredHeight = 22 * UI_SCALE;
            var hlgSec = secRow.AddComponent<HorizontalLayoutGroup>();
            hlgSec.childAlignment = TextAnchor.MiddleLeft;
            hlgSec.childControlWidth = false;
            hlgSec.childControlHeight = false;

            var secLeft = CreateText(secRow.transform, "ТОЧНАЯ НАСТРОЙКА СЕТКИ", font, 11, FontStyles.Bold, ColOnSurfaceVariant, tracking: 5);
            secLeft.textWrappingMode = TextWrappingModes.NoWrap;
            secLeft.GetComponent<RectTransform>().sizeDelta = new Vector2(300 * UI_SCALE, 20 * UI_SCALE);

            var secSpacer = CreateUIObject("Spacer", secRow.transform, Vector2.zero, Vector2.zero, Vector2.zero);
            secSpacer.AddComponent<LayoutElement>().flexibleWidth = 1f;

            var secRight = CreateText(secRow.transform, "Ручной ввод параметров", font, 11, FontStyles.Normal, ColOnSurfaceVariant, tracking: 2);
            secRight.textWrappingMode = TextWrappingModes.NoWrap;
            secRight.GetComponent<RectTransform>().sizeDelta = new Vector2(200 * UI_SCALE, 20 * UI_SCALE);

            var customCont = CreateUIObject("CustomContainer", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 132));
            var leCust = customCont.AddComponent<LayoutElement>();
            leCust.preferredHeight = 132 * UI_SCALE;
            var hlgCust = customCont.AddComponent<HorizontalLayoutGroup>();
            hlgCust.spacing = 10 * UI_SCALE;
            hlgCust.childAlignment = TextAnchor.MiddleLeft;
            hlgCust.childControlWidth = true;
            hlgCust.childControlHeight = true;
            hlgCust.childForceExpandWidth = true;

            CreateStepperWithSlider(customCont.transform, "Высота", "16", 9, 24, font, out var txtH, out var btnHMinus, out var btnHPlus, out var sldH);
            CreateStepperWithSlider(customCont.transform, "Ширина", "30", 9, 30, font, out var txtW, out var btnWMinus, out var btnWPlus, out var sldW);
            CreateStepperWithSlider(customCont.transform, "Число мин", "99", 10, 432, font, out var txtM, out var btnMMinus, out var btnMPlus, out var sldM);

            // ---------- Summary card ----------
            var summaryCard = CreateUIObject("SummaryCard", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 48));
            var leSum = summaryCard.AddComponent<LayoutElement>();
            leSum.preferredHeight = 48 * UI_SCALE;
            summaryCard.AddComponent<Image>().color = ColSurfaceLowest;
            var hlgSummary = summaryCard.AddComponent<HorizontalLayoutGroup>();
            hlgSummary.padding = Pad(14, 14, 0, 0);
            hlgSummary.spacing = 10 * UI_SCALE;
            hlgSummary.childAlignment = TextAnchor.MiddleLeft;
            hlgSummary.childControlWidth = false;
            hlgSummary.childControlHeight = true;

            var densityText = CreateText(summaryCard.transform, "Плотность мин: 20.6%", font, 12, FontStyles.Bold, ColPrimary, tracking: 2);
            densityText.textWrappingMode = TextWrappingModes.NoWrap;
            densityText.GetComponent<RectTransform>().sizeDelta = new Vector2(200 * UI_SCALE, 20 * UI_SCALE);

            var densityBadge = CreateText(summaryCard.transform, "[ОПТИМАЛЬНО]", font, 10, FontStyles.Bold, ColPrimaryContainer, tracking: 4);
            densityBadge.textWrappingMode = TextWrappingModes.NoWrap;
            densityBadge.GetComponent<RectTransform>().sizeDelta = new Vector2(130 * UI_SCALE, 20 * UI_SCALE);

            var spacerSC = CreateUIObject("Spacer", summaryCard.transform, Vector2.zero, Vector2.zero, Vector2.zero);
            spacerSC.AddComponent<LayoutElement>().flexibleWidth = 1f;

            var totalCellsText = CreateText(summaryCard.transform, "Всего ячеек: 480 • 1-й клик: 100% безопасен", font, 11, FontStyles.Normal, ColOnSurfaceVariant, tracking: 1);
            totalCellsText.textWrappingMode = TextWrappingModes.NoWrap;
            totalCellsText.GetComponent<RectTransform>().sizeDelta = new Vector2(330 * UI_SCALE, 20 * UI_SCALE);

            // ---------- Audio ----------
            var audioBox = CreateUIObject("AudioSettingsBox", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 96));
            var leAudio = audioBox.AddComponent<LayoutElement>();
            leAudio.preferredHeight = 96 * UI_SCALE;
            audioBox.AddComponent<Image>().color = ColSurfaceLow;
            var vlgAudio = audioBox.AddComponent<VerticalLayoutGroup>();
            vlgAudio.padding = Pad(14, 14, 8, 8);
            vlgAudio.spacing = 8 * UI_SCALE;
            vlgAudio.childControlWidth = true;
            vlgAudio.childControlHeight = true;
            vlgAudio.childForceExpandHeight = false;

            var audioTitle = CreateText(audioBox.transform, "ЗВУКОВОЕ СОПРОВОЖДЕНИЕ", font, 10, FontStyles.Bold, ColOnSurfaceVariant, tracking: 5);
            audioTitle.textWrappingMode = TextWrappingModes.NoWrap;
            audioTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(300 * UI_SCALE, 16 * UI_SCALE);

            var volRow = CreateUIObject("VolRow", audioBox.transform, Vector2.zero, Vector2.zero, new Vector2(0, 24));
            var leVR = volRow.AddComponent<LayoutElement>();
            leVR.preferredHeight = 24 * UI_SCALE;
            var hlgVR = volRow.AddComponent<HorizontalLayoutGroup>();
            hlgVR.spacing = 14 * UI_SCALE;
            hlgVR.childAlignment = TextAnchor.MiddleLeft;
            hlgVR.childControlWidth = false;
            hlgVR.childControlHeight = true;
            var volLbl = CreateText(volRow.transform, "Громкость звука:", font, 11, FontStyles.Bold, ColOnSurface, tracking: 2);
            volLbl.textWrappingMode = TextWrappingModes.NoWrap;
            volLbl.GetComponent<RectTransform>().sizeDelta = new Vector2(160 * UI_SCALE, 20 * UI_SCALE);
            var slider = CreateCyberSlider("VolumeSlider", volRow.transform, new Vector2(300, 18), 0f, 1f, 0.8f);

            var togglesRow = CreateUIObject("TogglesRow", audioBox.transform, Vector2.zero, Vector2.zero, new Vector2(0, 24));
            var leTR = togglesRow.AddComponent<LayoutElement>();
            leTR.preferredHeight = 24 * UI_SCALE;
            var hlgToggles = togglesRow.AddComponent<HorizontalLayoutGroup>();
            hlgToggles.spacing = 26 * UI_SCALE;
            hlgToggles.childAlignment = TextAnchor.MiddleLeft;
            hlgToggles.childControlWidth = false;
            hlgToggles.childControlHeight = true;

            var muteTgl = CreateToggle(togglesRow.transform, "Режим «Без звука»", font);
            var tickTgl = CreateToggle(togglesRow.transform, "Тиканье таймера", font);

            // ---------- Footer ----------
            var btnRow = CreateUIObject("ButtonRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 46));
            var leBtns = btnRow.AddComponent<LayoutElement>();
            leBtns.preferredHeight = 46 * UI_SCALE;
            var hlgBtn = btnRow.AddComponent<HorizontalLayoutGroup>();
            hlgBtn.spacing = 14 * UI_SCALE;
            hlgBtn.childAlignment = TextAnchor.MiddleRight;
            hlgBtn.childControlWidth = false;
            hlgBtn.childControlHeight = false;

            var footerNote = CreateText(btnRow.transform, "Новая игра начнется с выбранными параметрами", font, 11, FontStyles.Normal, ColOnSurfaceVariant, tracking: 2);
            footerNote.textWrappingMode = TextWrappingModes.NoWrap;
            footerNote.GetComponent<RectTransform>().sizeDelta = new Vector2(360 * UI_SCALE, 20 * UI_SCALE);

            var btnSpacer = CreateUIObject("Spacer", btnRow.transform, Vector2.zero, Vector2.zero, Vector2.zero);
            btnSpacer.AddComponent<LayoutElement>().flexibleWidth = 1f;

            var btnCancel = CreateActionButton(btnRow.transform, "Отмена", font, sBtnCyber, 140, 44);
            var btnOk = CreateActionButton(btnRow.transform, "Применить", font, sBtnAccent, 160, 44);
            var okText = btnOk.GetComponentInChildren<TextMeshProUGUI>();
            if (okText != null) okText.color = ColOnPrimaryContainer;

            var sm = modalRoot.AddComponent<SettingsModal>();
            SetPrivateField(sm, "_panel", backdrop);
            SetPrivateField(sm, "_btnBeginner", btnBeg);
            SetPrivateField(sm, "_btnIntermediate", btnInt);
            SetPrivateField(sm, "_btnExpert", btnExp);
            SetPrivateField(sm, "_btnCustom", btnCust);
            SetPrivateField(sm, "_indicatorBeginner", indBeg);
            SetPrivateField(sm, "_indicatorIntermediate", indInt);
            SetPrivateField(sm, "_indicatorExpert", indExp);
            SetPrivateField(sm, "_indicatorCustom", indCust);
            SetPrivateField(sm, "_customContainer", customCont);
            SetPrivateField(sm, "_widthInput", txtW);
            SetPrivateField(sm, "_widthMinus", btnWMinus);
            SetPrivateField(sm, "_widthPlus", btnWPlus);
            SetPrivateField(sm, "_widthSlider", sldW);
            SetPrivateField(sm, "_heightInput", txtH);
            SetPrivateField(sm, "_heightMinus", btnHMinus);
            SetPrivateField(sm, "_heightPlus", btnHPlus);
            SetPrivateField(sm, "_heightSlider", sldH);
            SetPrivateField(sm, "_minesInput", txtM);
            SetPrivateField(sm, "_minesMinus", btnMMinus);
            SetPrivateField(sm, "_minesPlus", btnMPlus);
            SetPrivateField(sm, "_minesSlider", sldM);
            SetPrivateField(sm, "_densityText", densityText);
            SetPrivateField(sm, "_densityStatusBadge", densityBadge);
            SetPrivateField(sm, "_totalCellsText", totalCellsText);
            SetPrivateField(sm, "_volumeSlider", slider);
            SetPrivateField(sm, "_muteToggle", muteTgl);
            SetPrivateField(sm, "_timerTickToggle", tickTgl);
            SetPrivateField(sm, "_okButton", btnOk);
            SetPrivateField(sm, "_cancelButton", btnCancel);
            SetPrivateField(sm, "_closeButton", btnClose);
            SetPrivateField(sm, "_headerCloseButton", btnClose);
            sm.ActiveCardSprite = sCardActive;
            sm.NormalCardSprite = sBtnCyber;
            backdrop.SetActive(false);
            return sm;
        }

        private static LeaderboardsModal BuildLeaderboardsModal(Transform parent, TMP_FontAsset font, Sprite sPanel, Sprite sBtnCyber, Sprite sBtnAccent, Sprite sIconBarChart, Sprite sIconClose, Sprite sReplaySprite, Sprite sDotSprite, Sprite sTabActive, Sprite sTabInactive)
        {
            var modalRoot = CreateUIObject("LeaderboardsModalRoot", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var backdrop = CreateModalBackdrop("Backdrop", modalRoot.transform, new Color(0.02f, 0.03f, 0.05f, 0.86f));
            var panel = CreateUIObject("Panel", backdrop.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(900, 620));
            var pImg = panel.AddComponent<Image>();
            pImg.sprite = sPanel;
            pImg.type = Image.Type.Sliced;

            var vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = Pad(26, 26, 20, 20);
            vlg.spacing = 10 * UI_SCALE;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // ---------- Header ----------
            var headerRow = CreateUIObject("ModalHeader", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 54));
            var leHeader = headerRow.AddComponent<LayoutElement>();
            leHeader.preferredHeight = 54 * UI_SCALE;
            var hlgHeader = headerRow.AddComponent<HorizontalLayoutGroup>();
            hlgHeader.spacing = 14 * UI_SCALE;
            hlgHeader.childAlignment = TextAnchor.MiddleLeft;
            hlgHeader.childControlWidth = false;
            hlgHeader.childControlHeight = false;

            var iconBox = CreateUIObject("IconBox", headerRow.transform, Vector2.zero, Vector2.zero, new Vector2(42, 42));
            iconBox.AddComponent<Image>().color = new Color(0f, 0.95f, 1f, 0.14f);
            if (sIconBarChart != null)
            {
                var bcGo = CreateUIObject("Icon", iconBox.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(22, 22));
                var bcImg = bcGo.AddComponent<Image>();
                bcImg.sprite = sIconBarChart;
                bcImg.color = ColPrimaryContainer;
                bcImg.raycastTarget = false;
            }

            var headerTextCol = CreateUIObject("Titles", headerRow.transform, Vector2.zero, Vector2.zero, new Vector2(560, 46));
            var vlgTitles = headerTextCol.AddComponent<VerticalLayoutGroup>();
            vlgTitles.spacing = 2 * UI_SCALE;
            vlgTitles.childAlignment = TextAnchor.MiddleLeft;
            vlgTitles.childControlWidth = false;
            vlgTitles.childControlHeight = false;

            var titleRow = CreateUIObject("TitleRow", headerTextCol.transform, Vector2.zero, Vector2.zero, new Vector2(560, 26));
            var hlgTitleRow = titleRow.AddComponent<HorizontalLayoutGroup>();
            hlgTitleRow.spacing = 10 * UI_SCALE;
            hlgTitleRow.childAlignment = TextAnchor.MiddleLeft;
            hlgTitleRow.childControlWidth = false;
            hlgTitleRow.childControlHeight = false;

            var title = CreateText(titleRow.transform, "Таблица рекордов", font, 18, FontStyles.Bold, ColPrimary, tracking: 2);
            title.textWrappingMode = TextWrappingModes.NoWrap;
            title.GetComponent<RectTransform>().sizeDelta = new Vector2(260 * UI_SCALE, 26 * UI_SCALE);

            var syncBadgeGo = CreateUIObject("SyncBadge", titleRow.transform, Vector2.zero, Vector2.zero, new Vector2(120, 22));
            if (sTabActive != null)
            {
                var syncImg = syncBadgeGo.AddComponent<Image>();
                syncImg.sprite = sTabActive;
                syncImg.type = Image.Type.Sliced;
            }
            else
            {
                syncBadgeGo.AddComponent<Image>().color = new Color(0f, 0.95f, 1f, 0.16f);
            }
            var syncBadge = CreateText(syncBadgeGo.transform, "GLOBAL_SYNC", font, 9, FontStyles.Bold, ColPrimaryContainer, tracking: 4);
            syncBadge.alignment = TextAlignmentOptions.Center;
            var sRt = syncBadge.GetComponent<RectTransform>();
            sRt.anchorMin = Vector2.zero; sRt.anchorMax = Vector2.one; sRt.offsetMin = Vector2.zero; sRt.offsetMax = Vector2.zero;

            var subtitle = CreateText(headerTextCol.transform, "Локальные и глобальные результаты очистки секторов", font, 11, FontStyles.Normal, ColOnSurfaceVariant, tracking: 2);
            subtitle.textWrappingMode = TextWrappingModes.NoWrap;
            subtitle.GetComponent<RectTransform>().sizeDelta = new Vector2(560 * UI_SCALE, 20 * UI_SCALE);

            var headerSpacer = CreateUIObject("Spacer", headerRow.transform, Vector2.zero, Vector2.zero, Vector2.zero);
            headerSpacer.AddComponent<LayoutElement>().flexibleWidth = 1f;

            var btnClose = CreateIconButton("BtnClose", headerRow.transform, sIconClose, out _).GetComponent<Button>();

            var divider = CreateUIObject("Divider", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 1));
            var leDiv = divider.AddComponent<LayoutElement>();
            leDiv.preferredHeight = 1 * UI_SCALE;
            divider.AddComponent<Image>().color = ColOutline;

            // ---------- Tabs ----------
            var tabsRow = CreateUIObject("TabsRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 40));
            var leTabs = tabsRow.AddComponent<LayoutElement>();
            leTabs.preferredHeight = 40 * UI_SCALE;
            tabsRow.AddComponent<Image>().color = ColSurfaceLow;
            var hlgTabs = tabsRow.AddComponent<HorizontalLayoutGroup>();
            hlgTabs.padding = Pad(8, 8, 5, 5);
            hlgTabs.spacing = 6 * UI_SCALE;
            hlgTabs.childAlignment = TextAnchor.MiddleLeft;
            hlgTabs.childControlWidth = false;
            hlgTabs.childControlHeight = false;

            var tBeg = CreateNavTab(tabsRow.transform, "Новичок (9×9)", font, sTabInactive, sTabActive, out var bgBeg, isPrimary: true, width: 150);
            var tInt = CreateNavTab(tabsRow.transform, "Любитель (16×16)", font, sTabInactive, sTabActive, out var bgInt, width: 170);
            var tExp = CreateNavTab(tabsRow.transform, "Профессионал (30×16)", font, sTabInactive, sTabActive, out var bgExp, width: 200);
            var tCust = CreateNavTab(tabsRow.transform, "Особый", font, sTabInactive, sTabActive, out var bgCust, width: 110);

            // ---------- Table header ----------
            var headerTable = CreateUIObject("TableHeader", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 30));
            var leHdr = headerTable.AddComponent<LayoutElement>();
            leHdr.preferredHeight = 30 * UI_SCALE;
            headerTable.AddComponent<Image>().color = ColSurface;
            var hlgHdr = headerTable.AddComponent<HorizontalLayoutGroup>();
            hlgHdr.padding = Pad(18, 18, 0, 0);
            hlgHdr.spacing = 10 * UI_SCALE;
            hlgHdr.childAlignment = TextAnchor.MiddleLeft;
            hlgHdr.childControlWidth = false;
            hlgHdr.childControlHeight = true;

            CreateLabel(headerTable.transform, "РАНГ", 52, ColOnSurfaceVariant, font);
            CreateLabel(headerTable.transform, "ИГРОК", 250, ColOnSurfaceVariant, font);
            CreateLabel(headerTable.transform, "ВРЕМЯ", 100, ColOnSurfaceVariant, font);
            CreateLabel(headerTable.transform, "ЗВУК/С", 86, ColOnSurfaceVariant, font);
            CreateLabel(headerTable.transform, "ТОЧНОСТЬ", 104, ColOnSurfaceVariant, font);
            CreateLabel(headerTable.transform, "ДАТА", 130, ColOnSurfaceVariant, font);
            CreateLabel(headerTable.transform, "РЕПЛЕЙ", 70, ColOnSurfaceVariant, font);

            // ---------- Rows ----------
            var rowsCont = CreateUIObject("RowsContainer", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 240));
            var leRows = rowsCont.AddComponent<LayoutElement>();
            leRows.preferredHeight = 240 * UI_SCALE;
            leRows.flexibleHeight = 1;
            var vlgRows = rowsCont.AddComponent<VerticalLayoutGroup>();
            vlgRows.spacing = 4 * UI_SCALE;
            vlgRows.childControlWidth = true;
            vlgRows.childControlHeight = true;
            vlgRows.childForceExpandHeight = false;

            var noticeGo = CreateUIObject("CustomNotice", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 40));
            var leNotice = noticeGo.AddComponent<LayoutElement>();
            leNotice.preferredHeight = 40 * UI_SCALE;
            noticeGo.AddComponent<Image>().color = ColSurfaceLow;
            var noticeTxt = CreateText(noticeGo.transform, "ОСОБЫЙ РЕЖИМ НЕ УЧИТЫВАЕТСЯ В ТАБЛИЦЕ РЕКОРДОВ (FR-025)", font, 11, FontStyles.Normal, ColAmber, tracking: 3);
            noticeTxt.alignment = TextAlignmentOptions.Center;
            noticeTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(800 * UI_SCALE, 40 * UI_SCALE);
            noticeGo.SetActive(false);

            // ---------- Best result banner ----------
            var bestBanner = CreateUIObject("BestBanner", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 46));
            var leBest = bestBanner.AddComponent<LayoutElement>();
            leBest.preferredHeight = 46 * UI_SCALE;
            bestBanner.AddComponent<Image>().color = ColSurfaceLowest;
            var hlgBest = bestBanner.AddComponent<HorizontalLayoutGroup>();
            hlgBest.padding = Pad(14, 14, 0, 0);
            hlgBest.spacing = 10 * UI_SCALE;
            hlgBest.childAlignment = TextAnchor.MiddleLeft;
            hlgBest.childControlWidth = false;
            hlgBest.childControlHeight = true;

            var bestTxt = CreateText(bestBanner.transform, "ВАШ ЛУЧШИЙ РЕЗУЛЬТАТ: 028.5s [РАНГ #1 — CYBER_VIPER]", font, 11, FontStyles.Bold, ColPrimaryContainer, tracking: 2);
            bestTxt.textWrappingMode = TextWrappingModes.NoWrap;
            bestTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(560 * UI_SCALE, 24 * UI_SCALE);

            var bestSpacer = CreateUIObject("Spacer", bestBanner.transform, Vector2.zero, Vector2.zero, Vector2.zero);
            bestSpacer.AddComponent<LayoutElement>().flexibleWidth = 1f;

            var bestStat = CreateText(bestBanner.transform, "Точность: 98.4%    ЗВУК/С: 4.10", font, 11, FontStyles.Normal, ColOnSurfaceVariant, tracking: 2);
            bestStat.textWrappingMode = TextWrappingModes.NoWrap;
            bestStat.GetComponent<RectTransform>().sizeDelta = new Vector2(280 * UI_SCALE, 24 * UI_SCALE);

            // ---------- Footer buttons ----------
            var btnRow = CreateUIObject("ButtonRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 44));
            var leBtns = btnRow.AddComponent<LayoutElement>();
            leBtns.preferredHeight = 44 * UI_SCALE;
            var hlgBtn = btnRow.AddComponent<HorizontalLayoutGroup>();
            hlgBtn.spacing = 14 * UI_SCALE;
            hlgBtn.childAlignment = TextAnchor.MiddleRight;
            hlgBtn.childControlWidth = false;
            hlgBtn.childControlHeight = false;

            var btnReset = CreateActionButton(btnRow.transform, "Сбросить рекорды", font, sBtnCyber, 180, 42);
            var btnClose2 = CreateActionButton(btnRow.transform, "Закрыть", font, sBtnAccent, 150, 42);
            var closeText = btnClose2.GetComponentInChildren<TextMeshProUGUI>();
            if (closeText != null) closeText.color = ColOnPrimaryContainer;

            var lm = modalRoot.AddComponent<LeaderboardsModal>();
            SetPrivateField(lm, "_panel", backdrop);
            SetPrivateField(lm, "_tabBeginner", tBeg);
            SetPrivateField(lm, "_tabIntermediate", tInt);
            SetPrivateField(lm, "_tabExpert", tExp);
            SetPrivateField(lm, "_tabCustom", tCust);
            SetPrivateField(lm, "_tabBeginnerBg", bgBeg);
            SetPrivateField(lm, "_tabIntermediateBg", bgInt);
            SetPrivateField(lm, "_tabExpertBg", bgExp);
            SetPrivateField(lm, "_tabCustomBg", bgCust);
            SetPrivateField(lm, "_rowsContainer", rowsCont.transform);
            SetPrivateField(lm, "_customNoticeObject", noticeGo);
            SetPrivateField(lm, "_bestResultText", bestTxt);
            SetPrivateField(lm, "_bestStatsText", bestStat);
            SetPrivateField(lm, "_resetScoresButton", btnReset);
            SetPrivateField(lm, "_closeButton", btnClose2);
            SetPrivateField(lm, "_headerCloseButton", btnClose);
            SetPrivateField(lm, "_fontAsset", font);
            SetPrivateField(lm, "_rowHeight", 42f);
            SetPrivateField(lm, "_avatarSprite", sDotSprite);
            SetPrivateField(lm, "_replaySprite", sReplaySprite);
            SetPrivateField(lm, "_playSprite", Load<Sprite>("Assets/Sprites/icon_play.png"));
            lm.ActiveTabSprite = sTabActive;
            lm.InactiveTabSprite = sTabInactive;
            backdrop.SetActive(false);
            return lm;
        }

        private static HelpAboutModal BuildHelpModal(Transform parent, TMP_FontAsset font, TMP_FontAsset fontSans, Sprite sPanel, Sprite sBtnCyber, Sprite sBtnDanger, Sprite sIconRules, Sprite sIconClose, Sprite sSlotMini, Sprite sTabActive)
        {
            var modalRoot = CreateUIObject("HelpAboutModalRoot", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var backdrop = CreateModalBackdrop("Backdrop", modalRoot.transform, new Color(0.02f, 0.03f, 0.05f, 0.86f));
            var panel = CreateUIObject("Panel", backdrop.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(720, 600));
            var pImg = panel.AddComponent<Image>();
            pImg.sprite = sPanel;
            pImg.type = Image.Type.Sliced;

            var vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = Pad(26, 26, 20, 20);
            vlg.spacing = 10 * UI_SCALE;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var headerRow = CreateUIObject("ModalHeader", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 54));
            var leHeader = headerRow.AddComponent<LayoutElement>();
            leHeader.preferredHeight = 54 * UI_SCALE;
            var hlgHeader = headerRow.AddComponent<HorizontalLayoutGroup>();
            hlgHeader.spacing = 14 * UI_SCALE;
            hlgHeader.childAlignment = TextAnchor.MiddleLeft;
            hlgHeader.childControlWidth = false;
            hlgHeader.childControlHeight = false;

            var iconBox = CreateUIObject("IconBox", headerRow.transform, Vector2.zero, Vector2.zero, new Vector2(42, 42));
            iconBox.AddComponent<Image>().color = new Color(0f, 0.95f, 1f, 0.14f);
            if (sIconRules != null)
            {
                var rlGo = CreateUIObject("Icon", iconBox.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(22, 22));
                var rlImg = rlGo.AddComponent<Image>();
                rlImg.sprite = sIconRules;
                rlImg.color = ColPrimaryContainer;
                rlImg.raycastTarget = false;
            }

            var title = CreateText(headerRow.transform, "Правила игры", font, 18, FontStyles.Bold, ColPrimary, tracking: 2);
            title.textWrappingMode = TextWrappingModes.NoWrap;
            title.GetComponent<RectTransform>().sizeDelta = new Vector2(420 * UI_SCALE, 26 * UI_SCALE);

            var headerSpacer = CreateUIObject("Spacer", headerRow.transform, Vector2.zero, Vector2.zero, Vector2.zero);
            headerSpacer.AddComponent<LayoutElement>().flexibleWidth = 1f;

            var btnClose = CreateIconButton("BtnClose", headerRow.transform, sIconClose, out _).GetComponent<Button>();

            var divider = CreateUIObject("Divider", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 1));
            var leDiv = divider.AddComponent<LayoutElement>();
            leDiv.preferredHeight = 1 * UI_SCALE;
            divider.AddComponent<Image>().color = ColOutline;

            var tabsRow = CreateUIObject("TabsRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 38));
            var leTabs = tabsRow.AddComponent<LayoutElement>();
            leTabs.preferredHeight = 38 * UI_SCALE;
            var hlgTabs = tabsRow.AddComponent<HorizontalLayoutGroup>();
            hlgTabs.spacing = 8 * UI_SCALE;
            hlgTabs.childAlignment = TextAnchor.MiddleLeft;
            hlgTabs.childControlWidth = false;
            hlgTabs.childControlHeight = false;

            var tabRules = CreateNavTab(tabsRow.transform, "Правила игры", font, sBtnCyber, sTabActive, out var bgRules, isPrimary: true, width: 150);
            var tabAbout = CreateNavTab(tabsRow.transform, "О программе", font, sBtnCyber, sTabActive, out var bgAbout, width: 150);

            var rulesCont = CreateUIObject("RulesContainer", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 380));
            var leRules = rulesCont.AddComponent<LayoutElement>();
            leRules.preferredHeight = 380 * UI_SCALE;
            leRules.flexibleHeight = 1;
            var vlgRules = rulesCont.AddComponent<VerticalLayoutGroup>();
            vlgRules.padding = Pad(18, 18, 14, 14);
            vlgRules.spacing = 8 * UI_SCALE;
            vlgRules.childControlWidth = true;
            vlgRules.childControlHeight = false;
            rulesCont.AddComponent<Image>().color = ColSurfaceLowest;

            CreateBullet(rulesCont.transform, "[ЛКМ] Открытие ячейки: первый клик всегда безопасен (зона 3×3).", font);
            CreateBullet(rulesCont.transform, "[ПКМ] Установка / снятие флажка: защищает ячейку от подрыва.", font);
            CreateBullet(rulesCont.transform, "[СКМ / ЛКМ+ПКМ] Аккорд: быстрое раскрытие соседей вокруг числа.", font);
            CreateBullet(rulesCont.transform, "[Пробел / Пауза] Приостановка таймера с сокрытием поля.", font);
            CreateBullet(rulesCont.transform, "[F2 / клик по дрону] Быстрый рестарт партии с сохранением сложности.", font);
            CreateBullet(rulesCont.transform, "Цель: открыть все свободные от мин ячейки за минимальное время.", font, ColPrimaryContainer);

            var aboutCont = CreateUIObject("AboutContainer", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 380));
            var leAbout = aboutCont.AddComponent<LayoutElement>();
            leAbout.preferredHeight = 380 * UI_SCALE;
            leAbout.flexibleHeight = 1;
            var vlgAbout = aboutCont.AddComponent<VerticalLayoutGroup>();
            vlgAbout.padding = Pad(18, 18, 14, 14);
            vlgAbout.spacing = 6 * UI_SCALE;
            vlgAbout.childControlWidth = true;
            vlgAbout.childControlHeight = false;
            aboutCont.AddComponent<Image>().color = ColSurfaceLowest;

            CreateBullet(aboutCont.transform, "Учебный проект по дисциплине «Технологии разработки программно-информационных систем»", font, ColOnSurfaceVariant);
            CreateBullet(aboutCont.transform, "Волгоградский государственный технический университет (ВолгГТУ), 2026 г.", font, ColOnSurfaceVariant);
            CreateBullet(aboutCont.transform, "Преподаватель: доцент Рак Д.В.", font, ColOnSurface);
            CreateBullet(aboutCont.transform, "Команда разработчиков (гр. ИВТ-1):", font, ColPrimaryContainer);
            CreateBullet(aboutCont.transform, "• Тюрин С.С. — Тимлид / Архитектор", font, ColOnSurface);
            CreateBullet(aboutCont.transform, "• Игнатов М.Р. — Frontend-разработчик (Cyberpunk HUD UI)", font, ColOnSurface);
            CreateBullet(aboutCont.transform, "• Сапунков А.Р. — Backend-разработчик (Core / игровая логика)", font, ColOnSurface);
            aboutCont.SetActive(false);

            var btnRow = CreateUIObject("ButtonRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 44));
            var leBtns = btnRow.AddComponent<LayoutElement>();
            leBtns.preferredHeight = 44 * UI_SCALE;
            var hlgBtn = btnRow.AddComponent<HorizontalLayoutGroup>();
            hlgBtn.spacing = 14 * UI_SCALE;
            hlgBtn.childAlignment = TextAnchor.MiddleRight;
            hlgBtn.childControlWidth = false;
            hlgBtn.childControlHeight = false;

            var btnExit = CreateActionButton(btnRow.transform, "Выход из приложения", font, sBtnDanger, 190, 42);
            var btnClose2 = CreateActionButton(btnRow.transform, "Закрыть", font, sBtnCyber, 150, 42);

            var hm = modalRoot.AddComponent<HelpAboutModal>();
            SetPrivateField(hm, "_panel", backdrop);
            SetPrivateField(hm, "_rulesContainer", rulesCont);
            SetPrivateField(hm, "_aboutContainer", aboutCont);
            SetPrivateField(hm, "_titleText", title);
            SetPrivateField(hm, "_tabRulesButton", tabRules);
            SetPrivateField(hm, "_tabAboutButton", tabAbout);
            SetPrivateField(hm, "_tabRulesBg", bgRules);
            SetPrivateField(hm, "_tabAboutBg", bgAbout);
            SetPrivateField(hm, "_closeButton", btnClose2);
            SetPrivateField(hm, "_headerCloseButton", btnClose);
            SetPrivateField(hm, "_exitButton", btnExit);
            backdrop.SetActive(false);
            return hm;
        }

        // ==================================================================
        // UI FACTORY HELPERS
        // ==================================================================
        private static T Load<T>(string path) where T : UnityEngine.Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) Debug.LogWarning($"[CyberMinesweeperSceneBuilder] Missing asset: {path}");
            return asset;
        }

        /// <summary>
        /// Makes sure the procedurally generated HUD chrome sprites are imported as
        /// 9-sliced sprites with sensible borders.
        /// </summary>
        private static void EnsureSpriteBorders()
        {
            var borders = new Dictionary<string, Vector4>
            {
                { "Assets/Sprites/tab_active.png", new Vector4(16, 16, 16, 16) },
                { "Assets/Sprites/card_active.png", new Vector4(16, 16, 16, 16) },
                { "Assets/Sprites/slot_counter.png", new Vector4(16, 16, 16, 16) },
                { "Assets/Sprites/slot_inset.png", new Vector4(20, 20, 20, 20) },
                { "Assets/Sprites/slot_mini.png", new Vector4(14, 14, 14, 14) },
                { "Assets/Sprites/panel_deck.png", new Vector4(28, 28, 28, 28) },
                { "Assets/Sprites/panel_modal.png", new Vector4(24, 24, 24, 24) },
            };

            foreach (var kv in borders)
            {
                var importer = AssetImporter.GetAtPath(kv.Key) as TextureImporter;
                if (importer == null) continue;

                var b = kv.Value;
                if (importer.spriteBorder == b) continue;

                importer.spriteBorder = b;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
        }

        /// <summary>Scaled padding helper - every RectOffset in this file is a HUD padding.</summary>
        private static RectOffset Pad(int left, int right, int top, int bottom)
        {
            return new RectOffset(
                Mathf.RoundToInt(left * UI_SCALE),
                Mathf.RoundToInt(right * UI_SCALE),
                Mathf.RoundToInt(top * UI_SCALE),
                Mathf.RoundToInt(bottom * UI_SCALE));
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }

        private static GameObject CreateModalBackdrop(string name, Transform parent, Color color)
        {
            var backdrop = CreateUIObject(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var img = backdrop.AddComponent<Image>();
            img.color = color;
            return backdrop;
        }

        private static GameObject CreateUIObject(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.sizeDelta = size * UI_SCALE;
            return go;
        }

        private static GameObject CreateUIObject(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            return go;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string text, TMP_FontAsset font, float size, FontStyles style, Color color, float tracking = 0f)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.text = text;
            tmp.fontSize = size * UI_SCALE;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.raycastTarget = false;
            tmp.characterSpacing = tracking;
            tmp.rectTransform.sizeDelta = Vector2.zero;
            return tmp;
        }

        private static TextMeshProUGUI CreateLabel(Transform parent, string text, float width, Color color, TMP_FontAsset font)
        {
            var tmp = CreateText(parent, text, font, 10, FontStyles.Bold, color, tracking: 4);
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.rectTransform.sizeDelta = new Vector2(width * UI_SCALE, 24 * UI_SCALE);
            var le = tmp.gameObject.AddComponent<LayoutElement>();
            le.minWidth = width * UI_SCALE;
            le.preferredWidth = width * UI_SCALE;
            return tmp;
        }

        private static TextMeshProUGUI CreateSectionLabel(Transform parent, string text, TMP_FontAsset font)
        {
            var tmp = CreateText(parent, text, font, 10, FontStyles.Bold, ColOnSurfaceVariant, tracking: 6);
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.rectTransform.sizeDelta = new Vector2(400 * UI_SCALE, 16 * UI_SCALE);
            var le = tmp.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 16 * UI_SCALE;
            return tmp;
        }

        private static TextMeshProUGUI CreateBullet(Transform parent, string text, TMP_FontAsset font, Color? color = null)
        {
            var tmp = CreateText(parent, text, font, 12, FontStyles.Normal, color ?? ColOnSurface, tracking: 1);
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.rectTransform.sizeDelta = new Vector2(620 * UI_SCALE, 20 * UI_SCALE);
            var le = tmp.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 20 * UI_SCALE;
            return tmp;
        }

        private static void AddCornerBracket(Transform parent, Vector2 anchor, Vector2 offset, Color color)
        {
            var corner = CreateUIObject("CornerBracket", parent, anchor, anchor, new Vector2(14, 14));
            corner.GetComponent<RectTransform>().anchoredPosition = offset;
            var le = corner.AddComponent<LayoutElement>();
            le.ignoreLayout = true;
            var hLine = CreateUIObject("H", corner.transform, new Vector2(anchor.x, anchor.y), new Vector2(anchor.x, anchor.y), new Vector2(14, 2));
            hLine.GetComponent<RectTransform>().anchoredPosition = new Vector2((anchor.x == 0 ? 7f : -7f) * UI_SCALE, (anchor.y == 0 ? 1f : -1f) * UI_SCALE);
            hLine.AddComponent<Image>().color = color;
            var vLine = CreateUIObject("V", corner.transform, new Vector2(anchor.x, anchor.y), new Vector2(anchor.x, anchor.y), new Vector2(2, 14));
            vLine.GetComponent<RectTransform>().anchoredPosition = new Vector2((anchor.x == 0 ? 1f : -1f) * UI_SCALE, (anchor.y == 0 ? 7f : -7f) * UI_SCALE);
            vLine.AddComponent<Image>().color = color;
        }

        private static GameObject CreatePulseDot(Transform parent, Color color, float size = 6f, Sprite dotSprite = null)
        {
            var dot = CreateUIObject("PulseDot", parent, Vector2.zero, Vector2.zero, new Vector2(size, size));
            var img = dot.AddComponent<Image>();
            img.sprite = dotSprite;
            img.color = color;
            var le = dot.AddComponent<LayoutElement>();
            le.minWidth = size * UI_SCALE;
            le.preferredWidth = size * UI_SCALE;
            le.minHeight = size * UI_SCALE;
            le.preferredHeight = size * UI_SCALE;
            dot.AddComponent<CyberPulseDot>();
            return dot;
        }

        private static Button CreateNavTab(Transform parent, string text, TMP_FontAsset font, Sprite inactiveSprite, Sprite activeSprite, out Image bgImage, bool isPrimary = false, float width = 0f)
        {
            if (width <= 0f) width = isPrimary ? 78f : 96f;
            var go = CreateUIObject("Tab_" + text, parent, Vector2.zero, Vector2.zero, new Vector2(width, 30));
            bgImage = go.AddComponent<Image>();
            if (isPrimary && activeSprite != null)
            {
                bgImage.sprite = activeSprite;
                bgImage.type = Image.Type.Sliced;
                bgImage.color = Color.white;
            }
            else if (inactiveSprite != null)
            {
                bgImage.sprite = inactiveSprite;
                bgImage.type = Image.Type.Sliced;
                bgImage.color = new Color(1f, 1f, 1f, 0f);
            }
            else
            {
                bgImage.color = isPrimary ? new Color(0f, 242f / 255f, 254f / 255f, 0.16f) : new Color(1f, 1f, 1f, 0f);
            }

            var btn = go.AddComponent<Button>();
            var txt = CreateText(go.transform, text, font, 11, FontStyles.Bold, isPrimary ? ColPrimary : ColOnSurfaceVariant, tracking: 3);
            txt.alignment = TextAlignmentOptions.Center;
            txt.textWrappingMode = TextWrappingModes.NoWrap;
            var rt = txt.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            return btn;
        }

        private static GameObject CreateIconButton(string name, Transform parent, Sprite icon, out Image iconImg)
        {
            var go = CreateUIObject(name, parent, Vector2.zero, Vector2.zero, new Vector2(34, 34));
            var img = go.AddComponent<Image>();
            img.color = ColSurfaceLow;
            var btn = go.AddComponent<Button>();

            var iconGo = CreateUIObject("Icon", go.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(16, 16));
            iconImg = iconGo.AddComponent<Image>();
            iconImg.sprite = icon;
            iconImg.color = ColOnSurfaceVariant;
            iconImg.raycastTarget = false;
            return go;
        }

        private static GameObject CreateActionButtonWithIcon(string name, Transform parent, string label, Sprite icon, float textWidth, TMP_FontAsset font, Sprite btnSprite, out TextMeshProUGUI outText, out Image outIcon)
        {
            float totalWidth = 16f + 8f + textWidth + 16f;
            var go = CreateUIObject(name, parent, Vector2.zero, Vector2.zero, new Vector2(totalWidth, 36));
            var img = go.AddComponent<Image>();
            if (btnSprite != null)
            {
                img.sprite = btnSprite;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
            }
            else
            {
                img.color = ColSurfaceLow;
            }
            go.AddComponent<Button>();

            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = Pad(10, 10, 0, 0);
            hlg.spacing = 8 * UI_SCALE;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            if (icon != null)
            {
                var iconGo = CreateUIObject("Icon", go.transform, Vector2.zero, Vector2.zero, new Vector2(16, 16));
                outIcon = iconGo.AddComponent<Image>();
                outIcon.sprite = icon;
                outIcon.color = ColPrimaryContainer;
                outIcon.raycastTarget = false;
            }
            else
            {
                outIcon = null;
            }

            outText = CreateText(go.transform, label, font, 11, FontStyles.Bold, ColPrimary, tracking: 3);
            outText.textWrappingMode = TextWrappingModes.NoWrap;
            outText.alignment = TextAlignmentOptions.MidlineLeft;
            outText.GetComponent<RectTransform>().sizeDelta = new Vector2(textWidth * UI_SCALE, 22 * UI_SCALE);

            return go;
        }

        private static GameObject CreateActionButtonWithIcon(string name, Transform parent, string label, Sprite icon, float textWidth, TMP_FontAsset font, Sprite btnSprite)
        {
            return CreateActionButtonWithIcon(name, parent, label, icon, textWidth, font, btnSprite, out _, out _);
        }

        private static Button CreateActionButton(Transform parent, string text, TMP_FontAsset font, Sprite sprite, float width, float height)
        {
            var go = CreateUIObject("Btn_" + text, parent, Vector2.zero, Vector2.zero, new Vector2(width, height));
            var img = go.AddComponent<Image>();
            if (sprite != null)
            {
                img.sprite = sprite;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
            }
            else
            {
                img.color = new Color(1f, 1f, 1f, 0f);
            }
            var btn = go.AddComponent<Button>();

            var txt = CreateText(go.transform, text, font, 11, FontStyles.Bold, ColPrimary, tracking: 3);
            txt.alignment = TextAlignmentOptions.Center;
            txt.textWrappingMode = TextWrappingModes.NoWrap;
            var rt = txt.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            return btn;
        }

        private static Button CreatePresetCard(Transform parent, string tag, string title, string subtitle, TMP_FontAsset font, Sprite sBtn, out Image indicator)
        {
            var card = CreateUIObject("Card_" + title, parent, Vector2.zero, Vector2.zero, new Vector2(158, 74));
            var leCard = card.AddComponent<LayoutElement>();
            leCard.minWidth = 140 * UI_SCALE;
            leCard.preferredWidth = 158 * UI_SCALE;
            leCard.preferredHeight = 74 * UI_SCALE;
            leCard.flexibleWidth = 1f;

            var img = card.AddComponent<Image>();
            img.sprite = sBtn;
            img.type = Image.Type.Sliced;
            var btn = card.AddComponent<Button>();

            var vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.padding = Pad(10, 10, 6, 6);
            vlg.spacing = 3 * UI_SCALE;
            vlg.childAlignment = TextAnchor.MiddleLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var topRow = CreateUIObject("TopRow", card.transform, Vector2.zero, Vector2.zero, new Vector2(0, 16));
            var leTR = topRow.AddComponent<LayoutElement>();
            leTR.preferredHeight = 16 * UI_SCALE;
            var hlgTR = topRow.AddComponent<HorizontalLayoutGroup>();
            hlgTR.childAlignment = TextAnchor.MiddleLeft;
            hlgTR.childControlWidth = false;
            hlgTR.childControlHeight = false;
            hlgTR.childForceExpandWidth = false;
            hlgTR.childForceExpandHeight = false;

            var tagTxt = CreateText(topRow.transform, tag, font, 9, FontStyles.Bold, ColOnSurfaceVariant, tracking: 4);
            tagTxt.textWrappingMode = TextWrappingModes.NoWrap;
            tagTxt.rectTransform.sizeDelta = new Vector2(64 * UI_SCALE, 16 * UI_SCALE);

            var trSpacer = CreateUIObject("Spacer", topRow.transform, Vector2.zero, Vector2.zero, new Vector2(0, 0));
            trSpacer.AddComponent<LayoutElement>().flexibleWidth = 1f;

            var indGo = CreateUIObject("Indicator", topRow.transform, Vector2.zero, Vector2.zero, new Vector2(10, 10));
            var leInd = indGo.AddComponent<LayoutElement>();
            leInd.minWidth = 10 * UI_SCALE;
            leInd.minHeight = 10 * UI_SCALE;
            leInd.preferredWidth = 10 * UI_SCALE;
            leInd.preferredHeight = 10 * UI_SCALE;
            indicator = indGo.AddComponent<Image>();
            indicator.sprite = Load<Sprite>("Assets/Sprites/badge_dot.png");
            indicator.color = new Color(1f, 1f, 1f, 0.22f);

            var tTitle = CreateText(card.transform, title, font, 12, FontStyles.Bold, ColPrimary, tracking: 2);
            tTitle.textWrappingMode = TextWrappingModes.NoWrap;
            tTitle.rectTransform.sizeDelta = new Vector2(140 * UI_SCALE, 18 * UI_SCALE);
            var leTitle = tTitle.gameObject.AddComponent<LayoutElement>();
            leTitle.preferredHeight = 18 * UI_SCALE;

            var tSub = CreateText(card.transform, subtitle, font, 10, FontStyles.Normal, ColOnSurfaceVariant, tracking: 1);
            tSub.textWrappingMode = TextWrappingModes.NoWrap;
            tSub.rectTransform.sizeDelta = new Vector2(140 * UI_SCALE, 16 * UI_SCALE);
            var leSub = tSub.gameObject.AddComponent<LayoutElement>();
            leSub.preferredHeight = 16 * UI_SCALE;

            return btn;
        }

        private static GameObject CreateStatTile(Transform parent, string title, string val, TMP_FontAsset font, Color valueColor, out TextMeshProUGUI valText)
        {
            var tile = CreateUIObject("Tile_" + title, parent, Vector2.zero, Vector2.zero, new Vector2(150, 74));
            tile.AddComponent<Image>().color = ColSurfaceLowest;
            var vlg = tile.AddComponent<VerticalLayoutGroup>();
            vlg.padding = Pad(8, 8, 8, 8);
            vlg.spacing = 4 * UI_SCALE;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var tTitle = CreateText(tile.transform, title, font, 9, FontStyles.Bold, ColOnSurfaceVariant, tracking: 4);
            tTitle.alignment = TextAlignmentOptions.Center;
            tTitle.textWrappingMode = TextWrappingModes.NoWrap;
            var leTitle = tTitle.gameObject.AddComponent<LayoutElement>();
            leTitle.preferredHeight = 16f * UI_SCALE;

            valText = CreateText(tile.transform, val, font, 17, FontStyles.Bold, valueColor, tracking: 3);
            valText.alignment = TextAlignmentOptions.Center;
            valText.textWrappingMode = TextWrappingModes.NoWrap;
            var leVal = valText.gameObject.AddComponent<LayoutElement>();
            leVal.preferredHeight = 28f * UI_SCALE;

            return tile;
        }

        private static Slider CreateCyberSlider(string name, Transform parent, Vector2 size, float min, float max, float val)
        {
            var sliderGo = CreateUIObject(name, parent, Vector2.zero, Vector2.zero, size);
            var le = sliderGo.AddComponent<LayoutElement>();
            le.preferredWidth = size.x * UI_SCALE;
            le.preferredHeight = size.y * UI_SCALE;

            var bgGo = CreateUIObject("Background", sliderGo.transform, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0, 6));
            bgGo.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = ColSurfaceHigh;

            var fillArea = CreateUIObject("Fill Area", sliderGo.transform, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(-12, 6));
            fillArea.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            var fill = CreateUIObject("Fill", fillArea.transform, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0));
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = ColPrimaryContainer;

            var handleArea = CreateUIObject("Handle Slide Area", sliderGo.transform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(-12, 0));
            var handle = CreateUIObject("Handle", handleArea.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(14, 14));
            var handleImg = handle.AddComponent<Image>();
            handleImg.color = ColPrimaryContainer;

            var slider = sliderGo.AddComponent<Slider>();
            slider.targetGraphic = handleImg;
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = val;

            return slider;
        }

        private static GameObject CreateStepperWithSlider(Transform parent, string label, string val, int min, int max, TMP_FontAsset font, out TMP_InputField valInput, out Button btnMinus, out Button btnPlus, out Slider slider)
        {
            var box = CreateUIObject("Stepper_" + label, parent, Vector2.zero, Vector2.zero, new Vector2(220, 132));
            var leBox = box.AddComponent<LayoutElement>();
            leBox.minWidth = 190 * UI_SCALE;
            leBox.preferredWidth = 220 * UI_SCALE;
            leBox.preferredHeight = 132 * UI_SCALE;
            leBox.flexibleWidth = 1f;

            box.AddComponent<Image>().color = ColSurfaceLowest;
            var vlg = box.AddComponent<VerticalLayoutGroup>();
            vlg.padding = Pad(10, 10, 8, 8);
            vlg.spacing = 6 * UI_SCALE;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var topRow = CreateUIObject("TopRow", box.transform, Vector2.zero, Vector2.zero, new Vector2(0, 18));
            var leTR = topRow.AddComponent<LayoutElement>();
            leTR.preferredHeight = 18 * UI_SCALE;
            var hlgTR = topRow.AddComponent<HorizontalLayoutGroup>();
            hlgTR.childAlignment = TextAnchor.MiddleLeft;
            hlgTR.childControlWidth = false;
            hlgTR.childControlHeight = false;
            hlgTR.childForceExpandWidth = false;
            hlgTR.childForceExpandHeight = false;

            var lbl = CreateText(topRow.transform, label, font, 11, FontStyles.Bold, ColPrimary, tracking: 2);
            lbl.textWrappingMode = TextWrappingModes.NoWrap;
            lbl.rectTransform.sizeDelta = new Vector2(120 * UI_SCALE, 18 * UI_SCALE);

            var trSpacer = CreateUIObject("Spacer", topRow.transform, Vector2.zero, Vector2.zero, new Vector2(0, 0));
            trSpacer.AddComponent<LayoutElement>().flexibleWidth = 1f;

            var rangeLbl = CreateText(topRow.transform, $"{min}—{max}", font, 9, FontStyles.Normal, ColOnSurfaceVariant, tracking: 2);
            rangeLbl.textWrappingMode = TextWrappingModes.NoWrap;
            rangeLbl.rectTransform.sizeDelta = new Vector2(60 * UI_SCALE, 18 * UI_SCALE);

            var stepRow = CreateUIObject("StepRow", box.transform, Vector2.zero, Vector2.zero, new Vector2(0, 38));
            var leSR = stepRow.AddComponent<LayoutElement>();
            leSR.preferredHeight = 38 * UI_SCALE;
            stepRow.AddComponent<Image>().color = ColSurfaceLowest;
            var hlgSR = stepRow.AddComponent<HorizontalLayoutGroup>();
            hlgSR.padding = Pad(8, 8, 4, 4);
            hlgSR.childAlignment = TextAnchor.MiddleCenter;
            hlgSR.childControlWidth = false;
            hlgSR.childControlHeight = false;
            hlgSR.childForceExpandWidth = false;
            hlgSR.childForceExpandHeight = false;

            var minusGo = CreateUIObject("BtnMinus", stepRow.transform, Vector2.zero, Vector2.zero, new Vector2(32, 30));
            var leM = minusGo.AddComponent<LayoutElement>();
            leM.minWidth = 32 * UI_SCALE; leM.preferredWidth = 32 * UI_SCALE;
            leM.minHeight = 30 * UI_SCALE; leM.preferredHeight = 30 * UI_SCALE;
            minusGo.AddComponent<Image>().color = ColSurfaceHigh;
            btnMinus = minusGo.AddComponent<Button>();
            var txtMinus = CreateText(minusGo.transform, "−", font, 14, FontStyles.Bold, ColPrimary, tracking: 0);
            txtMinus.alignment = TextAlignmentOptions.Center;
            txtMinus.rectTransform.sizeDelta = new Vector2(32 * UI_SCALE, 30 * UI_SCALE);

            var midSpacer = CreateUIObject("Spacer", stepRow.transform, Vector2.zero, Vector2.zero, new Vector2(0, 0));
            midSpacer.AddComponent<LayoutElement>().flexibleWidth = 1f;

            // Editable value field (SRS UI-012 — «поля ввода» для высоты/ширины/числа мин).
            var valueGo = CreateUIObject("ValueInput", stepRow.transform, Vector2.zero, Vector2.zero, new Vector2(64, 30));
            var leValue = valueGo.AddComponent<LayoutElement>();
            leValue.preferredWidth = 64 * UI_SCALE;
            leValue.preferredHeight = 30 * UI_SCALE;
            valueGo.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0f);

            var textAreaGo = CreateUIObject("TextArea", valueGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var textAreaRt = textAreaGo.GetComponent<RectTransform>();

            var textCompGo = CreateUIObject("Text", textAreaGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var textComp = textCompGo.AddComponent<TextMeshProUGUI>();
            if (font != null) textComp.font = font;
            textComp.fontSize = 19 * UI_SCALE;
            textComp.fontStyle = FontStyles.Bold;
            textComp.color = ColPrimaryContainer;
            textComp.characterSpacing = 4;
            textComp.alignment = TextAlignmentOptions.Center;
            textComp.enableWordWrapping = false;
            textComp.raycastTarget = false;

            valInput = valueGo.AddComponent<TMP_InputField>();
            valInput.textViewport = textAreaRt;
            valInput.textComponent = textComp;
            valInput.fontAsset = font;
            valInput.contentType = TMP_InputField.ContentType.IntegerNumber;
            valInput.characterLimit = 3;
            valInput.text = val;

            var midSpacer2 = CreateUIObject("Spacer2", stepRow.transform, Vector2.zero, Vector2.zero, new Vector2(0, 0));
            midSpacer2.AddComponent<LayoutElement>().flexibleWidth = 1f;
            var plusGo = CreateUIObject("BtnPlus", stepRow.transform, Vector2.zero, Vector2.zero, new Vector2(32, 30));
            var leP = plusGo.AddComponent<LayoutElement>();
            leP.minWidth = 32 * UI_SCALE; leP.preferredWidth = 32 * UI_SCALE;
            leP.minHeight = 30 * UI_SCALE; leP.preferredHeight = 30 * UI_SCALE;
            plusGo.AddComponent<Image>().color = ColSurfaceHigh;
            btnPlus = plusGo.AddComponent<Button>();
            var txtPlus = CreateText(plusGo.transform, "+", font, 14, FontStyles.Bold, ColPrimary, tracking: 0);
            txtPlus.alignment = TextAlignmentOptions.Center;
            txtPlus.rectTransform.sizeDelta = new Vector2(32 * UI_SCALE, 30 * UI_SCALE);

            int.TryParse(val, out int parsedVal);
            slider = CreateCyberSlider("Slider", box.transform, new Vector2(196, 16), min, max, parsedVal);
            slider.wholeNumbers = true;

            return box;
        }

        private static Toggle CreateToggle(Transform parent, string label, TMP_FontAsset font)
        {
            var go = CreateUIObject("Toggle_" + label, parent, Vector2.zero, Vector2.zero, new Vector2(200, 26));
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = 200 * UI_SCALE;
            le.preferredHeight = 26 * UI_SCALE;

            var tgl = go.AddComponent<Toggle>();
            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10 * UI_SCALE;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            var box = CreateUIObject("Box", go.transform, Vector2.zero, Vector2.zero, new Vector2(20, 20));
            var leBox = box.AddComponent<LayoutElement>();
            leBox.preferredWidth = 20 * UI_SCALE; leBox.preferredHeight = 20 * UI_SCALE;
            var boxImg = box.AddComponent<Image>();
            boxImg.color = ColSurfaceHigh;

            var check = CreateUIObject("Check", box.transform, new Vector2(0.2f, 0.2f), new Vector2(0.8f, 0.8f), Vector2.zero, Vector2.zero);
            var checkImg = check.AddComponent<Image>();
            checkImg.sprite = Load<Sprite>("Assets/Sprites/icon_check.png");
            checkImg.color = ColPrimaryContainer;
            tgl.graphic = checkImg;
            tgl.targetGraphic = boxImg;

            var txt = CreateText(go.transform, label, font, 11, FontStyles.Normal, ColOnSurface, tracking: 2);
            txt.textWrappingMode = TextWrappingModes.NoWrap;
            txt.rectTransform.sizeDelta = new Vector2(170 * UI_SCALE, 20 * UI_SCALE);
            return tgl;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            if (target == null) return;
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (field != null)
            {
                field.SetValue(target, value);
            }
            else
            {
                Debug.LogWarning($"[CyberMinesweeperSceneBuilder] Field '{fieldName}' not found on {target.GetType().Name}");
            }
            if (target is UnityEngine.Object uo)
            {
                EditorUtility.SetDirty(uo);
            }
        }

        private static void BindAllCustomScriptsInScene()
        {
            var allMonoBehaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int count = 0;
            foreach (var mb in allMonoBehaviours)
            {
                if (mb == null) continue;
                Type type = mb.GetType();
                if (type.Namespace != null && type.Namespace.StartsWith("Minesweeper"))
                {
                    string typeName = type.Name;
                    string[] guids = AssetDatabase.FindAssets($"{typeName} t:MonoScript");
                    foreach (var guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        if (Path.GetFileNameWithoutExtension(path) == typeName)
                        {
                            var ms = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                            if (ms != null && ms.GetClass() == type)
                            {
                                var so = new SerializedObject(mb);
                                var sp = so.FindProperty("m_Script");
                                if (sp != null)
                                {
                                    sp.objectReferenceValue = ms;
                                    so.ApplyModifiedPropertiesWithoutUndo();
                                    count++;
                                }
                                EditorUtility.SetDirty(mb);
                                break;
                            }
                        }
                    }
                }
            }
            Debug.Log($"Successfully bound {count} custom MonoScript references to assets!");
        }
    }
}
