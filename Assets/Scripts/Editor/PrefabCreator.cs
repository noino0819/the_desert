#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace TheSSand.Editor
{
    public class PrefabCreator
    {
        static readonly string PrefabRoot = "Assets/Prefabs";

        [MenuItem("The SSand/프리팹 전체 생성", false, 10)]
        static void CreateAllPrefabs()
        {
            EnsureFolders();
            CreateBootstrapperPrefab();
            CreatePlayerPrefab();
            CreateNPCPrefabs();
            CreateBossPrefab();
            CreateBossEaglePrefab();
            CreateBossWolfPrefab();
            CreateBossTurtlePrefab();
            CreateProjectilePrefabs();
            CreateItemPrefabs();
            CreateEnvironmentPrefabs();
            CreateCardUIPrefab();
            CreateLevelPrefabs();
            CreateUIPrefabs();

            AssetDatabase.Refresh();
            Debug.Log("[Prefab] 전체 프리팹 생성 완료!");
            EditorUtility.DisplayDialog("완료", "모든 프리팹이 생성되었습니다.", "확인");
        }

        [MenuItem("The SSand/프리팹 생성/GameBootstrapper")]
        static void CreateBootstrapperPrefab()
        {
            var obj = new GameObject("GameBootstrapper");
            obj.AddComponent<Core.GameManager>();
            obj.AddComponent<Core.SaveManager>();
            obj.AddComponent<Core.EndingManager>();

            var transitionObj = new GameObject("SceneTransitionManager");
            transitionObj.transform.SetParent(obj.transform);
            transitionObj.AddComponent<Scene.SceneTransitionManager>();

            var dialogueObj = new GameObject("DialogueManager");
            dialogueObj.transform.SetParent(obj.transform);
            dialogueObj.AddComponent<Dialogue.DialogueManager>();

            var questObj = new GameObject("QuestManager");
            questObj.transform.SetParent(obj.transform);
            questObj.AddComponent<Quest.QuestManager>();

            var audioObj = new GameObject("AudioManager");
            audioObj.transform.SetParent(obj.transform);
            audioObj.AddComponent<Audio.AudioManager>();

            var itemDBObj = new GameObject("ItemDatabase");
            itemDBObj.transform.SetParent(obj.transform);
            itemDBObj.AddComponent<Item.ItemDatabase>();

            var loadingObj = new GameObject("LoadingScreen");
            loadingObj.transform.SetParent(obj.transform);
            loadingObj.AddComponent<UI.LoadingScreen>();

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");

            PrefabUtility.SaveAsPrefabAsset(obj, "Assets/Resources/GameBootstrapper.prefab");
            Object.DestroyImmediate(obj);
            Debug.Log("[Prefab] GameBootstrapper 프리팹 생성 → Assets/Resources/GameBootstrapper.prefab");
        }

        static void EnsureFolders()
        {
            string[] subs = { "Characters", "NPC", "Boss", "Projectiles", "Items", "Environment", "UI", "Level" };
            if (!AssetDatabase.IsValidFolder(PrefabRoot))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            foreach (var s in subs)
            {
                string path = $"{PrefabRoot}/{s}";
                if (!AssetDatabase.IsValidFolder(path))
                    AssetDatabase.CreateFolder(PrefabRoot, s);
            }
        }

        [MenuItem("The SSand/프리팹 생성/Player")]
        static void CreatePlayerPrefab()
        {
            var obj = new GameObject("Player");
            obj.tag = "Player";
            obj.layer = LayerMask.NameToLayer("Default");

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("Assets/Art/Characters/hero_spritesheet.png");
            sr.sortingOrder = 10;

            var rb = obj.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.gravityScale = 3f;

            var col = obj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 1.2f);
            col.offset = new Vector2(0f, 0.1f);

            obj.AddComponent<Player.PlayerController>();

            var groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(obj.transform);
            groundCheck.transform.localPosition = new Vector3(0, -0.65f, 0);

            SavePrefab(obj, $"{PrefabRoot}/Characters/Player.prefab");
        }

        [MenuItem("The SSand/프리팹 생성/NPCs")]
        static void CreateNPCPrefabs()
        {
            CreateSingleNPC("NPC_CaptainBoy", "Assets/Art/Characters/captain_boy_npc.png",
                "captain_boy", 5);

            CreateSingleNPC("NPC_Fairy", "Assets/Art/Characters/fairy_sprite.png",
                "fairy", 12);

            var villagerSprite = LoadSprite("Assets/Art/Characters/villager_npcs.png");
            for (int i = 1; i <= 3; i++)
            {
                var obj = new GameObject($"NPC_Villager_{i}");
                var sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = villagerSprite;
                sr.sortingOrder = 5;

                var col = obj.AddComponent<CircleCollider2D>();
                col.radius = 1.2f;
                col.isTrigger = true;

                obj.AddComponent<Level.NPCInteractable>();

                SavePrefab(obj, $"{PrefabRoot}/NPC/NPC_Villager_{i}.prefab");
            }
        }

        static void CreateSingleNPC(string name, string spritePath, string npcId, int sortOrder)
        {
            var obj = new GameObject(name);
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite(spritePath);
            sr.sortingOrder = sortOrder;

            var col = obj.AddComponent<CircleCollider2D>();
            col.radius = 1.2f;
            col.isTrigger = true;

            obj.AddComponent<Level.NPCInteractable>();

            SavePrefab(obj, $"{PrefabRoot}/NPC/{name}.prefab");
        }

        [MenuItem("The SSand/프리팹 생성/Boss Hippo")]
        static void CreateBossPrefab()
        {
            var obj = new GameObject("Boss_Hippo");
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("Assets/Art/Bosses/boss_hippo.png");
            sr.sortingOrder = 8;

            obj.AddComponent<Boss.BossHippo>();

            for (int i = 0; i < 5; i++)
            {
                var sp = new GameObject($"SpawnPoint_{i}");
                sp.transform.SetParent(obj.transform);
                sp.transform.localPosition = new Vector3(-2 + i, -1f, 0);
            }

            SavePrefab(obj, $"{PrefabRoot}/Boss/Boss_Hippo.prefab");
        }

        [MenuItem("The SSand/프리팹 생성/Boss Eagle")]
        static void CreateBossEaglePrefab()
        {
            var obj = new GameObject("Boss_Eagle");
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 8;
            obj.AddComponent<Boss.BossEagle>();
            obj.AddComponent<Boss.NGBossModifier>();
            SavePrefab(obj, $"{PrefabRoot}/Boss/Boss_Eagle.prefab");
        }

        [MenuItem("The SSand/프리팹 생성/Boss Wolf")]
        static void CreateBossWolfPrefab()
        {
            var obj = new GameObject("Boss_Wolf");
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 8;
            obj.AddComponent<Boss.BossWolf>();
            obj.AddComponent<Boss.NGBossModifier>();
            SavePrefab(obj, $"{PrefabRoot}/Boss/Boss_Wolf.prefab");
        }

        [MenuItem("The SSand/프리팹 생성/Boss Turtle")]
        static void CreateBossTurtlePrefab()
        {
            var obj = new GameObject("Boss_Turtle");
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 8;
            obj.AddComponent<Boss.BossTurtle>();
            obj.AddComponent<Card.CardBattleManager>();
            obj.AddComponent<Boss.NGBossModifier>();
            SavePrefab(obj, $"{PrefabRoot}/Boss/Boss_Turtle.prefab");
        }

        [MenuItem("The SSand/프리팹 생성/Card UI")]
        static void CreateCardUIPrefab()
        {
            if (!AssetDatabase.IsValidFolder($"{PrefabRoot}/UI"))
                AssetDatabase.CreateFolder(PrefabRoot, "UI");

            var obj = new GameObject("CardUI");
            var rt = obj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(100, 140);

            var bg = obj.AddComponent<UnityEngine.UI.Image>();
            bg.color = new Color(0.93f, 0.87f, 0.73f);

            var group = obj.AddComponent<CanvasGroup>();

            obj.AddComponent<Card.CardUI>();

            var nameObj = new GameObject("CardName");
            nameObj.transform.SetParent(obj.transform, false);
            var nameTMP = nameObj.AddComponent<TMPro.TextMeshProUGUI>();
            nameTMP.fontSize = 10;
            nameTMP.alignment = TMPro.TextAlignmentOptions.Center;
            var nameRect = nameTMP.rectTransform;
            nameRect.anchorMin = new Vector2(0, 0.85f);
            nameRect.anchorMax = new Vector2(1, 1);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;

            var costObj = new GameObject("CostText");
            costObj.transform.SetParent(obj.transform, false);
            var costTMP = costObj.AddComponent<TMPro.TextMeshProUGUI>();
            costTMP.fontSize = 14;
            costTMP.fontStyle = TMPro.FontStyles.Bold;
            costTMP.alignment = TMPro.TextAlignmentOptions.Center;
            var costRect = costTMP.rectTransform;
            costRect.anchorMin = new Vector2(0, 0.85f);
            costRect.anchorMax = new Vector2(0.25f, 1);
            costRect.offsetMin = Vector2.zero;
            costRect.offsetMax = Vector2.zero;

            var descObj = new GameObject("DescText");
            descObj.transform.SetParent(obj.transform, false);
            var descTMP = descObj.AddComponent<TMPro.TextMeshProUGUI>();
            descTMP.fontSize = 8;
            descTMP.alignment = TMPro.TextAlignmentOptions.Center;
            var descRect = descTMP.rectTransform;
            descRect.anchorMin = new Vector2(0.05f, 0);
            descRect.anchorMax = new Vector2(0.95f, 0.35f);
            descRect.offsetMin = Vector2.zero;
            descRect.offsetMax = Vector2.zero;

            SavePrefab(obj, $"{PrefabRoot}/UI/CardUI.prefab");
        }

        [MenuItem("The SSand/프리팹 생성/Projectiles")]
        static void CreateProjectilePrefabs()
        {
            CreateProjectile("Proj_Mud", "Assets/Art/Projectiles/proj_mud.png", 1f, true);
            CreateProjectile("Proj_Water", "Assets/Art/Projectiles/proj_water.png", 0f, false);
        }

        static void CreateProjectile(string name, string spritePath, float gravity, bool useSplash)
        {
            var obj = new GameObject(name);
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite(spritePath);
            sr.sortingOrder = 15;

            var rb = obj.AddComponent<Rigidbody2D>();
            rb.gravityScale = gravity;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = obj.AddComponent<CircleCollider2D>();
            col.radius = 0.2f;
            col.isTrigger = true;

            obj.AddComponent<Boss.Projectile>();

            SavePrefab(obj, $"{PrefabRoot}/Projectiles/{name}.prefab");
        }

        [MenuItem("The SSand/프리팹 생성/Items")]
        static void CreateItemPrefabs()
        {
            string[] items = { "Note_Ch1", "GoldenIdol", "Scarf", "MudCookie", "Seed", "GoldCoin" };
            var itemSprite = LoadSprite("Assets/Art/Items/items_sheet.png");

            foreach (var item in items)
            {
                var obj = new GameObject(item);
                var sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = itemSprite;
                sr.sortingOrder = 5;

                var col = obj.AddComponent<CircleCollider2D>();
                col.radius = 0.4f;
                col.isTrigger = true;

                obj.AddComponent<Level.PickupItem>();

                SavePrefab(obj, $"{PrefabRoot}/Items/{item}.prefab");
            }
        }

        [MenuItem("The SSand/프리팹 생성/Environment")]
        static void CreateEnvironmentPrefabs()
        {
            var buildingSprite = LoadSprite("Assets/Art/Environment/env_oasis_buildings.png");
            var propsSprite = LoadSprite("Assets/Art/Environment/env_desert_props.png");

            string[] buildings = { "CaptainHouse", "VillagerHouse_1", "VillagerHouse_2", "Well" };
            foreach (var b in buildings)
            {
                var obj = new GameObject($"Building_{b}");
                var sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = buildingSprite;
                sr.sortingOrder = -2;
                SavePrefab(obj, $"{PrefabRoot}/Environment/Building_{b}.prefab");
            }

            string[] props = { "Cactus_Tall", "RockFormation", "DeadTree", "Cactus_Small", "AnimalSkull", "SandDune", "DesertGrass" };
            foreach (var p in props)
            {
                var obj = new GameObject($"Prop_{p}");
                var sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = propsSprite;
                sr.sortingOrder = -3;
                SavePrefab(obj, $"{PrefabRoot}/Environment/Prop_{p}.prefab");
            }

            var saveObj = new GameObject("SavePoint");
            var saveSr = saveObj.AddComponent<SpriteRenderer>();
            saveSr.sprite = LoadSprite("Assets/Art/Characters/fairy_sprite.png");
            saveSr.sortingOrder = 6;
            var saveCol = saveObj.AddComponent<CircleCollider2D>();
            saveCol.radius = 1f;
            saveCol.isTrigger = true;
            saveObj.AddComponent<Level.SavePoint>();
            SavePrefab(saveObj, $"{PrefabRoot}/Environment/SavePoint.prefab");
        }

        [MenuItem("The SSand/프리팹 생성/Level Objects")]
        static void CreateLevelPrefabs()
        {
            // 위험물
            var hazard = new GameObject("HazardZone_Spike");
            var hSr = hazard.AddComponent<SpriteRenderer>();
            hSr.sortingOrder = 1;
            hSr.color = new Color(0.8f, 0.2f, 0.2f, 0.6f);
            var hCol = hazard.AddComponent<BoxCollider2D>();
            hCol.isTrigger = true;
            hCol.size = new Vector2(2f, 0.5f);
            hazard.AddComponent<Level.HazardZone>();
            SavePrefab(hazard, $"{PrefabRoot}/Level/HazardZone_Spike.prefab");

            // 사막 적
            var enemy = new GameObject("DesertEnemy");
            var eSr = enemy.AddComponent<SpriteRenderer>();
            eSr.sprite = LoadSprite("Assets/Art/Characters/villager_npcs.png");
            eSr.sortingOrder = 8;
            eSr.color = new Color(0.7f, 0.3f, 0.3f);
            var eRb = enemy.AddComponent<Rigidbody2D>();
            eRb.freezeRotation = true;
            eRb.gravityScale = 3f;
            var eCol = enemy.AddComponent<BoxCollider2D>();
            eCol.size = new Vector2(0.7f, 0.9f);
            enemy.AddComponent<Level.DesertEnemy>();
            SavePrefab(enemy, $"{PrefabRoot}/Level/DesertEnemy.prefab");

            // 이동 발판
            var platform = new GameObject("MovingPlatform");
            var pSr = platform.AddComponent<SpriteRenderer>();
            pSr.sortingOrder = -1;
            pSr.color = new Color(0.6f, 0.5f, 0.3f);
            var pRb = platform.AddComponent<Rigidbody2D>();
            pRb.bodyType = RigidbodyType2D.Kinematic;
            var pCol = platform.AddComponent<BoxCollider2D>();
            pCol.size = new Vector2(3f, 0.4f);
            platform.AddComponent<Level.MovingPlatform>();
            SavePrefab(platform, $"{PrefabRoot}/Level/MovingPlatform.prefab");

            // 단방향 플랫폼
            var oneWay = new GameObject("OneWayPlatform");
            var owSr = oneWay.AddComponent<SpriteRenderer>();
            owSr.sortingOrder = -1;
            owSr.color = new Color(0.5f, 0.45f, 0.3f);
            var owCol = oneWay.AddComponent<BoxCollider2D>();
            owCol.size = new Vector2(3f, 0.3f);
            owCol.usedByEffector = true;
            var eff = oneWay.AddComponent<PlatformEffector2D>();
            eff.useOneWay = true;
            eff.surfaceArc = 180f;
            oneWay.AddComponent<Level.OneWayPlatform>();
            SavePrefab(oneWay, $"{PrefabRoot}/Level/OneWayPlatform.prefab");

            // 체크포인트
            var checkpoint = new GameObject("Checkpoint");
            var cpSr = checkpoint.AddComponent<SpriteRenderer>();
            cpSr.sortingOrder = 4;
            var cpCol = checkpoint.AddComponent<BoxCollider2D>();
            cpCol.isTrigger = true;
            cpCol.size = new Vector2(1f, 2f);
            checkpoint.AddComponent<Level.Checkpoint>();
            SavePrefab(checkpoint, $"{PrefabRoot}/Level/Checkpoint.prefab");

            // 상점 NPC
            var shopKeeper = new GameObject("ShopKeeper");
            var skSr = shopKeeper.AddComponent<SpriteRenderer>();
            skSr.sortingOrder = 5;
            var skCol = shopKeeper.AddComponent<CircleCollider2D>();
            skCol.radius = 1.5f;
            skCol.isTrigger = true;
            shopKeeper.AddComponent<Level.ShopKeeper>();
            SavePrefab(shopKeeper, $"{PrefabRoot}/Level/ShopKeeper.prefab");

            // 씬 전환 트리거
            var sceneTrigger = new GameObject("SceneTransitionTrigger");
            var stCol = sceneTrigger.AddComponent<BoxCollider2D>();
            stCol.isTrigger = true;
            stCol.size = new Vector2(1f, 5f);
            sceneTrigger.AddComponent<Level.SceneTransitionTrigger>();
            SavePrefab(sceneTrigger, $"{PrefabRoot}/Level/SceneTransitionTrigger.prefab");

            // 튜토리얼 트리거
            var tutorial = new GameObject("TutorialTrigger");
            var ttCol = tutorial.AddComponent<BoxCollider2D>();
            ttCol.isTrigger = true;
            ttCol.size = new Vector2(2f, 3f);
            tutorial.AddComponent<Level.TutorialTrigger>();
            SavePrefab(tutorial, $"{PrefabRoot}/Level/TutorialTrigger.prefab");
        }

        [MenuItem("The SSand/프리팹 생성/UI Prefabs")]
        static void CreateUIPrefabs()
        {
            // 알림 팝업 아이템
            var notifItem = new GameObject("NotificationItem");
            var niRt = notifItem.AddComponent<RectTransform>();
            niRt.sizeDelta = new Vector2(300, 50);
            var niBg = notifItem.AddComponent<UnityEngine.UI.Image>();
            niBg.color = new Color(0.2f, 0.2f, 0.2f, 0.85f);
            notifItem.AddComponent<CanvasGroup>();

            var niIcon = new GameObject("Icon");
            niIcon.transform.SetParent(notifItem.transform, false);
            var niImg = niIcon.AddComponent<UnityEngine.UI.Image>();
            var niImgRect = niImg.rectTransform;
            niImgRect.anchorMin = new Vector2(0, 0);
            niImgRect.anchorMax = new Vector2(0, 1);
            niImgRect.sizeDelta = new Vector2(40, 0);
            niImgRect.anchoredPosition = new Vector2(25, 0);
            niIcon.SetActive(false);

            var niText = new GameObject("Text");
            niText.transform.SetParent(notifItem.transform, false);
            var niTmp = niText.AddComponent<TMPro.TextMeshProUGUI>();
            niTmp.fontSize = 14;
            niTmp.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
            niTmp.richText = true;
            var niTxtRect = niTmp.rectTransform;
            niTxtRect.anchorMin = new Vector2(0.2f, 0);
            niTxtRect.anchorMax = new Vector2(1, 1);
            niTxtRect.offsetMin = Vector2.zero;
            niTxtRect.offsetMax = Vector2.zero;

            SavePrefab(notifItem, $"{PrefabRoot}/UI/NotificationItem.prefab");

            // 인벤토리 슬롯
            var invSlot = new GameObject("InventorySlot");
            var isRt = invSlot.AddComponent<RectTransform>();
            isRt.sizeDelta = new Vector2(64, 64);
            var isBg = invSlot.AddComponent<UnityEngine.UI.Image>();
            isBg.color = new Color(0.85f, 0.78f, 0.65f, 0.8f);
            invSlot.AddComponent<UnityEngine.UI.Button>();

            var isIcon = new GameObject("Icon");
            isIcon.transform.SetParent(invSlot.transform, false);
            var isImg = isIcon.AddComponent<UnityEngine.UI.Image>();
            isImg.rectTransform.sizeDelta = new Vector2(48, 48);

            var isCount = new GameObject("Count");
            isCount.transform.SetParent(invSlot.transform, false);
            var isTmp = isCount.AddComponent<TMPro.TextMeshProUGUI>();
            isTmp.fontSize = 12;
            isTmp.alignment = TMPro.TextAlignmentOptions.BottomRight;
            var isCRect = isTmp.rectTransform;
            isCRect.anchorMin = Vector2.zero;
            isCRect.anchorMax = Vector2.one;
            isCRect.offsetMin = Vector2.zero;
            isCRect.offsetMax = Vector2.zero;

            invSlot.AddComponent<UI.InventorySlotUI>();
            SavePrefab(invSlot, $"{PrefabRoot}/UI/InventorySlot.prefab");

            // 상점 아이템 항목
            var shopItem = new GameObject("ShopItem");
            var siRt = shopItem.AddComponent<RectTransform>();
            siRt.sizeDelta = new Vector2(280, 40);
            var siBg = shopItem.AddComponent<UnityEngine.UI.Image>();
            siBg.color = new Color(0.9f, 0.85f, 0.7f, 0.9f);
            shopItem.AddComponent<UnityEngine.UI.Button>();

            var siText = new GameObject("ItemText");
            siText.transform.SetParent(shopItem.transform, false);
            var siTmp = siText.AddComponent<TMPro.TextMeshProUGUI>();
            siTmp.fontSize = 14;
            siTmp.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
            siTmp.color = new Color(0.2f, 0.15f, 0.1f);
            var siTxtRect = siTmp.rectTransform;
            siTxtRect.anchorMin = new Vector2(0.05f, 0);
            siTxtRect.anchorMax = Vector2.one;
            siTxtRect.offsetMin = Vector2.zero;
            siTxtRect.offsetMax = Vector2.zero;

            SavePrefab(shopItem, $"{PrefabRoot}/UI/ShopItem.prefab");
        }

        #region 유틸

        static Sprite LoadSprite(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static void SavePrefab(GameObject obj, string path)
        {
            EnsureFolders();
            PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
        }

        #endregion
    }
}
#endif
