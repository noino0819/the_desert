#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace TheSSand.Editor
{
    public class SceneSetupEditor
    {
        static readonly string ScenesPath = "Assets/Scenes/";
        static readonly string ArtPath = "Assets/Art/";
        static readonly string PrefabPath = "Assets/Prefabs/";
        static readonly string UIPath = "Assets/Art/UI/";

        [MenuItem("The SSand/씬 전체 생성", false, 0)]
        static void CreateAllScenes()
        {
            CreateTitleMenuScene();
            CreatePrologueScene();
            CreateCh1DesertScene();
            CreateCh1OasisScene();
            CreateCh1BossScene();
            CreateCh2Scenes();
            CreateCh3Scenes();
            CreateCh4Scenes();
            CreateEndingScene();
            CreateNGScenes();
            RegisterScenesInBuildSettings();

            Debug.Log("[SceneSetup] 전체 씬 생성 + Build Settings 등록 완료!");
            EditorUtility.DisplayDialog("완료", "전체 씬이 생성되고 Build Settings에 자동 등록되었습니다.", "확인");
        }

        [MenuItem("The SSand/Build Settings 씬 등록", false, 1)]
        static void RegisterScenesInBuildSettings()
        {
            string[] sceneOrder = {
                "SCN_TitleMenu",
                "SCN_Prologue",
                "SCN_Ch1_Desert", "SCN_Ch1_Oasis", "SCN_Ch1_Boss",
                "SCN_Ch2_Desert", "SCN_Ch2_Oasis", "SCN_Ch2_Boss",
                "SCN_Ch3_Desert", "SCN_Ch3_Oasis", "SCN_Ch3_Boss",
                "SCN_Ch4_Desert", "SCN_Ch4_Oasis", "SCN_Ch4_Boss",
                "SCN_Ending",
                "SCN_NG_Ch4_Oasis", "SCN_NG_Ch4_Boss",
                "SCN_NG_Ch3_Oasis", "SCN_NG_Ch3_Boss",
                "SCN_NG_Ch2_Oasis", "SCN_NG_Ch2_Boss",
                "SCN_NG_Ch1_Oasis", "SCN_NG_Ch1_Boss"
            };

            var buildScenes = new EditorBuildSettingsScene[sceneOrder.Length];
            for (int i = 0; i < sceneOrder.Length; i++)
            {
                string path = ScenesPath + sceneOrder[i] + ".unity";
                buildScenes[i] = new EditorBuildSettingsScene(path, true);
            }

            EditorBuildSettings.scenes = buildScenes;
            Debug.Log($"[SceneSetup] Build Settings에 {sceneOrder.Length}개 씬 등록 완료");
        }

        #region 타이틀

        [MenuItem("The SSand/씬 생성/SCN_TitleMenu")]
        static void CreateTitleMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camObj = new GameObject("Main Camera");
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5;
            cam.backgroundColor = new Color(0.93f, 0.87f, 0.73f);
            camObj.tag = "MainCamera";

            var bgSprite = CreateSpriteObject("Background",
                LoadSprite($"{ArtPath}Backgrounds/bg_title.png"), -10);
            bgSprite.transform.position = new Vector3(0, 0, 5);
            bgSprite.transform.localScale = Vector3.one * 1.2f;

            var canvas = CreateCanvas("TitleCanvas");

            var title = CreateTMPText(canvas.transform, "TitleText", "The SSand\n사막의 진실",
                48, TextAlignmentOptions.Center);
            SetAnchoredPos(title.rectTransform, 0, 100, 600, 150);

            CreateStyledButton(canvas.transform, "NewGameButton", "새로 시작", 0, -20);
            CreateStyledButton(canvas.transform, "LoadGameButton", "이어하기", 0, -80);
            var ng2Btn = CreateStyledButton(canvas.transform, "NewGame2Button", "2회차", 0, -140);
            ng2Btn.gameObject.SetActive(false);
            CreateStyledButton(canvas.transform, "SettingsButton", "설정", 0, -200);
            CreateStyledButton(canvas.transform, "QuitButton", "게임 종료", 0, -260);

            var loadPanel = new GameObject("LoadPanel");
            loadPanel.transform.SetParent(canvas.transform, false);
            var loadPanelRect = loadPanel.AddComponent<RectTransform>();
            loadPanelRect.anchorMin = new Vector2(0.3f, 0.2f);
            loadPanelRect.anchorMax = new Vector2(0.7f, 0.8f);
            var loadBg = loadPanel.AddComponent<Image>();
            loadBg.sprite = LoadSprite($"{UIPath}ui_inventory_bg.png");
            loadBg.type = Image.Type.Sliced;
            if (loadBg.sprite == null)
                loadBg.color = new Color(0.8f, 0.7f, 0.5f, 0.95f);

            for (int i = 0; i < 3; i++)
                CreateStyledButton(loadPanel.transform, $"SlotButton_{i}", $"슬롯 {i + 1}: 비어있음", 0, 80 - i * 70);
            CreateStyledButton(loadPanel.transform, "LoadBackButton", "뒤로", 0, -140);
            loadPanel.SetActive(false);

            var controller = new GameObject("TitleMenuController");
            controller.AddComponent<UI.TitleMenuController>();

            EditorSceneManager.SaveScene(scene, ScenesPath + "SCN_TitleMenu.unity");
            Debug.Log("[SceneSetup] SCN_TitleMenu 생성");
        }

        #endregion

        #region 프롤로그

        [MenuItem("The SSand/씬 생성/SCN_Prologue")]
        static void CreatePrologueScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camObj = new GameObject("Main Camera");
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5;
            cam.backgroundColor = new Color(0.2f, 0.15f, 0.1f);
            camObj.tag = "MainCamera";

            var bgSprite = CreateSpriteObject("Background",
                LoadSprite($"{ArtPath}Backgrounds/bg_prologue.png"), -10);
            bgSprite.transform.position = new Vector3(0, 0, 5);
            bgSprite.transform.localScale = Vector3.one * 1.2f;

            var canvas = CreateCanvas("PrologueCanvas");

            var namePanel = new GameObject("NameInputPanel");
            namePanel.transform.SetParent(canvas.transform, false);
            var namePanelRect = namePanel.AddComponent<RectTransform>();
            namePanelRect.anchorMin = new Vector2(0.2f, 0.3f);
            namePanelRect.anchorMax = new Vector2(0.8f, 0.7f);
            var nameBg = namePanel.AddComponent<Image>();
            nameBg.sprite = LoadSprite($"{UIPath}ui_dialogue_box.png");
            nameBg.type = Image.Type.Sliced;
            if (nameBg.sprite == null)
                nameBg.color = new Color(0.93f, 0.87f, 0.73f, 0.9f);

            var prompt = CreateTMPText(namePanel.transform, "PromptText",
                "이 이야기의 주인공 이름을 지어줄래?", 24, TextAlignmentOptions.Center);
            SetAnchoredPos(prompt.rectTransform, 0, 60, 400, 40);

            var inputObj = new GameObject("NameInput");
            inputObj.transform.SetParent(namePanel.transform, false);
            var inputRect = inputObj.AddComponent<RectTransform>();
            SetAnchoredPos(inputRect, 0, 0, 300, 50);
            inputObj.AddComponent<Image>().color = new Color(1f, 0.98f, 0.92f);
            var inputField = inputObj.AddComponent<TMP_InputField>();
            var inputText = CreateTMPText(inputObj.transform, "Text", "", 20, TextAlignmentOptions.Left);
            inputField.textComponent = inputText;

            CreateStyledButton(namePanel.transform, "ConfirmButton", "확인", 0, -70);

            var storyPanel = new GameObject("StoryPanel");
            storyPanel.transform.SetParent(canvas.transform, false);
            var storyRect = storyPanel.AddComponent<RectTransform>();
            storyRect.anchorMin = Vector2.zero;
            storyRect.anchorMax = Vector2.one;
            storyPanel.AddComponent<Image>().color = new Color(0.15f, 0.1f, 0.08f, 0.95f);
            CreateTMPText(storyPanel.transform, "StoryText", "", 28, TextAlignmentOptions.Center);
            storyPanel.SetActive(false);

            var controller = new GameObject("PrologueController");
            controller.AddComponent<UI.PrologueController>();

            EditorSceneManager.SaveScene(scene, ScenesPath + "SCN_Prologue.unity");
            Debug.Log("[SceneSetup] SCN_Prologue 생성");
        }

        #endregion

        #region Ch1 사막

        [MenuItem("The SSand/씬 생성/SCN_Ch1_Desert")]
        static void CreateCh1DesertScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camObj = new GameObject("Main Camera");
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5;
            cam.backgroundColor = new Color(0.53f, 0.81f, 0.98f);
            camObj.tag = "MainCamera";

            var bgNames = new[] { "bg_desert_sky", "bg_desert_mid", "bg_desert_near" };
            for (int i = 0; i < bgNames.Length; i++)
            {
                var bgLayer = CreateSpriteObject($"BG_Layer_{i}",
                    LoadSprite($"{ArtPath}Backgrounds/{bgNames[i]}.png"), -10 + i);
                bgLayer.transform.position = new Vector3(0, 0, 10 - i);
                bgLayer.transform.localScale = Vector3.one * 2f;
            }

            var ground = CreateTiledGround("Ground",
                LoadSprite($"{ArtPath}Tilesets/tileset_desert.png"),
                200f, 2f, new Vector3(50, -3, 0));

            var player = InstantiateOrCreate("Player",
                $"{PrefabPath}Characters/Player.prefab",
                new Vector3(-8, 0, 0));

            CreateTutorialTrigger("Tut_Move", new Vector3(-5, 0, 0), "A/D 키로 이동, Shift로 달리기");
            CreateTutorialTrigger("Tut_Jump", new Vector3(5, 0, 0), "Space 키로 점프");
            CreateTutorialTrigger("Tut_Interact", new Vector3(15, 0, 0), "E 키로 상호작용");

            var note = InstantiateOrCreate("Note_Ch1",
                $"{PrefabPath}Items/Note_Ch1.prefab",
                new Vector3(25, -1.5f, 0));

            PlaceDesertProps();

            var exitTrigger = new GameObject("ExitTrigger_ToOasis");
            exitTrigger.transform.position = new Vector3(95, 0, 0);
            var exitCol = exitTrigger.AddComponent<BoxCollider2D>();
            exitCol.isTrigger = true;
            exitCol.size = new Vector2(2, 10);
            exitTrigger.AddComponent<Level.SceneTransitionTrigger>();

            var levelCtrl = new GameObject("DesertLevelController");
            levelCtrl.AddComponent<Level.DesertLevelController>();

            CreateDialogueUI(scene);
            CreateInGameHUD();

            EditorSceneManager.SaveScene(scene, ScenesPath + "SCN_Ch1_Desert.unity");
            Debug.Log("[SceneSetup] SCN_Ch1_Desert 생성");
        }

        static void PlaceDesertProps()
        {
            var propsSprite = LoadSprite($"{ArtPath}Environment/env_desert_props.png");
            float[] xPositions = { 10, 20, 35, 50, 65, 75, 85 };
            for (int i = 0; i < xPositions.Length; i++)
            {
                var prop = CreateSpriteObject($"DesertProp_{i}", propsSprite, -3);
                float yOffset = Random.Range(-2.5f, -1.5f);
                prop.transform.position = new Vector3(xPositions[i], yOffset, 0);
            }
        }

        #endregion

        #region Ch1 오아시스

        [MenuItem("The SSand/씬 생성/SCN_Ch1_Oasis")]
        static void CreateCh1OasisScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camObj = new GameObject("Main Camera");
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5;
            cam.backgroundColor = new Color(0.53f, 0.81f, 0.92f);
            camObj.tag = "MainCamera";

            var bgObj = CreateSpriteObject("Background",
                LoadSprite($"{ArtPath}Backgrounds/bg_oasis.png"), -10);
            bgObj.transform.position = new Vector3(0, 0, 5);
            bgObj.transform.localScale = Vector3.one * 2f;

            var ground = CreateTiledGround("Ground",
                LoadSprite($"{ArtPath}Tilesets/tileset_oasis.png"),
                60f, 2f, new Vector3(0, -3, 0));

            var player = InstantiateOrCreate("Player",
                $"{PrefabPath}Characters/Player.prefab",
                new Vector3(-12, 0, 0));

            InstantiateOrCreate("NPC_CaptainBoy",
                $"{PrefabPath}NPC/NPC_CaptainBoy.prefab",
                new Vector3(0, -1.5f, 0));

            InstantiateOrCreate("NPC_Villager_1",
                $"{PrefabPath}NPC/NPC_Villager_1.prefab",
                new Vector3(-5, -1.5f, 0));
            InstantiateOrCreate("NPC_Villager_2",
                $"{PrefabPath}NPC/NPC_Villager_2.prefab",
                new Vector3(5, -1.5f, 0));
            InstantiateOrCreate("NPC_Villager_3",
                $"{PrefabPath}NPC/NPC_Villager_3.prefab",
                new Vector3(10, -1.5f, 0));

            InstantiateOrCreate("SavePoint",
                $"{PrefabPath}Environment/SavePoint.prefab",
                new Vector3(-2, -1.5f, 0));

            PlaceOasisBuildings();

            var bossZone = new GameObject("BossZoneTrigger");
            bossZone.transform.position = new Vector3(20, 0, 0);
            var bossCol = bossZone.AddComponent<BoxCollider2D>();
            bossCol.isTrigger = true;
            bossCol.size = new Vector2(2, 10);
            bossZone.AddComponent<Level.SceneTransitionTrigger>();
            bossZone.SetActive(false);

            var oasisCtrl = new GameObject("OasisController");
            oasisCtrl.AddComponent<Level.OasisController>();

            CreateDialogueUI(scene);
            CreateInGameHUD();

            EditorSceneManager.SaveScene(scene, ScenesPath + "SCN_Ch1_Oasis.unity");
            Debug.Log("[SceneSetup] SCN_Ch1_Oasis 생성");
        }

        static void PlaceOasisBuildings()
        {
            var buildingSprite = LoadSprite($"{ArtPath}Environment/env_oasis_buildings.png");

            var captainHouse = CreateSpriteObject("Building_CaptainHouse", buildingSprite, -2);
            captainHouse.transform.position = new Vector3(0, 1, 0);

            var house1 = CreateSpriteObject("Building_House1", buildingSprite, -2);
            house1.transform.position = new Vector3(-5, 1, 0);

            var house2 = CreateSpriteObject("Building_House2", buildingSprite, -2);
            house2.transform.position = new Vector3(5, 1, 0);

            var well = CreateSpriteObject("Building_Well", buildingSprite, -2);
            well.transform.position = new Vector3(10, 0.5f, 0);
        }

        #endregion

        #region Ch1 보스

        [MenuItem("The SSand/씬 생성/SCN_Ch1_Boss")]
        static void CreateCh1BossScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camObj = new GameObject("Main Camera");
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6;
            cam.backgroundColor = new Color(0.6f, 0.5f, 0.4f);
            camObj.tag = "MainCamera";

            var bgObj = CreateSpriteObject("Background",
                LoadSprite($"{ArtPath}Backgrounds/bg_boss_arena.png"), -10);
            bgObj.transform.position = new Vector3(0, 0, 5);
            bgObj.transform.localScale = Vector3.one * 1.5f;

            CreateArenaWall("ArenaWallLeft", new Vector3(-7, 0, 0));
            CreateArenaWall("ArenaWallRight", new Vector3(7, 0, 0));

            var arenaFloor = new GameObject("ArenaFloor");
            arenaFloor.tag = "Ground";
            var floorCol = arenaFloor.AddComponent<BoxCollider2D>();
            floorCol.size = new Vector2(14, 1);
            arenaFloor.transform.position = new Vector3(0, -5, 0);
            var floorSr = arenaFloor.AddComponent<SpriteRenderer>();
            floorSr.sprite = LoadSprite($"{ArtPath}Tilesets/tileset_desert.png");
            floorSr.sortingOrder = -1;
            floorSr.drawMode = SpriteDrawMode.Tiled;
            floorSr.size = new Vector2(14, 1);

            var player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = new Vector3(0, -3.5f, 0);
            var playerSr = player.AddComponent<SpriteRenderer>();
            playerSr.sprite = LoadSprite($"{ArtPath}Characters/hero_spritesheet.png");
            playerSr.sortingOrder = 10;
            var playerCol = player.AddComponent<BoxCollider2D>();
            playerCol.size = new Vector2(0.5f, 0.5f);
            playerCol.isTrigger = true;

            var hippo = InstantiateOrCreate("Boss_Hippo",
                $"{PrefabPath}Boss/Boss_Hippo.prefab",
                new Vector3(0, 3, 0));

            var uiCanvas = CreateCanvas("BossUICanvas");

            var hpBarBg = new GameObject("BossHPBar");
            hpBarBg.transform.SetParent(uiCanvas.transform, false);
            var hpRect = hpBarBg.AddComponent<RectTransform>();
            hpRect.anchorMin = new Vector2(0.2f, 0.9f);
            hpRect.anchorMax = new Vector2(0.8f, 0.95f);
            var hpImg = hpBarBg.AddComponent<Image>();
            hpImg.sprite = LoadSprite($"{UIPath}ui_boss_hpbar.png");
            hpImg.type = Image.Type.Sliced;
            if (hpImg.sprite == null) hpImg.color = new Color(0.5f, 0.3f, 0.2f);

            var hpFill = new GameObject("HPFill");
            hpFill.transform.SetParent(hpBarBg.transform, false);
            var fillRect = hpFill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(4, 2);
            fillRect.offsetMax = new Vector2(-4, -2);
            hpFill.AddComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f);

            var bossNameText = CreateTMPText(uiCanvas.transform, "BossNameText",
                "사막하마", 28, TextAlignmentOptions.Center);
            SetAnchoredPos(bossNameText.rectTransform, 0, -30, 400, 40);
            bossNameText.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            bossNameText.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            bossNameText.color = Color.white;

            var bossUI = new GameObject("BossUIController");
            bossUI.transform.SetParent(uiCanvas.transform, false);
            bossUI.AddComponent<Boss.BossUIController>();

            var ctrl = new GameObject("Ch1BossController");
            ctrl.AddComponent<Boss.Ch1BossController>();

            EditorSceneManager.SaveScene(scene, ScenesPath + "SCN_Ch1_Boss.unity");
            Debug.Log("[SceneSetup] SCN_Ch1_Boss 생성");
        }

        static void CreateArenaWall(string name, Vector3 pos)
        {
            var wall = new GameObject(name);
            var col = wall.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1, 14);
            wall.transform.position = pos;
        }

        #endregion

        #region Ch.2~4 + 엔딩 + NG 씬

        [MenuItem("The SSand/씬 생성/Ch.2 전체")]
        static void CreateCh2Scenes()
        {
            CreateDesertScene("SCN_Ch2_Desert", new Color(0.95f, 0.65f, 0.35f),
                "BGM_Ch2_Desert", "SCN_Ch2_Oasis");
            CreateOasisScene("SCN_Ch2_Oasis", new Color(0.45f, 0.75f, 0.55f),
                "잼잼크래프트", "SCN_Ch2_Boss", typeof(Level.Ch2OasisController));
            CreateBossScene("SCN_Ch2_Boss", "독수리", new Color(0.55f, 0.75f, 0.95f),
                typeof(Boss.BossEagle), typeof(Boss.Ch2BossController), "SCN_Ch3_Desert");
        }

        [MenuItem("The SSand/씬 생성/Ch.3 전체")]
        static void CreateCh3Scenes()
        {
            CreateDesertScene("SCN_Ch3_Desert", new Color(0.6f, 0.4f, 0.65f),
                "BGM_Ch3_Desert", "SCN_Ch3_Oasis");
            CreateOasisScene("SCN_Ch3_Oasis", new Color(0.5f, 0.6f, 0.55f),
                "솔", "SCN_Ch3_Boss", typeof(Level.Ch3OasisController));
            CreateBossScene("SCN_Ch3_Boss", "늑대", new Color(0.25f, 0.2f, 0.35f),
                typeof(Boss.BossWolf), typeof(Boss.Ch3BossController), "SCN_Ch4_Desert");
        }

        [MenuItem("The SSand/씬 생성/Ch.4 전체")]
        static void CreateCh4Scenes()
        {
            CreateDesertScene("SCN_Ch4_Desert", new Color(0.15f, 0.18f, 0.3f),
                "BGM_Ch4_Desert", "SCN_Ch4_Oasis");
            CreateOasisScene("SCN_Ch4_Oasis", new Color(0.3f, 0.35f, 0.45f),
                "연구자", "SCN_Ch4_Boss", typeof(Level.Ch4OasisController));
            CreateBossScene("SCN_Ch4_Boss", "과일거북", new Color(0.2f, 0.25f, 0.35f),
                typeof(Boss.BossTurtle), typeof(Boss.Ch4BossController), "SCN_Ending");
        }

        [MenuItem("The SSand/씬 생성/SCN_Ending")]
        static void CreateEndingScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camObj = new GameObject("Main Camera");
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5;
            cam.backgroundColor = new Color(0.2f, 0.15f, 0.1f);
            camObj.tag = "MainCamera";

            var canvas = CreateCanvas("EndingCanvas");
            var storyPanel = new GameObject("StoryPanel");
            storyPanel.transform.SetParent(canvas.transform, false);
            var storyRect = storyPanel.AddComponent<RectTransform>();
            storyRect.anchorMin = Vector2.zero;
            storyRect.anchorMax = Vector2.one;
            storyPanel.AddComponent<Image>().color = new Color(0.1f, 0.08f, 0.06f, 0.95f);

            CreateTMPText(storyPanel.transform, "EndingText", "", 28, TextAlignmentOptions.Center);

            var endingCtrl = new GameObject("EndingController");
            endingCtrl.AddComponent<UI.EndingController>();

            var endingMgr = new GameObject("EndingManager");
            endingMgr.AddComponent<Core.EndingManager>();

            EditorSceneManager.SaveScene(scene, ScenesPath + "SCN_Ending.unity");
            Debug.Log("[SceneSetup] SCN_Ending 생성");
        }

        [MenuItem("The SSand/씬 생성/NG 전체")]
        static void CreateNGScenes()
        {
            string[] ngScenes = {
                "SCN_NG_Ch4_Oasis", "SCN_NG_Ch4_Boss",
                "SCN_NG_Ch3_Oasis", "SCN_NG_Ch3_Boss",
                "SCN_NG_Ch2_Oasis", "SCN_NG_Ch2_Boss",
                "SCN_NG_Ch1_Oasis", "SCN_NG_Ch1_Boss"
            };

            foreach (var sceneName in ngScenes)
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                var camObj = new GameObject("Main Camera");
                var cam = camObj.AddComponent<Camera>();
                cam.orthographic = true;
                cam.orthographicSize = 5;
                cam.backgroundColor = new Color(0.15f, 0.12f, 0.18f);
                camObj.tag = "MainCamera";

                var ctrl = new GameObject("NGChapterController");
                ctrl.AddComponent<Level.NGChapterController>();

                if (sceneName.Contains("Boss"))
                {
                    var ngMod = new GameObject("NGBossModifier");
                    ngMod.AddComponent<Boss.NGBossModifier>();
                }

                CreateDialogueUI(scene);
                CreateInGameHUD();

                EditorSceneManager.SaveScene(scene, ScenesPath + sceneName + ".unity");
                Debug.Log($"[SceneSetup] {sceneName} 생성");
            }
        }

        static void CreateDesertScene(string sceneName, Color skyColor, string bgmName, string nextScene)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camObj = new GameObject("Main Camera");
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5;
            cam.backgroundColor = skyColor;
            camObj.tag = "MainCamera";
            camObj.AddComponent<CameraSystem.CameraController>();

            var ground = CreateTiledGround("Ground",
                LoadSprite($"{ArtPath}Tilesets/tileset_desert.png"),
                200f, 2f, new Vector3(50, -3, 0));

            var player = InstantiateOrCreate("Player",
                $"{PrefabPath}Characters/Player.prefab", new Vector3(-8, 0, 0));

            var exitTrigger = new GameObject("ExitTrigger");
            exitTrigger.transform.position = new Vector3(95, 0, 0);
            var exitCol = exitTrigger.AddComponent<BoxCollider2D>();
            exitCol.isTrigger = true;
            exitCol.size = new Vector2(2, 10);
            exitTrigger.AddComponent<Level.SceneTransitionTrigger>();

            for (int i = 0; i < 3; i++)
            {
                var cp = new GameObject($"Checkpoint_{i}");
                cp.transform.position = new Vector3(20 + i * 25, -1.5f, 0);
                var cpCol = cp.AddComponent<BoxCollider2D>();
                cpCol.isTrigger = true;
                cpCol.size = new Vector2(1f, 2f);
                cp.AddComponent<Level.Checkpoint>();
            }

            var levelCtrl = new GameObject("DesertLevelController");
            levelCtrl.AddComponent<Level.DesertLevelController>();

            CreateDialogueUI(scene);
            CreateInGameHUD();
            CreateGameplayUI(scene);

            EditorSceneManager.SaveScene(scene, ScenesPath + sceneName + ".unity");
            Debug.Log($"[SceneSetup] {sceneName} 생성");
        }

        static void CreateOasisScene(string sceneName, Color bgColor, string mainNPC, string bossScene,
            System.Type oasisControllerType = null)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camObj = new GameObject("Main Camera");
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5;
            cam.backgroundColor = bgColor;
            camObj.tag = "MainCamera";
            camObj.AddComponent<CameraSystem.CameraController>();

            var ground = CreateTiledGround("Ground",
                LoadSprite($"{ArtPath}Tilesets/tileset_oasis.png"),
                60f, 2f, new Vector3(0, -3, 0));

            var player = InstantiateOrCreate("Player",
                $"{PrefabPath}Characters/Player.prefab", new Vector3(-12, 0, 0));

            var npc = new GameObject($"NPC_{mainNPC}");
            npc.transform.position = new Vector3(0, -1.5f, 0);
            npc.AddComponent<Level.NPCInteractable>();

            var savePoint = new GameObject("SavePoint");
            savePoint.transform.position = new Vector3(-5, -1.5f, 0);
            savePoint.AddComponent<Level.SavePoint>();

            var checkpoint = new GameObject("Checkpoint");
            checkpoint.transform.position = new Vector3(-8, -1.5f, 0);
            var cpCol = checkpoint.AddComponent<BoxCollider2D>();
            cpCol.isTrigger = true;
            cpCol.size = new Vector2(1f, 2f);
            checkpoint.AddComponent<Level.Checkpoint>();

            var bossZone = new GameObject("BossZoneTrigger");
            bossZone.transform.position = new Vector3(20, 0, 0);
            var bossCol = bossZone.AddComponent<BoxCollider2D>();
            bossCol.isTrigger = true;
            bossCol.size = new Vector2(2, 10);
            bossZone.AddComponent<Level.SceneTransitionTrigger>();
            bossZone.SetActive(false);

            var oasisCtrl = new GameObject("OasisController");
            if (oasisControllerType != null)
                oasisCtrl.AddComponent(oasisControllerType);
            else
                oasisCtrl.AddComponent<Level.OasisController>();

            CreateDialogueUI(scene);
            CreateInGameHUD();
            CreateGameplayUI(scene);

            EditorSceneManager.SaveScene(scene, ScenesPath + sceneName + ".unity");
            Debug.Log($"[SceneSetup] {sceneName} 생성");
        }

        static void CreateBossScene(string sceneName, string bossName, Color bgColor,
            System.Type bossType, System.Type controllerType, string nextScene)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camObj = new GameObject("Main Camera");
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6;
            cam.backgroundColor = bgColor;
            camObj.tag = "MainCamera";

            var player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = new Vector3(0, -3.5f, 0);
            player.AddComponent<SpriteRenderer>().sortingOrder = 10;
            player.AddComponent<BoxCollider2D>().isTrigger = true;

            var boss = new GameObject($"Boss_{bossName}");
            boss.transform.position = new Vector3(0, 3, 0);
            boss.AddComponent(bossType);
            boss.AddComponent<Boss.NGBossModifier>();

            var uiCanvas = CreateCanvas("BossUICanvas");
            var bossUI = new GameObject("BossUIController");
            bossUI.transform.SetParent(uiCanvas.transform, false);
            bossUI.AddComponent<Boss.BossUIController>();

            var ctrl = new GameObject($"BossController");
            ctrl.AddComponent(controllerType);

            EditorSceneManager.SaveScene(scene, ScenesPath + sceneName + ".unity");
            Debug.Log($"[SceneSetup] {sceneName} 생성");
        }

        #endregion

        #region 공용 대화 UI

        static void CreateDialogueUI(UnityEngine.SceneManagement.Scene scene)
        {
            var dialogueCanvas = CreateCanvas("DialogueCanvas");
            dialogueCanvas.sortingOrder = 200;

            var dialoguePanel = new GameObject("DialoguePanel");
            dialoguePanel.transform.SetParent(dialogueCanvas.transform, false);
            var panelRect = dialoguePanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.05f, 0.02f);
            panelRect.anchorMax = new Vector2(0.95f, 0.3f);
            var panelImg = dialoguePanel.AddComponent<Image>();
            panelImg.sprite = LoadSprite($"{UIPath}ui_dialogue_box.png");
            panelImg.type = Image.Type.Sliced;
            if (panelImg.sprite == null)
                panelImg.color = new Color(0.93f, 0.87f, 0.73f, 0.95f);

            var portrait = new GameObject("Portrait");
            portrait.transform.SetParent(dialoguePanel.transform, false);
            var portraitRect = portrait.AddComponent<RectTransform>();
            portraitRect.anchorMin = new Vector2(0, 0);
            portraitRect.anchorMax = new Vector2(0, 1);
            portraitRect.pivot = new Vector2(0, 0.5f);
            portraitRect.sizeDelta = new Vector2(120, 0);
            portraitRect.anchoredPosition = new Vector2(10, 0);
            portrait.AddComponent<Image>().color = new Color(1, 1, 1, 0);

            var speakerName = CreateTMPText(dialoguePanel.transform, "SpeakerName",
                "", 20, TextAlignmentOptions.TopLeft);
            speakerName.rectTransform.anchorMin = new Vector2(0.15f, 0.75f);
            speakerName.rectTransform.anchorMax = new Vector2(0.6f, 0.95f);
            speakerName.fontStyle = FontStyles.Bold;

            var dialogueText = CreateTMPText(dialoguePanel.transform, "DialogueText",
                "", 18, TextAlignmentOptions.TopLeft);
            dialogueText.rectTransform.anchorMin = new Vector2(0.15f, 0.08f);
            dialogueText.rectTransform.anchorMax = new Vector2(0.95f, 0.72f);

            var choicePanel = new GameObject("ChoicePanel");
            choicePanel.transform.SetParent(dialogueCanvas.transform, false);
            var choiceRect = choicePanel.AddComponent<RectTransform>();
            choiceRect.anchorMin = new Vector2(0.3f, 0.35f);
            choiceRect.anchorMax = new Vector2(0.7f, 0.65f);

            var vlg = choicePanel.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            for (int i = 0; i < 3; i++)
            {
                var choiceBtn = CreateStyledButton(choicePanel.transform, $"Choice_{i}", $"선택지 {i + 1}", 0, 0);
                var le = choiceBtn.gameObject.AddComponent<LayoutElement>();
                le.preferredHeight = 45;
            }

            choicePanel.SetActive(false);
            dialoguePanel.SetActive(false);
        }

        static void CreateGameplayUI(UnityEngine.SceneManagement.Scene scene)
        {
            var uiCanvas = CreateCanvas("GameplayUICanvas");
            uiCanvas.sortingOrder = 300;

            var pauseObj = new GameObject("PauseMenu");
            pauseObj.transform.SetParent(uiCanvas.transform, false);
            pauseObj.AddComponent<UI.PauseMenu>();

            var gameOverObj = new GameObject("GameOverUI");
            gameOverObj.transform.SetParent(uiCanvas.transform, false);
            gameOverObj.AddComponent<UI.GameOverUI>();

            var tooltipObj = new GameObject("ItemTooltip");
            tooltipObj.transform.SetParent(uiCanvas.transform, false);
            tooltipObj.AddComponent<UI.ItemTooltip>();

            var notifContainer = new GameObject("NotificationContainer");
            notifContainer.transform.SetParent(uiCanvas.transform, false);
            var ncRect = notifContainer.AddComponent<RectTransform>();
            ncRect.anchorMin = new Vector2(0.65f, 0.6f);
            ncRect.anchorMax = new Vector2(0.98f, 0.95f);
            var vlg = notifContainer.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 5;
            vlg.childAlignment = TextAnchor.UpperRight;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            var notifUI = notifContainer.AddComponent<UI.NotificationUI>();

            var narrationObj = new GameObject("NarrationUI");
            narrationObj.transform.SetParent(uiCanvas.transform, false);
            narrationObj.AddComponent<UI.NarrationUI>();

            var choiceUIObj = new GameObject("ChoiceUI");
            choiceUIObj.transform.SetParent(uiCanvas.transform, false);
            choiceUIObj.AddComponent<UI.ChoiceUI>();
        }

        #endregion

        #region 헬퍼

        static Canvas CreateCanvas(string name)
        {
            var obj = new GameObject(name);
            var canvas = obj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = obj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            obj.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        static TextMeshProUGUI CreateTMPText(Transform parent, string name, string text,
            int fontSize, TextAlignmentOptions alignment)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = new Color(0.2f, 0.15f, 0.1f);

            var rect = tmp.rectTransform;
            rect.anchorMin = new Vector2(0.1f, 0.1f);
            rect.anchorMax = new Vector2(0.9f, 0.9f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            return tmp;
        }

        static Button CreateStyledButton(Transform parent, string name, string label, float x, float y)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            var rect = obj.AddComponent<RectTransform>();
            SetAnchoredPos(rect, x, y, 250, 50);

            var img = obj.AddComponent<Image>();
            var btnSprite = LoadSprite($"{UIPath}ui_choice_button.png");
            if (btnSprite != null)
            {
                img.sprite = btnSprite;
                img.type = Image.Type.Sliced;
            }
            else
            {
                img.color = new Color(0.65f, 0.5f, 0.3f);
            }

            var btn = obj.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.95f, 0.85f);
            colors.pressedColor = new Color(0.85f, 0.75f, 0.6f);
            btn.colors = colors;

            var textObj = new GameObject("Label");
            textObj.transform.SetParent(obj.transform, false);
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 22;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.2f, 0.15f, 0.1f);
            var textRect = tmp.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return btn;
        }

        static void SetAnchoredPos(RectTransform rect, float x, float y, float w, float h)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(w, h);
        }

        static GameObject CreateSpriteObject(string name, Sprite sprite, int sortOrder)
        {
            var obj = new GameObject(name);
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortOrder;
            return obj;
        }

        static GameObject CreateTiledGround(string name, Sprite sprite, float width, float height, Vector3 pos)
        {
            var obj = new GameObject(name);
            obj.tag = "Ground";
            obj.transform.position = pos;

            var col = obj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(width, height);

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -1;
            if (sprite != null)
            {
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = new Vector2(width, height);
            }
            else
            {
                sr.color = new Color(0.85f, 0.75f, 0.55f);
            }

            return obj;
        }

        static void CreateTutorialTrigger(string name, Vector3 pos, string message)
        {
            var obj = new GameObject(name);
            obj.transform.position = pos;
            var col = obj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(3, 5);
            obj.AddComponent<Level.TutorialTrigger>();
        }

        static GameObject InstantiateOrCreate(string name, string prefabPath, Vector3 pos)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            GameObject obj;

            if (prefab != null)
            {
                obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                obj.name = name;
            }
            else
            {
                obj = new GameObject(name);
                Debug.LogWarning($"[SceneSetup] 프리팹 없음: {prefabPath} — 빈 오브젝트로 대체");
            }

            obj.transform.position = pos;
            return obj;
        }

        static Sprite LoadSprite(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static void CreateInGameHUD()
        {
            var hudCanvas = CreateCanvas("HUDCanvas");
            hudCanvas.sortingOrder = 150;

            var heartPanel = new GameObject("HeartPanel");
            heartPanel.transform.SetParent(hudCanvas.transform, false);
            var heartRect = heartPanel.AddComponent<RectTransform>();
            heartRect.anchorMin = new Vector2(0, 1);
            heartRect.anchorMax = new Vector2(0, 1);
            heartRect.pivot = new Vector2(0, 1);
            heartRect.anchoredPosition = new Vector2(20, -20);
            heartRect.sizeDelta = new Vector2(200, 50);

            var hlg = heartPanel.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            for (int i = 0; i < 3; i++)
            {
                var heart = new GameObject($"Heart_{i}");
                heart.transform.SetParent(heartPanel.transform, false);
                var hImg = heart.AddComponent<Image>();
                hImg.color = Color.red;
                var hRect = heart.GetComponent<RectTransform>();
                hRect.sizeDelta = new Vector2(40, 40);
            }

            var goldPanel = new GameObject("GoldPanel");
            goldPanel.transform.SetParent(hudCanvas.transform, false);
            var goldRect = goldPanel.AddComponent<RectTransform>();
            goldRect.anchorMin = new Vector2(0, 1);
            goldRect.anchorMax = new Vector2(0, 1);
            goldRect.pivot = new Vector2(0, 1);
            goldRect.anchoredPosition = new Vector2(20, -70);
            goldRect.sizeDelta = new Vector2(200, 40);

            var coinIcon = new GameObject("CoinIcon");
            coinIcon.transform.SetParent(goldPanel.transform, false);
            var coinImg = coinIcon.AddComponent<Image>();
            coinImg.color = new Color(1f, 0.85f, 0f);
            var coinRect = coinIcon.GetComponent<RectTransform>();
            coinRect.anchorMin = new Vector2(0, 0.5f);
            coinRect.anchorMax = new Vector2(0, 0.5f);
            coinRect.pivot = new Vector2(0, 0.5f);
            coinRect.anchoredPosition = Vector2.zero;
            coinRect.sizeDelta = new Vector2(30, 30);

            var goldText = CreateTMPText(goldPanel.transform, "GoldText", "0", 24, TextAlignmentOptions.Left);
            goldText.color = Color.white;
            goldText.rectTransform.anchorMin = new Vector2(0, 0);
            goldText.rectTransform.anchorMax = new Vector2(1, 1);
            goldText.rectTransform.offsetMin = new Vector2(40, 0);
            goldText.rectTransform.offsetMax = Vector2.zero;

            var locationText = CreateTMPText(hudCanvas.transform, "LocationText", "", 20, TextAlignmentOptions.Center);
            locationText.color = new Color(1, 1, 1, 0.8f);
            locationText.rectTransform.anchorMin = new Vector2(0.3f, 0.92f);
            locationText.rectTransform.anchorMax = new Vector2(0.7f, 0.98f);

            var interactPrompt = new GameObject("InteractPrompt");
            interactPrompt.transform.SetParent(hudCanvas.transform, false);
            var ipRect = interactPrompt.AddComponent<RectTransform>();
            ipRect.anchorMin = new Vector2(0.5f, 0.15f);
            ipRect.anchorMax = new Vector2(0.5f, 0.15f);
            ipRect.sizeDelta = new Vector2(150, 40);
            interactPrompt.AddComponent<Image>().color = new Color(0, 0, 0, 0.6f);
            var ipText = CreateTMPText(interactPrompt.transform, "PromptLabel", "E  상호작용",
                18, TextAlignmentOptions.Center);
            ipText.color = Color.white;
            ipText.rectTransform.anchorMin = Vector2.zero;
            ipText.rectTransform.anchorMax = Vector2.one;
            ipText.rectTransform.offsetMin = Vector2.zero;
            ipText.rectTransform.offsetMax = Vector2.zero;
            interactPrompt.SetActive(false);

            var hudCtrl = hudCanvas.gameObject.AddComponent<UI.InGameHUD>();
        }

        #endregion
    }
}
#endif
