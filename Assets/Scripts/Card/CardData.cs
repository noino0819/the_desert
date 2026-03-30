using UnityEngine;

namespace TheSSand.Card
{
    public enum CardType { Attack, Defense, Special }
    public enum CardRarity { Common, Rare, Unique }

    [CreateAssetMenu(fileName = "NewCard", menuName = "TheSSand/Card Data")]
    public class CardData : ScriptableObject
    {
        public string cardId;
        public string cardName;
        [TextArea(2, 4)] public string description;
        [TextArea(2, 4)] public string ngDescription;

        public CardType cardType;
        public CardRarity rarity;
        public int energyCost = 1;

        public int attackValue;
        public int defenseValue;
        public int healValue;
        public int drawCount;

        public bool applyPoison;
        public int poisonStacks;

        public bool chainEffect;
        public int chainMultiplier = 1;

        public bool aoeEffect;

        public Sprite artwork;

        public string GetActiveDescription(bool isNG)
        {
            return isNG && !string.IsNullOrEmpty(ngDescription) ? ngDescription : description;
        }
    }
}
