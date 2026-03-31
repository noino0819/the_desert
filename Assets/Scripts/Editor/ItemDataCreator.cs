#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TheSSand.Item;

namespace TheSSand.Editor
{
    public class ItemDataCreator
    {
        static readonly string BasePath = "Assets/Resources/Items";

        [MenuItem("The SSand/아이템 데이터/전체 생성")]
        static void CreateAllItems()
        {
            EnsureFolder();

            // ─── 소비 아이템 ───
            Create("water_bottle", "물병", ItemType.Consumable,
                "사막에서 가장 귀한 것. HP를 1 회복한다.",
                buyPrice: 15, sellPrice: 5, healAmount: 1);

            Create("oasis_water", "오아시스 물", ItemType.Consumable,
                "맑은 오아시스의 물. HP를 2 회복한다.",
                buyPrice: 30, sellPrice: 12, healAmount: 2);

            Create("date_fruit", "대추야자", ItemType.Consumable,
                "사막의 과일. HP를 1 회복한다.",
                buyPrice: 10, sellPrice: 3, healAmount: 1);

            Create("fig", "무화과", ItemType.Consumable,
                "달콤한 무화과. HP를 2 회복한다.",
                buyPrice: 25, sellPrice: 8, healAmount: 2);

            Create("pomegranate", "석류", ItemType.Consumable,
                "붉은 석류. HP를 3 회복 (최대).",
                buyPrice: 50, sellPrice: 20, healAmount: 3);

            Create("mud_cookie", "진흙쿠키", ItemType.Consumable,
                "아이들이 구운 진흙쿠키. 먹을 순 있지만...\nHP 1 회복.",
                buyPrice: 0, sellPrice: 1, healAmount: 1,
                ngDesc: "흙과 설탕을 섞어 구운 것. 기아의 증거.");

            // ─── 퀘스트 아이템 ───
            Create("well_parts", "우물 부품", ItemType.QuestItem,
                "고장난 우물을 고치기 위한 부품.", isStackable: false);

            Create("survival_kit", "생존 키트", ItemType.QuestItem,
                "사막 생존에 필요한 도구 모음.", isStackable: false);

            Create("bandit_evidence", "도적 증거", ItemType.QuestItem,
                "도적단의 활동 증거.", isStackable: false);

            Create("wall_fragment", "벽 조각", ItemType.QuestItem,
                "분리의 벽에서 떨어진 조각.", isStackable: false);

            Create("unite_letter", "통합 서신", ItemType.QuestItem,
                "태양마을과 달마을을 잇는 서신.", isStackable: false);

            Create("chem_reagent_a", "시약 A", ItemType.QuestItem,
                "연구자의 화학 실험에 필요한 시약.", isStackable: false);

            Create("chem_reagent_b", "시약 B", ItemType.QuestItem,
                "연구자의 화학 실험에 필요한 시약.", isStackable: false);

            Create("chem_reagent_c", "시약 C", ItemType.QuestItem,
                "연구자의 화학 실험에 필요한 시약.", isStackable: false);

            Create("bio_sample", "생체 표본", ItemType.QuestItem,
                "과일거북 등딱지의 일부.", isStackable: false);

            Create("phys_part_1", "발전기 부품 1", ItemType.QuestItem,
                "고장난 발전기의 부품.", isStackable: false);

            Create("phys_part_2", "발전기 부품 2", ItemType.QuestItem,
                "고장난 발전기의 부품.", isStackable: false);

            Create("phys_part_3", "발전기 부품 3", ItemType.QuestItem,
                "고장난 발전기의 부품.", isStackable: false);

            // ─── 핵심 아이템 ───
            Create("seed_1", "녹지화 씨앗 1", ItemType.KeyItem,
                "정령에게 받은 씨앗. 사막을 살릴 수 있다고 한다.",
                isStackable: false,
                ngDesc: "이것이 정말 생명을 살리는 것인지 의심스럽다.");

            Create("seed_2", "녹지화 씨앗 2", ItemType.KeyItem,
                "두 번째 씨앗.", isStackable: false,
                ngDesc: "할아버지가 심었던 씨앗. 되돌려야 한다.");

            Create("seed_3", "녹지화 씨앗 3", ItemType.KeyItem,
                "세 번째 씨앗.", isStackable: false,
                ngDesc: "진실이 담긴 씨앗.");

            Create("seed_4", "녹지화 씨앗 4", ItemType.KeyItem,
                "네 번째 씨앗.", isStackable: false,
                ngDesc: "마지막 씨앗. 이것으로 끝낼 수 있다.");

            Create("scarf", "스카프", ItemType.KeyItem,
                "따뜻한 스카프. 누군가에게 선물할 수 있다.",
                isStackable: false,
                ngDesc: "할아버지가 빼앗은 스카프. 원래 주인에게 돌려줘야 한다.");

            // ─── 기록 ───
            Create("note_ch1", "기록 - 제1 오아시스", ItemType.Note,
                "첫 번째 오아시스에서 발견한 기록.\n'물이 없어진 지 열흘째...'",
                isStackable: false,
                ngDesc: "'용사가 왔다 간 뒤, 우물이 말랐다...'");

            Create("note_ch2", "기록 - 제2 오아시스", ItemType.Note,
                "두 번째 오아시스에서 발견한 기록.\n'아이들이 진흙쿠키를 만들고 있다...'",
                isStackable: false,
                ngDesc: "'용사가 아이들의 식량을 가져갔다...'");

            Create("note_ch3", "기록 - 제3 오아시스", ItemType.Note,
                "세 번째 오아시스에서 발견한 기록.\n'두 마을 사이에 벽이 생겼다...'",
                isStackable: false,
                ngDesc: "'용사가 벽을 세우라고 시켰다...'");

            Create("note_ch4", "기록 - 제4 오아시스", ItemType.Note,
                "네 번째 오아시스에서 발견한 기록.\n'연구자가 무언가를 발견했다...'",
                isStackable: false,
                ngDesc: "'연구자는 씨앗의 정체를 알아냈다. 그리고 죽었다.'");

            // ─── 수집품 ───
            Create("golden_idol", "황금 우상", ItemType.GoldenIdol,
                "반짝이는 황금 우상. 높은 가격에 팔 수 있다.",
                buyPrice: 0, sellPrice: 100,
                ngDesc: "거짓 우상. 할아버지의 탐욕의 증거.");

            // ─── 장비 ───
            Create("desert_boots", "사막 부츠", ItemType.Equipment,
                "사막 여행자의 부츠. 이동 속도가 약간 빨라진다.",
                buyPrice: 80, sellPrice: 30, isStackable: false);

            Create("sun_amulet", "태양 부적", ItemType.Equipment,
                "태양마을의 부적. 피해를 약간 줄여준다.",
                buyPrice: 120, sellPrice: 45, isStackable: false);

            Create("moon_pendant", "달 펜던트", ItemType.Equipment,
                "달마을의 펜던트. 야간에 시야가 밝아진다.",
                buyPrice: 120, sellPrice: 45, isStackable: false);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[ItemDataCreator] 전체 아이템 생성 완료!");
            EditorUtility.DisplayDialog("완료", "아이템 데이터가 생성되었습니다.", "확인");
        }

        static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder(BasePath))
                AssetDatabase.CreateFolder("Assets/Resources", "Items");
        }

        static void Create(string id, string name, ItemType type, string desc,
            int buyPrice = 0, int sellPrice = 0,
            int healAmount = 0, bool isStackable = true, int maxStack = 99,
            string ngDesc = "")
        {
            string path = $"{BasePath}/Item_{id}.asset";
            if (AssetDatabase.LoadAssetAtPath<ItemData>(path) != null) return;

            var item = ScriptableObject.CreateInstance<ItemData>();
            item.itemId = id;
            item.itemName = name;
            item.itemType = type;
            item.description = desc;
            item.buyPrice = buyPrice;
            item.sellPrice = sellPrice;
            item.healAmount = healAmount;
            item.isStackable = isStackable;
            item.maxStack = maxStack;
            item.ngDescription = ngDesc;

            AssetDatabase.CreateAsset(item, path);
        }
    }
}
#endif
