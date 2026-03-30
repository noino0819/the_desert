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

        [MenuItem("The SSand/씬 전체 생성", false, 0)]
        static void CreateAllScenes()
        {
            CreateTitleMenuScene();
            CreatePrologueScene();
            CreateCh1DesertScene();
            CreateCh1OasisScene();
            CreateCh1BossScene();

            Debug.Log("[SceneSetup] 5개 씬 생성 완료!");
            EditorUtility.DisplayDialog("완료", "5개 씬이 생성되었습니다.\nBuild Settings에 추가해주세요.", "확인");
        }

        [MenuItem("The SSand/씬 생성/SCN_TitleMenu")]
        static void CreateTitleMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 카메라
            var camObj = new GameObject("Main Camera");
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5;
            cam.backgroundColor = new Color(0.93f, 0.87f, 0.73f);
            camObj.tag = "MainCamera";

            // Canvas
            var canvas = CreateCanvas("TitleCanvas");

            // 배경
            var bg = CreateUIImage(canvas.transform, "Background", new Color(0.93f, 0.87f, 0.73f));
            bg.rectTransform.anchorMin = Vector2.zero;
            bg.rectTransform.anchorMax = Vector2.one;

            // 타이틀
            var title = CreateTMPText(canvas.transform, "TitleText", "The SSand\n사막의 진실",
                48, TextAlignmentOptions.Center);
            SetAnchoredPos(title.rectTransform, 0, 100, 600, 150);

            // 버튼들
            CreateButton(canvas.transform, "NewGameButton", "새로 시작", 0, -20);
            CreateButton(canvas.transform, "LoadGameButton", "이어하기", 0, -80);
            var ng2Btn = CreateButton(canvas.transform, "NewGame2Button", "2회차", 0, -140);
            ng2Btn.gameObject.SetActive(false);
            CreateButton(canvas.transform, "SettingsButton", "설정", 0, -200);
            CreateButton(canvas.transform, "QuitButton", "게임 종료", 0, -260);

            // 로드 패널
            var loadPanel = new GameObject("LoadPanel");
            loadPanel.transform.SetParent(canvas.transform, false);
            var loadPanelRect = loadPanel.AddComponent<RectTransform>();
            loadPanelRect.anchorMin = new Vector2(0.3f, 0.2f);
            loadPanelRect.anchorMax = new Vector2(0.7f, 0.8f);
            loadPanel.AddComponent<Image>().color = new Color(0.8f, 0.7f, 0.5f, 0.95f);
            for (int i = 0; i < 3; i++)
                CreateButton(loadPanel.transform, $"SlotButton_{i}", $"슬롯 {i + 1}: 비어있음", 0, 80 - i * 70);
            CreateButton(loadPanel.transform, "LoadBackButton", "뒤로", 0, -140);
            loadPanel.SetActive(false);

            // 컨트롤러 추가
            var controller = new GameObject("TitleMenuController");
            controller.AddComponent<UI.TitleMenuController>();

            EditorSceneManager.SaveScene(scene, ScenesPath + "SCN_TitleMenu.unity");
            Debug.Log("[SceneSetup] SCN_TitleMenu 생성");
        }

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

            var canvas = CreateCanvas("PrologueCanvas");

            // 이름 입력 패널
            var namePanel = new GameObject("NameInputPanel");
            namePanel.transform.SetParent(canvas.transform, false);
            var namePanelRect = namePanel.AddComponent<RectTransform>();
            namePanelRect.anchorMin = new Vector2(0.2f, 0.3f);
            namePanelRect.anchorMax = new Vector2(0.8f, 0.7f);
            namePanel.AddComponent<Image>().color = new Color(0.93f, 0.87f, 0.73f, 0.9f);

            var prompt = CreateTMPText(namePanel.transform, "PromptText",
                "이 이야기의 주인공 이름을 지어줄래?", 24, TextAlignmentOptions.Center);
            SetAnchoredPos(prompt.rectTransform, 0, 60, 400, 40);

            var inputObj = new GameObject("NameInput");
            inputObj.transform.SetParent(namePanel.transform, false);
            var inputRect = inputObj.AddComponent<RectTransform>();
            SetAnchoredPos(inputRect, 0, 0, 300, 50);
            inputObj.AddComponent<Image>().color = Color.white;
            var inputField = inputObj.AddComponent<TMP_InputField>();
            var inputText = CreateTMPText(inputObj.transform, "Text", "", 20, TextAlignmentOptions.Left);
            inputField.textComponent = inputText;

            CreateButton(namePanel.transform, "ConfirmButton", "확인", 0, -70);

            // 스토리 패널
            var storyPanel = new GameObject("StoryPanel");
            storyPanel.transform.SetParent(canvas.transform, false);
            var storyRect = storyPanel.AddComponent<RectTransform>();
            storyRect.anchorMin = Vector2.zero;
            storyRect.anchorMax = Vector2.one;
            storyPanel.AddComponent<Image>().color = new Color(0.15f, 0.1f, 0.08f, 0.95f);

            CreateTMPText(storyPanel.transform, "StoryText", "", 28, TextAlignmentOptions.Center);
            storyPanel.SetActive(false);

            // 컨트롤러
            var controller = new GameObject("PrologueController");
            controller.AddComponent<UI.PrologueController>();

            EditorSceneManager.SaveScene(scene, ScenesPath + "SCN_Prologue.unity");
            Debug.Log("[SceneSetup] SCN_Prologue 생성");
        }

        [MenuItem("The SSand/씬 생성/SCN_Ch1_Desert")]
        static void CreateCh1DesertScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camObj = new GameObject("Main Camera");
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5;
            cam.backgroundColor = new Color(0.95f, 0.9f, 0.7f);
            camObj.tag = "MainCamera";

            // 배경 레이어
            for (int i = 0; i < 3; i++)
            {
                var bgLayer = new GameObject($"BG_Layer_{i}");
                bgLayer.AddComponent<SpriteRenderer>().sortingOrder = -10 + i;
                bgLayer.transform.position = new Vector3(0, 0, 10 - i);
            }

            // 지면
            var ground = new GameObject("Ground");
            ground.tag = "Ground";
            ground.layer = LayerMask.NameToLayer("Default");
            var groundCol = ground.AddComponent<BoxCollider2D>();
            groundCol.size = new Vector2(200, 2);
            ground.transform.position = new Vector3(50, -3, 0);
            var groundSr = ground.AddComponent<SpriteRenderer>();
            groundSr.color = new Color(0.85f, 0.75f, 0.55f);
            groundSr.sortingOrder = -1;

            // 플레이어 스폰
            var player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = new Vector3(-8, 0, 0);
            player.AddComponent<SpriteRenderer>().color = Color.green;
            player.AddComponent<Rigidbody2D>();
            player.AddComponent<BoxCollider2D>();
            player.AddComponent<Player.PlayerController>();

            // 튜토리얼 트리거들
            CreateTutorialTrigger("Tut_Move", new Vector3(-5, 0, 0), "A/D 키로 이동, Shift로 달리기");
            CreateTutorialTrigger("Tut_Jump", new Vector3(5, 0, 0), "Space 키로 점프");
            CreateTutorialTrigger("Tut_Interact", new Vector3(15, 0, 0), "E 키로 상호작용");

            // 쪽지 아이템
            var note = new GameObject("Note_Ch1");
            note.transform.position = new Vector3(25, -1.5f, 0);
            note.AddComponent<CircleCollider2D>().isTrigger = true;
            note.AddComponent<SpriteRenderer>().color = new Color(1f, 0.95f, 0.8f);
            var pickup = note.AddComponent<Level.PickupItem>();

            // 씬 전환 트리거
            var exitTrigger = new GameObject("ExitTrigger");
            exitTrigger.transform.position = new Vector3(95, 0, 0);
            var exitCol = exitTrigger.AddComponent<BoxCollider2D>();
            exitCol.isTrigger = true;
            exitCol.size = new Vector2(2, 10);
            var transition = exitTrigger.AddComponent<Level.SceneTransitionTrigger>();

            // 레벨 컨트롤러
            var levelCtrl = new GameObject("DesertLevelController");
            levelCtrl.AddComponent<Level.DesertLevelController>();

            EditorSceneManager.SaveScene(scene, ScenesPath + "SCN_Ch1_Desert.unity");
            Debug.Log("[SceneSetup] SCN_Ch1_Desert 생성");
        }

        [MenuItem("The SSand/씬 생성/SCN_Ch1_Oasis")]
        static void CreateCh1OasisScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camObj = new GameObject("Main Camera");
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5;
            cam.backgroundColor = new Color(0.85f, 0.9f, 0.75f);
            camObj.tag = "MainCamera";

            // 지면
            var ground = new GameObject("Ground");
            ground.tag = "Ground";
            var groundCol = ground.AddComponent<BoxCollider2D>();
            groundCol.size = new Vector2(60, 2);
            ground.transform.position = new Vector3(0, -3, 0);
            ground.AddComponent<SpriteRenderer>().color = new Color(0.7f, 0.8f, 0.5f);

            // 플레이어
            var player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = new Vector3(-12, 0, 0);
            player.AddComponent<SpriteRenderer>().color = Color.green;
            player.AddComponent<Rigidbody2D>();
            player.AddComponent<BoxCollider2D>();
            player.AddComponent<Player.PlayerController>();

            // NPC 배치 — 대장소년
            CreateNPC("NPC_CaptainBoy", new Vector3(0, -1.5f, 0), "captain_boy");

            // NPC — 주민 3명 (진흙쿠키 배달 대상)
            CreateNPC("NPC_Villager_1", new Vector3(-5, -1.5f, 0), "villager_1");
            CreateNPC("NPC_Villager_2", new Vector3(5, -1.5f, 0), "villager_2");
            CreateNPC("NPC_Villager_3", new Vector3(10, -1.5f, 0), "villager_3");

            // 세이브 포인트
            var savePoint = new GameObject("SavePoint");
            savePoint.transform.position = new Vector3(-2, -1.5f, 0);
            savePoint.AddComponent<CircleCollider2D>().isTrigger = true;
            savePoint.AddComponent<SpriteRenderer>().color = new Color(0.5f, 1f, 0.5f);
            savePoint.AddComponent<Level.SavePoint>();

            // 보스존 트리거
            var bossZone = new GameObject("BossZoneTrigger");
            bossZone.transform.position = new Vector3(20, 0, 0);
            var bossCol = bossZone.AddComponent<BoxCollider2D>();
            bossCol.isTrigger = true;
            bossCol.size = new Vector2(2, 10);
            var bossTransition = bossZone.AddComponent<Level.SceneTransitionTrigger>();
            bossZone.SetActive(false);

            // 건물 스켈레톤 (시각적 마커)
            CreateBuildingPlaceholder("Building_CaptainHouse", new Vector3(0, 1, 0), "대장소년의 집");
            CreateBuildingPlaceholder("Building_Square", new Vector3(-2, 1, 0), "중앙 광장");
            CreateBuildingPlaceholder("Building_House1", new Vector3(-5, 1, 0), "주민 집 1");
            CreateBuildingPlaceholder("Building_House2", new Vector3(5, 1, 0), "주민 집 2");
            CreateBuildingPlaceholder("Building_House3", new Vector3(10, 1, 0), "주민 집 3");

            // 오아시스 컨트롤러
            var oasisCtrl = new GameObject("OasisController");
            oasisCtrl.AddComponent<Level.OasisController>();

            EditorSceneManager.SaveScene(scene, ScenesPath + "SCN_Ch1_Oasis.unity");
            Debug.Log("[SceneSetup] SCN_Ch1_Oasis 생성");
        }

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

            // 아레나 경계
            var arenaLeft = new GameObject("ArenaWallLeft");
            var leftCol = arenaLeft.AddComponent<BoxCollider2D>();
            leftCol.size = new Vector2(1, 14);
            arenaLeft.transform.position = new Vector3(-7, 0, 0);

            var arenaRight = new GameObject("ArenaWallRight");
            var rightCol = arenaRight.AddComponent<BoxCollider2D>();
            rightCol.size = new Vector2(1, 14);
            arenaRight.transform.position = new Vector3(7, 0, 0);

            var arenaFloor = new GameObject("ArenaFloor");
            arenaFloor.tag = "Ground";
            var floorCol = arenaFloor.AddComponent<BoxCollider2D>();
            floorCol.size = new Vector2(14, 1);
            arenaFloor.transform.position = new Vector3(0, -5, 0);

            // 플레이어 (탄막용 간소화)
            var player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = new Vector3(0, -3.5f, 0);
            player.AddComponent<SpriteRenderer>().color = Color.green;
            var playerCol = player.AddComponent<BoxCollider2D>();
            playerCol.size = new Vector2(0.5f, 0.5f);
            playerCol.isTrigger = true;

            // 사막하마 보스
            var hippo = new GameObject("Boss_Hippo");
            hippo.transform.position = new Vector3(0, 3, 0);
            hippo.AddComponent<SpriteRenderer>().color = new Color(0.6f, 0.5f, 0.3f);
            var bossComp = hippo.AddComponent<Boss.BossHippo>();

            // 투사체 스폰 포인트
            for (int i = 0; i < 5; i++)
            {
                var sp = new GameObject($"SpawnPoint_{i}");
                sp.transform.SetParent(hippo.transform);
                sp.transform.localPosition = new Vector3(-2 + i, -0.5f, 0);
            }

            // Boss UI Canvas
            var uiCanvas = CreateCanvas("BossUICanvas");
            var bossUI = new GameObject("BossUIController");
            bossUI.transform.SetParent(uiCanvas.transform, false);
            bossUI.AddComponent<Boss.BossUIController>();

            // 전체 컨트롤러
            var ctrl = new GameObject("Ch1BossController");
            ctrl.AddComponent<Boss.Ch1BossController>();

            EditorSceneManager.SaveScene(scene, ScenesPath + "SCN_Ch1_Boss.unity");
            Debug.Log("[SceneSetup] SCN_Ch1_Boss 생성");
        }

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

        static Image CreateUIImage(Transform parent, string name, Color color)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var img = obj.AddComponent<Image>();
            img.color = color;
            return img;
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

        static Button CreateButton(Transform parent, string name, string label, float x, float y)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            var rect = obj.AddComponent<RectTransform>();
            SetAnchoredPos(rect, x, y, 250, 50);

            var img = obj.AddComponent<Image>();
            img.color = new Color(0.65f, 0.5f, 0.3f);

            var btn = obj.AddComponent<Button>();

            var textObj = new GameObject("Label");
            textObj.transform.SetParent(obj.transform, false);
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 22;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
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

        static void CreateTutorialTrigger(string name, Vector3 pos, string message)
        {
            var obj = new GameObject(name);
            obj.transform.position = pos;
            var col = obj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(3, 5);
            obj.AddComponent<Level.TutorialTrigger>();
        }

        static void CreateNPC(string name, Vector3 pos, string npcId)
        {
            var obj = new GameObject(name);
            obj.transform.position = pos;
            obj.AddComponent<SpriteRenderer>().color = new Color(0.8f, 0.6f, 0.4f);
            obj.AddComponent<CircleCollider2D>().isTrigger = true;
            obj.AddComponent<Level.NPCInteractable>();
        }

        static void CreateBuildingPlaceholder(string name, Vector3 pos, string label)
        {
            var obj = new GameObject(name);
            obj.transform.position = pos;
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.7f, 0.6f, 0.4f, 0.5f);
            sr.sortingOrder = -5;
        }

        #endregion
    }
}
#endif
