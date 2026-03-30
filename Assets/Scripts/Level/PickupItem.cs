using UnityEngine;

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

        void Start()
        {
            if (interactPrompt != null)
                interactPrompt.SetActive(false);
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

            switch (pickupType)
            {
                case PickupType.GoldenIdol:
                    gm.AddGoldenIdol();
                    break;
                case PickupType.Gold:
                    gm.AddGold(goldAmount);
                    break;
                case PickupType.Scarf:
                    gm.CurrentSave.inventoryItems.Add(itemId);
                    break;
                case PickupType.Note:
                case PickupType.HealthItem:
                case PickupType.QuestItem:
                    gm.CurrentSave.inventoryItems.Add(itemId);
                    break;
            }

            if (pickupEffect != null)
                Instantiate(pickupEffect, transform.position, Quaternion.identity);

            Debug.Log($"[Pickup] {itemName} 획득");
            gameObject.SetActive(false);
        }
    }
}
