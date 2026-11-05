using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
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
            Debug.Log("Starting Cyber Minesweeper Scene generation...");
            Directory.CreateDirectory("Assets/Prefabs");
            Directory.CreateDirectory("Assets/Scenes");

            // 1. Load Font & Sprites
            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if (fontAsset == null)
            {
                Debug.LogError("Could not find LiberationSans SDF font asset!");
            }

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

            // 2. Build Cell Prefab
            var cellObj = new GameObject("CellView", typeof(RectTransform), typeof(Image), typeof(CellView));
            var cellRt = cellObj.GetComponent<RectTransform>();
            cellRt.sizeDelta = new Vector2(40, 40);

            var cellImg = cellObj.GetComponent<Image>();
            cellImg.sprite = sCellClosed;
            cellImg.type = Image.Type.Simple;

            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(cellObj.transform, false);
            var iconRt = iconGo.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.15f, 0.15f);
            iconRt.anchorMax = new Vector2(0.85f, 0.85f);
            iconRt.offsetMin = Vector2.zero;
            iconRt.offsetMax = Vector2.zero;
            var iconImg = iconGo.GetComponent<Image>();
            iconImg.raycastTarget = false;
            iconGo.SetActive(false);

            var numGo = new GameObject("NumberText", typeof(RectTransform), typeof(TextMeshProUGUI));
            numGo.transform.SetParent(cellObj.transform, false);
            var numRt = numGo.GetComponent<RectTransform>();
            numRt.anchorMin = Vector2.zero;
            numRt.anchorMax = Vector2.one;
            numRt.offsetMin = Vector2.zero;
            numRt.offsetMax = Vector2.zero;
            var numTmp = numGo.GetComponent<TextMeshProUGUI>();
            if (fontAsset != null) numTmp.font = fontAsset;
            numTmp.fontSize = 20;
            numTmp.fontStyle = FontStyles.Bold;
            numTmp.alignment = TextAlignmentOptions.Center;
            numTmp.raycastTarget = false;
            numTmp.text = "";

            var cv = cellObj.GetComponent<CellView>();
            SetPrivateField(cv, "_bgImage", cellImg);
            SetPrivateField(cv, "_iconImage", iconImg);
            SetPrivateField(cv, "_numberText", numTmp);
            cv.ClosedSprite = sCellClosed;
            cv.HoverSprite = sCellHover;
            cv.PressedSprite = sCellPressed;
            cv.RevealedSprite = sCellRevealed;
            cv.ExplodedSprite = sCellExploded;
            cv.FlagSprite = sIconFlag;
            cv.MineSprite = sIconMine;
            cv.FalseFlagSprite = sIconFalseFlag;

            string prefabPath = "Assets/Prefabs/CellViewPrefab.prefab";
            var cellPrefab = PrefabUtility.SaveAsPrefabAsset(cellObj, prefabPath);
            UnityEngine.Object.DestroyImmediate(cellObj);
            Debug.Log("Created CellViewPrefab!");

            // 3. Create Scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera
            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(11f / 255f, 15f / 255f, 25f / 255f); // #0B0F19
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
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // Event System
            var esGo = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));

            // Main Background Panel
            var bgGo = CreateUIObject("Background", canvasGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(11f / 255f, 15f / 255f, 25f / 255f); // #0B0F19

            // --- TOP NAVIGATION BAR ---
            var topBar = CreateUIObject("TopMenuBar", canvasGo.transform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 50));
            topBar.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
            topBar.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            var topBarImg = topBar.AddComponent<Image>();
            topBarImg.color = new Color(10f / 255f, 14f / 255f, 24f / 255f, 0.95f); // #0A0E18

            // TopBar Left Container
            var topBarLeft = CreateUIObject("LeftContainer", topBar.transform, new Vector2(0, 0), new Vector2(0.6f, 1), Vector2.zero);
            var hlgLeft = topBarLeft.AddComponent<HorizontalLayoutGroup>();
            hlgLeft.padding = new RectOffset(20, 10, 8, 8);
            hlgLeft.spacing = 15;
            hlgLeft.childAlignment = TextAnchor.MiddleLeft;
            hlgLeft.childControlWidth = false;
            hlgLeft.childControlHeight = false;

            // Green dot indicator
            var dotGo = CreateUIObject("DotIndicator", topBarLeft.transform, Vector2.zero, Vector2.zero, new Vector2(10, 10));
            var dotImg = dotGo.AddComponent<Image>();
            dotImg.color = new Color(0f, 1f, 102f / 255f);

            // Title
            var titleText = CreateText(topBarLeft.transform, "САПЁР // CYBER-SWEEPER HUD V2.0", fontAsset, 15, FontStyles.Bold, new Color(224f / 255f, 253f / 255f, 1f));

            // System online badge
            var badgeText = CreateText(topBarLeft.transform, "[ SYSTEM ONLINE ]", fontAsset, 11, FontStyles.Bold, new Color(0f, 242f / 255f, 254f / 255f));

            // Top Menu Buttons
            var btnNewGame = CreateMenuButton(topBarLeft.transform, "ИГРА", fontAsset, sBtnCyber);
            var btnRecords = CreateMenuButton(topBarLeft.transform, "РЕКОРДЫ", fontAsset, sBtnCyber);
            var btnSettings = CreateMenuButton(topBarLeft.transform, "ПАРАМЕТРЫ", fontAsset, sBtnCyber);
            var btnRules = CreateMenuButton(topBarLeft.transform, "СПРАВКА", fontAsset, sBtnCyber);
            var btnAbout = CreateMenuButton(topBarLeft.transform, "О ПРОГРАММЕ", fontAsset, sBtnCyber);

            // TopBar Right Container
            var topBarRight = CreateUIObject("RightContainer", topBar.transform, new Vector2(0.6f, 0), new Vector2(1, 1), Vector2.zero);
            var hlgRight = topBarRight.AddComponent<HorizontalLayoutGroup>();
            hlgRight.padding = new RectOffset(10, 20, 8, 8);
            hlgRight.spacing = 15;
            hlgRight.childAlignment = TextAnchor.MiddleRight;
            hlgRight.childControlWidth = false;
            hlgRight.childControlHeight = false;

            var telemetryTop = CreateText(topBarRight.transform, "SEC: 0x7F-88X12 SYNCED", fontAsset, 12, FontStyles.Normal, new Color(132f / 255f, 148f / 255f, 149f / 255f));

            var btnMin = CreateSmallSquareButton(topBarRight.transform, "—", fontAsset);
            var btnMax = CreateSmallSquareButton(topBarRight.transform, "□", fontAsset);
            var btnClose = CreateSmallSquareButton(topBarRight.transform, "✕", fontAsset, new Color(255f / 255f, 0f, 85f / 255f));

            // --- TACTICAL CONSOLE FRAME ---
            var consoleFrame = CreateUIObject("TacticalConsoleFrame", canvasGo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1040, 840));
            consoleFrame.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -15);
            var cfImg = consoleFrame.AddComponent<Image>();
            cfImg.sprite = sPanelCyber;
            cfImg.type = Image.Type.Sliced;
            cfImg.color = Color.white;

            var screenShake = consoleFrame.AddComponent<ScreenShake>();
            SetPrivateField(screenShake, "_targetTransform", consoleFrame.GetComponent<RectTransform>());

            var vlgConsole = consoleFrame.AddComponent<VerticalLayoutGroup>();
            vlgConsole.padding = new RectOffset(18, 18, 16, 16);
            vlgConsole.spacing = 12;
            vlgConsole.childControlWidth = true;
            vlgConsole.childControlHeight = false;

            // 1. Header Telemetry Strip
            var telemStrip = CreateUIObject("HeaderTelemetryStrip", consoleFrame.transform, Vector2.zero, Vector2.zero, new Vector2(0, 36));
            var telemImg = telemStrip.AddComponent<Image>();
            telemImg.color = new Color(23f / 255f, 27f / 255f, 38f / 255f, 0.9f);
            var hlgTelem = telemStrip.AddComponent<HorizontalLayoutGroup>();
            hlgTelem.padding = new RectOffset(12, 12, 4, 4);
            hlgTelem.spacing = 15;
            hlgTelem.childAlignment = TextAnchor.MiddleCenter;

            var radarTitle = CreateText(telemStrip.transform, "DEFUSAL // MATRIX-09", fontAsset, 14, FontStyles.Bold, new Color(224f / 255f, 253f / 255f, 1f));
            var gridDensityText = CreateText(telemStrip.transform, "GRID: 9x9 [LIGHT]", fontAsset, 12, FontStyles.Normal, new Color(0f, 220f / 255f, 230f / 255f));
            var chanceText = CreateText(telemStrip.transform, "CHANCE: 87.7% CLEAR", fontAsset, 12, FontStyles.Normal, new Color(185f / 255f, 202f / 255f, 203f / 255f));
            var memText = CreateText(telemStrip.transform, "MEM: 128TB LIVE", fontAsset, 12, FontStyles.Normal, new Color(132f / 255f, 148f / 255f, 149f / 255f));

            // 2. Top Status Bar (Counters & Drone AI Core Face)
            var statusBar = CreateUIObject("TopStatusBar", consoleFrame.transform, Vector2.zero, Vector2.zero, new Vector2(0, 78));
            var sbImg = statusBar.AddComponent<Image>();
            sbImg.color = new Color(38f / 255f, 42f / 255f, 53f / 255f, 0.9f);
            var hlgStatus = statusBar.AddComponent<HorizontalLayoutGroup>();
            hlgStatus.padding = new RectOffset(20, 20, 6, 6);
            hlgStatus.spacing = 20;
            hlgStatus.childAlignment = TextAnchor.MiddleCenter;
            hlgStatus.childControlWidth = false;
            hlgStatus.childControlHeight = false;

            // Mine Counter Bay (Left)
            var mineBay = CreateUIObject("MineCounterBay", statusBar.transform, Vector2.zero, Vector2.zero, new Vector2(170, 64));
            var mbImg = mineBay.AddComponent<Image>();
            mbImg.color = new Color(10f / 255f, 14f / 255f, 24f / 255f, 0.95f);
            var vlgMine = mineBay.AddComponent<VerticalLayoutGroup>();
            vlgMine.padding = new RectOffset(10, 10, 4, 4);
            vlgMine.childAlignment = TextAnchor.MiddleCenter;
            var mineLabel = CreateText(mineBay.transform, "MINES [REM]", fontAsset, 10, FontStyles.Bold, new Color(255f / 255f, 180f / 255f, 171f / 255f));
            var mineCounterText = CreateText(mineBay.transform, "010", fontAsset, 32, FontStyles.Bold, new Color(255f / 255f, 75f / 255f, 75f / 255f));

            // Center Drone AI Face Button
            var faceBtnGo = CreateUIObject("DroneFaceButton", statusBar.transform, Vector2.zero, Vector2.zero, new Vector2(64, 64));
            var faceBtnImg = faceBtnGo.AddComponent<Image>();
            faceBtnImg.color = new Color(15f / 255f, 19f / 255f, 29f / 255f);
            var faceBtn = faceBtnGo.AddComponent<Button>();

            var faceIconGo = CreateUIObject("FaceIcon", faceBtnGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
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

            // Timer Bay (Right)
            var timerBay = CreateUIObject("TimerBay", statusBar.transform, Vector2.zero, Vector2.zero, new Vector2(170, 64));
            var tbImg = timerBay.AddComponent<Image>();
            tbImg.color = new Color(10f / 255f, 14f / 255f, 24f / 255f, 0.95f);
            var vlgTimer = timerBay.AddComponent<VerticalLayoutGroup>();
            vlgTimer.padding = new RectOffset(10, 10, 4, 4);
            vlgTimer.childAlignment = TextAnchor.MiddleCenter;
            var timerLabel = CreateText(timerBay.transform, "TIMER [SEC]", fontAsset, 10, FontStyles.Bold, new Color(111f / 255f, 246f / 255f, 255f / 255f));
            var timerCounterText = CreateText(timerBay.transform, "000", fontAsset, 32, FontStyles.Bold, new Color(0f, 242f / 255f, 254f / 255f));

            var headerView = statusBar.AddComponent<HeaderStatusView>();
            SetPrivateField(headerView, "_mineCounterText", mineCounterText);
            SetPrivateField(headerView, "_timerText", timerCounterText);
            SetPrivateField(headerView, "_gridDensityText", gridDensityText);
            SetPrivateField(headerView, "_chanceText", chanceText);

            // 3. Central Minefield Grid
            var gridWrapper = CreateUIObject("GridWrapper", consoleFrame.transform, Vector2.zero, Vector2.zero, new Vector2(0, 580));
            var gridWrapperLe = gridWrapper.AddComponent<LayoutElement>();
            gridWrapperLe.preferredHeight = 580;
            gridWrapperLe.flexibleHeight = 1;

            var gridContainerGo = CreateUIObject("GridContainer", gridWrapper.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(500, 500));
            var gridLayout = gridContainerGo.AddComponent<GridLayoutGroup>();
            gridLayout.childAlignment = TextAnchor.MiddleCenter;

            var boardView = gridContainerGo.AddComponent<BoardGridView>();
            SetPrivateField(boardView, "_gridContainer", gridContainerGo.GetComponent<RectTransform>());
            SetPrivateField(boardView, "_gridLayout", gridLayout);
            boardView.SetCellPrefab(cellPrefab);

            // 4. Footer Telemetry Strip
            var footerStrip = CreateUIObject("FooterTelemetryStrip", consoleFrame.transform, Vector2.zero, Vector2.zero, new Vector2(0, 32));
            var hlgFooter = footerStrip.AddComponent<HorizontalLayoutGroup>();
            hlgFooter.padding = new RectOffset(10, 10, 4, 4);
            hlgFooter.spacing = 20;
            hlgFooter.childAlignment = TextAnchor.MiddleCenter;

            var footerStatusText = CreateText(footerStrip.transform, "СЕКТОР: РАЗМИНИРОВАНИЕ // ОПЕРАЦИЯ АКТИВНА", fontAsset, 11, FontStyles.Bold, new Color(0f, 242f / 255f, 254f / 255f));
            var footerHintsText = CreateText(footerStrip.transform, "[ЛКМ] ВСКРЫТИЕ  |  [ПКМ] МАРКЕР  |  [СКМ] АККОРД  |  [F2] РЕСТАРТ  |  [ESC] ПАУЗА", fontAsset, 11, FontStyles.Normal, new Color(132f / 255f, 148f / 255f, 149f / 255f));

            // --- MODALS LAYER ---
            var modalsLayer = CreateUIObject("ModalsLayer", canvasGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // 1. Defeat Modal
            var defeatModal = BuildDefeatModal(modalsLayer.transform, fontAsset, sPanelDanger, sBtnDanger, sBtnCyber);
            // 2. Victory Modal
            var victoryModal = BuildVictoryModal(modalsLayer.transform, fontAsset, sPanelCyber, sBtnCyber);
            // 3. Settings Modal
            var settingsModal = BuildSettingsModal(modalsLayer.transform, fontAsset, sPanelCyber, sBtnCyber);
            // 4. Leaderboards Modal
            var leaderboardsModal = BuildLeaderboardsModal(modalsLayer.transform, fontAsset, sPanelCyber, sBtnCyber);
            // 5. Help / About Modal
            var helpModal = BuildHelpModal(modalsLayer.transform, fontAsset, sPanelCyber, sBtnCyber);

            // Wire TopMenuBar
            var topMenuComp = topBar.AddComponent<TopMenuBar>();
            SetPrivateField(topMenuComp, "_btnNewGame", btnNewGame);
            SetPrivateField(topMenuComp, "_btnRecords", btnRecords);
            SetPrivateField(topMenuComp, "_btnSettings", btnSettings);
            SetPrivateField(topMenuComp, "_btnHelpRules", btnRules);
            SetPrivateField(topMenuComp, "_btnHelpAbout", btnAbout);
            SetPrivateField(topMenuComp, "_btnMinimize", btnMin);
            SetPrivateField(topMenuComp, "_btnMaximize", btnMax);
            SetPrivateField(topMenuComp, "_btnClose", btnClose);
            SetPrivateField(topMenuComp, "_settingsModal", settingsModal);
            SetPrivateField(topMenuComp, "_leaderboardsModal", leaderboardsModal);
            SetPrivateField(topMenuComp, "_helpAboutModal", helpModal);

            // Save Scene
            string scenePath = "Assets/Scenes/CyberMinesweeper.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"Successfully created and saved scene to {scenePath}!");

            // Set Build Settings
            var buildScenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };
            EditorBuildSettings.scenes = buildScenes;
            Debug.Log("Configured EditorBuildSettings scenes!");
        }

        private static DefeatModal BuildDefeatModal(Transform parent, TMP_FontAsset font, Sprite sPanel, Sprite sBtnDanger, Sprite sBtnCyber)
        {
            var modalRoot = CreateModalBackdrop("DefeatModal", parent);
            var panel = CreateUIObject("Panel", modalRoot.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(560, 420));
            var pImg = panel.AddComponent<Image>();
            pImg.sprite = sPanel;
            pImg.type = Image.Type.Sliced;

            var vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(24, 24, 20, 20);
            vlg.spacing = 14;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;

            var badge = CreateText(panel.transform, "[ ОПАСНОСТЬ // НЕОБРАТИМАЯ ДЕТОНАЦИЯ ]", font, 12, FontStyles.Bold, new Color(255f / 255f, 0f, 85f / 255f));
            badge.alignment = TextAlignmentOptions.Center;

            var title = CreateText(panel.transform, "КРИТИЧЕСКИЙ СБОЙ", font, 28, FontStyles.Bold, new Color(255f / 255f, 180f / 255f, 171f / 255f));
            title.alignment = TextAlignmentOptions.Center;

            var subtitle = CreateText(panel.transform, "ДЕТОНАЦИЯ СЕКТОРА [ERROR 0x9F]", font, 14, FontStyles.Normal, new Color(255f / 255f, 100f / 255f, 100f / 255f));
            subtitle.alignment = TextAlignmentOptions.Center;

            // Stats box
            var statsBox = CreateUIObject("StatsBox", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 110));
            statsBox.AddComponent<Image>().color = new Color(10f / 255f, 14f / 255f, 24f / 255f, 0.9f);
            var vlgStats = statsBox.AddComponent<VerticalLayoutGroup>();
            vlgStats.padding = new RectOffset(16, 16, 10, 10);
            vlgStats.spacing = 6;
            vlgStats.childAlignment = TextAnchor.MiddleLeft;

            var tPoint = CreateText(statsBox.transform, "ТОЧКА РАЗРЫВА: GRID_X: 0 // Y: 0", font, 13, FontStyles.Normal, Color.white);
            var tAcc = CreateText(statsBox.transform, "ТОЧНОСТЬ РАЗВЕДКИ: 0.0% (НЕУДАЧА)", font, 13, FontStyles.Normal, Color.yellow);
            var tFalseFlags = CreateText(statsBox.transform, "ЛОЖНЫЕ МАРКИРОВКИ: 0 КРИТ. ОШИБОК", font, 13, FontStyles.Normal, new Color(255f / 255f, 120f / 255f, 120f / 255f));
            var tStatus = CreateText(statsBox.transform, "СТАТУС ИГРОКА: ПОРАЖЕНИЕ", font, 13, FontStyles.Bold, new Color(255f / 255f, 0f, 85f / 255f));

            // Buttons row
            var btnRow = CreateUIObject("ButtonRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 48));
            var hlg = btnRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 16;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;

            var btnRetry = CreateActionButton(btnRow.transform, "ПОВТОРИТЬ ОПЕРАЦИЮ", font, sBtnDanger);
            var btnInspect = CreateActionButton(btnRow.transform, "АНАЛИЗ ДАННЫХ", font, sBtnCyber);

            var dm = modalRoot.AddComponent<DefeatModal>();
            SetPrivateField(dm, "_panel", modalRoot);
            SetPrivateField(dm, "_pointOfFailureText", tPoint);
            SetPrivateField(dm, "_accuracyText", tAcc);
            SetPrivateField(dm, "_falseFlagsText", tFalseFlags);
            SetPrivateField(dm, "_statusText", tStatus);
            SetPrivateField(dm, "_retryButton", btnRetry);
            SetPrivateField(dm, "_inspectButton", btnInspect);
            modalRoot.SetActive(false);
            return dm;
        }

        private static VictoryModal BuildVictoryModal(Transform parent, TMP_FontAsset font, Sprite sPanel, Sprite sBtnCyber)
        {
            var modalRoot = CreateModalBackdrop("VictoryModal", parent);
            var panel = CreateUIObject("Panel", modalRoot.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(640, 520));
            var pImg = panel.AddComponent<Image>();
            pImg.sprite = sPanel;
            pImg.type = Image.Type.Sliced;

            var vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(24, 24, 20, 20);
            vlg.spacing = 12;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;

            var badge = CreateText(panel.transform, "[ СТАТУС: ТРИУМФ // 100% ACTIVE ]", font, 12, FontStyles.Bold, new Color(0f, 255f / 255f, 102f / 255f));
            badge.alignment = TextAlignmentOptions.Center;

            var title = CreateText(panel.transform, "ПОБЕДА!", font, 32, FontStyles.Bold, new Color(224f / 255f, 253f / 255f, 1f));
            title.alignment = TextAlignmentOptions.Center;

            var subtitle = CreateText(panel.transform, "МАТРИЦА УСПЕШНО ОБЕЗВРЕЖЕНА // УГРОЗ НЕТ", font, 14, FontStyles.Normal, new Color(0f, 242f / 255f, 254f / 255f));
            subtitle.alignment = TextAlignmentOptions.Center;

            var bannerGo = CreateUIObject("RecordBanner", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 30));
            bannerGo.AddComponent<Image>().color = new Color(0f, 242f / 255f, 254f / 255f, 0.2f);
            var bannerText = CreateText(bannerGo.transform, "★ НОВЫЙ РЕКОРД В СЕТИ ОРБИТЫ! ★", font, 13, FontStyles.Bold, new Color(245f / 255f, 158f / 255f, 11f));
            bannerText.alignment = TextAlignmentOptions.Center;

            // Stat tiles grid
            var statsGrid = CreateUIObject("StatsGrid", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 80));
            var glg = statsGrid.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(140, 70);
            glg.spacing = new Vector2(8, 8);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 4;
            glg.childAlignment = TextAnchor.MiddleCenter;

            var tile1 = CreateStatTile(statsGrid.transform, "ВРЕМЯ ОПЕРАЦИИ", "000 c", font, out var tTime);
            var tile2 = CreateStatTile(statsGrid.transform, "СЛОЖНОСТЬ", "9x9 [10 МИН]", font, out var tDiff);
            var tile3 = CreateStatTile(statsGrid.transform, "ТОЧНОСТЬ", "100%", font, out var tAcc);
            var tile4 = CreateStatTile(statsGrid.transform, "ЭФФЕКТИВНОСТЬ", "4.0 ЗВУ/с", font, out var tEff);

            // Callsign input
            var inputRow = CreateUIObject("InputRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 46));
            var hlgInput = inputRow.AddComponent<HorizontalLayoutGroup>();
            hlgInput.spacing = 10;
            hlgInput.childAlignment = TextAnchor.MiddleCenter;
            hlgInput.childControlWidth = false;

            var inputLabel = CreateText(inputRow.transform, "ПОЗЫВНОЙ:", font, 13, FontStyles.Bold, Color.white);
            var inputGo = CreateUIObject("CallsignInput", inputRow.transform, Vector2.zero, Vector2.zero, new Vector2(240, 38));
            inputGo.AddComponent<Image>().color = new Color(23f / 255f, 27f / 255f, 38f / 255f);
            var tmpInput = inputGo.AddComponent<TMP_InputField>();

            var textCompGo = CreateUIObject("Text", inputGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var textComp = textCompGo.AddComponent<TextMeshProUGUI>();
            if (font != null) textComp.font = font;
            textComp.fontSize = 15;
            textComp.color = Color.cyan;
            textComp.alignment = TextAlignmentOptions.MidlineLeft;
            tmpInput.textComponent = textComp;
            tmpInput.text = "CYBER_OPERATOR";

            // Buttons row
            var btnRow = CreateUIObject("ButtonRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 48));
            var hlgBtn = btnRow.AddComponent<HorizontalLayoutGroup>();
            hlgBtn.spacing = 12;
            hlgBtn.childAlignment = TextAnchor.MiddleCenter;
            hlgBtn.childControlWidth = true;

            var btnSave = CreateActionButton(btnRow.transform, "СОХРАНИТЬ РЕКОРД", font, sBtnCyber);
            var btnNew = CreateActionButton(btnRow.transform, "НОВАЯ ИГРА", font, sBtnCyber);
            var btnMenu = CreateActionButton(btnRow.transform, "МЕНЮ", font, sBtnCyber);

            var vm = modalRoot.AddComponent<VictoryModal>();
            SetPrivateField(vm, "_panel", modalRoot);
            SetPrivateField(vm, "_timeText", tTime);
            SetPrivateField(vm, "_difficultyText", tDiff);
            SetPrivateField(vm, "_accuracyText", tAcc);
            SetPrivateField(vm, "_efficiencyText", tEff);
            SetPrivateField(vm, "_recordBanner", bannerGo);
            SetPrivateField(vm, "_callsignInput", tmpInput);
            SetPrivateField(vm, "_saveRecordButton", btnSave);
            SetPrivateField(vm, "_newGameButton", btnNew);
            SetPrivateField(vm, "_menuButton", btnMenu);
            modalRoot.SetActive(false);
            return vm;
        }

        private static SettingsModal BuildSettingsModal(Transform parent, TMP_FontAsset font, Sprite sPanel, Sprite sBtnCyber)
        {
            var modalRoot = CreateModalBackdrop("SettingsModal", parent);
            var panel = CreateUIObject("Panel", modalRoot.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(680, 600));
            var pImg = panel.AddComponent<Image>();
            pImg.sprite = sPanel;
            pImg.type = Image.Type.Sliced;

            var vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(24, 24, 20, 20);
            vlg.spacing = 14;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;

            var title = CreateText(panel.transform, "ПРОТОКОЛ НАСТРОЙКИ: ОСОБЫЕ ПАРАМЕТРЫ", font, 20, FontStyles.Bold, new Color(224f / 255f, 253f / 255f, 1f));
            title.alignment = TextAlignmentOptions.Center;

            // Presets row
            var presetsRow = CreateUIObject("PresetsRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 64));
            var hlgPresets = presetsRow.AddComponent<HorizontalLayoutGroup>();
            hlgPresets.spacing = 10;
            hlgPresets.childAlignment = TextAnchor.MiddleCenter;
            hlgPresets.childControlWidth = true;

            var btnBeg = CreateActionButton(presetsRow.transform, "Новичок\n9x9 // 10 мин", font, sBtnCyber);
            var btnInt = CreateActionButton(presetsRow.transform, "Любитель\n16x16 // 40 мин", font, sBtnCyber);
            var btnExp = CreateActionButton(presetsRow.transform, "Профессионал\n30x16 // 99 мин", font, sBtnCyber);
            var btnCust = CreateActionButton(presetsRow.transform, "Особый\nПользовательский", font, sBtnCyber);

            // Custom Steppers Container
            var customCont = CreateUIObject("CustomContainer", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 130));
            var hlgCust = customCont.AddComponent<HorizontalLayoutGroup>();
            hlgCust.spacing = 12;
            hlgCust.childAlignment = TextAnchor.MiddleCenter;
            hlgCust.childControlWidth = true;

            var stepW = CreateStepper(customCont.transform, "Ширина матрицы (9-30)", "16", font, out var txtW, out var btnWMinus, out var btnWPlus);
            var stepH = CreateStepper(customCont.transform, "Высота матрицы (9-24)", "16", font, out var txtH, out var btnHMinus, out var btnHPlus);
            var stepM = CreateStepper(customCont.transform, "Число зарядов", "40", font, out var txtM, out var btnMMinus, out var btnMPlus);

            var densityText = CreateText(panel.transform, "Тактическая плотность поля: 15.6%", font, 13, FontStyles.Normal, Color.cyan);
            densityText.alignment = TextAlignmentOptions.Center;

            // Audio Section
            var audioBox = CreateUIObject("AudioSettingsBox", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 110));
            audioBox.AddComponent<Image>().color = new Color(15f / 255f, 20f / 255f, 30f / 255f, 0.9f);
            var vlgAudio = audioBox.AddComponent<VerticalLayoutGroup>();
            vlgAudio.padding = new RectOffset(16, 16, 8, 8);
            vlgAudio.spacing = 8;
            vlgAudio.childControlWidth = true;

            var volLabel = CreateText(audioBox.transform, "ГРОМКОСТЬ ЗВУКОВЫХ ЭФФЕКТОВ (0% - 100%)", font, 12, FontStyles.Bold, Color.white);
            var sliderGo = CreateUIObject("VolumeSlider", audioBox.transform, Vector2.zero, Vector2.zero, new Vector2(0, 24));
            var slider = sliderGo.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.8f;

            var togglesRow = CreateUIObject("TogglesRow", audioBox.transform, Vector2.zero, Vector2.zero, new Vector2(0, 26));
            var hlgToggles = togglesRow.AddComponent<HorizontalLayoutGroup>();
            hlgToggles.spacing = 20;

            var muteTgl = CreateToggle(togglesRow.transform, "Режим «Без звука»", font);
            var tickTgl = CreateToggle(togglesRow.transform, "Тиканье таймера", font);

            // Action Buttons
            var btnRow = CreateUIObject("ButtonRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 48));
            var hlgBtn = btnRow.AddComponent<HorizontalLayoutGroup>();
            hlgBtn.spacing = 16;
            hlgBtn.childAlignment = TextAnchor.MiddleCenter;
            hlgBtn.childControlWidth = true;

            var btnCancel = CreateActionButton(btnRow.transform, "ОТМЕНА", font, sBtnCyber);
            var btnOk = CreateActionButton(btnRow.transform, "ПРИНЯТЬ ПРОТОКОЛ // OK", font, sBtnCyber);

            var sm = modalRoot.AddComponent<SettingsModal>();
            SetPrivateField(sm, "_panel", modalRoot);
            SetPrivateField(sm, "_btnBeginner", btnBeg);
            SetPrivateField(sm, "_btnIntermediate", btnInt);
            SetPrivateField(sm, "_btnExpert", btnExp);
            SetPrivateField(sm, "_btnCustom", btnCust);
            SetPrivateField(sm, "_customContainer", customCont);
            SetPrivateField(sm, "_widthText", txtW);
            SetPrivateField(sm, "_widthMinus", btnWMinus);
            SetPrivateField(sm, "_widthPlus", btnWPlus);
            SetPrivateField(sm, "_heightText", txtH);
            SetPrivateField(sm, "_heightMinus", btnHMinus);
            SetPrivateField(sm, "_heightPlus", btnHPlus);
            SetPrivateField(sm, "_minesText", txtM);
            SetPrivateField(sm, "_minesMinus", btnMMinus);
            SetPrivateField(sm, "_minesPlus", btnMPlus);
            SetPrivateField(sm, "_densityText", densityText);
            SetPrivateField(sm, "_volumeSlider", slider);
            SetPrivateField(sm, "_muteToggle", muteTgl);
            SetPrivateField(sm, "_timerTickToggle", tickTgl);
            SetPrivateField(sm, "_okButton", btnOk);
            SetPrivateField(sm, "_cancelButton", btnCancel);
            modalRoot.SetActive(false);
            return sm;
        }

        private static LeaderboardsModal BuildLeaderboardsModal(Transform parent, TMP_FontAsset font, Sprite sPanel, Sprite sBtnCyber)
        {
            var modalRoot = CreateModalBackdrop("LeaderboardsModal", parent);
            var panel = CreateUIObject("Panel", modalRoot.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(720, 560));
            var pImg = panel.AddComponent<Image>();
            pImg.sprite = sPanel;
            pImg.type = Image.Type.Sliced;

            var vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(24, 24, 20, 20);
            vlg.spacing = 12;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;

            var title = CreateText(panel.transform, "АРХИВ ТАКТИЧЕСКОГО МОДУЛЯ // ТАБЛИЦА РЕКОРДОВ", font, 20, FontStyles.Bold, new Color(224f / 255f, 253f / 255f, 1f));
            title.alignment = TextAlignmentOptions.Center;

            // Tabs row
            var tabsRow = CreateUIObject("TabsRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 42));
            var hlgTabs = tabsRow.AddComponent<HorizontalLayoutGroup>();
            hlgTabs.spacing = 8;
            hlgTabs.childAlignment = TextAnchor.MiddleCenter;
            hlgTabs.childControlWidth = true;

            var tBeg = CreateActionButton(tabsRow.transform, "01 НОВИЧОК", font, sBtnCyber);
            var tInt = CreateActionButton(tabsRow.transform, "02 ЛЮБИТЕЛЬ", font, sBtnCyber);
            var tExp = CreateActionButton(tabsRow.transform, "03 ПРОФЕССИОНАЛ", font, sBtnCyber);
            var tCust = CreateActionButton(tabsRow.transform, "04 ОСОБЫЙ", font, sBtnCyber);

            // Table Header
            var headerRow = CreateUIObject("TableHeader", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 30));
            var hlgHdr = headerRow.AddComponent<HorizontalLayoutGroup>();
            hlgHdr.padding = new RectOffset(16, 16, 4, 4);
            hlgHdr.spacing = 10;
            hlgHdr.childControlWidth = true;

            CreateLabel(headerRow.transform, "РАНГ", 50, Color.gray, font);
            CreateLabel(headerRow.transform, "ПОЗЫВНОЙ ОПЕРАТОРА", 240, Color.gray, font);
            CreateLabel(headerRow.transform, "ВРЕМЯ (СЕК)", 100, Color.gray, font);
            CreateLabel(headerRow.transform, "ДАТА СЕАНСА", 140, Color.gray, font);

            // Rows Container
            var rowsCont = CreateUIObject("RowsContainer", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 220));
            var vlgRows = rowsCont.AddComponent<VerticalLayoutGroup>();
            vlgRows.spacing = 6;
            vlgRows.childControlWidth = true;

            // Custom notice
            var noticeGo = CreateUIObject("CustomNotice", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 60));
            var noticeTxt = CreateText(noticeGo.transform, "ОСОБЫЙ ПРОТОКОЛ [CUSTOM] НЕ УЧИТЫВАЕТСЯ В ТАБЛИЦЕ РЕКОРДОВ (FR-025)", font, 13, FontStyles.Normal, Color.yellow);
            noticeTxt.alignment = TextAlignmentOptions.Center;
            noticeGo.SetActive(false);

            // Best record banner
            var bestBanner = CreateUIObject("BestBanner", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 38));
            bestBanner.AddComponent<Image>().color = new Color(15f / 255f, 25f / 255f, 40f / 255f);
            var bestTxt = CreateText(bestBanner.transform, "ВАШ ЛУЧШИЙ РЕЗУЛЬТАТ: 014.2s (РАНГ #1)", font, 13, FontStyles.Bold, Color.cyan);
            bestTxt.alignment = TextAlignmentOptions.Center;

            // Buttons row
            var btnRow = CreateUIObject("ButtonRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 48));
            var hlgBtn = btnRow.AddComponent<HorizontalLayoutGroup>();
            hlgBtn.spacing = 16;
            hlgBtn.childAlignment = TextAnchor.MiddleCenter;
            hlgBtn.childControlWidth = true;

            var btnReset = CreateActionButton(btnRow.transform, "СБРОСИТЬ ДАННЫЕ", font, sBtnCyber);
            var btnClose = CreateActionButton(btnRow.transform, "ЗАКРЫТЬ ТАБЛИЦУ", font, sBtnCyber);

            var lm = modalRoot.AddComponent<LeaderboardsModal>();
            SetPrivateField(lm, "_panel", modalRoot);
            SetPrivateField(lm, "_tabBeginner", tBeg);
            SetPrivateField(lm, "_tabIntermediate", tInt);
            SetPrivateField(lm, "_tabExpert", tExp);
            SetPrivateField(lm, "_tabCustom", tCust);
            SetPrivateField(lm, "_rowsContainer", rowsCont.transform);
            SetPrivateField(lm, "_customNoticeObject", noticeGo);
            SetPrivateField(lm, "_bestResultText", bestTxt);
            SetPrivateField(lm, "_resetScoresButton", btnReset);
            SetPrivateField(lm, "_closeButton", btnClose);
            modalRoot.SetActive(false);
            return lm;
        }

        private static HelpAboutModal BuildHelpModal(Transform parent, TMP_FontAsset font, Sprite sPanel, Sprite sBtnCyber)
        {
            var modalRoot = CreateModalBackdrop("HelpAboutModal", parent);
            var panel = CreateUIObject("Panel", modalRoot.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(660, 520));
            var pImg = panel.AddComponent<Image>();
            pImg.sprite = sPanel;
            pImg.type = Image.Type.Sliced;

            var vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(24, 24, 20, 20);
            vlg.spacing = 12;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;

            var title = CreateText(panel.transform, "ПРАВИЛА ИГРЫ // СПРАВОЧНЫЙ МОДУЛЬ", font, 20, FontStyles.Bold, new Color(224f / 255f, 253f / 255f, 1f));
            title.alignment = TextAlignmentOptions.Center;

            // Tabs row
            var tabsRow = CreateUIObject("TabsRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 42));
            var hlgTabs = tabsRow.AddComponent<HorizontalLayoutGroup>();
            hlgTabs.spacing = 12;
            hlgTabs.childAlignment = TextAnchor.MiddleCenter;
            hlgTabs.childControlWidth = true;

            var tabRules = CreateActionButton(tabsRow.transform, "ПРАВИЛА ИГРЫ", font, sBtnCyber);
            var tabAbout = CreateActionButton(tabsRow.transform, "О ПРОГРАММЕ", font, sBtnCyber);

            // Rules Container
            var rulesCont = CreateUIObject("RulesContainer", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 300));
            var vlgRules = rulesCont.AddComponent<VerticalLayoutGroup>();
            vlgRules.padding = new RectOffset(16, 16, 12, 12);
            vlgRules.spacing = 8;
            rulesCont.AddComponent<Image>().color = new Color(15f / 255f, 20f / 255f, 30f / 255f, 0.9f);

            CreateText(rulesCont.transform, "• [ЛКМ] Открытие ячейки: первый ход гарантированно безопасен (зона 3x3 без мин).", font, 13, FontStyles.Normal, Color.white);
            CreateText(rulesCont.transform, "• [ПКМ] Установка / снятие флажка: блокирует ячейку от случайного подрыва.", font, 13, FontStyles.Normal, Color.white);
            CreateText(rulesCont.transform, "• [СКМ / Двойной ЛКМ] Аккорд (Chording): быстрое раскрытие смежных ячеек при верном числе флагов.", font, 13, FontStyles.Normal, Color.white);
            CreateText(rulesCont.transform, "• [F2 / Клик по Дрону] Быстрый перезапуск миссии с сохранением параметров.", font, 13, FontStyles.Normal, Color.white);
            CreateText(rulesCont.transform, "• Цель: открыть все безопасные ячейки за минимальное время.", font, 13, FontStyles.Bold, Color.cyan);

            // About Container
            var aboutCont = CreateUIObject("AboutContainer", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 300));
            var vlgAbout = aboutCont.AddComponent<VerticalLayoutGroup>();
            vlgAbout.padding = new RectOffset(16, 16, 12, 12);
            vlgAbout.spacing = 8;
            aboutCont.AddComponent<Image>().color = new Color(15f / 255f, 20f / 255f, 30f / 255f, 0.9f);

            CreateText(aboutCont.transform, "Учебный проект по дисциплине: «Технологии разработки программно-информационных систем»", font, 13, FontStyles.Normal, Color.gray);
            CreateText(aboutCont.transform, "Волгоградский государственный технический университет (ВолгГТУ), 2026", font, 13, FontStyles.Normal, Color.gray);
            CreateText(aboutCont.transform, "Преподаватель: Рак Д.В.", font, 13, FontStyles.Normal, Color.white);
            CreateText(aboutCont.transform, "Разработчики:", font, 14, FontStyles.Bold, Color.cyan);
            CreateText(aboutCont.transform, "  • Тюрин С.С. — Тимлид / Архитектор (Team Lead)", font, 13, FontStyles.Normal, Color.white);
            CreateText(aboutCont.transform, "  • Игнатов М.Р. — Frontend-разработчик (UI / HUD)", font, 13, FontStyles.Normal, Color.white);
            CreateText(aboutCont.transform, "  • Сапунков А.Р. — Backend-разработчик (Core / Logic)", font, 13, FontStyles.Normal, Color.white);
            aboutCont.SetActive(false);

            // Close button
            var btnClose = CreateActionButton(panel.transform, "ЗАКРЫТЬ", font, sBtnCyber);

            var hm = modalRoot.AddComponent<HelpAboutModal>();
            SetPrivateField(hm, "_panel", modalRoot);
            SetPrivateField(hm, "_rulesContainer", rulesCont);
            SetPrivateField(hm, "_aboutContainer", aboutCont);
            SetPrivateField(hm, "_titleText", title);
            SetPrivateField(hm, "_tabRulesButton", tabRules);
            SetPrivateField(hm, "_tabAboutButton", tabAbout);
            SetPrivateField(hm, "_closeButton", btnClose);
            modalRoot.SetActive(false);
            return hm;
        }

        private static GameObject CreateModalBackdrop(string name, Transform parent)
        {
            var backdrop = CreateUIObject(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var img = backdrop.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.75f);
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
            return tmp;
        }

        private static TextMeshProUGUI CreateLabel(Transform parent, string text, float width, Color color, TMP_FontAsset font)
        {
            var tmp = CreateText(parent, text, font, 13, FontStyles.Bold, color);
            var le = tmp.gameObject.AddComponent<LayoutElement>();
            le.minWidth = width;
            le.preferredWidth = width;
            return tmp;
        }

        private static Button CreateMenuButton(Transform parent, string text, TMP_FontAsset font, Sprite sprite)
        {
            var go = CreateUIObject("Btn_" + text, parent, Vector2.zero, Vector2.zero, new Vector2(110, 34));
            var img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            var btn = go.AddComponent<Button>();

            var txt = CreateText(go.transform, text, font, 13, FontStyles.Bold, Color.white);
            txt.alignment = TextAlignmentOptions.Center;
            var rt = txt.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return btn;
        }

        private static Button CreateActionButton(Transform parent, string text, TMP_FontAsset font, Sprite sprite)
        {
            var go = CreateUIObject("Btn_" + text.Replace("\n", "_"), parent, Vector2.zero, Vector2.zero, new Vector2(140, 44));
            var img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            var btn = go.AddComponent<Button>();

            var txt = CreateText(go.transform, text, font, 13, FontStyles.Bold, Color.white);
            txt.alignment = TextAlignmentOptions.Center;
            var rt = txt.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return btn;
        }

        private static Button CreateSmallSquareButton(Transform parent, string text, TMP_FontAsset font, Color? color = null)
        {
            var go = CreateUIObject("Btn_" + text, parent, Vector2.zero, Vector2.zero, new Vector2(30, 30));
            var img = go.AddComponent<Image>();
            img.color = new Color(25f / 255f, 30f / 255f, 42f / 255f);
            var btn = go.AddComponent<Button>();

            var txt = CreateText(go.transform, text, font, 15, FontStyles.Bold, color ?? Color.white);
            txt.alignment = TextAlignmentOptions.Center;
            var rt = txt.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return btn;
        }

        private static GameObject CreateStatTile(Transform parent, string title, string val, TMP_FontAsset font, out TextMeshProUGUI valText)
        {
            var tile = CreateUIObject("Tile", parent, Vector2.zero, Vector2.zero, new Vector2(140, 70));
            tile.AddComponent<Image>().color = new Color(15f / 255f, 20f / 255f, 30f / 255f);
            var vlg = tile.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(6, 6, 6, 6);
            vlg.childAlignment = TextAnchor.MiddleCenter;

            var tTitle = CreateText(tile.transform, title, font, 10, FontStyles.Bold, Color.gray);
            tTitle.alignment = TextAlignmentOptions.Center;

            valText = CreateText(tile.transform, val, font, 18, FontStyles.Bold, Color.cyan);
            valText.alignment = TextAlignmentOptions.Center;
            return tile;
        }

        private static GameObject CreateStepper(Transform parent, string label, string val, TMP_FontAsset font, out TextMeshProUGUI valText, out Button btnMinus, out Button btnPlus)
        {
            var box = CreateUIObject("Stepper_" + label, parent, Vector2.zero, Vector2.zero, new Vector2(180, 110));
            box.AddComponent<Image>().color = new Color(15f / 255f, 20f / 255f, 30f / 255f);
            var vlg = box.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.spacing = 8;
            vlg.childAlignment = TextAnchor.MiddleCenter;

            var lbl = CreateText(box.transform, label, font, 11, FontStyles.Bold, Color.gray);
            lbl.alignment = TextAlignmentOptions.Center;

            var row = CreateUIObject("Row", box.transform, Vector2.zero, Vector2.zero, new Vector2(160, 44));
            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleCenter;

            btnMinus = CreateSmallSquareButton(row.transform, "—", font);
            valText = CreateText(row.transform, val, font, 22, FontStyles.Bold, Color.white);
            valText.alignment = TextAlignmentOptions.Center;
            btnPlus = CreateSmallSquareButton(row.transform, "+", font);
            return box;
        }

        private static Toggle CreateToggle(Transform parent, string label, TMP_FontAsset font)
        {
            var go = CreateUIObject("Toggle_" + label, parent, Vector2.zero, Vector2.zero, new Vector2(220, 26));
            var tgl = go.AddComponent<Toggle>();
            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            var box = CreateUIObject("Box", go.transform, Vector2.zero, Vector2.zero, new Vector2(20, 20));
            var boxImg = box.AddComponent<Image>();
            boxImg.color = new Color(30f / 255f, 40f / 255f, 55f / 255f);

            var check = CreateUIObject("Check", box.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var checkImg = check.AddComponent<Image>();
            checkImg.color = Color.cyan;
            tgl.graphic = checkImg;
            tgl.targetGraphic = boxImg;

            var lbl = CreateText(go.transform, label, font, 13, FontStyles.Normal, Color.white);
            return tgl;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }
    }
}
