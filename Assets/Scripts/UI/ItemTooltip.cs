using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TheSSand.Item;

namespace TheSSand.UI
{
    public class ItemTooltip : MonoBehaviour
    {
        public static ItemTooltip Instance { get; private set; }

        [SerializeField] GameObject tooltipPanel;
        [SerializeField] Image itemIcon;
        [SerializeField] TextMeshProUGUI itemNameText;
        [SerializeField] TextMeshProUGUI itemTypeText;
        [SerializeField] TextMeshProUGUI itemDescText;
        [SerializeField] TextMeshProUGUI itemStatsText;
        [SerializeField] Vector2 offset = new(20f, -20f);

        RectTransform _panelRect;
        Canvas _parentCanvas;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            if (tooltipPanel != null)
            {
                _panelRect = tooltipPanel.GetComponent<RectTransform>();
                _parentCanvas = tooltipPanel.GetComponentInParent<Canvas>();
                tooltipPanel.SetActive(false);
            }
        }

        void Update()
        {
            if (tooltipPanel == null || !tooltipPanel.activeSelf) return;
            FollowMouse();
        }

        public void Show(ItemData data)
        {
            if (tooltipPanel == null || data == null) return;

            if (itemIcon != null)
            {
                itemIcon.sprite = data.icon;
                itemIcon.enabled = data.icon != null;
            }

            if (itemNameText != null)
                itemNameText.text = data.itemName;

            if (itemTypeText != null)
                itemTypeText.text = GetTypeLabel(data.itemType);

            bool isNG = Core.GameManager.Instance != null &&
                        Core.GameManager.Instance.CurrentSave.isNewGame2;
            if (itemDescText != null)
                itemDescText.text = data.GetActiveDescription(isNG);

            if (itemStatsText != null)
            {
                var stats = new System.Text.StringBuilder();
                if (data.healAmount > 0) stats.AppendLine($"회복: ♥ {data.healAmount}");
                if (data.buyPrice > 0) stats.AppendLine($"구매: {data.buyPrice}G");
                if (data.sellPrice > 0) stats.AppendLine($"판매: {data.sellPrice}G");
                itemStatsText.text = stats.ToString().TrimEnd();
            }

            tooltipPanel.SetActive(true);
            FollowMouse();
        }

        public void Show(InventoryItem item)
        {
            if (tooltipPanel == null || item == null) return;

            var data = ItemDatabase.Instance?.GetItem(item.itemId);
            if (data != null)
            {
                Show(data);
                return;
            }

            if (itemIcon != null)
            {
                itemIcon.sprite = item.icon;
                itemIcon.enabled = item.icon != null;
            }
            if (itemNameText != null) itemNameText.text = item.displayName;
            if (itemTypeText != null) itemTypeText.text = GetCategoryLabel(item.category);
            if (itemDescText != null) itemDescText.text = item.description;
            if (itemStatsText != null) itemStatsText.text = "";

            tooltipPanel.SetActive(true);
            FollowMouse();
        }

        public void Hide()
        {
            if (tooltipPanel != null)
                tooltipPanel.SetActive(false);
        }

        void FollowMouse()
        {
            if (_panelRect == null || _parentCanvas == null) return;

            Vector2 mousePos = Input.mousePosition;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentCanvas.transform as RectTransform,
                mousePos, _parentCanvas.worldCamera, out Vector2 localPoint);

            _panelRect.anchoredPosition = localPoint + offset;

            ClampToScreen();
        }

        void ClampToScreen()
        {
            if (_panelRect == null) return;

            Vector3[] corners = new Vector3[4];
            _panelRect.GetWorldCorners(corners);

            float screenW = Screen.width;
            float screenH = Screen.height;

            Vector2 shift = Vector2.zero;
            if (corners[2].x > screenW) shift.x = screenW - corners[2].x;
            if (corners[0].x < 0) shift.x = -corners[0].x;
            if (corners[2].y > screenH) shift.y = screenH - corners[2].y;
            if (corners[0].y < 0) shift.y = -corners[0].y;

            _panelRect.anchoredPosition += shift;
        }

        static string GetTypeLabel(ItemType type) => type switch
        {
            ItemType.Consumable => "소비",
            ItemType.QuestItem => "퀘스트",
            ItemType.KeyItem => "핵심",
            ItemType.Equipment => "장비",
            ItemType.Note => "기록",
            ItemType.GoldenIdol => "수집품",
            _ => "기타"
        };

        static string GetCategoryLabel(ItemCategory cat) => cat switch
        {
            ItemCategory.Quest => "퀘스트",
            ItemCategory.Consumable => "소비",
            ItemCategory.Note => "기록",
            ItemCategory.GoldenIdol => "수집품",
            _ => "기타"
        };
    }
}
