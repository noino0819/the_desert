using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TheSSand.UI
{
    public class InventorySlotUI : MonoBehaviour
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
    }
}
