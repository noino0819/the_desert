using System;
using System.Collections;
using UnityEngine;
using TheSSand.Player;

namespace TheSSand.Boss
{
    public abstract class BossBase : MonoBehaviour
    {
        [Header("보스 기본 설정")]
        [SerializeField] protected int maxHP = 100;
        [SerializeField] protected int totalPhases = 3;

        [Header("플레이어")]
        [SerializeField] protected int playerMaxHP = 3;
        [SerializeField] protected bool bridgeToPlayerController = true;

        protected int currentHP;
        protected int currentPhase = 1;
        protected int playerHP;
        protected bool isBattleActive;
        protected bool isStunned;

        protected PlayerController _playerController;

        public event Action<int, int> OnBossHPChanged;
        public event Action<int, int> OnPlayerHPChanged;
        public event Action<int> OnPhaseChanged;
        public event Action<bool> OnBattleEnded;

        protected virtual void Start()
        {
            currentHP = maxHP;
            playerHP = playerMaxHP;
            _playerController = FindFirstObjectByType<PlayerController>();
        }

        public virtual void StartBattle()
        {
            isBattleActive = true;
            currentPhase = 1;
            currentHP = maxHP;
            playerHP = playerMaxHP;

            if (_playerController == null)
                _playerController = FindFirstObjectByType<PlayerController>();

            if (bridgeToPlayerController && _playerController != null)
            {
                _playerController.ResetHP();
                playerHP = _playerController.MaxHP;
                playerMaxHP = _playerController.MaxHP;
            }

            OnBossHPChanged?.Invoke(currentHP, maxHP);
            OnPlayerHPChanged?.Invoke(playerHP, playerMaxHP);
            OnPhaseChanged?.Invoke(currentPhase);

            StartCoroutine(BattleLoop());
        }

        protected abstract IEnumerator BattleLoop();

        public virtual void DamageBoss(int damage)
        {
            if (!isBattleActive) return;

            currentHP = Mathf.Max(0, currentHP - damage);
            OnBossHPChanged?.Invoke(currentHP, maxHP);

            if (currentHP <= 0)
            {
                OnBossDefeated();
                return;
            }

            CheckPhaseTransition();
        }

        public virtual void DamagePlayer(int damage = 1)
        {
            if (!isBattleActive) return;

            if (bridgeToPlayerController && _playerController != null)
            {
                _playerController.TakeDamage(damage);
                playerHP = _playerController.CurrentHP;
                OnPlayerHPChanged?.Invoke(playerHP, playerMaxHP);

                if (_playerController.CurrentHP <= 0)
                    OnPlayerDefeated();
                return;
            }

            playerHP = Mathf.Max(0, playerHP - damage);
            OnPlayerHPChanged?.Invoke(playerHP, playerMaxHP);

            if (playerHP <= 0)
                OnPlayerDefeated();
        }

        protected virtual void CheckPhaseTransition()
        {
            float hpRatio = (float)currentHP / maxHP;
            int newPhase = hpRatio switch
            {
                > 0.66f => 1,
                > 0.33f => 2,
                _ => 3
            };

            if (newPhase != currentPhase)
            {
                currentPhase = newPhase;
                OnPhaseChanged?.Invoke(currentPhase);
            }
        }

        protected virtual void OnBossDefeated()
        {
            isBattleActive = false;
            OnBattleEnded?.Invoke(true);
        }

        protected virtual void OnPlayerDefeated()
        {
            isBattleActive = false;
            OnBattleEnded?.Invoke(false);
        }

        protected void RaiseBossHPChanged(int current, int max) => OnBossHPChanged?.Invoke(current, max);
        protected void RaisePlayerHPChanged(int current, int max) => OnPlayerHPChanged?.Invoke(current, max);
        protected void RaisePhaseChanged(int phase) => OnPhaseChanged?.Invoke(phase);
    }
}
