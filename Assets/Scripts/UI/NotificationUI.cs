using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TheSSand.UI
{
    public class NotificationUI : MonoBehaviour
    {
        public static NotificationUI Instance { get; private set; }

        [SerializeField] GameObject notificationPrefab;
        [SerializeField] Transform container;
        [SerializeField] float displayDuration = 3f;
        [SerializeField] float fadeOutDuration = 0.5f;
        [SerializeField] int maxVisible = 4;

        readonly Queue<NotificationEntry> _queue = new();
        readonly List<GameObject> _activeNotifs = new();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void ShowItemObtained(string itemName, Sprite icon = null)
        {
            Enqueue($"<b>{itemName}</b> 획득!", icon, new Color(1f, 0.9f, 0.6f));
        }

        public void ShowQuestUpdate(string questName, bool completed = false)
        {
            string msg = completed
                ? $"퀘스트 완료: <b>{questName}</b>"
                : $"퀘스트 갱신: <b>{questName}</b>";
            Enqueue(msg, null, completed ? new Color(0.6f, 1f, 0.6f) : new Color(0.7f, 0.85f, 1f));
        }

        public void ShowGold(int amount)
        {
            string msg = amount > 0 ? $"+{amount}G" : $"{amount}G";
            Enqueue(msg, null, new Color(1f, 0.85f, 0.3f));
        }

        public void ShowMessage(string message, Color? color = null)
        {
            Enqueue(message, null, color ?? Color.white);
        }

        void Enqueue(string text, Sprite icon, Color bgColor)
        {
            _queue.Enqueue(new NotificationEntry { text = text, icon = icon, bgColor = bgColor });
            TryShowNext();
        }

        void TryShowNext()
        {
            while (_activeNotifs.Count < maxVisible && _queue.Count > 0)
            {
                var entry = _queue.Dequeue();
                SpawnNotification(entry);
            }
        }

        void SpawnNotification(NotificationEntry entry)
        {
            if (notificationPrefab == null || container == null) return;

            var go = Instantiate(notificationPrefab, container);
            _activeNotifs.Add(go);

            var text = go.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.text = entry.text;

            var icon = go.transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null)
            {
                icon.sprite = entry.icon;
                icon.gameObject.SetActive(entry.icon != null);
            }

            var bg = go.GetComponent<Image>();
            if (bg != null)
            {
                var c = entry.bgColor;
                c.a = 0.85f;
                bg.color = c;
            }

            StartCoroutine(LifecycleRoutine(go));
        }

        IEnumerator LifecycleRoutine(GameObject go)
        {
            var cg = go.GetComponent<CanvasGroup>();
            if (cg == null) cg = go.AddComponent<CanvasGroup>();
            cg.alpha = 0f;

            float fadeIn = 0.2f;
            float t = 0f;
            while (t < fadeIn)
            {
                t += Time.unscaledDeltaTime;
                cg.alpha = t / fadeIn;
                yield return null;
            }
            cg.alpha = 1f;

            yield return new WaitForSecondsRealtime(displayDuration);

            t = 0f;
            while (t < fadeOutDuration)
            {
                t += Time.unscaledDeltaTime;
                cg.alpha = 1f - (t / fadeOutDuration);
                yield return null;
            }

            _activeNotifs.Remove(go);
            Destroy(go);
            TryShowNext();
        }

        struct NotificationEntry
        {
            public string text;
            public Sprite icon;
            public Color bgColor;
        }
    }
}
