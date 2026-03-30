using System.Collections;
using UnityEngine;
using TheSSand.Card;

namespace TheSSand.Boss
{
    /// <summary>
    /// Ch.4 보스 — 과일거북 (카드 배틀)
    /// BossBase를 직접 사용하지 않고, CardBattleManager가 전투 로직을 관리.
    /// 이 클래스는 보스 비주얼/애니메이션과 CardBattleManager를 연결하는 브릿지.
    /// </summary>
    public class BossTurtle : BossBase
    {
        [Header("카드 배틀")]
        [SerializeField] CardBattleManager cardBattleManager;

        [Header("거북이 비주얼")]
        [SerializeField] Transform turtleTransform;
        [SerializeField] Animator turtleAnimator;

        bool _battleStarted;

        protected override void Start()
        {
            base.Start();

            if (cardBattleManager != null)
            {
                cardBattleManager.OnBattleEnded += OnCardBattleEnded;
                cardBattleManager.OnPhaseChanged += OnCardPhaseChanged;
                cardBattleManager.OnStateChanged += SyncVisuals;
            }
        }

        void OnDestroy()
        {
            if (cardBattleManager != null)
            {
                cardBattleManager.OnBattleEnded -= OnCardBattleEnded;
                cardBattleManager.OnPhaseChanged -= OnCardPhaseChanged;
                cardBattleManager.OnStateChanged -= SyncVisuals;
            }
        }

        public override void StartBattle()
        {
            isBattleActive = true;
            _battleStarted = true;
            cardBattleManager?.StartBattle();
        }

        protected override IEnumerator BattleLoop()
        {
            yield break;
        }

        void OnCardBattleEnded(bool playerWon)
        {
            isBattleActive = false;

            if (playerWon)
                OnBossDefeated();
            else
                OnPlayerDefeated();
        }

        void OnCardPhaseChanged(int phase)
        {
            currentPhase = phase;
            OnPhaseChanged?.Invoke(phase);

            if (turtleAnimator != null)
                turtleAnimator.SetInteger("Phase", phase);
        }

        void SyncVisuals()
        {
            if (cardBattleManager == null) return;

            currentHP = cardBattleManager.BossHP;
            OnBossHPChanged?.Invoke(currentHP, maxHP);

            playerHP = cardBattleManager.PlayerHP;
            OnPlayerHPChanged?.Invoke(playerHP, playerMaxHP);

            if (turtleAnimator != null)
            {
                float hpRatio = (float)cardBattleManager.BossHP / cardBattleManager.BossMaxHP;
                turtleAnimator.SetFloat("HPRatio", hpRatio);

                if (cardBattleManager.BossShield > 0)
                    turtleAnimator.SetBool("Shielded", true);
                else
                    turtleAnimator.SetBool("Shielded", false);
            }
        }
    }
}
