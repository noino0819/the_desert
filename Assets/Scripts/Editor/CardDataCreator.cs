#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TheSSand.Card;

namespace TheSSand.Editor
{
    public class CardDataCreator
    {
        [MenuItem("The SSand/카드 데이터/기본 덱 생성")]
        static void CreateDefaultDeck()
        {
            string basePath = "Assets/Resources/Cards";
            if (!AssetDatabase.IsValidFolder(basePath))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Cards");
            }

            CreateCard(basePath, "strike", "타격", CardType.Attack, CardRarity.Common,
                1, 6, 0, 0, 0, "기본 공격. 6 데미지를 입힌다.",
                "진실의 타격. 거짓을 벗겨낸다.");

            CreateCard(basePath, "heavy_strike", "강타", CardType.Attack, CardRarity.Common,
                2, 12, 0, 0, 0, "강력한 일격. 12 데미지.",
                "진실의 일격. 12 데미지.");

            CreateCard(basePath, "defend", "방어", CardType.Defense, CardRarity.Common,
                1, 0, 5, 0, 0, "5 방어도를 얻는다.",
                "거짓을 막는 방패. 5 방어도.");

            CreateCard(basePath, "iron_wall", "철벽", CardType.Defense, CardRarity.Common,
                2, 0, 12, 0, 0, "12 방어도를 얻는다.",
                "진실의 장벽. 12 방어도.");

            CreateCard(basePath, "sand_throw", "모래 던지기", CardType.Attack, CardRarity.Common,
                1, 4, 0, 0, 0, "4 데미지. 약하지만 가볍다.",
                "모래 한 줌의 진실.");

            CreateCard(basePath, "desert_wind", "사막의 바람", CardType.Attack, CardRarity.Rare,
                2, 8, 0, 0, 0, "8 데미지 + 카드 1장 드로우.",
                "진실을 실어 나르는 바람.", drawCount: 1);

            CreateCard(basePath, "oasis_water", "오아시스의 물", CardType.Special, CardRarity.Rare,
                1, 0, 0, 4, 0, "HP를 4 회복한다.",
                "속죄의 물. 진짜 치유.");

            CreateCard(basePath, "poison_thorn", "독가시", CardType.Attack, CardRarity.Rare,
                1, 3, 0, 0, 2, "3 데미지 + 독 2 부여.",
                "거짓의 가시. 양심의 독.", applyPoison: true);

            CreateCard(basePath, "chain_lightning", "연쇄 번개", CardType.Attack, CardRarity.Rare,
                2, 5, 0, 0, 0, "5 데미지를 2회 연속 적용.",
                "진실의 연쇄. 거짓을 둘로 가른다.", chainEffect: true, chainMult: 2);

            CreateCard(basePath, "fruit_shield", "과일 방패", CardType.Defense, CardRarity.Rare,
                1, 0, 7, 2, 0, "7 방어도 + HP 2 회복.",
                "수확의 은혜. 사막이 주는 보호.");

            CreateCard(basePath, "golden_idol", "황금 우상", CardType.Special, CardRarity.Unique,
                0, 0, 0, 0, 0, "비용 0. 카드 2장 드로우. 탐욕의 상징.",
                "거짓 우상. 빛나지만 가치 없는 것.", drawCount: 2);

            CreateCard(basePath, "seed_of_hope", "희망의 씨앗", CardType.Special, CardRarity.Unique,
                3, 0, 10, 10, 0, "10 방어도 + HP 10 회복. 기적 같은 카드.",
                "진짜 씨앗. 진실된 희망을 심는다.");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CardDataCreator] 기본 덱 카드 12장 생성 완료!");
        }

        static void CreateCard(string basePath, string id, string name,
            CardType type, CardRarity rarity,
            int cost, int atk, int def, int heal, int poison,
            string desc, string ngDesc,
            int drawCount = 0, bool applyPoison = false,
            bool chainEffect = false, int chainMult = 1)
        {
            if (applyPoison == false && poison > 0) applyPoison = true;

            string assetPath = $"{basePath}/Card_{id}.asset";
            if (AssetDatabase.LoadAssetAtPath<CardData>(assetPath) != null) return;

            var card = ScriptableObject.CreateInstance<CardData>();
            card.cardId = id;
            card.cardName = name;
            card.description = desc;
            card.ngDescription = ngDesc;
            card.cardType = type;
            card.rarity = rarity;
            card.energyCost = cost;
            card.attackValue = atk;
            card.defenseValue = def;
            card.healValue = heal;
            card.drawCount = drawCount;
            card.applyPoison = applyPoison;
            card.poisonStacks = poison;
            card.chainEffect = chainEffect;
            card.chainMultiplier = chainMult;

            AssetDatabase.CreateAsset(card, assetPath);
        }
    }
}
#endif
