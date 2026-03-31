using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TheSSand.Core;
using TheSSand.Player;

namespace TheSSand.UI
{
    public class InGameHUD : MonoBehaviour
    {
        [Header("하트 (HP)")]
        [SerializeField] Image[] heartIcons;
        [SerializeField] Sprite heartFull;
        [SerializeField] Sprite heartEmpty;
        [SerializeField] int maxHearts = 3;

        [Header("골드")]
        [SerializeField] TextMeshProUGUI goldText;

        [Header("위치")]
        [SerializeField] TextMeshProUGUI locationText;

        [Header("상호작용 프롬프트")]
        [SerializeField] GameObject interactPrompt;

        int _currentHP;

        PlayerController _player;

        void Start()
        {
            _currentHP = maxHearts;
            UpdateHearts();
            UpdateGold();
            HideInteractPrompt();

            if (GameManager.Instance != null)
                GameManager.Instance.OnFlagChanged += OnFlagChanged;

            _player = FindFirstObjectByType<PlayerController>();
            if (_player != null)
                _player.OnHPChanged += OnPlayerHPChanged;
        }

        void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnFlagChanged -= OnFlagChanged;

            if (_player != null)
                _player.OnHPChanged -= OnPlayerHPChanged;
        }

        void OnPlayerHPChanged(int current, int max)
        {
            SetHP(current, max);
        }

        void OnFlagChanged(string flag)
        {
            if (flag == "gold")
                UpdateGold();
        }

        public void SetLocation(string name)
        {
            if (locationText != null)
                locationText.text = name;
        }

        public void SetHP(int current, int max)
        {
            _currentHP = current;
            maxHearts = max;
            UpdateHearts();
        }

        public void ShowInteractPrompt()
        {
            if (interactPrompt != null)
                interactPrompt.SetActive(true);
        }

        public void HideInteractPrompt()
        {
            if (interactPrompt != null)
                interactPrompt.SetActive(false);
        }

        void UpdateHearts()
        {
            if (heartIcons == null) return;
            for (int i = 0; i < heartIcons.Length; i++)
            {
                if (heartIcons[i] == null) continue;
                heartIcons[i].gameObject.SetActive(i < maxHearts);
                heartIcons[i].sprite = i < _currentHP ? heartFull : heartEmpty;
            }
        }

        void UpdateGold()
        {
            if (goldText == null || GameManager.Instance == null) return;
            goldText.text = GameManager.Instance.CurrentSave.gold.ToString();
        }
    }
}
