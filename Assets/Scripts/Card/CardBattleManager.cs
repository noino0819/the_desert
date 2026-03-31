using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheSSand.Card
{
    /// <summary>
    /// 카드 배틀 시스템 — 턴 기반 전투 관리
    /// 에너지/드로우/플레이/버림 사이클 + 보스 AI
    /// </summary>
    public class CardBattleManager : MonoBehaviour
    {
        [Header("기본 설정")]
        [SerializeField] int maxEnergy = 3;
        [SerializeField] int drawPerTurn = 5;
        [SerializeField] int maxHandSize = 10;

        [Header("보스 설정")]
        [SerializeField] int bossMaxHP = 100;
        [SerializeField] int bossShieldPerTurn = 5;
        [SerializeField] int bossHealPhase2 = 5;
        [SerializeField] int bossStunChancePhase3 = 30;

        [Header("플레이어 설정")]
        [SerializeField] int playerMaxHP = 3;

        [Header("시작 덱")]
        [SerializeField] CardData[] starterDeck;

        int _currentEnergy;
        int _playerHP;
        int _playerShield;
        int _bossHP;
        int _bossShield;
        int _currentPhase = 1;
        int _turnCount;
        bool _isPlayerTurn;
        bool _battleActive;
        bool _playerStunned;
        int _poisonOnBoss;

        DeckManager _deck;

        public int CurrentEnergy => _currentEnergy;
        public int MaxEnergy => maxEnergy;
        public int PlayerHP => _playerHP;
        public int PlayerShield => _playerShield;
        public int BossHP => _bossHP;
        public int BossMaxHP => bossMaxHP;
        public int BossShield => _bossShield;
        public int TurnCount => _turnCount;
        public int CurrentPhase => _currentPhase;
        public bool IsPlayerTurn => _isPlayerTurn;
        public bool BattleActive => _battleActive;
        public DeckManager Deck => _deck;

        public event Action OnTurnStart;
        public event Action OnTurnEnd;
        public event Action<CardData> OnCardPlayed;
        public event Action OnStateChanged;
        public event Action<bool> OnBattleEnded;
        public event Action<int> OnPhaseChanged;
        public event Action<string> OnBattleLog;

        void Awake()
        {
            _deck = new DeckManager();
        }

        public void StartBattle()
        {
            _deck.InitDeck(starterDeck);

            _playerHP = playerMaxHP;
            _playerShield = 0;
            _bossHP = bossMaxHP;
            _bossShield = 0;
            _currentPhase = 1;
            _turnCount = 0;
            _poisonOnBoss = 0;
            _playerStunned = false;
            _battleActive = true;

            OnStateChanged?.Invoke();
            StartCoroutine(BattleLoop());
        }

        IEnumerator BattleLoop()
        {
            while (_battleActive)
            {
                _turnCount++;
                yield return StartCoroutine(PlayerTurn());

                if (!_battleActive) break;

                yield return StartCoroutine(BossTurn());

                if (!_battleActive) break;

                CheckPhaseTransition();
            }
        }

        #region 플레이어 턴

        IEnumerator PlayerTurn()
        {
            _isPlayerTurn = true;
            _currentEnergy = maxEnergy;
            _playerShield = 0;

            _deck.DiscardHand();
            _deck.Draw(Mathf.Min(drawPerTurn, maxHandSize));

            OnTurnStart?.Invoke();
            OnStateChanged?.Invoke();
            OnBattleLog?.Invoke($"턴 {_turnCount} — 당신의 차례");

            if (_playerStunned)
            {
                OnBattleLog?.Invoke("뿌리박기! 이번 턴은 카드를 쓸 수 없습니다.");
                _playerStunned = false;
                yield return new WaitForSeconds(1.5f);
            }
            else
            {
                while (_isPlayerTurn && _battleActive)
                    yield return null;
            }

            OnTurnEnd?.Invoke();
        }

        public bool TryPlayCard(CardData card)
        {
            if (!_isPlayerTurn || !_battleActive) return false;
            if (_currentEnergy < card.energyCost) return false;
            if (!_deck.Hand.Contains(card)) return false;

            _currentEnergy -= card.energyCost;
            _deck.PlayCard(card);
            ResolveCard(card);

            OnCardPlayed?.Invoke(card);
            OnStateChanged?.Invoke();

            if (_bossHP <= 0)
            {
                _battleActive = false;
                OnBattleEnded?.Invoke(true);
            }

            return true;
        }

        void ResolveCard(CardData card)
        {
            if (card.attackValue > 0)
            {
                int dmg = card.attackValue;

                if (card.chainEffect)
                    dmg *= card.chainMultiplier;

                int actualDmg = Mathf.Max(0, dmg - _bossShield);
                _bossShield = Mathf.Max(0, _bossShield - dmg);
                _bossHP = Mathf.Max(0, _bossHP - actualDmg);
                OnBattleLog?.Invoke($"{card.cardName}: {dmg} 공격 → 보스에게 {actualDmg} 데미지");
            }

            if (card.defenseValue > 0)
            {
                _playerShield += card.defenseValue;
                OnBattleLog?.Invoke($"{card.cardName}: 방어 +{card.defenseValue}");
            }

            if (card.healValue > 0)
            {
                _playerHP = Mathf.Min(playerMaxHP, _playerHP + card.healValue);
                OnBattleLog?.Invoke($"{card.cardName}: HP +{card.healValue}");
            }

            if (card.drawCount > 0)
            {
                _deck.Draw(card.drawCount);
                OnBattleLog?.Invoke($"{card.cardName}: 카드 {card.drawCount}장 드로우");
            }

            if (card.applyPoison)
            {
                _poisonOnBoss += card.poisonStacks;
                OnBattleLog?.Invoke($"{card.cardName}: 독 {card.poisonStacks} 부여");
            }
        }

        public void EndPlayerTurn()
        {
            if (!_isPlayerTurn) return;
            _isPlayerTurn = false;
        }

        #endregion

        #region 보스 턴

        IEnumerator BossTurn()
        {
            OnBattleLog?.Invoke("보스의 차례");
            yield return new WaitForSeconds(0.5f);

            if (_poisonOnBoss > 0)
            {
                int poisonDmg = _poisonOnBoss;
                _bossHP = Mathf.Max(0, _bossHP - poisonDmg);
                _poisonOnBoss = Mathf.Max(0, _poisonOnBoss - 1);
                OnBattleLog?.Invoke($"독: 보스에게 {poisonDmg} 데미지");
                OnStateChanged?.Invoke();

                if (_bossHP <= 0)
                {
                    _battleActive = false;
                    OnBattleEnded?.Invoke(true);
                    yield break;
                }

                yield return new WaitForSeconds(0.3f);
            }

            _bossShield = bossShieldPerTurn * _currentPhase;
            OnBattleLog?.Invoke($"등딱지 방어: 실드 {_bossShield}");
            OnStateChanged?.Invoke();
            yield return new WaitForSeconds(0.3f);

            int bossAttack = GetBossAttack();
            int actualDmg = Mathf.Max(0, bossAttack - _playerShield);
            _playerShield = Mathf.Max(0, _playerShield - bossAttack);

            if (actualDmg > 0)
            {
                _playerHP = Mathf.Max(0, _playerHP - actualDmg);
                OnBattleLog?.Invoke($"보스 공격: {bossAttack} → 당신에게 {actualDmg} 데미지");
            }
            else
            {
                OnBattleLog?.Invoke($"보스 공격: {bossAttack} → 방어로 막음!");
            }

            OnStateChanged?.Invoke();
            yield return new WaitForSeconds(0.3f);

            if (_currentPhase >= 2)
            {
                _bossHP = Mathf.Min(bossMaxHP, _bossHP + bossHealPhase2);
                OnBattleLog?.Invoke($"보스 회복: +{bossHealPhase2} HP");
                OnStateChanged?.Invoke();
                yield return new WaitForSeconds(0.3f);
            }

            if (_currentPhase >= 3 && UnityEngine.Random.Range(0, 100) < bossStunChancePhase3)
            {
                _playerStunned = true;
                OnBattleLog?.Invoke("뿌리박기! 다음 턴 행동 불가");
                yield return new WaitForSeconds(0.3f);
            }

            if (_playerHP <= 0)
            {
                _battleActive = false;
                OnBattleEnded?.Invoke(false);
            }

            OnStateChanged?.Invoke();
            yield return new WaitForSeconds(0.5f);
        }

        int GetBossAttack()
        {
            return _currentPhase switch
            {
                1 => UnityEngine.Random.Range(1, 3),
                2 => UnityEngine.Random.Range(1, 4),
                _ => UnityEngine.Random.Range(2, 5)
            };
        }

        #endregion

        void CheckPhaseTransition()
        {
            float hpRatio = (float)_bossHP / bossMaxHP;
            int newPhase = hpRatio switch
            {
                > 0.66f => 1,
                > 0.33f => 2,
                _ => 3
            };

            if (newPhase != _currentPhase)
            {
                _currentPhase = newPhase;
                OnPhaseChanged?.Invoke(_currentPhase);
                OnBattleLog?.Invoke($"Phase {_currentPhase}!");
            }
        }

        public void AddCardToDeck(CardData card)
        {
            _deck.AddCard(card);
        }
    }
}
