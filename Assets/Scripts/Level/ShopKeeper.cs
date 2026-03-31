using UnityEngine;
using TheSSand.Item;
using TheSSand.UI;

namespace TheSSand.Level
{
    /// <summary>
    /// 상인 NPC — E키로 상호작용하면 상점 오픈
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class ShopKeeper : MonoBehaviour
    {
        [SerializeField] string shopName = "상점";
        [SerializeField] ItemData[] shopInventory;
        [SerializeField] float interactRadius = 2f;
        [SerializeField] GameObject interactPrompt;

        bool _playerInRange;

        void Start()
        {
            var col = GetComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = interactRadius;

            if (interactPrompt != null)
                interactPrompt.SetActive(false);
        }

        void Update()
        {
            if (!_playerInRange) return;
            if (Input.GetKeyDown(KeyCode.E))
                ShopUI.Instance?.OpenShop(shopName, shopInventory);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = true;
            if (interactPrompt != null) interactPrompt.SetActive(true);
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = false;
            if (interactPrompt != null) interactPrompt.SetActive(false);
        }
    }
}
