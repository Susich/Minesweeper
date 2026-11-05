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
            Debug.Log("Starting Cyber Minesweeper Scene generation (v2 UI)...");
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

            // 2. Create Scene
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
            scaler.referenceResolution = new Vector2(1280, 720); // Responsive 720p base resolution
            scaler.matchWidthOrHeight = 0.5f;

            // Event System with modern InputSystemUIInputModule (fixes buttons & cell clicks!)
            var esGo = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(InputSystemUIInputModule));

            // Main Background Panel
            var bgGo = CreateUIObject("Background", canvasGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(11f / 255f, 15f / 255f, 25f / 255f); // #0B0F19

            // --- TOP NAVIGATION BAR ---
            var topBar = CreateUIObject("TopMenuBar", canvasGo.transform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 48));
            topBar.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
            topBar.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            var topBarImg = topBar.AddComponent<Image>();
            topBarImg.color = new Color(10f / 255f, 14f / 255f, 24f / 255f, 0.98f); // #0A0E18

            var hlgTop = topBar.AddComponent<HorizontalLayoutGroup>();
            hlgTop.padding = new RectOffset(20, 20, 6, 6);
            hlgTop.spacing = 14;
            hlgTop.childAlignment = TextAnchor.MiddleLeft;
            hlgTop.childControlWidth = false;
            hlgTop.childControlHeight = true;

            // Green online dot
            var dotGo = CreateUIObject("DotIndicator", topBar.transform, Vector2.zero, Vector2.zero, new Vector2(8, 8));
            var dotImg = dotGo.AddComponent<Image>();
            dotImg.color = new Color(0f, 242f / 255f, 254f / 255f);

            // Title (unwrapped!)
            var titleText = CreateText(topBar.transform, "САПЁР // CYBER-SWEEPER HUD v2.0", fontAsset, 14, FontStyles.Bold, new Color(224f / 255f, 253f / 255f, 1f));
            titleText.enableWordWrapping = false;
            var titleLe = titleText.gameObject.AddComponent<LayoutElement>();
            titleLe.preferredWidth = 350;

            // System online badge
            var badgeGo = CreateUIObject("BadgeOnline", topBar.transform, Vector2.zero, Vector2.zero, new Vector2(120, 26));
            badgeGo.AddComponent<Image>().color = new Color(24f / 255f, 32f / 255f, 48f / 255f);
            var badgeText = CreateText(badgeGo.transform, "[SYSTEM ONLINE]", fontAsset, 10, FontStyles.Bold, new Color(0f, 242f / 255f, 254f / 255f));
            badgeText.alignment = TextAlignmentOptions.Center;
            var badgeRt = badgeText.GetComponent<RectTransform>();
            badgeRt.anchorMin = Vector2.zero;
            badgeRt.anchorMax = Vector2.one;
            badgeRt.offsetMin = Vector2.zero;
            badgeRt.offsetMax = Vector2.zero;

            // Top Menu Tabs
            var btnNewGame = CreateMenuTab(topBar.transform, "ИГРА", fontAsset, sBtnCyber, isPrimary: true);
            var btnRecords = CreateMenuTab(topBar.transform, "РЕКОРДЫ", fontAsset, sBtnCyber);
            var btnSettings = CreateMenuTab(topBar.transform, "ПАРАМЕТРЫ", fontAsset, sBtnCyber);
            var btnRules = CreateMenuTab(topBar.transform, "СПРАВКА", fontAsset, sBtnCyber);
            var btnAbout = CreateMenuTab(topBar.transform, "О ПРОГРАММЕ", fontAsset, sBtnCyber);

            // Flexible Spacer
            var spacer = CreateUIObject("Spacer", topBar.transform, Vector2.zero, Vector2.zero, Vector2.zero);
            var spacerLe = spacer.AddComponent<LayoutElement>();
            spacerLe.flexibleWidth = 1f;

            // Right side telemetry
            var telemetryTop = CreateText(topBar.transform, "SEC: 0x7F-88X12 SYNCED", fontAsset, 11, FontStyles.Normal, new Color(132f / 255f, 148f / 255f, 149f / 255f));
            telemetryTop.enableWordWrapping = false;

            // Window controls
            var btnMin = CreateSmallSquareButton(topBar.transform, "—", fontAsset);
            var btnMax = CreateSmallSquareButton(topBar.transform, "□", fontAsset);
            var btnClose = CreateSmallSquareButton(topBar.transform, "✕", fontAsset, new Color(255f / 255f, 0f, 85f / 255f));

            // --- TACTICAL CONSOLE FRAME ---
            var consoleFrame = CreateUIObject("TacticalConsoleFrame", canvasGo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(620, 680));
            consoleFrame.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -18);
            var cfImg = consoleFrame.AddComponent<Image>();
            cfImg.sprite = sPanelCyber;
            cfImg.type = Image.Type.Sliced;
            cfImg.color = Color.white;

            var screenShake = consoleFrame.AddComponent<ScreenShake>();
            SetPrivateField(screenShake, "_targetTransform", consoleFrame.GetComponent<RectTransform>());

            var vlgConsole = consoleFrame.AddComponent<VerticalLayoutGroup>();
            vlgConsole.padding = new RectOffset(16, 16, 12, 12);
            vlgConsole.spacing = 10;
            vlgConsole.childControlWidth = true;
            vlgConsole.childControlHeight = false;

            // 1. Header Telemetry Strip
            var telemStrip = CreateUIObject("HeaderTelemetryStrip", consoleFrame.transform, Vector2.zero, Vector2.zero, new Vector2(0, 32));
            var telemImg = telemStrip.AddComponent<Image>();
            telemImg.color = new Color(23f / 255f, 27f / 255f, 38f / 255f, 0.95f);
            var hlgTelem = telemStrip.AddComponent<HorizontalLayoutGroup>();
            hlgTelem.padding = new RectOffset(12, 12, 4, 4);
            hlgTelem.spacing = 15;
            hlgTelem.childAlignment = TextAnchor.MiddleCenter;

            var radarTitle = CreateText(telemStrip.transform, "DEFUSAL // MATRIX-09", fontAsset, 13, FontStyles.Bold, new Color(224f / 255f, 253f / 255f, 1f));
            var gridDensityText = CreateText(telemStrip.transform, "GRID: 9x9 [LIGHT]", fontAsset, 11, FontStyles.Normal, new Color(0f, 220f / 255f, 230f / 255f));
            var chanceText = CreateText(telemStrip.transform, "CHANCE: 87.7% CLEAR", fontAsset, 11, FontStyles.Normal, new Color(185f / 255f, 202f / 255f, 203f / 255f));
            var memText = CreateText(telemStrip.transform, "MEM: 128TB LIVE", fontAsset, 11, FontStyles.Normal, new Color(132f / 255f, 148f / 255f, 149f / 255f));

            // 2. Top Status Bar (Counters & Drone AI Core Face)
            var statusBar = CreateUIObject("TopStatusBar", consoleFrame.transform, Vector2.zero, Vector2.zero, new Vector2(0, 72));
            var sbImg = statusBar.AddComponent<Image>();
            sbImg.color = new Color(28f / 255f, 34f / 255f, 48f / 255f, 0.95f);
            var hlgStatus = statusBar.AddComponent<HorizontalLayoutGroup>();
            hlgStatus.padding = new RectOffset(16, 16, 6, 6);
            hlgStatus.spacing = 15;
            hlgStatus.childAlignment = TextAnchor.MiddleCenter;
            hlgStatus.childControlWidth = false;
            hlgStatus.childControlHeight = false;

            // Mine Counter Bay (Left)
            var mineBay = CreateUIObject("MineCounterBay", statusBar.transform, Vector2.zero, Vector2.zero, new Vector2(150, 58));
            var mbImg = mineBay.AddComponent<Image>();
            mbImg.color = new Color(10f / 255f, 14f / 255f, 24f / 255f, 0.95f);
            var vlgMine = mineBay.AddComponent<VerticalLayoutGroup>();
            vlgMine.padding = new RectOffset(8, 8, 2, 2);
            vlgMine.childAlignment = TextAnchor.MiddleCenter;
            var mineLabel = CreateText(mineBay.transform, "MINES [REM]", fontAsset, 10, FontStyles.Bold, new Color(255f / 255f, 180f / 255f, 171f / 255f));
            var mineCounterText = CreateText(mineBay.transform, "010", fontAsset, 28, FontStyles.Bold, new Color(255f / 255f, 75f / 255f, 75f / 255f));

            // Center Drone AI Face Button
            var faceBtnGo = CreateUIObject("DroneFaceButton", statusBar.transform, Vector2.zero, Vector2.zero, new Vector2(56, 56));
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
            var timerBay = CreateUIObject("TimerBay", statusBar.transform, Vector2.zero, Vector2.zero, new Vector2(150, 58));
            var tbImg = timerBay.AddComponent<Image>();
            tbImg.color = new Color(10f / 255f, 14f / 255f, 24f / 255f, 0.95f);
            var vlgTimer = timerBay.AddComponent<VerticalLayoutGroup>();
            vlgTimer.padding = new RectOffset(8, 8, 2, 2);
            vlgTimer.childAlignment = TextAnchor.MiddleCenter;
            var timerLabel = CreateText(timerBay.transform, "TIMER [SEC]", fontAsset, 10, FontStyles.Bold, new Color(111f / 255f, 246f / 255f, 255f / 255f));
            var timerCounterText = CreateText(timerBay.transform, "000", fontAsset, 28, FontStyles.Bold, new Color(0f, 242f / 255f, 254f / 255f));

            var headerView = statusBar.AddComponent<HeaderStatusView>();
            SetPrivateField(headerView, "_mineCounterText", mineCounterText);
            SetPrivateField(headerView, "_timerText", timerCounterText);
            SetPrivateField(headerView, "_gridDensityText", gridDensityText);
            SetPrivateField(headerView, "_chanceText", chanceText);

            // 3. Central Minefield Grid
            var gridWrapper = CreateUIObject("GridWrapper", consoleFrame.transform, Vector2.zero, Vector2.zero, new Vector2(0, 480));
            var gridWrapperLe = gridWrapper.AddComponent<LayoutElement>();
            gridWrapperLe.preferredHeight = 480;
            gridWrapperLe.flexibleHeight = 1;

            var gridContainerGo = CreateUIObject("GridContainer", gridWrapper.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(460, 460));
            var gridLayout = gridContainerGo.AddComponent<GridLayoutGroup>();
            gridLayout.childAlignment = TextAnchor.MiddleCenter;

            var boardView = gridContainerGo.AddComponent<BoardGridView>();
            SetPrivateField(boardView, "_gridContainer", gridContainerGo.GetComponent<RectTransform>());
            SetPrivateField(boardView, "_gridLayout", gridLayout);
            SetPrivateField(boardView, "_consoleFrame", consoleFrame.GetComponent<RectTransform>());
            boardView.ConfigureResources(sCellClosed, sCellHover, sCellPressed, sCellRevealed, sCellExploded, sIconFlag, sIconMine, sIconFalseFlag, fontAsset);
            boardView.SetConsoleFrame(consoleFrame.GetComponent<RectTransform>());

            // 4. Footer Telemetry Strip
            var footerStrip = CreateUIObject("FooterTelemetryStrip", consoleFrame.transform, Vector2.zero, Vector2.zero, new Vector2(0, 28));
            var hlgFooter = footerStrip.AddComponent<HorizontalLayoutGroup>();
            hlgFooter.padding = new RectOffset(8, 8, 2, 2);
            hlgFooter.spacing = 15;
            hlgFooter.childAlignment = TextAnchor.MiddleCenter;

            var footerStatusText = CreateText(footerStrip.transform, "СЕКТОР: РАЗМИНИРОВАНИЕ // ОПЕРАЦИЯ АКТИВНА", fontAsset, 10, FontStyles.Bold, new Color(0f, 242f / 255f, 254f / 255f));
            var footerHintsText = CreateText(footerStrip.transform, "[ЛКМ] ВСКРЫТИЕ  |  [ПКМ] МАРКЕР  |  [СКМ] АККОРД  |  [F2] РЕСТАРТ  |  [ESC] ПАУЗА", fontAsset, 10, FontStyles.Normal, new Color(132f / 255f, 148f / 255f, 149f / 255f));

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

            // Bind all custom scripts to their MonoScript assets (fixes missing scripts in standalone builds!)
            BindAllCustomScriptsInScene();

            // Save Scene
            string scenePath = "Assets/Scenes/CyberMinesweeper.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"Successfully updated and saved scene to {scenePath}!");

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
            var modalRoot = CreateUIObject("DefeatModalRoot", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var backdrop = CreateModalBackdrop("Backdrop", modalRoot.transform);
            var panel = CreateUIObject("Panel", backdrop.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(500, 360));
            var pImg = panel.AddComponent<Image>();
            pImg.sprite = sPanel;
            pImg.type = Image.Type.Sliced;

            var vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 16, 16);
            vlg.spacing = 10;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;

            var badge = CreateText(panel.transform, "[ ОПАСНОСТЬ // НЕОБРАТИМАЯ ДЕТОНАЦИЯ ]", font, 11, FontStyles.Bold, new Color(255f / 255f, 0f, 85f / 255f));
            badge.alignment = TextAlignmentOptions.Center;

            var title = CreateText(panel.transform, "КРИТИЧЕСКИЙ СБОЙ", font, 24, FontStyles.Bold, new Color(255f / 255f, 180f / 255f, 171f / 255f));
            title.alignment = TextAlignmentOptions.Center;

            var subtitle = CreateText(panel.transform, "ДЕТОНАЦИЯ СЕКТОРА [ERROR 0x9F]", font, 13, FontStyles.Normal, new Color(255f / 255f, 100f / 255f, 100f / 255f));
            subtitle.alignment = TextAlignmentOptions.Center;

            // Stats box
            var statsBox = CreateUIObject("StatsBox", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 100));
            statsBox.AddComponent<Image>().color = new Color(10f / 255f, 14f / 255f, 24f / 255f, 0.95f);
            var vlgStats = statsBox.AddComponent<VerticalLayoutGroup>();
            vlgStats.padding = new RectOffset(14, 14, 8, 8);
            vlgStats.spacing = 5;
            vlgStats.childAlignment = TextAnchor.MiddleLeft;

            var tPoint = CreateText(statsBox.transform, "ТОЧКА РАЗРЫВА: GRID_X: 0 // Y: 0", font, 12, FontStyles.Normal, Color.white);
            var tAcc = CreateText(statsBox.transform, "ТОЧНОСТЬ РАЗВЕДКИ: 0.0% (НЕУДАЧА)", font, 12, FontStyles.Normal, Color.yellow);
            var tFalseFlags = CreateText(statsBox.transform, "ЛОЖНЫЕ МАРКИРОВКИ: 0 КРИТ. ОШИБОК", font, 12, FontStyles.Normal, new Color(255f / 255f, 120f / 255f, 120f / 255f));
            var tStatus = CreateText(statsBox.transform, "СТАТУС ИГРОКА: ПОРАЖЕНИЕ", font, 12, FontStyles.Bold, new Color(255f / 255f, 0f, 85f / 255f));

            // Buttons row
            var btnRow = CreateUIObject("ButtonRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 42));
            var hlg = btnRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 14;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;

            var btnRetry = CreateActionButton(btnRow.transform, "ПОВТОРИТЬ ОПЕРАЦИЮ", font, sBtnDanger);
            var btnInspect = CreateActionButton(btnRow.transform, "АНАЛИЗ ДАННЫХ", font, sBtnCyber);

            var dm = modalRoot.AddComponent<DefeatModal>();
            SetPrivateField(dm, "_panel", backdrop);
            SetPrivateField(dm, "_pointOfFailureText", tPoint);
            SetPrivateField(dm, "_accuracyText", tAcc);
            SetPrivateField(dm, "_falseFlagsText", tFalseFlags);
            SetPrivateField(dm, "_statusText", tStatus);
            SetPrivateField(dm, "_retryButton", btnRetry);
            SetPrivateField(dm, "_inspectButton", btnInspect);
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
            vlg.padding = new RectOffset(20, 20, 16, 16);
            vlg.spacing = 10;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;

            var badge = CreateText(panel.transform, "[ СТАТУС: ТРИУМФ // 100% ACTIVE ]", font, 11, FontStyles.Bold, new Color(0f, 255f / 255f, 102f / 255f));
            badge.alignment = TextAlignmentOptions.Center;

            var title = CreateText(panel.transform, "ПОБЕДА!", font, 28, FontStyles.Bold, new Color(224f / 255f, 253f / 255f, 1f));
            title.alignment = TextAlignmentOptions.Center;

            var subtitle = CreateText(panel.transform, "МАТРИЦА УСПЕШНО ОБЕЗВРЕЖЕНА // УГРОЗ НЕТ", font, 13, FontStyles.Normal, new Color(0f, 242f / 255f, 254f / 255f));
            subtitle.alignment = TextAlignmentOptions.Center;

            var bannerGo = CreateUIObject("RecordBanner", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 26));
            bannerGo.AddComponent<Image>().color = new Color(0f, 242f / 255f, 254f / 255f, 0.2f);
            var bannerText = CreateText(bannerGo.transform, "★ НОВЫЙ РЕКОРД В СЕТИ ОРБИТЫ! ★", font, 12, FontStyles.Bold, new Color(245f / 255f, 158f / 255f, 11f));
            bannerText.alignment = TextAlignmentOptions.Center;

            // Stat tiles grid
            var statsGrid = CreateUIObject("StatsGrid", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 68));
            var glg = statsGrid.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(120, 62);
            glg.spacing = new Vector2(6, 6);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 4;
            glg.childAlignment = TextAnchor.MiddleCenter;

            var tile1 = CreateStatTile(statsGrid.transform, "ВРЕМЯ ОПЕРАЦИИ", "000 c", font, out var tTime);
            var tile2 = CreateStatTile(statsGrid.transform, "СЛОЖНОСТЬ", "9x9 [10 МИН]", font, out var tDiff);
            var tile3 = CreateStatTile(statsGrid.transform, "ТОЧНОСТЬ", "100%", font, out var tAcc);
            var tile4 = CreateStatTile(statsGrid.transform, "ЭФФЕКТИВНОСТЬ", "4.0 ЗВУ/с", font, out var tEff);

            // Callsign input
            var inputRow = CreateUIObject("InputRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 40));
            var hlgInput = inputRow.AddComponent<HorizontalLayoutGroup>();
            hlgInput.spacing = 10;
            hlgInput.childAlignment = TextAnchor.MiddleCenter;
            hlgInput.childControlWidth = false;

            var inputLabel = CreateText(inputRow.transform, "ПОЗЫВНОЙ:", font, 12, FontStyles.Bold, Color.white);
            var inputGo = CreateUIObject("CallsignInput", inputRow.transform, Vector2.zero, Vector2.zero, new Vector2(220, 34));
            inputGo.AddComponent<Image>().color = new Color(23f / 255f, 27f / 255f, 38f / 255f);
            var tmpInput = inputGo.AddComponent<TMP_InputField>();

            var textCompGo = CreateUIObject("Text", inputGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var textComp = textCompGo.AddComponent<TextMeshProUGUI>();
            if (font != null) textComp.font = font;
            textComp.fontSize = 14;
            textComp.color = Color.cyan;
            textComp.alignment = TextAlignmentOptions.MidlineLeft;
            tmpInput.textComponent = textComp;
            tmpInput.text = "CYBER_OPERATOR";

            // Buttons row
            var btnRow = CreateUIObject("ButtonRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 42));
            var hlgBtn = btnRow.AddComponent<HorizontalLayoutGroup>();
            hlgBtn.spacing = 10;
            hlgBtn.childAlignment = TextAnchor.MiddleCenter;
            hlgBtn.childControlWidth = true;

            var btnSave = CreateActionButton(btnRow.transform, "СОХРАНИТЬ РЕКОРД", font, sBtnCyber);
            var btnNew = CreateActionButton(btnRow.transform, "НОВАЯ ИГРА", font, sBtnCyber);
            var btnMenu = CreateActionButton(btnRow.transform, "МЕНЮ", font, sBtnCyber);

            var vm = modalRoot.AddComponent<VictoryModal>();
            SetPrivateField(vm, "_panel", backdrop);
            SetPrivateField(vm, "_timeText", tTime);
            SetPrivateField(vm, "_difficultyText", tDiff);
            SetPrivateField(vm, "_accuracyText", tAcc);
            SetPrivateField(vm, "_efficiencyText", tEff);
            SetPrivateField(vm, "_recordBanner", bannerGo);
            SetPrivateField(vm, "_callsignInput", tmpInput);
            SetPrivateField(vm, "_saveRecordButton", btnSave);
            SetPrivateField(vm, "_newGameButton", btnNew);
            SetPrivateField(vm, "_menuButton", btnMenu);
            backdrop.SetActive(false);
            return vm;
        }

        private static SettingsModal BuildSettingsModal(Transform parent, TMP_FontAsset font, Sprite sPanel, Sprite sBtnCyber)
        {
            var modalRoot = CreateUIObject("SettingsModalRoot", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var backdrop = CreateModalBackdrop("Backdrop", modalRoot.transform);
            var panel = CreateUIObject("Panel", backdrop.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(580, 520));
            var pImg = panel.AddComponent<Image>();
            pImg.sprite = sPanel;
            pImg.type = Image.Type.Sliced;

            var vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 16, 16);
            vlg.spacing = 10;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;

            var title = CreateText(panel.transform, "ПРОТОКОЛ НАСТРОЙКИ: ОСОБЫЕ ПАРАМЕТРЫ", font, 16, FontStyles.Bold, new Color(224f / 255f, 253f / 255f, 1f));
            title.alignment = TextAlignmentOptions.Center;

            // Presets row
            var presetsRow = CreateUIObject("PresetsRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 56));
            var hlgPresets = presetsRow.AddComponent<HorizontalLayoutGroup>();
            hlgPresets.spacing = 8;
            hlgPresets.childAlignment = TextAnchor.MiddleCenter;
            hlgPresets.childControlWidth = true;

            var btnBeg = CreateActionButton(presetsRow.transform, "Новичок\n9x9 // 10 мин", font, sBtnCyber);
            var btnInt = CreateActionButton(presetsRow.transform, "Любитель\n16x16 // 40 мин", font, sBtnCyber);
            var btnExp = CreateActionButton(presetsRow.transform, "Профессионал\n30x16 // 99 мин", font, sBtnCyber);
            var btnCust = CreateActionButton(presetsRow.transform, "Особый\nПользовательский", font, sBtnCyber);

            // Custom Steppers Container
            var customCont = CreateUIObject("CustomContainer", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 100));
            var hlgCust = customCont.AddComponent<HorizontalLayoutGroup>();
            hlgCust.spacing = 10;
            hlgCust.childAlignment = TextAnchor.MiddleCenter;
            hlgCust.childControlWidth = true;

            var stepW = CreateStepper(customCont.transform, "Ширина (9-30)", "16", font, out var txtW, out var btnWMinus, out var btnWPlus);
            var stepH = CreateStepper(customCont.transform, "Высота (9-24)", "16", font, out var txtH, out var btnHMinus, out var btnHPlus);
            var stepM = CreateStepper(customCont.transform, "Число зарядов", "40", font, out var txtM, out var btnMMinus, out var btnMPlus);

            var densityText = CreateText(panel.transform, "Тактическая плотность поля: 15.6%", font, 12, FontStyles.Normal, Color.cyan);
            densityText.alignment = TextAlignmentOptions.Center;

            // Audio Section
            var audioBox = CreateUIObject("AudioSettingsBox", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 95));
            audioBox.AddComponent<Image>().color = new Color(15f / 255f, 20f / 255f, 30f / 255f, 0.95f);
            var vlgAudio = audioBox.AddComponent<VerticalLayoutGroup>();
            vlgAudio.padding = new RectOffset(12, 12, 6, 6);
            vlgAudio.spacing = 6;
            vlgAudio.childControlWidth = true;

            var volLabel = CreateText(audioBox.transform, "ГРОМКОСТЬ ЗВУКОВЫХ ЭФФЕКТОВ (0% - 100%)", font, 11, FontStyles.Bold, Color.white);
            var sliderGo = CreateUIObject("VolumeSlider", audioBox.transform, Vector2.zero, Vector2.zero, new Vector2(0, 20));
            var slider = sliderGo.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.8f;

            var togglesRow = CreateUIObject("TogglesRow", audioBox.transform, Vector2.zero, Vector2.zero, new Vector2(0, 24));
            var hlgToggles = togglesRow.AddComponent<HorizontalLayoutGroup>();
            hlgToggles.spacing = 16;

            var muteTgl = CreateToggle(togglesRow.transform, "Режим «Без звука»", font);
            var tickTgl = CreateToggle(togglesRow.transform, "Тиканье таймера", font);

            // Action Buttons
            var btnRow = CreateUIObject("ButtonRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 42));
            var hlgBtn = btnRow.AddComponent<HorizontalLayoutGroup>();
            hlgBtn.spacing = 14;
            hlgBtn.childAlignment = TextAnchor.MiddleCenter;
            hlgBtn.childControlWidth = true;

            var btnCancel = CreateActionButton(btnRow.transform, "ОТМЕНА", font, sBtnCyber);
            var btnOk = CreateActionButton(btnRow.transform, "ПРИНЯТЬ ПРОТОКОЛ // OK", font, sBtnCyber);

            var sm = modalRoot.AddComponent<SettingsModal>();
            SetPrivateField(sm, "_panel", backdrop);
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
            backdrop.SetActive(false);
            return sm;
        }

        private static LeaderboardsModal BuildLeaderboardsModal(Transform parent, TMP_FontAsset font, Sprite sPanel, Sprite sBtnCyber)
        {
            var modalRoot = CreateUIObject("LeaderboardsModalRoot", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var backdrop = CreateModalBackdrop("Backdrop", modalRoot.transform);
            var panel = CreateUIObject("Panel", backdrop.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(600, 480));
            var pImg = panel.AddComponent<Image>();
            pImg.sprite = sPanel;
            pImg.type = Image.Type.Sliced;

            var vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 16, 16);
            vlg.spacing = 10;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;

            var title = CreateText(panel.transform, "АРХИВ ТАКТИЧЕСКОГО МОДУЛЯ // ТАБЛИЦА РЕКОРДОВ", font, 16, FontStyles.Bold, new Color(224f / 255f, 253f / 255f, 1f));
            title.alignment = TextAlignmentOptions.Center;

            // Tabs row
            var tabsRow = CreateUIObject("TabsRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 36));
            var hlgTabs = tabsRow.AddComponent<HorizontalLayoutGroup>();
            hlgTabs.spacing = 6;
            hlgTabs.childAlignment = TextAnchor.MiddleCenter;
            hlgTabs.childControlWidth = true;

            var tBeg = CreateActionButton(tabsRow.transform, "01 НОВИЧОК", font, sBtnCyber);
            var tInt = CreateActionButton(tabsRow.transform, "02 ЛЮБИТЕЛЬ", font, sBtnCyber);
            var tExp = CreateActionButton(tabsRow.transform, "03 ПРОФЕССИОНАЛ", font, sBtnCyber);
            var tCust = CreateActionButton(tabsRow.transform, "04 ОСОБЫЙ", font, sBtnCyber);

            // Table Header
            var headerRow = CreateUIObject("TableHeader", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 26));
            var hlgHdr = headerRow.AddComponent<HorizontalLayoutGroup>();
            hlgHdr.padding = new RectOffset(12, 12, 2, 2);
            hlgHdr.spacing = 8;
            hlgHdr.childControlWidth = true;

            CreateLabel(headerRow.transform, "РАНГ", 45, Color.gray, font);
            CreateLabel(headerRow.transform, "ПОЗЫВНОЙ ОПЕРАТОРА", 200, Color.gray, font);
            CreateLabel(headerRow.transform, "ВРЕМЯ", 90, Color.gray, font);
            CreateLabel(headerRow.transform, "ДАТА", 120, Color.gray, font);

            // Rows Container
            var rowsCont = CreateUIObject("RowsContainer", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 180));
            var vlgRows = rowsCont.AddComponent<VerticalLayoutGroup>();
            vlgRows.spacing = 4;
            vlgRows.childControlWidth = true;

            // Custom notice
            var noticeGo = CreateUIObject("CustomNotice", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 50));
            var noticeTxt = CreateText(noticeGo.transform, "ОСОБЫЙ ПРОТОКОЛ [CUSTOM] НЕ УЧИТЫВАЕТСЯ В ТАБЛИЦЕ РЕКОРДОВ (FR-025)", font, 12, FontStyles.Normal, Color.yellow);
            noticeTxt.alignment = TextAlignmentOptions.Center;
            noticeGo.SetActive(false);

            // Best record banner
            var bestBanner = CreateUIObject("BestBanner", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 32));
            bestBanner.AddComponent<Image>().color = new Color(15f / 255f, 25f / 255f, 40f / 255f);
            var bestTxt = CreateText(bestBanner.transform, "ВАШ ЛУЧШИЙ РЕЗУЛЬТАТ: 014.2s (РАНГ #1)", font, 12, FontStyles.Bold, Color.cyan);
            bestTxt.alignment = TextAlignmentOptions.Center;

            // Buttons row
            var btnRow = CreateUIObject("ButtonRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 42));
            var hlgBtn = btnRow.AddComponent<HorizontalLayoutGroup>();
            hlgBtn.spacing = 14;
            hlgBtn.childAlignment = TextAnchor.MiddleCenter;
            hlgBtn.childControlWidth = true;

            var btnReset = CreateActionButton(btnRow.transform, "СБРОСИТЬ ДАННЫЕ", font, sBtnCyber);
            var btnClose = CreateActionButton(btnRow.transform, "ЗАКРЫТЬ ТАБЛИЦУ", font, sBtnCyber);

            var lm = modalRoot.AddComponent<LeaderboardsModal>();
            SetPrivateField(lm, "_panel", backdrop);
            SetPrivateField(lm, "_tabBeginner", tBeg);
            SetPrivateField(lm, "_tabIntermediate", tInt);
            SetPrivateField(lm, "_tabExpert", tExp);
            SetPrivateField(lm, "_tabCustom", tCust);
            SetPrivateField(lm, "_rowsContainer", rowsCont.transform);
            SetPrivateField(lm, "_customNoticeObject", noticeGo);
            SetPrivateField(lm, "_bestResultText", bestTxt);
            SetPrivateField(lm, "_resetScoresButton", btnReset);
            SetPrivateField(lm, "_closeButton", btnClose);
            backdrop.SetActive(false);
            return lm;
        }

        private static HelpAboutModal BuildHelpModal(Transform parent, TMP_FontAsset font, Sprite sPanel, Sprite sBtnCyber)
        {
            var modalRoot = CreateUIObject("HelpAboutModalRoot", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var backdrop = CreateModalBackdrop("Backdrop", modalRoot.transform);
            var panel = CreateUIObject("Panel", backdrop.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(580, 440));
            var pImg = panel.AddComponent<Image>();
            pImg.sprite = sPanel;
            pImg.type = Image.Type.Sliced;

            var vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 16, 16);
            vlg.spacing = 10;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;

            var title = CreateText(panel.transform, "ПРАВИЛА ИГРЫ // СПРАВОЧНЫЙ МОДУЛЬ", font, 16, FontStyles.Bold, new Color(224f / 255f, 253f / 255f, 1f));
            title.alignment = TextAlignmentOptions.Center;

            // Tabs row
            var tabsRow = CreateUIObject("TabsRow", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 36));
            var hlgTabs = tabsRow.AddComponent<HorizontalLayoutGroup>();
            hlgTabs.spacing = 10;
            hlgTabs.childAlignment = TextAnchor.MiddleCenter;
            hlgTabs.childControlWidth = true;

            var tabRules = CreateActionButton(tabsRow.transform, "ПРАВИЛА ИГРЫ", font, sBtnCyber);
            var tabAbout = CreateActionButton(tabsRow.transform, "О ПРОГРАММЕ", font, sBtnCyber);

            // Rules Container
            var rulesCont = CreateUIObject("RulesContainer", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 240));
            var vlgRules = rulesCont.AddComponent<VerticalLayoutGroup>();
            vlgRules.padding = new RectOffset(14, 14, 10, 10);
            vlgRules.spacing = 6;
            rulesCont.AddComponent<Image>().color = new Color(15f / 255f, 20f / 255f, 30f / 255f, 0.95f);

            CreateText(rulesCont.transform, "• [ЛКМ] Открытие ячейки: первый ход гарантированно безопасен (зона 3x3 без мин).", font, 12, FontStyles.Normal, Color.white);
            CreateText(rulesCont.transform, "• [ПКМ] Установка / снятие флажка: блокирует ячейку от случайного подрыва.", font, 12, FontStyles.Normal, Color.white);
            CreateText(rulesCont.transform, "• [СКМ / Двойной ЛКМ] Аккорд (Chording): быстрое раскрытие смежных ячеек при верном числе флагов.", font, 12, FontStyles.Normal, Color.white);
            CreateText(rulesCont.transform, "• [F2 / Клик по Дрону] Быстрый перезапуск миссии с сохранением параметров.", font, 12, FontStyles.Normal, Color.white);
            CreateText(rulesCont.transform, "• Цель: открыть все безопасные ячейки за минимальное время.", font, 12, FontStyles.Bold, Color.cyan);

            // About Container
            var aboutCont = CreateUIObject("AboutContainer", panel.transform, Vector2.zero, Vector2.zero, new Vector2(0, 240));
            var vlgAbout = aboutCont.AddComponent<VerticalLayoutGroup>();
            vlgAbout.padding = new RectOffset(14, 14, 10, 10);
            vlgAbout.spacing = 6;
            aboutCont.AddComponent<Image>().color = new Color(15f / 255f, 20f / 255f, 30f / 255f, 0.95f);

            CreateText(aboutCont.transform, "Учебный проект по дисциплине: «Технологии разработки программно-информационных систем»", font, 12, FontStyles.Normal, Color.gray);
            CreateText(aboutCont.transform, "Волгоградский государственный технический университет (ВолгГТУ), 2026", font, 12, FontStyles.Normal, Color.gray);
            CreateText(aboutCont.transform, "Преподаватель: Рак Д.В.", font, 12, FontStyles.Normal, Color.white);
            CreateText(aboutCont.transform, "Разработчики:", font, 13, FontStyles.Bold, Color.cyan);
            CreateText(aboutCont.transform, "  • Тюрин С.С. — Тимлид / Архитектор (Team Lead)", font, 12, FontStyles.Normal, Color.white);
            CreateText(aboutCont.transform, "  • Игнатов М.Р. — Frontend-разработчик (UI / HUD)", font, 12, FontStyles.Normal, Color.white);
            CreateText(aboutCont.transform, "  • Сапунков А.Р. — Backend-разработчик (Core / Logic)", font, 12, FontStyles.Normal, Color.white);
            aboutCont.SetActive(false);

            // Close button
            var btnClose = CreateActionButton(panel.transform, "ЗАКРЫТЬ", font, sBtnCyber);

            var hm = modalRoot.AddComponent<HelpAboutModal>();
            SetPrivateField(hm, "_panel", backdrop);
            SetPrivateField(hm, "_rulesContainer", rulesCont);
            SetPrivateField(hm, "_aboutContainer", aboutCont);
            SetPrivateField(hm, "_titleText", title);
            SetPrivateField(hm, "_tabRulesButton", tabRules);
            SetPrivateField(hm, "_tabAboutButton", tabAbout);
            SetPrivateField(hm, "_closeButton", btnClose);
            backdrop.SetActive(false);
            return hm;
        }

        private static GameObject CreateModalBackdrop(string name, Transform parent)
        {
            var backdrop = CreateUIObject(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var img = backdrop.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.8f);
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
            var tmp = CreateText(parent, text, font, 12, FontStyles.Bold, color);
            var le = tmp.gameObject.AddComponent<LayoutElement>();
            le.minWidth = width;
            le.preferredWidth = width;
            return tmp;
        }

        private static Button CreateMenuTab(Transform parent, string text, TMP_FontAsset font, Sprite sprite, bool isPrimary = false)
        {
            var go = CreateUIObject("Tab_" + text, parent, Vector2.zero, Vector2.zero, new Vector2(isPrimary ? 80 : 100, 32));
            var img = go.AddComponent<Image>();
            if (isPrimary)
            {
                img.sprite = sprite;
                img.type = Image.Type.Sliced;
                img.color = new Color(0f, 242f / 255f, 254f / 255f, 0.35f);
            }
            else
            {
                img.color = new Color(20f / 255f, 26f / 255f, 38f / 255f, 0.6f);
            }

            var btn = go.AddComponent<Button>();

            var txt = CreateText(go.transform, text, font, 12, FontStyles.Bold, isPrimary ? Color.cyan : Color.white);
            txt.alignment = TextAlignmentOptions.Center;
            txt.enableWordWrapping = false;
            var rt = txt.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return btn;
        }

        private static Button CreateActionButton(Transform parent, string text, TMP_FontAsset font, Sprite sprite)
        {
            var go = CreateUIObject("Btn_" + text.Replace("\n", "_"), parent, Vector2.zero, Vector2.zero, new Vector2(130, 38));
            var img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            var btn = go.AddComponent<Button>();

            var txt = CreateText(go.transform, text, font, 12, FontStyles.Bold, Color.white);
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
            var go = CreateUIObject("Btn_" + text, parent, Vector2.zero, Vector2.zero, new Vector2(28, 28));
            var img = go.AddComponent<Image>();
            img.color = new Color(25f / 255f, 30f / 255f, 42f / 255f);
            var btn = go.AddComponent<Button>();

            var txt = CreateText(go.transform, text, font, 14, FontStyles.Bold, color ?? Color.white);
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
            var tile = CreateUIObject("Tile", parent, Vector2.zero, Vector2.zero, new Vector2(120, 62));
            tile.AddComponent<Image>().color = new Color(15f / 255f, 20f / 255f, 30f / 255f);
            var vlg = tile.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(4, 4, 4, 4);
            vlg.childAlignment = TextAnchor.MiddleCenter;

            var tTitle = CreateText(tile.transform, title, font, 9, FontStyles.Bold, Color.gray);
            tTitle.alignment = TextAlignmentOptions.Center;

            valText = CreateText(tile.transform, val, font, 16, FontStyles.Bold, Color.cyan);
            valText.alignment = TextAlignmentOptions.Center;
            return tile;
        }

        private static GameObject CreateStepper(Transform parent, string label, string val, TMP_FontAsset font, out TextMeshProUGUI valText, out Button btnMinus, out Button btnPlus)
        {
            var box = CreateUIObject("Stepper_" + label, parent, Vector2.zero, Vector2.zero, new Vector2(160, 95));
            box.AddComponent<Image>().color = new Color(15f / 255f, 20f / 255f, 30f / 255f);
            var vlg = box.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(6, 6, 6, 6);
            vlg.spacing = 6;
            vlg.childAlignment = TextAnchor.MiddleCenter;

            var lbl = CreateText(box.transform, label, font, 10, FontStyles.Bold, Color.gray);
            lbl.alignment = TextAlignmentOptions.Center;

            var row = CreateUIObject("Row", box.transform, Vector2.zero, Vector2.zero, new Vector2(140, 36));
            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8;
            hlg.childAlignment = TextAnchor.MiddleCenter;

            btnMinus = CreateSmallSquareButton(row.transform, "—", font);
            valText = CreateText(row.transform, val, font, 20, FontStyles.Bold, Color.white);
            valText.alignment = TextAlignmentOptions.Center;
            btnPlus = CreateSmallSquareButton(row.transform, "+", font);
            return box;
        }

        private static Toggle CreateToggle(Transform parent, string label, TMP_FontAsset font)
        {
            var go = CreateUIObject("Toggle_" + label, parent, Vector2.zero, Vector2.zero, new Vector2(200, 24));
            var tgl = go.AddComponent<Toggle>();
            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            var box = CreateUIObject("Box", go.transform, Vector2.zero, Vector2.zero, new Vector2(18, 18));
            var boxImg = box.AddComponent<Image>();
            boxImg.color = new Color(30f / 255f, 40f / 255f, 55f / 255f);

            var check = CreateUIObject("Check", box.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var checkImg = check.AddComponent<Image>();
            checkImg.color = Color.cyan;
            tgl.graphic = checkImg;
            tgl.targetGraphic = boxImg;

            var lbl = CreateText(go.transform, label, font, 12, FontStyles.Normal, Color.white);
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
