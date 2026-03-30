using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TheSSand.Card
{
    /// <summary>
    /// 카드 배틀 UI — 손패 표시, 드래그 플레이, 상태 바
    /// </summary>
    public class CardBattleUI : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] CardBattleManager battleManager;

        [Header("손패")]
        [SerializeField] RectTransform handArea;
        [SerializeField] GameObject cardUIPrefab;
        [SerializeField] float cardSpacing = 120f;

        [Header("상태 바")]
        [SerializeField] Slider bossHPBar;
        [SerializeField] TextMeshProUGUI bossHPText;
        [SerializeField] TextMeshProUGUI bossShieldText;
        [SerializeField] Image[] playerHearts;
        [SerializeField] Sprite heartFull;
        [SerializeField] Sprite heartEmpty;
        [SerializeField] TextMeshProUGUI playerShieldText;

        [Header("에너지")]
        [SerializeField] TextMeshProUGUI energyText;
        [SerializeField] Image[] energyOrbs;
        [SerializeField] Sprite orbFull;
        [SerializeField] Sprite orbEmpty;

        [Header("덱 정보")]
        [SerializeField] TextMeshProUGUI drawPileText;
        [SerializeField] TextMeshProUGUI discardPileText;

        [Header("페이즈 / 턴")]
        [SerializeField] TextMeshProUGUI phaseText;
        [SerializeField] TextMeshProUGUI turnText;

        [Header("턴 종료")]
        [SerializeField] Button endTurnButton;

        [Header("전투 로그")]
        [SerializeField] TextMeshProUGUI battleLogText;
        [SerializeField] int maxLogLines = 6;

        [Header("결과")]
        [SerializeField] GameObject victoryPanel;
        [SerializeField] GameObject defeatPanel;

        readonly List<GameObject> _cardUIInstances = new();
        readonly Queue<string> _logQueue = new();

        void Start()
        {
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (defeatPanel != null) defeatPanel.SetActive(false);

            if (battleManager != null)
            {
                battleManager.OnStateChanged += RefreshUI;
                battleManager.OnTurnStart += OnTurnStart;
                battleManager.OnTurnEnd += OnTurnEnd;
                battleManager.OnBattleEnded += OnBattleEnd;
                battleManager.OnPhaseChanged += OnPhaseChanged;
                battleManager.OnBattleLog += AddLog;
            }

            if (endTurnButton != null)
                endTurnButton.onClick.AddListener(OnEndTurnClicked);
        }

        void OnDestroy()
        {
            if (battleManager != null)
            {
                battleManager.OnStateChanged -= RefreshUI;
                battleManager.OnTurnStart -= OnTurnStart;
                battleManager.OnTurnEnd -= OnTurnEnd;
                battleManager.OnBattleEnded -= OnBattleEnd;
                battleManager.OnPhaseChanged -= OnPhaseChanged;
                battleManager.OnBattleLog -= AddLog;
            }
        }

        void RefreshUI()
        {
            RefreshBossHP();
            RefreshPlayerHP();
            RefreshEnergy();
            RefreshDeckInfo();
            RefreshHand();
            RefreshTurnInfo();
        }

        void RefreshBossHP()
        {
            if (bossHPBar != null)
            {
                bossHPBar.maxValue = battleManager.BossMaxHP;
                bossHPBar.value = battleManager.BossHP;
            }

            if (bossHPText != null)
                bossHPText.text = $"{battleManager.BossHP}/{battleManager.BossMaxHP}";

            if (bossShieldText != null)
                bossShieldText.text = battleManager.BossShield > 0
                    ? $"🛡 {battleManager.BossShield}" : "";
        }

        void RefreshPlayerHP()
        {
            if (playerHearts != null)
            {
                for (int i = 0; i < playerHearts.Length; i++)
                {
                    if (playerHearts[i] == null) continue;
                    playerHearts[i].sprite = i < battleManager.PlayerHP ? heartFull : heartEmpty;
                }
            }

            if (playerShieldText != null)
                playerShieldText.text = battleManager.PlayerShield > 0
                    ? $"🛡 {battleManager.PlayerShield}" : "";
        }

        void RefreshEnergy()
        {
            if (energyText != null)
                energyText.text = $"{battleManager.CurrentEnergy}/{battleManager.MaxEnergy}";

            if (energyOrbs != null)
            {
                for (int i = 0; i < energyOrbs.Length; i++)
                {
                    if (energyOrbs[i] == null) continue;
                    energyOrbs[i].sprite = i < battleManager.CurrentEnergy ? orbFull : orbEmpty;
                }
            }
        }

        void RefreshDeckInfo()
        {
            if (drawPileText != null)
                drawPileText.text = $"드로우: {battleManager.Deck.DrawPileCount}";
            if (discardPileText != null)
                discardPileText.text = $"버림: {battleManager.Deck.DiscardPileCount}";
        }

        void RefreshHand()
        {
            foreach (var go in _cardUIInstances)
            {
                if (go != null) Destroy(go);
            }
            _cardUIInstances.Clear();

            if (handArea == null || cardUIPrefab == null) return;

            var hand = battleManager.Deck.Hand;
            float totalWidth = (hand.Count - 1) * cardSpacing;
            float startX = -totalWidth * 0.5f;

            for (int i = 0; i < hand.Count; i++)
            {
                var cardObj = Instantiate(cardUIPrefab, handArea);
                var rt = cardObj.GetComponent<RectTransform>();
                if (rt != null)
                    rt.anchoredPosition = new Vector2(startX + i * cardSpacing, 0);

                var cardUI = cardObj.GetComponent<CardUI>();
                if (cardUI != null)
                {
                    cardUI.Setup(hand[i], battleManager);
                }

                _cardUIInstances.Add(cardObj);
            }
        }

        void RefreshTurnInfo()
        {
            if (phaseText != null)
                phaseText.text = $"Phase {battleManager.CurrentPhase}";
            if (turnText != null)
                turnText.text = $"Turn {battleManager.TurnCount}";
        }

        void OnTurnStart()
        {
            if (endTurnButton != null)
                endTurnButton.interactable = true;
        }

        void OnTurnEnd()
        {
            if (endTurnButton != null)
                endTurnButton.interactable = false;
        }

        void OnEndTurnClicked()
        {
            battleManager?.EndPlayerTurn();
        }

        void OnPhaseChanged(int phase)
        {
            if (phaseText != null)
                phaseText.text = $"Phase {phase}";
        }

        void OnBattleEnd(bool playerWon)
        {
            if (playerWon && victoryPanel != null)
                victoryPanel.SetActive(true);
            else if (!playerWon && defeatPanel != null)
                defeatPanel.SetActive(true);
        }

        void AddLog(string message)
        {
            _logQueue.Enqueue(message);
            while (_logQueue.Count > maxLogLines)
                _logQueue.Dequeue();

            if (battleLogText != null)
                battleLogText.text = string.Join("\n", _logQueue);
        }
    }
}
