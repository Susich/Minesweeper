using System;
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
    public static class CyberMinesweeperSceneBuilder
    {
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

            // 1. Load Fonts
            var fontMono = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/JetBrainsMono SDF.asset");
            var fontSans = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/SpaceGrotesk SDF.asset");
            if (fontMono == null) fontMono = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if (fontSans == null) fontSans = fontMono;

            // Load Sprites
            var sCellClosed = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cell_closed.png");
            var sCellHover = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cell_hover.png");
            var sCellPressed = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cell_pressed.png");
            var sCellRevealed = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cell_revealed.png");
            var sCellExploded = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cell_exploded.png");
            var sIconFlag = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/icon_flag.png");
            var sIconMine = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/icon_mine.png");
            var sIconFalseFlag = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/icon_false_flag.png");

            var sFaceNormal = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/face_normal.png");
            var sFaceAlert = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/face_alert.png");
            var sFaceWin = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/face_win.png");
            var sFaceDefeat = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/face_defeat.png");

            var sPanelCyber = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/panel_cyber.png");
            var sPanelDanger = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/panel_danger.png");
            var sBtnCyber = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/button_cyber.png");
            var sBtnDanger = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/button_danger.png");
            var sBtnAccent = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/button_accent.png");

            var sIconRestart = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/icon_restart.png");
            var sIconPause = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/icon_pause.png");
            var sIconPlay = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/icon_play.png");
            var sIconHint = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/icon_hint.png");
            var sIconSoundOn = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/icon_sound_on.png");
            var sIconSoundOff = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/icon_sound_off.png");
            var sIconFullscreen = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/icon_fullscreen.png");
            var sIconSettings = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/icon_settings.png");
            var sIconGamepad = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/icon_gamepad.png");

            // 2. Create Scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera
            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(11f / 255f, 14f / 255f, 23f / 255f); // #0B0E17
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            camGo.transform.position = new Vector3(0, 0, -10);

            // Managers
            var managersGo = new GameObject("GameManagers", typeof(GameController), typeof(AudioManager));

            // Canvas
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 1.0f; // Lock vertical scale to 720px so UI never overflows vertically

            // EventSystem
            var esGo = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(InputSystemUIInputModule));

            // Main Background Panel
            var bgGo = CreateUIObject("Background", canvasGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(11f / 255f, 14f / 255f, 23f / 255f); // #0B0E17

            // --- TOP NAVIGATION BAR (modern_game_screen mockup) ---
            var topBar = CreateUIObject("TopMenuBar", canvasGo.transform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 50));
            topBar.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
            topBar.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            var topBarImg = topBar.AddComponent<Image>();
            topBarImg.color = new Color(13f / 255f, 18f / 255f, 28f / 255f, 0.98f); // #0D121C

            var hlgTop = topBar.AddComponent<HorizontalLayoutGroup>();
            hlgTop.padding = new RectOffset(20, 20, 0, 0);
            hlgTop.spacing = 16;
            hlgTop.childAlignment = TextAnchor.MiddleCenter;
            hlgTop.childControlWidth = false;
            hlgTop.childControlHeight = false;

            // Brand Group (Icon + САПЁР + Badge v2.4 PRO)
            var brandGo = CreateUIObject("Brand", topBar.transform, Vector2.zero, Vector2.zero, new Vector2(170, 32));
            var hlgBrand = brandGo.AddComponent<HorizontalLayoutGroup>();
            hlgBrand.spacing = 8;
            hlgBrand.childAlignment = TextAnchor.MiddleLeft;
            hlgBrand.childControlWidth = false;
            hlgBrand.childControlHeight = false;

            if (sIconGamepad != null)
            {
                var iconGo = CreateUIObject("GamepadIcon", brandGo.transform, Vector2.zero, Vector2.zero, new Vector2(20, 20));
                var gpImg = iconGo.AddComponent<Image>();
                gpImg.sprite = sIconGamepad;
                gpImg.color = new Color(0f, 242f / 255f, 254f / 255f);
            }

            var titleText = CreateText(brandGo.transform, "САПЁР", fontMono, 16, FontStyles.Bold, new Color(224f / 255f, 253f / 255f, 1f));
            titleText.textWrappingMode = TextWrappingModes.NoWrap;
            titleText.GetComponent<RectTransform>().sizeDelta = new Vector2(58, 22);

            var badgeGo = CreateUIObject("BadgePro", brandGo.transform, Vector2.zero, Vector2.zero, new Vector2(64, 22));
            badgeGo.AddComponent<Image>().color = new Color(34f / 255f, 42f / 255f, 58f / 255f);
            var badgeText = CreateText(badgeGo.transform, "v2.4 PRO", fontMono, 10, FontStyles.Bold, new Color(146f / 255f, 157f / 255f, 178f / 255f));
            badgeText.alignment = TextAlignmentOptions.Center;
            var btRt = badgeText.GetComponent<RectTransform>();
            btRt.anchorMin = Vector2.zero; btRt.anchorMax = Vector2.one; btRt.offsetMin = Vector2.zero; btRt.offsetMax = Vector2.zero;

            // Nav Tabs Group
            var tabsGroup = CreateUIObject("NavTabs", topBar.transform, Vector2.zero, Vector2.zero, new Vector2(360, 32));
            var hlgTabs = tabsGroup.AddComponent<HorizontalLayoutGroup>();
            hlgTabs.spacing = 6;
            hlgTabs.childAlignment = TextAnchor.MiddleLeft;
            hlgTabs.childControlHeight = false;
            hlgTabs.childControlWidth = false;

            var btnGame = CreateNavTab(tabsGroup.transform, "Игра", fontMono, out var gameTabBg, isPrimary: true);
            var btnRecords = CreateNavTab(tabsGroup.transform, "Рекорды", fontMono, out var recordsTabBg);
            var btnSettings = CreateNavTab(tabsGroup.transform, "Параметры", fontMono, out var settingsTabBg);
            var btnHelp = CreateNavTab(tabsGroup.transform, "Справка", fontMono, out var helpTabBg);

            // Flexible Spacer
            var topSpacer = CreateUIObject("Spacer", topBar.transform, Vector2.zero, Vector2.zero, Vector2.zero);
            topSpacer.AddComponent<LayoutElement>().flexibleWidth = 1f;

            // Right Controls Group
            var rightGroup = CreateUIObject("RightControls", topBar.transform, Vector2.zero, Vector2.zero, new Vector2(360, 32));
            var hlgRight = rightGroup.AddComponent<HorizontalLayoutGroup>();
            hlgRight.spacing = 8;
            hlgRight.childAlignment = TextAnchor.MiddleRight;
            hlgRight.childControlHeight = false;
            hlgRight.childControlWidth = false;

            var diffPillGo = CreateUIObject("DifficultyPill", rightGroup.transform, Vector2.zero, Vector2.zero, new Vector2(170, 30));
            diffPillGo.AddComponent<Image>().color = new Color(24f / 255f, 30f / 255f, 43f / 255f);
            var btnDiffPill = diffPillGo.AddComponent<Button>();
            var hlgDP = diffPillGo.AddComponent<HorizontalLayoutGroup>();
            hlgDP.padding = new RectOffset(8, 8, 0, 0);
            hlgDP.spacing = 6;
            hlgDP.childAlignment = TextAnchor.MiddleCenter;
            hlgDP.childControlWidth = false;
            hlgDP.childControlHeight = false;

            CreatePulseDot(diffPillGo.transform, new Color(52f / 255f, 211f / 255f, 153f / 255f), 6f);
            var diffPillText = CreateText(diffPillGo.transform, "Любитель • 40 мин", fontMono, 11, FontStyles.Bold, new Color(224f / 255f, 253f / 255f, 1f));
            diffPillText.textWrappingMode = TextWrappingModes.NoWrap;

            var btnSoundGo = CreateIconButton("BtnSound", rightGroup.transform, sIconSoundOn, out var soundIconImg);
            var btnSound = btnSoundGo.GetComponent<Button>();

            var btnFsGo = CreateIconButton("BtnFullscreen", rightGroup.transform, sIconFullscreen, out _);
            var btnFullscreen = btnFsGo.GetComponent<Button>();

            var btnGearGo = CreateIconButton("BtnSettingsGear", rightGroup.transform, sIconSettings, out _);
            var btnSettingsGear = btnGearGo.GetComponent<Button>();

            var fpsText = CreateText(rightGroup.transform, "60 FPS", fontMono, 11, FontStyles.Normal, new Color(146f / 255f, 157f / 255f, 178f / 255f));
            fpsText.textWrappingMode = TextWrappingModes.NoWrap;

            // --- CENTRAL TACTICAL DECK (Console Frame) ---
            var consoleDeck = CreateUIObject("TacticalDeck", canvasGo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(960, 570));
            consoleDeck.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -18);
            var deckImg = consoleDeck.AddComponent<Image>();
            deckImg.sprite = sPanelCyber;
            deckImg.type = Image.Type.Sliced;
            deckImg.color = Color.white;

            // Corner brackets
            AddCornerBracket(consoleDeck.transform, new Vector2(0, 1), new Vector2(10, -10), new Color(0f, 242f / 255f, 254f / 255f));
            AddCornerBracket(consoleDeck.transform, new Vector2(1, 1), new Vector2(-10, -10), new Color(0f, 242f / 255f, 254f / 255f));
            AddCornerBracket(consoleDeck.transform, new Vector2(0, 0), new Vector2(10, 10), new Color(0f, 242f / 255f, 254f / 255f));
            AddCornerBracket(consoleDeck.transform, new Vector2(1, 0), new Vector2(-10, 10), new Color(0f, 242f / 255f, 254f / 255f));

            var screenShake = consoleDeck.AddComponent<ScreenShake>();
            SetPrivateField(screenShake, "_targetTransform", consoleDeck.GetComponent<RectTransform>());

            var vlgDeck = consoleDeck.AddComponent<VerticalLayoutGroup>();
            vlgDeck.padding = new RectOffset(16, 16, 10, 14);
            vlgDeck.spacing = 6;
            vlgDeck.childControlWidth = true;
            vlgDeck.childControlHeight = true;
            vlgDeck.childForceExpandWidth = true;
            vlgDeck.childForceExpandHeight = false;

            // 1. Header Status Deck (Counters + Secondary Bar)
            var headerStatusDeck = CreateUIObject("HeaderStatusDeck", consoleDeck.transform, Vector2.zero, Vector2.zero, new Vector2(0, 52));
            headerStatusDeck.AddComponent<Image>().color = new Color(17f / 255f, 22f / 255f, 34f / 255f, 0.95f);
            var hsdLe = headerStatusDeck.AddComponent<LayoutElement>();
            hsdLe.preferredHeight = 52;
            hsdLe.minHeight = 52;

            var hlgHSD = headerStatusDeck.AddComponent<HorizontalLayoutGroup>();
            hlgHSD.padding = new RectOffset(16, 16, 2, 2);
            hlgHSD.childAlignment = TextAnchor.MiddleCenter;
            hlgHSD.childControlWidth = false;
            hlgHSD.childControlHeight = false;

            // Mine Counter Bay (Left)
            var mineBay = CreateUIObject("MineBay", headerStatusDeck.transform, Vector2.zero, Vector2.zero, new Vector2(120, 48));
            mineBay.AddComponent<Image>().color = new Color(9f / 255f, 13f / 255f, 20f / 255f);
            var vlgMB = mineBay.AddComponent<VerticalLayoutGroup>();
            vlgMB.padding = new RectOffset(4, 4, 2, 2);
            vlgMB.spacing = 0;
            vlgMB.childAlignment = TextAnchor.MiddleCenter;
            vlgMB.childControlWidth = true;
            vlgMB.childControlHeight = true;

            var mineLabel = CreateText(mineBay.transform, "МИНЫ [ОСТ]", fontMono, 9, FontStyles.Bold, new Color(248f / 255f, 113f / 255f, 113f / 255f));
            mineLabel.alignment = TextAlignmentOptions.Center;
            var mineCounterText = CreateText(mineBay.transform, "040", fontMono, 24, FontStyles.Bold, new Color(248f / 255f, 113f / 255f, 113f / 255f));
            mineCounterText.alignment = TextAlignmentOptions.Center;

            // Left Spacer
            var spacerLeft = CreateUIObject("SpacerLeft", headerStatusDeck.transform, Vector2.zero, Vector2.zero, Vector2.zero);
            spacerLeft.AddComponent<LayoutElement>().flexibleWidth = 1f;

            // Center Secondary Action Controls
            var centerDeckControls = CreateUIObject("CenterControls", headerStatusDeck.transform, Vector2.zero, Vector2.zero, new Vector2(360, 48));
            var hlgCDC = centerDeckControls.AddComponent<HorizontalLayoutGroup>();
            hlgCDC.spacing = 8;
            hlgCDC.childAlignment = TextAnchor.MiddleCenter;
            hlgCDC.childControlWidth = false;
            hlgCDC.childControlHeight = false;

            var btnRestartGo = CreateActionButtonWithIcon("BtnRestart", centerDeckControls.transform, "Заново", sIconRestart, 50f, fontMono, sBtnCyber);
            var btnRestart = btnRestartGo.GetComponent<Button>();

            // Center Drone Face Button
            var faceBtnGo = CreateUIObject("DroneFaceButton", centerDeckControls.transform, Vector2.zero, Vector2.zero, new Vector2(42, 42));
            faceBtnGo.AddComponent<Image>().color = new Color(22f / 255f, 28f / 255f, 42f / 255f);
            var faceBtn = faceBtnGo.AddComponent<Button>();

            var faceIconGo = CreateUIObject("FaceIcon", faceBtnGo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(32, 32));
            var faceIconImg = faceIconGo.AddComponent<Image>();
            faceIconImg.sprite = sFaceNormal;
            faceIconImg.raycastTarget = false;

            var sfb = faceBtnGo.AddComponent<StatusFaceButton>();
            SetPrivateField(sfb, "_faceImage", faceIconImg);
            SetPrivateField(sfb, "_button", faceBtn);
            sfb.FaceNormal = sFaceNormal;
            sfb.FaceAlert = sFaceAlert;
            sfb.FaceWin = sFaceWin;
            sfb.FaceDefeat = sFaceDefeat;

            var btnPauseGo = CreateActionButtonWithIcon("BtnPause", centerDeckControls.transform, "Пауза", sIconPause, 44f, fontMono, sBtnCyber, out var pauseText, out var pauseIcon);
            var btnPause = btnPauseGo.GetComponent<Button>();

            var btnHintGo = CreateActionButtonWithIcon("BtnHint", centerDeckControls.transform, "Подсказка", sIconHint, 76f, fontMono, sBtnAccent);
            var btnHint = btnHintGo.GetComponent<Button>();

            // Right Spacer
            var spacerRight = CreateUIObject("SpacerRight", headerStatusDeck.transform, Vector2.zero, Vector2.zero, Vector2.zero);
            spacerRight.AddComponent<LayoutElement>().flexibleWidth = 1f;

            // Timer Counter Bay (Right)
            var timerBay = CreateUIObject("TimerBay", headerStatusDeck.transform, Vector2.zero, Vector2.zero, new Vector2(120, 48));
            timerBay.AddComponent<Image>().color = new Color(9f / 255f, 13f / 255f, 20f / 255f);
            var vlgTB = timerBay.AddComponent<VerticalLayoutGroup>();
            vlgTB.padding = new RectOffset(4, 4, 2, 2);
            vlgTB.spacing = 0;
            vlgTB.childAlignment = TextAnchor.MiddleCenter;
            vlgTB.childControlWidth = true;
            vlgTB.childControlHeight = true;

            var timerLabel = CreateText(timerBay.transform, "ВРЕМЯ [СЕК]", fontMono, 9, FontStyles.Bold, new Color(0f, 242f / 255f, 254f / 255f));
            timerLabel.alignment = TextAlignmentOptions.Center;
            var timerCounterText = CreateText(timerBay.transform, "000", fontMono, 24, FontStyles.Bold, new Color(0f, 242f / 255f, 254f / 255f));
            timerCounterText.alignment = TextAlignmentOptions.Center;

            // 2. Central Minefield Grid Bay
            var gridWrapper = CreateUIObject("GridWrapper", consoleDeck.transform, Vector2.zero, Vector2.zero, new Vector2(0, 440));
            gridWrapper.AddComponent<Image>().color = new Color(9f / 255f, 13f / 255f, 20f / 255f, 0.98f);
            var gwLe = gridWrapper.AddComponent<LayoutElement>();
            gwLe.preferredHeight = 440;
            gwLe.flexibleHeight = 1;

            var gridContainerGo = CreateUIObject("GridContainer", gridWrapper.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(440, 440));
            var gridLayout = gridContainerGo.AddComponent<GridLayoutGroup>();
            gridLayout.childAlignment = TextAnchor.MiddleCenter;

            var boardView = gridContainerGo.AddComponent<BoardGridView>();
            SetPrivateField(boardView, "_gridContainer", gridContainerGo.GetComponent<RectTransform>());
            SetPrivateField(boardView, "_gridLayout", gridLayout);
            SetPrivateField(boardView, "_consoleFrame", consoleDeck.GetComponent<RectTransform>());
            boardView.ConfigureResources(sCellClosed, sCellHover, sCellPressed, sCellRevealed, sCellExploded, sIconFlag, sIconMine, sIconFalseFlag, fontMono);
            boardView.SetConsoleFrame(consoleDeck.GetComponent<RectTransform>());

            // 3. Bottom Deck Status Footer
            var footerDeck = CreateUIObject("FooterDeck", consoleDeck.transform, Vector2.zero, Vector2.zero, new Vector2(0, 28));
            footerDeck.AddComponent<Image>().color = new Color(17f / 255f, 22f / 255f, 34f / 255f, 0.95f);
            var fdLe = footerDeck.AddComponent<LayoutElement>();
            fdLe.preferredHeight = 28;
            fdLe.minHeight = 28;

            var hlgFD = footerDeck.AddComponent<HorizontalLayoutGroup>();
            hlgFD.padding = new RectOffset(16, 16, 0, 0);
            hlgFD.spacing = 16;
            hlgFD.childAlignment = TextAnchor.MiddleCenter;
            hlgFD.childControlWidth = false;
            hlgFD.childControlHeight = true;

            var flagsCountText = CreateText(footerDeck.transform, "Флаги: 0 / 10", fontMono, 11, FontStyles.Normal, new Color(203f / 255f, 213f / 255f, 225f / 255f));
            flagsCountText.textWrappingMode = TextWrappingModes.NoWrap;

            var fs1 = CreateUIObject("Spacer1", footerDeck.transform, Vector2.zero, Vector2.zero, Vector2.zero);
            fs1.AddComponent<LayoutElement>().flexibleWidth = 1f;

            var bottomDiffText = CreateText(footerDeck.transform, "Сложность: Новичок (9×9)", fontMono, 11, FontStyles.Normal, new Color(146f / 255f, 157f / 255f, 178f / 255f));
            bottomDiffText.textWrappingMode = TextWrappingModes.NoWrap;

            var fs2 = CreateUIObject("Spacer2", footerDeck.transform, Vector2.zero, Vector2.zero, Vector2.zero);
            fs2.AddComponent<LayoutElement>().flexibleWidth = 1f;

            var statusPill = CreateUIObject("StatusPill", footerDeck.transform, Vector2.zero, Vector2.zero, new Vector2(130, 24));
            var hlgSP = statusPill.AddComponent<HorizontalLayoutGroup>();
            hlgSP.spacing = 6;
            hlgSP.childAlignment = TextAnchor.MiddleRight;
            hlgSP.childControlWidth = false;
            hlgSP.childControlHeight = true;

            var statusDot = CreatePulseDot(statusPill.transform, new Color(0f, 242f / 255f, 254f / 255f), 6f).GetComponent<Image>();
            var statusLabel = CreateText(statusPill.transform, "[В ПРОЦЕССЕ]", fontMono, 11, FontStyles.Bold, new Color(0f, 242f / 255f, 254f / 255f));
            statusLabel.textWrappingMode = TextWrappingModes.NoWrap;

            // Configure HeaderStatusView
            var headerStatusView = headerStatusDeck.AddComponent<HeaderStatusView>();
            SetPrivateField(headerStatusView, "_mineCounterText", mineCounterText);
            SetPrivateField(headerStatusView, "_timerText", timerCounterText);
            SetPrivateField(headerStatusView, "_btnRestart", btnRestart);
            SetPrivateField(headerStatusView, "_btnPause", btnPause);
            SetPrivateField(headerStatusView, "_btnHint", btnHint);
            SetPrivateField(headerStatusView, "_pauseText", pauseText);
            SetPrivateField(headerStatusView, "_pauseIcon", pauseIcon);
            SetPrivateField(headerStatusView, "_flagsCountText", flagsCountText);
            SetPrivateField(headerStatusView, "_bottomDifficultyText", bottomDiffText);
            SetPrivateField(headerStatusView, "_statusDot", statusDot);
            SetPrivateField(headerStatusView, "_statusLabel", statusLabel);
            headerStatusView.PauseSprite = sIconPause;
            headerStatusView.PlaySprite = sIconPlay;

            // --- MODALS LAYER ---
            var modalsLayer = CreateUIObject("ModalsLayer", canvasGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // 1. Defeat Modal
            var defeatModal = BuildDefeatModal(modalsLayer.transform, fontMono, sPanelDanger, sBtnDanger, sBtnCyber);
            // 2. Victory Modal
            var victoryModal = BuildVictoryModal(modalsLayer.transform, fontMono, sPanelCyber, sBtnCyber);
            // 3. Settings Modal
            var settingsModal = BuildSettingsModal(modalsLayer.transform, fontMono, sPanelCyber, sBtnCyber);
            // 4. Leaderboards Modal
            var leaderboardsModal = BuildLeaderboardsModal(modalsLayer.transform, fontMono, sPanelCyber, sBtnCyber);
            // 5. Help / About Modal
            var helpModal = BuildHelpModal(modalsLayer.transform, fontMono, sPanelCyber, sBtnCyber);

            // Configure TopMenuBar
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
            SetPrivateField(topMenuComp, "_btnSettingsGear", btnSettingsGear);
            SetPrivateField(topMenuComp, "_fpsText", fpsText);
            SetPrivateField(topMenuComp, "_settingsModal", settingsModal);
            SetPrivateField(topMenuComp, "_leaderboardsModal", leaderboardsModal);
            SetPrivateField(topMenuComp, "_helpAboutModal", helpModal);
            topMenuComp.SoundOnSprite = sIconSoundOn;
            topMenuComp.SoundOffSprite = sIconSoundOff;

            // Bind all custom scripts to MonoScript assets
            BindAllCustomScriptsInScene();

            // Save Scene
            string scenePath = "Assets/Scenes/CyberMinesweeper.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"Successfully updated and saved scene to {scenePath}!");

            // Editor Build Settings
            var buildScenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };
            EditorBuildSettings.scenes = buildScenes;
        }

        private static DefeatModal BuildDefeatModal(Transform parent, TMP_FontAsset font, Sprite sPanel, Sprite sBtnDanger, Sprite sBtnCyber)
        {
            var modalRoot = CreateUIObject("DefeatModalRoot", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var backdrop = CreateModalBackdrop("Backdrop", modalRoot.transform);
            var panel = CreateUIObject("Panel", backdrop.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(540, 420));
            var pImg = panel.AddComponent<Image>();
            pImg.sprite = sPanel;
            pImg.type = Image.Type.Sliced;

            var vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(24, 24, 20, 20);
            vlg.spacing = 10;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var badge = CreateText(panel.transform, "[ ОПАСНОСТЬ // НЕОБРАТИМАЯ ДЕТОНАЦИЯ ]", font, 11, FontStyles.Bold, new Color(248f / 255f, 113f / 255f, 113f / 255f));
            badge.alignment = TextAlignmentOptions.Center;
            var leBadge = badge.gameObject.AddComponent<LayoutElement>();
            leBadge.preferredHeight = 18;

            var title = CreateText(panel.transform, "Детонация", font, 24, FontStyles.Bold, new Color(255f / 255f, 220f / 255f, 220f / 255f));
            title.alignment = TextAlignmentOptions.Center;
            var leTitle = title.gameObject.AddComponent<LayoutElement>();
            leTitle.preferredHeight = 32;

            var subtitle = CreateText(panel.transform, "Подрыв мины в секторе (ряд 0, колонка 0)", font, 12, FontStyles.Normal, new Color(248f / 255f, 113f / 255f, 113f / 255f));
            subtitle.alignment = TextAlignmentOptions.Center;
            var leSub = subtitle.gameObject.AddComponent<LayoutElement>();
            leSub.preferredHeight = 20;

            var statsRow = CreateUIObject("StatsRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 60));
            var leStats = statsRow.AddComponent<LayoutElement>();
            leStats.preferredHeight = 60;
            var hlgStats = statsRow.AddComponent<HorizontalLayoutGroup>();
            hlgStats.spacing = 10;
            hlgStats.childAlignment = TextAnchor.MiddleCenter;
            hlgStats.childControlWidth = true;
            hlgStats.childControlHeight = true;

            CreateStatTile(statsRow.transform, "ВРЕМЯ", "00:00", font, out var tTime);
            CreateStatTile(statsRow.transform, "ОТКРЫТО", "0%", font, out var tOpened);
            CreateStatTile(statsRow.transform, "ОСТАЛОСЬ МИН", "0", font, out var tMines);

            var btnRow = CreateUIObject("ButtonRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 38));
            var leBtns = btnRow.AddComponent<LayoutElement>();
            leBtns.preferredHeight = 38;
            var hlg = btnRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            var btnRetry = CreateActionButton(btnRow.transform, "Попробовать снова", font, sBtnDanger);
            var btnInspect = CreateActionButton(btnRow.transform, "Обзор поля", font, sBtnCyber);
            var btnMenu = CreateActionButton(btnRow.transform, "В меню", font, sBtnCyber);

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

        private static VictoryModal BuildVictoryModal(Transform parent, TMP_FontAsset font, Sprite sPanel, Sprite sBtnCyber)
        {
            var modalRoot = CreateUIObject("VictoryModalRoot", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var backdrop = CreateModalBackdrop("Backdrop", modalRoot.transform);
            var panel = CreateUIObject("Panel", backdrop.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(560, 440));
            var pImg = panel.AddComponent<Image>();
            pImg.sprite = sPanel;
            pImg.type = Image.Type.Sliced;

            var vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(24, 24, 20, 20);
            vlg.spacing = 10;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var badge = CreateText(panel.transform, "[ МИССИЯ ВЫПОЛНЕНА // СЕКТОР ЧИСТ ]", font, 11, FontStyles.Bold, new Color(52f / 255f, 211f / 255f, 153f / 255f));
            badge.alignment = TextAlignmentOptions.Center;
            var leBadge = badge.gameObject.AddComponent<LayoutElement>();
            leBadge.preferredHeight = 18;

            var title = CreateText(panel.transform, "Победа!", font, 26, FontStyles.Bold, new Color(224f / 255f, 253f / 255f, 1f));
            title.alignment = TextAlignmentOptions.Center;
            var leTitle = title.gameObject.AddComponent<LayoutElement>();
            leTitle.preferredHeight = 34;

            var subtitle = CreateText(panel.transform, "Все заряды обезврежены, сектор зачищен", font, 12, FontStyles.Normal, new Color(0f, 242f / 255f, 254f / 255f));
            subtitle.alignment = TextAlignmentOptions.Center;
            var leSub = subtitle.gameObject.AddComponent<LayoutElement>();
            leSub.preferredHeight = 20;

            var statsRow = CreateUIObject("StatsRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 60));
            var leStats = statsRow.AddComponent<LayoutElement>();
            leStats.preferredHeight = 60;
            var hlgStats = statsRow.AddComponent<HorizontalLayoutGroup>();
            hlgStats.spacing = 10;
            hlgStats.childAlignment = TextAnchor.MiddleCenter;
            hlgStats.childControlWidth = true;
            hlgStats.childControlHeight = true;

            CreateStatTile(statsRow.transform, "ВРЕМЯ", "00:00", font, out var tTime);
            CreateStatTile(statsRow.transform, "СЛОЖНОСТЬ", "Любитель", font, out var tDiff);
            CreateStatTile(statsRow.transform, "РЕКОРД", "Новый!", font, out var tRecord);

            var inputRow = CreateUIObject("InputRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 36));
            var leInput = inputRow.AddComponent<LayoutElement>();
            leInput.preferredHeight = 36;
            var hlgInput = inputRow.AddComponent<HorizontalLayoutGroup>();
            hlgInput.spacing = 10;
            hlgInput.childAlignment = TextAnchor.MiddleCenter;
            hlgInput.childControlWidth = false;
            hlgInput.childControlHeight = true;

            CreateText(inputRow.transform, "ПОЗЫВНОЙ:", font, 12, FontStyles.Bold, Color.white);
            var inputGo = CreateUIObject("CallsignInput", inputRow.transform, Vector2.zero, Vector2.zero, new Vector2(180, 32));
            inputGo.AddComponent<Image>().color = new Color(24f / 255f, 30f / 255f, 43f / 255f);
            var tmpInput = inputGo.AddComponent<TMP_InputField>();

            var textCompGo = CreateUIObject("Text", inputGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var textComp = textCompGo.AddComponent<TextMeshProUGUI>();
            if (font != null) textComp.font = font;
            textComp.fontSize = 13;
            textComp.color = Color.cyan;
            textComp.alignment = TextAlignmentOptions.MidlineLeft;
            tmpInput.textComponent = textComp;
            tmpInput.text = "CYBER_VIPER";

            var btnSave = CreateActionButton(inputRow.transform, "Сохранить", font, sBtnCyber);
            var saveBtnTxt = btnSave.GetComponentInChildren<TextMeshProUGUI>();

            var btnRow = CreateUIObject("ActionsRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 38));
            var leBtns = btnRow.AddComponent<LayoutElement>();
            leBtns.preferredHeight = 38;
            var hlgBtn = btnRow.AddComponent<HorizontalLayoutGroup>();
            hlgBtn.spacing = 12;
            hlgBtn.childAlignment = TextAnchor.MiddleCenter;
            hlgBtn.childControlWidth = true;
            hlgBtn.childControlHeight = true;

            var btnNew = CreateActionButton(btnRow.transform, "Новая игра", font, sBtnCyber);
            var btnMenu = CreateActionButton(btnRow.transform, "В меню", font, sBtnCyber);

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

        private static SettingsModal BuildSettingsModal(Transform parent, TMP_FontAsset font, Sprite sPanel, Sprite sBtnCyber)
        {
            var modalRoot = CreateUIObject("SettingsModalRoot", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var backdrop = CreateModalBackdrop("Backdrop", modalRoot.transform);
            var panel = CreateUIObject("Panel", backdrop.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(640, 440));
            var pImg = panel.AddComponent<Image>();
            pImg.sprite = sPanel;
            pImg.type = Image.Type.Sliced;

            var vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 16, 16);
            vlg.spacing = 8;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var title = CreateText(panel.transform, "Параметры игры", font, 16, FontStyles.Bold, new Color(224f / 255f, 253f / 255f, 1f));
            title.alignment = TextAlignmentOptions.Center;
            title.rectTransform.sizeDelta = new Vector2(600, 24);
            var leTitle = title.gameObject.AddComponent<LayoutElement>();
            leTitle.preferredHeight = 24;

            // 4 Presets
            var presetsRow = CreateUIObject("PresetsRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 56));
            var lePresets = presetsRow.AddComponent<LayoutElement>();
            lePresets.preferredHeight = 56;
            var hlgPresets = presetsRow.AddComponent<HorizontalLayoutGroup>();
            hlgPresets.spacing = 8;
            hlgPresets.childAlignment = TextAnchor.MiddleCenter;
            hlgPresets.childControlWidth = true;
            hlgPresets.childControlHeight = true;
            hlgPresets.childForceExpandWidth = true;
            hlgPresets.childForceExpandHeight = false;

            var btnBeg = CreatePresetCard(presetsRow.transform, "LVL 1", "Новичок", "9 × 9 • 10 мин", font, sBtnCyber, out var indBeg);
            var btnInt = CreatePresetCard(presetsRow.transform, "LVL 2", "Любитель", "16 × 16 • 40 мин", font, sBtnCyber, out var indInt);
            var btnExp = CreatePresetCard(presetsRow.transform, "LVL 3", "Профессионал", "30 × 16 • 99 мин", font, sBtnCyber, out var indExp);
            var btnCust = CreatePresetCard(presetsRow.transform, "ОСОБЫЙ", "Особый", "Настраиваемый", font, sBtnCyber, out var indCust);

            // Custom steppers
            var customCont = CreateUIObject("CustomContainer", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 88));
            var leCust = customCont.AddComponent<LayoutElement>();
            leCust.preferredHeight = 88;
            var hlgCust = customCont.AddComponent<HorizontalLayoutGroup>();
            hlgCust.spacing = 8;
            hlgCust.childAlignment = TextAnchor.MiddleCenter;
            hlgCust.childControlWidth = true;
            hlgCust.childControlHeight = true;
            hlgCust.childForceExpandWidth = true;
            hlgCust.childForceExpandHeight = false;

            CreateStepperWithSlider(customCont.transform, "Высота (ряды)", "16", 9, 24, font, out var txtH, out var btnHMinus, out var btnHPlus, out var sldH);
            CreateStepperWithSlider(customCont.transform, "Ширина (колонки)", "30", 9, 30, font, out var txtW, out var btnWMinus, out var btnWPlus, out var sldW);
            CreateStepperWithSlider(customCont.transform, "Число мин", "99", 10, 432, font, out var txtM, out var btnMMinus, out var btnMPlus, out var sldM);

            // Summary card
            var summaryCard = CreateUIObject("SummaryCard", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 36));
            var leSum = summaryCard.AddComponent<LayoutElement>();
            leSum.preferredHeight = 36;
            summaryCard.AddComponent<Image>().color = new Color(11f / 255f, 14f / 255f, 22f / 255f);
            var hlgSummary = summaryCard.AddComponent<HorizontalLayoutGroup>();
            hlgSummary.padding = new RectOffset(12, 12, 0, 0);
            hlgSummary.spacing = 10;
            hlgSummary.childAlignment = TextAnchor.MiddleCenter;
            hlgSummary.childControlWidth = false;
            hlgSummary.childControlHeight = true;

            var densityText = CreateText(summaryCard.transform, "Плотность мин: 20.6%", font, 11, FontStyles.Bold, Color.white);
            densityText.rectTransform.sizeDelta = new Vector2(160, 20);
            var densityBadge = CreateText(summaryCard.transform, "[ОПТИМАЛЬНО]", font, 10, FontStyles.Bold, new Color(0f, 242f / 255f, 254f / 255f));
            densityBadge.rectTransform.sizeDelta = new Vector2(110, 20);
            var spacerSC = CreateUIObject("Spacer", summaryCard.transform, Vector2.zero, Vector2.zero, new Vector2(0, 0));
            spacerSC.AddComponent<LayoutElement>().flexibleWidth = 1f;
            var totalCellsText = CreateText(summaryCard.transform, "Всего ячеек: 480 • 1-й клик: 100% безопасен", font, 10, FontStyles.Normal, new Color(146f / 255f, 157f / 255f, 178f / 255f));
            totalCellsText.rectTransform.sizeDelta = new Vector2(280, 20);

            // Audio Box
            var audioBox = CreateUIObject("AudioSettingsBox", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 80));
            var leAudio = audioBox.AddComponent<LayoutElement>();
            leAudio.preferredHeight = 80;
            audioBox.AddComponent<Image>().color = new Color(17f / 255f, 22f / 255f, 34f / 255f, 0.95f);
            var vlgAudio = audioBox.AddComponent<VerticalLayoutGroup>();
            vlgAudio.padding = new RectOffset(14, 14, 6, 6);
            vlgAudio.spacing = 6;
            vlgAudio.childControlWidth = true;
            vlgAudio.childControlHeight = true;
            vlgAudio.childForceExpandHeight = false;

            var volRow = CreateUIObject("VolRow", audioBox.transform, Vector2.zero, Vector2.zero, new Vector2(0, 24));
            var leVR = volRow.AddComponent<LayoutElement>();
            leVR.preferredHeight = 24;
            var hlgVR = volRow.AddComponent<HorizontalLayoutGroup>();
            hlgVR.spacing = 10;
            hlgVR.childAlignment = TextAnchor.MiddleLeft;
            hlgVR.childControlWidth = false;
            hlgVR.childControlHeight = true;
            var volLbl = CreateText(volRow.transform, "ГРОМКОСТЬ ЗВУКА:", font, 10, FontStyles.Bold, Color.white);
            volLbl.rectTransform.sizeDelta = new Vector2(130, 20);
            var slider = CreateCyberSlider("VolumeSlider", volRow.transform, new Vector2(240, 18), 0f, 1f, 0.8f);

            var togglesRow = CreateUIObject("TogglesRow", audioBox.transform, Vector2.zero, Vector2.zero, new Vector2(0, 24));
            var leTR = togglesRow.AddComponent<LayoutElement>();
            leTR.preferredHeight = 24;
            var hlgToggles = togglesRow.AddComponent<HorizontalLayoutGroup>();
            hlgToggles.spacing = 20;
            hlgToggles.childAlignment = TextAnchor.MiddleLeft;
            hlgToggles.childControlWidth = false;
            hlgToggles.childControlHeight = true;

            var muteTgl = CreateToggle(togglesRow.transform, "Режим «Без звука»", font);
            var tickTgl = CreateToggle(togglesRow.transform, "Тиканье таймера", font);

            // Action buttons
            var btnRow = CreateUIObject("ButtonRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 36));
            var leBtns = btnRow.AddComponent<LayoutElement>();
            leBtns.preferredHeight = 36;
            var hlgBtn = btnRow.AddComponent<HorizontalLayoutGroup>();
            hlgBtn.spacing = 12;
            hlgBtn.childAlignment = TextAnchor.MiddleRight;
            hlgBtn.childControlWidth = false;
            hlgBtn.childControlHeight = false;

            var btnCancel = CreateActionButton(btnRow.transform, "Отмена", font, sBtnCyber);
            var btnOk = CreateActionButton(btnRow.transform, "Применить", font, sBtnCyber);

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
            SetPrivateField(sm, "_widthText", txtW);
            SetPrivateField(sm, "_widthMinus", btnWMinus);
            SetPrivateField(sm, "_widthPlus", btnWPlus);
            SetPrivateField(sm, "_widthSlider", sldW);
            SetPrivateField(sm, "_heightText", txtH);
            SetPrivateField(sm, "_heightMinus", btnHMinus);
            SetPrivateField(sm, "_heightPlus", btnHPlus);
            SetPrivateField(sm, "_heightSlider", sldH);
            SetPrivateField(sm, "_minesText", txtM);
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
            SetPrivateField(sm, "_closeButton", btnCancel);
            backdrop.SetActive(false);
            return sm;
        }

        private static LeaderboardsModal BuildLeaderboardsModal(Transform parent, TMP_FontAsset font, Sprite sPanel, Sprite sBtnCyber)
        {
            var modalRoot = CreateUIObject("LeaderboardsModalRoot", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var backdrop = CreateModalBackdrop("Backdrop", modalRoot.transform);
            var panel = CreateUIObject("Panel", backdrop.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(640, 420));
            var pImg = panel.AddComponent<Image>();
            pImg.sprite = sPanel;
            pImg.type = Image.Type.Sliced;

            var vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 16, 16);
            vlg.spacing = 8;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var title = CreateText(panel.transform, "Таблица рекордов", font, 16, FontStyles.Bold, new Color(224f / 255f, 253f / 255f, 1f));
            title.alignment = TextAlignmentOptions.Center;
            title.rectTransform.sizeDelta = new Vector2(600, 24);
            var leTitle = title.gameObject.AddComponent<LayoutElement>();
            leTitle.preferredHeight = 24;

            // Tabs row
            var tabsRow = CreateUIObject("TabsRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 32));
            var leTabs = tabsRow.AddComponent<LayoutElement>();
            leTabs.preferredHeight = 32;
            var hlgTabs = tabsRow.AddComponent<HorizontalLayoutGroup>();
            hlgTabs.spacing = 6;
            hlgTabs.childAlignment = TextAnchor.MiddleCenter;
            hlgTabs.childControlWidth = true;
            hlgTabs.childControlHeight = true;

            var tBeg = CreateNavTab(tabsRow.transform, "Новичок (9×9)", font, out var bgBeg, isPrimary: true);
            var tInt = CreateNavTab(tabsRow.transform, "Любитель (16×16)", font, out var bgInt);
            var tExp = CreateNavTab(tabsRow.transform, "Профессионал (30×16)", font, out var bgExp);
            var tCust = CreateNavTab(tabsRow.transform, "Особый", font, out var bgCust);

            // Table Header
            var headerRow = CreateUIObject("TableHeader", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 26));
            var leHdr = headerRow.AddComponent<LayoutElement>();
            leHdr.preferredHeight = 26;
            headerRow.AddComponent<Image>().color = new Color(14f / 255f, 19f / 255f, 29f / 255f);
            var hlgHdr = headerRow.AddComponent<HorizontalLayoutGroup>();
            hlgHdr.padding = new RectOffset(16, 16, 0, 0);
            hlgHdr.spacing = 10;
            hlgHdr.childAlignment = TextAnchor.MiddleLeft;
            hlgHdr.childControlWidth = false;
            hlgHdr.childControlHeight = true;
            hlgHdr.childForceExpandWidth = false;

            CreateLabel(headerRow.transform, "РАНГ", 50, Color.gray, font);
            CreateLabel(headerRow.transform, "ИГРОК", 270, Color.gray, font);
            CreateLabel(headerRow.transform, "ВРЕМЯ", 100, Color.gray, font);
            CreateLabel(headerRow.transform, "ДАТА", 120, Color.gray, font);

            var rowsCont = CreateUIObject("RowsContainer", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 172));
            var leRows = rowsCont.AddComponent<LayoutElement>();
            leRows.preferredHeight = 172;
            var vlgRows = rowsCont.AddComponent<VerticalLayoutGroup>();
            vlgRows.spacing = 3;
            vlgRows.childControlWidth = true;
            vlgRows.childControlHeight = true;
            vlgRows.childForceExpandHeight = false;

            var noticeGo = CreateUIObject("CustomNotice", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 36));
            var leNotice = noticeGo.AddComponent<LayoutElement>();
            leNotice.preferredHeight = 36;
            var noticeTxt = CreateText(noticeGo.transform, "ОСОБЫЙ РЕЖИМ НЕ УЧИТЫВАЕТСЯ В ТАБЛИЦЕ РЕКОРДОВ (FR-026)", font, 11, FontStyles.Normal, Color.yellow);
            noticeTxt.alignment = TextAlignmentOptions.Center;
            noticeTxt.rectTransform.sizeDelta = new Vector2(600, 30);
            noticeGo.SetActive(false);

            var bestBanner = CreateUIObject("BestBanner", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 28));
            var leBest = bestBanner.AddComponent<LayoutElement>();
            leBest.preferredHeight = 28;
            bestBanner.AddComponent<Image>().color = new Color(17f / 255f, 22f / 255f, 34f / 255f);
            var bestTxt = CreateText(bestBanner.transform, "ВАШ ЛУЧШИЙ РЕЗУЛЬТАТ: 014.2s [РАНГ #1 — CYBER_VIPER]", font, 11, FontStyles.Bold, Color.cyan);
            bestTxt.alignment = TextAlignmentOptions.Center;
            bestTxt.rectTransform.sizeDelta = new Vector2(600, 24);

            var btnRow = CreateUIObject("ButtonRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 36));
            var leBtns = btnRow.AddComponent<LayoutElement>();
            leBtns.preferredHeight = 36;
            var hlgBtn = btnRow.AddComponent<HorizontalLayoutGroup>();
            hlgBtn.spacing = 12;
            hlgBtn.childAlignment = TextAnchor.MiddleCenter;
            hlgBtn.childControlWidth = false;
            hlgBtn.childControlHeight = false;

            var btnReset = CreateActionButton(btnRow.transform, "Сбросить рекорды", font, sBtnCyber);
            var btnClose = CreateActionButton(btnRow.transform, "Закрыть", font, sBtnCyber);

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
            SetPrivateField(lm, "_resetScoresButton", btnReset);
            SetPrivateField(lm, "_closeButton", btnClose);
            SetPrivateField(lm, "_fontAsset", font);
            backdrop.SetActive(false);
            return lm;
        }

        private static HelpAboutModal BuildHelpModal(Transform parent, TMP_FontAsset font, Sprite sPanel, Sprite sBtnCyber)
        {
            var modalRoot = CreateUIObject("HelpAboutModalRoot", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var backdrop = CreateModalBackdrop("Backdrop", modalRoot.transform);
            var panel = CreateUIObject("Panel", backdrop.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(640, 520));
            var pImg = panel.AddComponent<Image>();
            pImg.sprite = sPanel;
            pImg.type = Image.Type.Sliced;

            var vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(24, 24, 18, 18);
            vlg.spacing = 10;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var title = CreateText(panel.transform, "ПРАВИЛА ИГРЫ // СПРАВОЧНЫЙ МОДУЛЬ", font, 15, FontStyles.Bold, new Color(224f / 255f, 253f / 255f, 1f));
            title.alignment = TextAlignmentOptions.Center;
            var leTitle = title.gameObject.AddComponent<LayoutElement>();
            leTitle.preferredHeight = 24;

            var tabsRow = CreateUIObject("TabsRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 32));
            var leTabs = tabsRow.AddComponent<LayoutElement>();
            leTabs.preferredHeight = 32;
            var hlgTabs = tabsRow.AddComponent<HorizontalLayoutGroup>();
            hlgTabs.spacing = 8;
            hlgTabs.childAlignment = TextAnchor.MiddleCenter;
            hlgTabs.childControlWidth = true;
            hlgTabs.childControlHeight = true;

            var tabRules = CreateNavTab(tabsRow.transform, "Правила игры", font, out var bgRules, isPrimary: true);
            var tabAbout = CreateNavTab(tabsRow.transform, "О программе", font, out var bgAbout);

            var rulesCont = CreateUIObject("RulesContainer", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 340));
            var leRules = rulesCont.AddComponent<LayoutElement>();
            leRules.preferredHeight = 340;
            leRules.flexibleHeight = 1;
            var vlgRules = rulesCont.AddComponent<VerticalLayoutGroup>();
            vlgRules.padding = new RectOffset(16, 16, 12, 12);
            vlgRules.spacing = 6;
            vlgRules.childControlWidth = true;
            vlgRules.childControlHeight = false;
            rulesCont.AddComponent<Image>().color = new Color(14f / 255f, 19f / 255f, 29f / 255f, 0.95f);

            CreateText(rulesCont.transform, "• [ЛКМ] Открытие ячейки: первый клик всегда безопасен (зона 3x3).", font, 10.5f, FontStyles.Normal, Color.white);
            CreateText(rulesCont.transform, "• [ПКМ] Установка / снятие флажка: защищает ячейку от подрыва.", font, 10.5f, FontStyles.Normal, Color.white);
            CreateText(rulesCont.transform, "• [СКМ / Одновременно ЛКМ+ПКМ] Аккорд (Chording): быстрое раскрытие соседей.", font, 10.5f, FontStyles.Normal, Color.white);
            CreateText(rulesCont.transform, "• [Кнопка «Подсказка»] Подсвечивает безопасную ячейку со штрафом +5 сек.", font, 10.5f, FontStyles.Normal, Color.yellow);
            CreateText(rulesCont.transform, "• [Space / Кнопка «Пауза»] Приостановка таймера с сокрытием поля.", font, 10.5f, FontStyles.Normal, Color.white);
            CreateText(rulesCont.transform, "• [F2 / Клик по Дрону] Быстрый рестарт партии с сохранением сложности.", font, 10.5f, FontStyles.Normal, Color.white);
            CreateText(rulesCont.transform, "• Цель: открыть все свободные от мин ячейки за минимальное время.", font, 10.5f, FontStyles.Bold, Color.cyan);

            var aboutCont = CreateUIObject("AboutContainer", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 340));
            var leAbout = aboutCont.AddComponent<LayoutElement>();
            leAbout.preferredHeight = 340;
            leAbout.flexibleHeight = 1;
            var vlgAbout = aboutCont.AddComponent<VerticalLayoutGroup>();
            vlgAbout.padding = new RectOffset(16, 16, 12, 12);
            vlgAbout.spacing = 6;
            vlgAbout.childControlWidth = true;
            vlgAbout.childControlHeight = false;
            aboutCont.AddComponent<Image>().color = new Color(14f / 255f, 19f / 255f, 29f / 255f, 0.95f);

            CreateText(aboutCont.transform, "Учебный проект по дисциплине: «Технологии разработки программно-информационных систем»", font, 10.5f, FontStyles.Normal, Color.gray);
            CreateText(aboutCont.transform, "Волгоградский государственный технический университет (ВолгГТУ), 2026 г.", font, 10.5f, FontStyles.Normal, Color.gray);
            CreateText(aboutCont.transform, "Преподаватель: доцент Рак Д.В.", font, 10.5f, FontStyles.Normal, Color.white);
            CreateText(aboutCont.transform, "Команда разработчиков (гр. ИВТ-1):", font, 11f, FontStyles.Bold, Color.cyan);
            CreateText(aboutCont.transform, "  • Тюрин С.С. — Тимлид / Архитектор (Team Lead / Architecture)", font, 10.5f, FontStyles.Normal, Color.white);
            CreateText(aboutCont.transform, "  • Игнатов М.Р. — Frontend-разработчик (Cyberpunk HUD UI)", font, 10.5f, FontStyles.Normal, Color.white);
            CreateText(aboutCont.transform, "  • Сапунков А.Р. — Backend-разработчик (Core / Game Mechanics)", font, 10.5f, FontStyles.Normal, Color.white);
            aboutCont.SetActive(false);

            var btnClose = CreateActionButton(panel.transform, "Закрыть", font, sBtnCyber);
            var leBtn = btnClose.gameObject.AddComponent<LayoutElement>();
            leBtn.preferredHeight = 36;
            leBtn.preferredWidth = 140;

            var hm = modalRoot.AddComponent<HelpAboutModal>();
            SetPrivateField(hm, "_panel", backdrop);
            SetPrivateField(hm, "_rulesContainer", rulesCont);
            SetPrivateField(hm, "_aboutContainer", aboutCont);
            SetPrivateField(hm, "_titleText", title);
            SetPrivateField(hm, "_tabRulesButton", tabRules);
            SetPrivateField(hm, "_tabAboutButton", tabAbout);
            SetPrivateField(hm, "_tabRulesBg", bgRules);
            SetPrivateField(hm, "_tabAboutBg", bgAbout);
            SetPrivateField(hm, "_closeButton", btnClose);
            backdrop.SetActive(false);
            return hm;
        }

        // --- UI FACTORY HELPERS ---

        private static GameObject CreateModalBackdrop(string name, Transform parent)
        {
            var backdrop = CreateUIObject(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var img = backdrop.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.85f);
            return backdrop;
        }

        private static GameObject CreateUIObject(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.sizeDelta = size;
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

        private static TextMeshProUGUI CreateText(Transform parent, string text, TMP_FontAsset font, float size, FontStyles style, Color color)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.raycastTarget = false;
            tmp.rectTransform.sizeDelta = Vector2.zero;
            return tmp;
        }

        private static TextMeshProUGUI CreateLabel(Transform parent, string text, float width, Color color, TMP_FontAsset font)
        {
            var tmp = CreateText(parent, text, font, 11, FontStyles.Bold, color);
            tmp.rectTransform.sizeDelta = new Vector2(width, 24);
            var le = tmp.gameObject.AddComponent<LayoutElement>();
            le.minWidth = width;
            le.preferredWidth = width;
            return tmp;
        }

        private static void AddCornerBracket(Transform parent, Vector2 anchor, Vector2 offset, Color color)
        {
            var corner = CreateUIObject("CornerBracket", parent, anchor, anchor, new Vector2(14, 14));
            corner.GetComponent<RectTransform>().anchoredPosition = offset;
            var le = corner.AddComponent<LayoutElement>();
            le.ignoreLayout = true;
            var hLine = CreateUIObject("H", corner.transform, new Vector2(anchor.x, anchor.y), new Vector2(anchor.x, anchor.y), new Vector2(14, 2));
            hLine.GetComponent<RectTransform>().anchoredPosition = new Vector2(anchor.x == 0 ? 7 : -7, anchor.y == 0 ? 1 : -1);
            hLine.AddComponent<Image>().color = color;
            var vLine = CreateUIObject("V", corner.transform, new Vector2(anchor.x, anchor.y), new Vector2(anchor.x, anchor.y), new Vector2(2, 14));
            vLine.GetComponent<RectTransform>().anchoredPosition = new Vector2(anchor.x == 0 ? 1 : -1, anchor.y == 0 ? 7 : -7);
            vLine.AddComponent<Image>().color = color;
        }

        private static GameObject CreatePulseDot(Transform parent, Color color, float size = 6f)
        {
            var dot = CreateUIObject("PulseDot", parent, Vector2.zero, Vector2.zero, new Vector2(size, size));
            dot.AddComponent<Image>().color = color;
            dot.AddComponent<CyberPulseDot>();
            return dot;
        }

        private static Button CreateNavTab(Transform parent, string text, TMP_FontAsset font, out Image bgImage, bool isPrimary = false)
        {
            var go = CreateUIObject("Tab_" + text, parent, Vector2.zero, Vector2.zero, new Vector2(isPrimary ? 76 : 86, 30));
            bgImage = go.AddComponent<Image>();
            bgImage.color = isPrimary ? new Color(0f, 242f / 255f, 254f / 255f, 0.25f) : new Color(24f / 255f, 30f / 255f, 43f / 255f, 0f);

            var btn = go.AddComponent<Button>();
            var txt = CreateText(go.transform, text, font, 11, FontStyles.Bold, isPrimary ? new Color(224f / 255f, 253f / 255f, 1f) : new Color(146f / 255f, 157f / 255f, 178f / 255f));
            txt.alignment = TextAlignmentOptions.Center;
            txt.textWrappingMode = TextWrappingModes.NoWrap;
            var rt = txt.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            return btn;
        }

        private static GameObject CreateIconButton(string name, Transform parent, Sprite icon, out Image iconImg)
        {
            var go = CreateUIObject(name, parent, Vector2.zero, Vector2.zero, new Vector2(30, 30));
            var img = go.AddComponent<Image>();
            img.color = new Color(24f / 255f, 30f / 255f, 43f / 255f);
            var btn = go.AddComponent<Button>();

            var iconGo = CreateUIObject("Icon", go.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(16, 16));
            iconImg = iconGo.AddComponent<Image>();
            iconImg.sprite = icon;
            iconImg.color = new Color(146f / 255f, 157f / 255f, 178f / 255f);
            iconImg.raycastTarget = false;
            return go;
        }

        private static GameObject CreateActionButtonWithIcon(string name, Transform parent, string label, Sprite icon, float textWidth, TMP_FontAsset font, Sprite btnSprite, out TextMeshProUGUI outText, out Image outIcon)
        {
            float totalWidth = 16f + 8f + textWidth + 14f; // icon + spacing + text + padding
            var go = CreateUIObject(name, parent, Vector2.zero, Vector2.zero, new Vector2(totalWidth, 34));
            var img = go.AddComponent<Image>();
            img.sprite = btnSprite;
            img.type = Image.Type.Sliced;
            go.AddComponent<Button>();

            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(8, 8, 0, 0);
            hlg.spacing = 6;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            if (icon != null)
            {
                var iconGo = CreateUIObject("Icon", go.transform, Vector2.zero, Vector2.zero, new Vector2(16, 16));
                outIcon = iconGo.AddComponent<Image>();
                outIcon.sprite = icon;
                outIcon.color = Color.white;
            }
            else
            {
                outIcon = null;
            }

            outText = CreateText(go.transform, label, font, 11, FontStyles.Bold, Color.white);
            outText.textWrappingMode = TextWrappingModes.NoWrap;
            outText.alignment = TextAlignmentOptions.MidlineLeft;
            outText.GetComponent<RectTransform>().sizeDelta = new Vector2(textWidth, 22);

            return go;
        }

        private static GameObject CreateActionButtonWithIcon(string name, Transform parent, string label, Sprite icon, float textWidth, TMP_FontAsset font, Sprite btnSprite)
        {
            return CreateActionButtonWithIcon(name, parent, label, icon, textWidth, font, btnSprite, out _, out _);
        }

        private static Button CreateActionButton(Transform parent, string text, TMP_FontAsset font, Sprite sprite)
        {
            var go = CreateUIObject("Btn_" + text, parent, Vector2.zero, Vector2.zero, new Vector2(110, 34));
            var img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            var btn = go.AddComponent<Button>();

            var txt = CreateText(go.transform, text, font, 11, FontStyles.Bold, Color.white);
            txt.alignment = TextAlignmentOptions.Center;
            var rt = txt.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            return btn;
        }

        private static Button CreatePresetCard(Transform parent, string tag, string title, string subtitle, TMP_FontAsset font, Sprite sBtn, out Image indicator)
        {
            var card = CreateUIObject("Card_" + title, parent, Vector2.zero, Vector2.zero, new Vector2(144, 56));
            var leCard = card.AddComponent<LayoutElement>();
            leCard.minWidth = 120;
            leCard.preferredWidth = 144;
            leCard.preferredHeight = 56;
            leCard.flexibleWidth = 1f;

            var img = card.AddComponent<Image>();
            img.sprite = sBtn;
            img.type = Image.Type.Sliced;
            var btn = card.AddComponent<Button>();

            var vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(8, 8, 5, 5);
            vlg.spacing = 2;
            vlg.childAlignment = TextAnchor.MiddleLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var topRow = CreateUIObject("TopRow", card.transform, Vector2.zero, Vector2.zero, new Vector2(0, 14));
            var leTR = topRow.AddComponent<LayoutElement>();
            leTR.preferredHeight = 14;
            var hlgTR = topRow.AddComponent<HorizontalLayoutGroup>();
            hlgTR.childAlignment = TextAnchor.MiddleRight;
            hlgTR.childControlWidth = false;
            hlgTR.childControlHeight = false;
            hlgTR.childForceExpandWidth = false;
            hlgTR.childForceExpandHeight = false;

            var tagTxt = CreateText(topRow.transform, tag, font, 9, FontStyles.Bold, new Color(146f / 255f, 157f / 255f, 178f / 255f));
            tagTxt.textWrappingMode = TextWrappingModes.NoWrap;
            tagTxt.rectTransform.sizeDelta = new Vector2(40, 14);

            var trSpacer = CreateUIObject("Spacer", topRow.transform, Vector2.zero, Vector2.zero, new Vector2(0, 0));
            trSpacer.AddComponent<LayoutElement>().flexibleWidth = 1f;

            var indGo = CreateUIObject("Indicator", topRow.transform, Vector2.zero, Vector2.zero, new Vector2(8, 8));
            var leInd = indGo.AddComponent<LayoutElement>();
            leInd.minWidth = 8;
            leInd.minHeight = 8;
            leInd.preferredWidth = 8;
            leInd.preferredHeight = 8;
            indicator = indGo.AddComponent<Image>();
            indicator.color = new Color(1f, 1f, 1f, 0.2f);

            var tTitle = CreateText(card.transform, title, font, 11, FontStyles.Bold, Color.white);
            tTitle.textWrappingMode = TextWrappingModes.NoWrap;
            tTitle.rectTransform.sizeDelta = new Vector2(120, 16);
            var leTitle = tTitle.gameObject.AddComponent<LayoutElement>();
            leTitle.preferredHeight = 16;

            var tSub = CreateText(card.transform, subtitle, font, 9, FontStyles.Normal, new Color(146f / 255f, 157f / 255f, 178f / 255f));
            tSub.textWrappingMode = TextWrappingModes.NoWrap;
            tSub.rectTransform.sizeDelta = new Vector2(120, 14);
            var leSub = tSub.gameObject.AddComponent<LayoutElement>();
            leSub.preferredHeight = 14;

            return btn;
        }

        private static GameObject CreateStatTile(Transform parent, string title, string val, TMP_FontAsset font, out TextMeshProUGUI valText)
        {
            var tile = CreateUIObject("Tile_" + title, parent, Vector2.zero, Vector2.zero, new Vector2(130, 60));
            tile.AddComponent<Image>().color = new Color(11f / 255f, 14f / 255f, 22f / 255f);
            var vlg = tile.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(6, 6, 6, 6);
            vlg.spacing = 2;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var tTitle = CreateText(tile.transform, title, font, 9, FontStyles.Bold, new Color(146f / 255f, 157f / 255f, 178f / 255f));
            tTitle.alignment = TextAlignmentOptions.Center;
            tTitle.textWrappingMode = TextWrappingModes.NoWrap;
            var leTitle = tTitle.gameObject.AddComponent<LayoutElement>();
            leTitle.preferredHeight = 16f;

            valText = CreateText(tile.transform, val, font, 16, FontStyles.Bold, new Color(0f, 242f / 255f, 254f / 255f));
            valText.alignment = TextAlignmentOptions.Center;
            valText.textWrappingMode = TextWrappingModes.NoWrap;
            var leVal = valText.gameObject.AddComponent<LayoutElement>();
            leVal.preferredHeight = 26f;

            return tile;
        }

        private static Slider CreateCyberSlider(string name, Transform parent, Vector2 size, float min, float max, float val)
        {
            var sliderGo = CreateUIObject(name, parent, Vector2.zero, Vector2.zero, size);
            var le = sliderGo.AddComponent<LayoutElement>();
            le.preferredWidth = size.x;
            le.preferredHeight = size.y;

            // Background track
            var bgGo = CreateUIObject("Background", sliderGo.transform, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0, 6));
            bgGo.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(26f / 255f, 36f / 255f, 52f / 255f);

            // Fill Area
            var fillArea = CreateUIObject("Fill Area", sliderGo.transform, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(-12, 6));
            fillArea.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            var fill = CreateUIObject("Fill", fillArea.transform, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0));
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0f, 242f / 255f, 254f / 255f);

            // Handle Slide Area
            var handleArea = CreateUIObject("Handle Slide Area", sliderGo.transform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(-12, 0));
            var handle = CreateUIObject("Handle", handleArea.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(12, 12));
            var handleImg = handle.AddComponent<Image>();
            handleImg.color = Color.white;

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

        private static GameObject CreateStepperWithSlider(Transform parent, string label, string val, int min, int max, TMP_FontAsset font, out TextMeshProUGUI valText, out Button btnMinus, out Button btnPlus, out Slider slider)
        {
            var box = CreateUIObject("Stepper_" + label, parent, Vector2.zero, Vector2.zero, new Vector2(194, 88));
            var leBox = box.AddComponent<LayoutElement>();
            leBox.minWidth = 160;
            leBox.preferredWidth = 194;
            leBox.preferredHeight = 88;
            leBox.flexibleWidth = 1f;

            box.AddComponent<Image>().color = new Color(11f / 255f, 14f / 255f, 22f / 255f);
            var vlg = box.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(8, 8, 6, 6);
            vlg.spacing = 4;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var topRow = CreateUIObject("TopRow", box.transform, Vector2.zero, Vector2.zero, new Vector2(0, 16));
            var leTR = topRow.AddComponent<LayoutElement>();
            leTR.preferredHeight = 16;
            var hlgTR = topRow.AddComponent<HorizontalLayoutGroup>();
            hlgTR.childAlignment = TextAnchor.MiddleLeft;
            hlgTR.childControlWidth = false;
            hlgTR.childControlHeight = false;
            hlgTR.childForceExpandWidth = false;
            hlgTR.childForceExpandHeight = false;

            var lbl = CreateText(topRow.transform, label, font, 10, FontStyles.Bold, Color.white);
            lbl.textWrappingMode = TextWrappingModes.NoWrap;
            lbl.rectTransform.sizeDelta = new Vector2(110, 16);

            var trSpacer = CreateUIObject("Spacer", topRow.transform, Vector2.zero, Vector2.zero, new Vector2(0, 0));
            trSpacer.AddComponent<LayoutElement>().flexibleWidth = 1f;

            var rangeLbl = CreateText(topRow.transform, $"{min}—{max}", font, 9, FontStyles.Normal, Color.gray);
            rangeLbl.textWrappingMode = TextWrappingModes.NoWrap;
            rangeLbl.rectTransform.sizeDelta = new Vector2(50, 16);

            var stepRow = CreateUIObject("StepRow", box.transform, Vector2.zero, Vector2.zero, new Vector2(0, 32));
            var leSR = stepRow.AddComponent<LayoutElement>();
            leSR.preferredHeight = 32;
            stepRow.AddComponent<Image>().color = new Color(6f / 255f, 9f / 255f, 16f / 255f);
            var hlgSR = stepRow.AddComponent<HorizontalLayoutGroup>();
            hlgSR.padding = new RectOffset(6, 6, 3, 3);
            hlgSR.childAlignment = TextAnchor.MiddleCenter;
            hlgSR.childControlWidth = false;
            hlgSR.childControlHeight = false;
            hlgSR.childForceExpandWidth = false;
            hlgSR.childForceExpandHeight = false;

            var minusGo = CreateUIObject("BtnMinus", stepRow.transform, Vector2.zero, Vector2.zero, new Vector2(28, 26));
            var leM = minusGo.AddComponent<LayoutElement>();
            leM.minWidth = 28; leM.preferredWidth = 28;
            leM.minHeight = 26; leM.preferredHeight = 26;
            minusGo.AddComponent<Image>().color = new Color(22f / 255f, 29f / 255f, 43f / 255f);
            btnMinus = minusGo.AddComponent<Button>();
            var txtMinus = CreateText(minusGo.transform, "—", font, 11, FontStyles.Bold, Color.white);
            txtMinus.alignment = TextAlignmentOptions.Center;
            txtMinus.rectTransform.sizeDelta = new Vector2(28, 26);

            var midSpacer = CreateUIObject("Spacer", stepRow.transform, Vector2.zero, Vector2.zero, new Vector2(0, 0));
            midSpacer.AddComponent<LayoutElement>().flexibleWidth = 1f;

            valText = CreateText(stepRow.transform, val, font, 16, FontStyles.Bold, new Color(0f, 242f / 255f, 254f / 255f));
            valText.alignment = TextAlignmentOptions.Center;
            valText.rectTransform.sizeDelta = new Vector2(50, 26);
            var leVal = valText.gameObject.AddComponent<LayoutElement>();
            leVal.preferredWidth = 50; leVal.preferredHeight = 26;

            var midSpacer2 = CreateUIObject("Spacer2", stepRow.transform, Vector2.zero, Vector2.zero, new Vector2(0, 0));
            midSpacer2.AddComponent<LayoutElement>().flexibleWidth = 1f;

            var plusGo = CreateUIObject("BtnPlus", stepRow.transform, Vector2.zero, Vector2.zero, new Vector2(28, 26));
            var leP = plusGo.AddComponent<LayoutElement>();
            leP.minWidth = 28; leP.preferredWidth = 28;
            leP.minHeight = 26; leP.preferredHeight = 26;
            plusGo.AddComponent<Image>().color = new Color(22f / 255f, 29f / 255f, 43f / 255f);
            btnPlus = plusGo.AddComponent<Button>();
            var txtPlus = CreateText(plusGo.transform, "+", font, 13, FontStyles.Bold, Color.white);
            txtPlus.alignment = TextAlignmentOptions.Center;
            txtPlus.rectTransform.sizeDelta = new Vector2(28, 26);

            int.TryParse(val, out int parsedVal);
            slider = CreateCyberSlider("Slider", box.transform, new Vector2(178, 14), min, max, parsedVal);
            slider.wholeNumbers = true;

            return box;
        }

        private static Toggle CreateToggle(Transform parent, string label, TMP_FontAsset font)
        {
            var go = CreateUIObject("Toggle_" + label, parent, Vector2.zero, Vector2.zero, new Vector2(180, 24));
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = 180;
            le.preferredHeight = 24;

            var tgl = go.AddComponent<Toggle>();
            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            var box = CreateUIObject("Box", go.transform, Vector2.zero, Vector2.zero, new Vector2(18, 18));
            var leBox = box.AddComponent<LayoutElement>();
            leBox.preferredWidth = 18; leBox.preferredHeight = 18;
            var boxImg = box.AddComponent<Image>();
            boxImg.color = new Color(22f / 255f, 29f / 255f, 43f / 255f);

            var check = CreateUIObject("Check", box.transform, new Vector2(0.2f, 0.2f), new Vector2(0.8f, 0.8f), Vector2.zero, Vector2.zero);
            var checkImg = check.AddComponent<Image>();
            checkImg.color = new Color(0f, 242f / 255f, 254f / 255f);
            tgl.graphic = checkImg;
            tgl.targetGraphic = boxImg;

            var txt = CreateText(go.transform, label, font, 11, FontStyles.Normal, Color.white);
            txt.rectTransform.sizeDelta = new Vector2(150, 20);
            return tgl;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (field != null)
            {
                field.SetValue(target, value);
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
