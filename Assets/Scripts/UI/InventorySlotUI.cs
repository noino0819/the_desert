using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace TheSSand.UI
{
    public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] Image iconImage;
        [SerializeField] TextMeshProUGUI nameText;
        [SerializeField] TextMeshProUGUI countText;
        [SerializeField] TextMeshProUGUI descriptionText;

        InventoryItem _item;

        public void Setup(InventoryItem item)
        {
            _item = item;

            if (iconImage != null)
            {
                iconImage.sprite = item.icon;
                iconImage.enabled = item.icon != null;
            }
            if (nameText != null)
                nameText.text = item.displayName;
            if (countText != null)
            {
                countText.text = item.count > 1 ? $"x{item.count}" : "";
                countText.gameObject.SetActive(item.count > 1);
            }
            if (descriptionText != null)
                descriptionText.text = item.description;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_item == null) return;
            ItemTooltip.Instance?.Show(_item);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ItemTooltip.Instance?.Hide();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_item == null || _item.category != ItemCategory.Consumable) return;
            InventoryManager.Instance?.UseItem(_item.itemId);
        }
    }
}
