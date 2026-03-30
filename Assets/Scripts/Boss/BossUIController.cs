using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TheSSand.Boss
{
    public class BossUIController : MonoBehaviour
    {
        [Header("보스 HP")]
        [SerializeField] Slider bossHPSlider;
        [SerializeField] TextMeshProUGUI bossNameText;

        [Header("플레이어 HP")]
        [SerializeField] Image[] heartIcons;
        [SerializeField] Sprite heartFull;
        [SerializeField] Sprite heartEmpty;

        [Header("페이즈")]
        [SerializeField] TextMeshProUGUI phaseText;

        [Header("결과")]
        [SerializeField] GameObject victoryPanel;
        [SerializeField] GameObject defeatPanel;
        [SerializeField] Button retryButton;
        [SerializeField] Button retryFromStartButton;

        [Header("보스 참조")]
        [SerializeField] BossBase boss;

        void Start()
        {
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (defeatPanel != null) defeatPanel.SetActive(false);

            if (boss != null)
            {
                boss.OnBossHPChanged += UpdateBossHP;
                boss.OnPlayerHPChanged += UpdatePlayerHP;
                boss.OnPhaseChanged += UpdatePhase;
                boss.OnBattleEnded += OnBattleEnd;
            }
        }

        void OnDestroy()
        {
            if (boss != null)
            {
                boss.OnBossHPChanged -= UpdateBossHP;
                boss.OnPlayerHPChanged -= UpdatePlayerHP;
                boss.OnPhaseChanged -= UpdatePhase;
                boss.OnBattleEnded -= OnBattleEnd;
            }
        }

        void UpdateBossHP(int current, int max)
        {
            if (bossHPSlider != null)
            {
                bossHPSlider.maxValue = max;
                bossHPSlider.value = current;
            }
        }

        void UpdatePlayerHP(int current, int max)
        {
            for (int i = 0; i < heartIcons.Length; i++)
            {
                if (heartIcons[i] == null) continue;
                heartIcons[i].sprite = i < current ? heartFull : heartEmpty;
                heartIcons[i].gameObject.SetActive(i < max);
            }
        }

        void UpdatePhase(int phase)
        {
            if (phaseText != null)
                phaseText.text = $"Phase {phase}";
        }

        void OnBattleEnd(bool playerWon)
        {
            if (playerWon)
            {
                if (victoryPanel != null)
                    victoryPanel.SetActive(true);
            }
            else
            {
                if (defeatPanel != null)
                    defeatPanel.SetActive(true);
            }
        }
    }
}
