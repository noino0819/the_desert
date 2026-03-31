using UnityEngine;

namespace TheSSand.Item
{
    public enum ItemType
    {
        Consumable,
        QuestItem,
        KeyItem,
        Equipment,
        Note,
        GoldenIdol
    }

    [CreateAssetMenu(fileName = "NewItem", menuName = "The SSand/Item Data")]
    public class ItemData : ScriptableObject
    {
        public string itemId;
        public string itemName;
        [TextArea(2, 4)] public string description;
        public ItemType itemType;
        public Sprite icon;
        public int buyPrice;
        public int sellPrice;
        public bool isStackable = true;
        public int maxStack = 99;

        [Header("소비 효과")]
        public int healAmount;
        public bool removeOnUse = true;

        [Header("NG+ 설명")]
        [TextArea(2, 4)] public string ngDescription;

        public string GetActiveDescription(bool isNG)
        {
            return isNG && !string.IsNullOrEmpty(ngDescription) ? ngDescription : description;
        }
    }
}
