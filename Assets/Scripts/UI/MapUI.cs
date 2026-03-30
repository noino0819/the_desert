using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TheSSand.UI
{
    /// <summary>
    /// 손그림 스타일 맵 UI — M키로 토글
    /// 현재 챕터/위치 표시, 퀘스트 마커, 사막 구간 진행률
    /// </summary>
    public class MapUI : MonoBehaviour
    {
        [Header("맵 패널")]
        [SerializeField] GameObject mapPanel;
        [SerializeField] Image mapImage;
        [SerializeField] Sprite[] chapterMaps;

        [Header("플레이어 위치")]
        [SerializeField] RectTransform playerMarker;
        [SerializeField] float mapMinX = -400f;
        [SerializeField] float mapMaxX = 400f;

        [Header("마커")]
        [SerializeField] GameObject markerPrefab;
        [SerializeField] RectTransform markerContainer;
        [SerializeField] Sprite questMarkerSprite;
        [SerializeField] Sprite savePointMarkerSprite;
        [SerializeField] Sprite bossMarkerSprite;

        [Header("정보")]
        [SerializeField] TextMeshProUGUI locationText;
        [SerializeField] TextMeshProUGUI chapterText;

        [Header("안개 (미탐색 영역)")]
        [SerializeField] RectTransform fogMask;
        [SerializeField] float fogRevealSpeed = 0.5f;

        bool _isOpen;
        float _exploredRatio;
        readonly List<GameObject> _activeMarkers = new();

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
                ToggleMap();

            if (_isOpen)
                UpdatePlayerPosition();
        }

        public void ToggleMap()
        {
            _isOpen = !_isOpen;

            if (mapPanel != null)
                mapPanel.SetActive(_isOpen);

            if (_isOpen)
            {
                RefreshMap();
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = 1f;
            }
        }

        void RefreshMap()
        {
            int chapter = GetCurrentChapter();

            if (mapImage != null && chapterMaps != null && chapter > 0 && chapter <= chapterMaps.Length)
                mapImage.sprite = chapterMaps[chapter - 1];

            if (chapterText != null)
                chapterText.text = $"Chapter {chapter}";

            RefreshMarkers();
        }

        void RefreshMarkers()
        {
            foreach (var m in _activeMarkers)
            {
                if (m != null) Destroy(m);
            }
            _activeMarkers.Clear();

            if (markerPrefab == null || markerContainer == null) return;

            var questMgr = Quest.QuestManager.Instance;
            if (questMgr == null) return;

            foreach (var quest in questMgr.GetActiveQuests())
            {
                if (quest.mapMarkerPosition == Vector2.zero) continue;

                var marker = Instantiate(markerPrefab, markerContainer);
                var rt = marker.GetComponent<RectTransform>();
                if (rt != null)
                    rt.anchoredPosition = quest.mapMarkerPosition;

                var img = marker.GetComponent<Image>();
                if (img != null)
                    img.sprite = questMarkerSprite;

                _activeMarkers.Add(marker);
            }
        }

        void UpdatePlayerPosition()
        {
            if (playerMarker == null) return;

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            float sceneWidth = 100f;
            float normalizedX = Mathf.InverseLerp(0, sceneWidth, player.transform.position.x);
            float mapX = Mathf.Lerp(mapMinX, mapMaxX, normalizedX);

            playerMarker.anchoredPosition = new Vector2(mapX, playerMarker.anchoredPosition.y);

            _exploredRatio = Mathf.Max(_exploredRatio, normalizedX);
            UpdateFog();
        }

        void UpdateFog()
        {
            if (fogMask == null) return;

            float targetX = Mathf.Lerp(mapMinX, mapMaxX, _exploredRatio);
            Vector2 pos = fogMask.anchoredPosition;
            pos.x = Mathf.Lerp(pos.x, targetX, fogRevealSpeed * Time.unscaledDeltaTime);
            fogMask.anchoredPosition = pos;
        }

        public void SetLocation(string location)
        {
            if (locationText != null)
                locationText.text = location;
        }

        int GetCurrentChapter()
        {
            var gm = Core.GameManager.Instance;
            if (gm == null) return 1;
            return gm.CurrentSave.currentChapter;
        }

        void OnDisable()
        {
            if (_isOpen)
            {
                _isOpen = false;
                Time.timeScale = 1f;
            }
        }
    }
}
