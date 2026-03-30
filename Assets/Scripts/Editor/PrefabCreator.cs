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
            CreatePlayerPrefab();
            CreateNPCPrefabs();
            CreateBossPrefab();
            CreateProjectilePrefabs();
            CreateItemPrefabs();
            CreateEnvironmentPrefabs();

            AssetDatabase.Refresh();
            Debug.Log("[Prefab] 전체 프리팹 생성 완료!");
            EditorUtility.DisplayDialog("완료", "모든 프리팹이 생성되었습니다.", "확인");
        }

        static void EnsureFolders()
        {
            string[] subs = { "Characters", "NPC", "Boss", "Projectiles", "Items", "Environment" };
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
