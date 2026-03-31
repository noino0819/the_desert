using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TheSSand.Core;
using TheSSand.Item;

namespace TheSSand.UI
{
    /// <summary>
    /// 상점 UI — 구매/판매 탭, 아이템 목록, 확인 팝업
    /// </summary>
    public class ShopUI : MonoBehaviour
    {
        public static ShopUI Instance { get; private set; }

        [Header("패널")]
        [SerializeField] GameObject shopPanel;
        [SerializeField] TextMeshProUGUI shopNameText;
        [SerializeField] TextMeshProUGUI playerGoldText;

        [Header("탭")]
        [SerializeField] Button buyTab;
        [SerializeField] Button sellTab;

        [Header("아이템 목록")]
        [SerializeField] Transform itemListContainer;
        [SerializeField] GameObject shopItemPrefab;

        [Header("상세 정보")]
        [SerializeField] Image itemIcon;
        [SerializeField] TextMeshProUGUI itemNameText;
        [SerializeField] TextMeshProUGUI itemDescText;
        [SerializeField] TextMeshProUGUI itemPriceText;
        [SerializeField] Button actionButton;
        [SerializeField] TextMeshProUGUI actionButtonText;

        bool _isBuyMode = true;
        ItemData _selectedItem;
        ItemData[] _shopInventory;
        readonly List<GameObject> _listItems = new();
        readonly Dictionary<string, ItemData> _itemLookup = new();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            if (shopPanel != null) shopPanel.SetActive(false);

            buyTab?.onClick.AddListener(() => SwitchTab(true));
            sellTab?.onClick.AddListener(() => SwitchTab(false));
            actionButton?.onClick.AddListener(OnActionClicked);
        }

        public void OpenShop(string shopName, ItemData[] inventory)
        {
            _shopInventory = inventory;
            _isBuyMode = true;
            _selectedItem = null;

            _itemLookup.Clear();
            if (inventory != null)
                foreach (var item in inventory)
                    _itemLookup[item.itemId] = item;

            if (shopNameText != null) shopNameText.text = shopName;
            if (shopPanel != null) shopPanel.SetActive(true);

            Time.timeScale = 0f;
            RefreshGold();
            RefreshList();
        }

        public void CloseShop()
        {
            if (shopPanel != null) shopPanel.SetActive(false);
            Time.timeScale = 1f;
        }

        void SwitchTab(bool buyMode)
        {
            _isBuyMode = buyMode;
            _selectedItem = null;
            RefreshList();
            ClearDetail();
        }

        void RefreshList()
        {
            foreach (var go in _listItems) Destroy(go);
            _listItems.Clear();

            if (_isBuyMode)
            {
                if (_shopInventory == null) return;
                foreach (var item in _shopInventory)
                    CreateListEntry(item, item.buyPrice);
            }
            else
            {
                if (InventoryManager.Instance == null) return;
                foreach (var entry in InventoryManager.Instance.GetItems())
                {
                    var data = _itemLookup.ContainsKey(entry.itemId) ? _itemLookup[entry.itemId] : null;
                    if (data == null || data.sellPrice <= 0) continue;
                    CreateListEntry(data, data.sellPrice);
                }
            }
        }

        void CreateListEntry(ItemData item, int price)
        {
            if (shopItemPrefab == null || itemListContainer == null) return;

            var go = Instantiate(shopItemPrefab, itemListContainer);
            _listItems.Add(go);

            var nameText = go.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null) nameText.text = $"{item.itemName}  {price}G";

            var btn = go.GetComponent<Button>();
            btn?.onClick.AddListener(() => SelectItem(item));
        }

        void SelectItem(ItemData item)
        {
            _selectedItem = item;
            if (itemIcon != null) itemIcon.sprite = item.icon;
            if (itemNameText != null) itemNameText.text = item.itemName;
            if (itemDescText != null) itemDescText.text = item.description;

            int price = _isBuyMode ? item.buyPrice : item.sellPrice;
            if (itemPriceText != null) itemPriceText.text = $"{price}G";
            if (actionButtonText != null) actionButtonText.text = _isBuyMode ? "구매" : "판매";
        }

        void ClearDetail()
        {
            if (itemIcon != null) itemIcon.sprite = null;
            if (itemNameText != null) itemNameText.text = "";
            if (itemDescText != null) itemDescText.text = "";
            if (itemPriceText != null) itemPriceText.text = "";
        }

        void OnActionClicked()
        {
            if (_selectedItem == null || GameManager.Instance == null) return;

            if (_isBuyMode)
            {
                int cost = _selectedItem.buyPrice;
                if (GameManager.Instance.CurrentSave.gold < cost) return;

                GameManager.Instance.DeductGold(cost);
                InventoryManager.Instance?.AddItem(_selectedItem.itemId);
                Audio.AudioManager.Instance?.PlaySFX("shop_buy");
            }
            else
            {
                int earn = _selectedItem.sellPrice;
                InventoryManager.Instance?.RemoveItem(_selectedItem.itemId);
                GameManager.Instance.AddGold(earn);
                Audio.AudioManager.Instance?.PlaySFX("shop_sell");
            }

            RefreshGold();
            RefreshList();
            ClearDetail();
            _selectedItem = null;
        }

        void RefreshGold()
        {
            if (playerGoldText != null && GameManager.Instance != null)
                playerGoldText.text = $"{GameManager.Instance.CurrentSave.gold}G";
        }

        void Update()
        {
            if (shopPanel != null && shopPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
                CloseShop();
        }
    }
}
