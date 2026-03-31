using UnityEngine;
using TheSSand.Item;
using TheSSand.Player;

namespace TheSSand.Level
{
    public enum PickupType
    {
        Note,
        GoldenIdol,
        Scarf,
        Gold,
        HealthItem,
        QuestItem
    }

    public class PickupItem : MonoBehaviour
    {
        [Header("ItemData SO (우선 사용)")]
        [SerializeField] ItemData itemData;

        [Header("수동 설정 (ItemData 없을 때 폴백)")]
        [SerializeField] PickupType pickupType;
        [SerializeField] string itemId;
        [SerializeField] string itemName;
        [SerializeField] string description;
        [SerializeField] int goldAmount = 10;
        [SerializeField] bool requireInteraction = true;

        [Header("UI")]
        [SerializeField] GameObject interactPrompt;
        [SerializeField] GameObject pickupEffect;

        bool _playerInRange;

        string ResolvedId => itemData != null ? itemData.itemId : itemId;
        string ResolvedName => itemData != null ? itemData.itemName : itemName;

        void Start()
        {
            if (interactPrompt != null)
                interactPrompt.SetActive(false);

            if (itemData == null && !string.IsNullOrEmpty(itemId))
                itemData = ItemDatabase.Instance?.GetItem(itemId);
        }

        void Update()
        {
            if (!_playerInRange) return;
            if (!requireInteraction) return;

            if (Input.GetKeyDown(KeyCode.E))
                Collect();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            if (requireInteraction)
            {
                _playerInRange = true;
                if (interactPrompt != null)
                    interactPrompt.SetActive(true);
                return;
            }

            Collect();
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = false;
            if (interactPrompt != null)
                interactPrompt.SetActive(false);
        }

        void Collect()
        {
            var gm = Core.GameManager.Instance;
            if (gm == null) return;

            PickupType resolvedType = itemData != null ? MapItemType(itemData.itemType) : pickupType;

            switch (resolvedType)
            {
                case PickupType.GoldenIdol:
                    gm.AddGoldenIdol();
                    break;
                case PickupType.Gold:
                    gm.AddGold(goldAmount);
                    break;
                case PickupType.HealthItem:
                    var player = FindFirstObjectByType<PlayerController>();
                    if (player != null && itemData != null)
                        player.Heal(itemData.healAmount > 0 ? itemData.healAmount : 1);
                    gm.CurrentSave.inventoryItems.Add(ResolvedId);
                    break;
                default:
                    gm.CurrentSave.inventoryItems.Add(ResolvedId);
                    break;
            }

            if (itemData != null)
            {
                UI.InventoryManager.Instance?.AddItem(
                    itemData.itemId,
                    itemData.itemName,
                    itemData.description,
                    MapToCategory(itemData.itemType),
                    itemData.icon);
            }
            else
            {
                UI.InventoryManager.Instance?.AddItem(ResolvedId, ResolvedName, description);
            }

            Audio.AudioManager.Instance?.PlaySFX("item_pickup");
            UI.NotificationUI.Instance?.ShowItemObtained(ResolvedName, itemData?.icon);

            if (pickupEffect != null)
                Instantiate(pickupEffect, transform.position, Quaternion.identity);

            Debug.Log($"[Pickup] {ResolvedName} 획득");
            gameObject.SetActive(false);
        }

        static PickupType MapItemType(ItemType type) => type switch
        {
            ItemType.Consumable => PickupType.HealthItem,
            ItemType.QuestItem => PickupType.QuestItem,
            ItemType.KeyItem => PickupType.QuestItem,
            ItemType.Note => PickupType.Note,
            ItemType.GoldenIdol => PickupType.GoldenIdol,
            _ => PickupType.QuestItem
        };

        static UI.ItemCategory MapToCategory(ItemType type) => type switch
        {
            ItemType.Consumable => UI.ItemCategory.Consumable,
            ItemType.QuestItem => UI.ItemCategory.Quest,
            ItemType.KeyItem => UI.ItemCategory.Quest,
            ItemType.Note => UI.ItemCategory.Note,
            ItemType.GoldenIdol => UI.ItemCategory.GoldenIdol,
            _ => UI.ItemCategory.Misc
        };
    }
}
