using System.Collections.Generic;
using UnityEngine;

namespace TheSSand.Item
{
    public class ItemDatabase : MonoBehaviour
    {
        public static ItemDatabase Instance { get; private set; }

        readonly Dictionary<string, ItemData> _items = new();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllItems();
        }

        void LoadAllItems()
        {
            var allItems = Resources.LoadAll<ItemData>("Items");
            foreach (var item in allItems)
            {
                if (!string.IsNullOrEmpty(item.itemId))
                    _items[item.itemId] = item;
            }
            Debug.Log($"[ItemDatabase] {_items.Count}개 아이템 로드 완료");
        }

        public ItemData GetItem(string itemId)
        {
            return _items.TryGetValue(itemId, out var data) ? data : null;
        }

        public bool HasItem(string itemId) => _items.ContainsKey(itemId);

        public IEnumerable<ItemData> GetAllItems() => _items.Values;

        public List<ItemData> GetItemsByType(ItemType type)
        {
            var result = new List<ItemData>();
            foreach (var item in _items.Values)
                if (item.itemType == type) result.Add(item);
            return result;
        }
    }
}
