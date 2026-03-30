using System;
using System.Collections.Generic;
using UnityEngine;
using TheSSand.Core;

namespace TheSSand.UI
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        [SerializeField] GameObject inventoryPanel;
        [SerializeField] Transform gridContainer;
        [SerializeField] GameObject itemSlotPrefab;
        [SerializeField] int gridSize = 24;

        readonly List<InventoryItem> _items = new();
        bool _isOpen;

        public event Action<InventoryItem> OnItemAdded;
        public event Action<InventoryItem> OnItemUsed;
        public event Action<InventoryItem> OnItemRemoved;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (inventoryPanel != null)
                inventoryPanel.SetActive(false);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
                ToggleInventory();
        }

        #region 공개 API

        public void AddItem(string itemId, string displayName = null, string description = null,
            ItemCategory category = ItemCategory.Misc, Sprite icon = null)
        {
            var existing = _items.Find(i => i.itemId == itemId);
            if (existing != null && existing.stackable)
            {
                existing.count++;
                OnItemAdded?.Invoke(existing);
                SyncToSave();
                return;
            }

            var item = new InventoryItem
            {
                itemId = itemId,
                displayName = displayName ?? itemId,
                description = description ?? "",
                category = category,
                icon = icon,
                count = 1,
                stackable = category == ItemCategory.Consumable
            };
            _items.Add(item);
            OnItemAdded?.Invoke(item);
            SyncToSave();
        }

        public bool HasItem(string itemId) => _items.Exists(i => i.itemId == itemId);

        public bool RemoveItem(string itemId)
        {
            var item = _items.Find(i => i.itemId == itemId);
            if (item == null) return false;

            if (item.stackable && item.count > 1)
            {
                item.count--;
            }
            else
            {
                _items.Remove(item);
            }
            OnItemRemoved?.Invoke(item);
            SyncToSave();
            return true;
        }

        public void UseItem(string itemId)
        {
            var item = _items.Find(i => i.itemId == itemId);
            if (item == null) return;
            if (item.category != ItemCategory.Consumable) return;

            OnItemUsed?.Invoke(item);
            RemoveItem(itemId);
        }

        public List<InventoryItem> GetItems() => new List<InventoryItem>(_items);
        public List<InventoryItem> GetItemsByCategory(ItemCategory cat) =>
            _items.FindAll(i => i.category == cat);

        #endregion

        #region UI

        void ToggleInventory()
        {
            _isOpen = !_isOpen;
            if (inventoryPanel != null)
                inventoryPanel.SetActive(_isOpen);

            if (_isOpen) RefreshUI();
            Time.timeScale = _isOpen ? 0f : 1f;
        }

        public void CloseInventory()
        {
            _isOpen = false;
            if (inventoryPanel != null) inventoryPanel.SetActive(false);
            Time.timeScale = 1f;
        }

        void RefreshUI()
        {
            if (gridContainer == null) return;

            for (int i = gridContainer.childCount - 1; i >= 0; i--)
                Destroy(gridContainer.GetChild(i).gameObject);

            foreach (var item in _items)
            {
                if (itemSlotPrefab == null) break;
                var slot = Instantiate(itemSlotPrefab, gridContainer);
                var slotUI = slot.GetComponent<InventorySlotUI>();
                if (slotUI != null) slotUI.Setup(item);
            }
        }

        #endregion

        #region 세이브 연동

        void SyncToSave()
        {
            if (GameManager.Instance == null) return;
            var save = GameManager.Instance.CurrentSave;
            save.inventoryItems.Clear();
            foreach (var item in _items)
                save.inventoryItems.Add(item.itemId);
        }

        public void RestoreFromSave()
        {
            if (GameManager.Instance == null) return;
            var save = GameManager.Instance.CurrentSave;
            _items.Clear();
            foreach (var id in save.inventoryItems)
                AddItem(id);
        }

        #endregion
    }

    [Serializable]
    public class InventoryItem
    {
        public string itemId;
        public string displayName;
        public string description;
        public ItemCategory category;
        public Sprite icon;
        public int count;
        public bool stackable;
    }

    public enum ItemCategory
    {
        Quest,
        Consumable,
        Note,
        GoldenIdol,
        Misc
    }
}
