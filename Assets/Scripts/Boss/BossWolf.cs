using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheSSand.Boss
{
    /// <summary>
    /// Ch.3 보스 — 보름달 아래의 춤 (리듬게임 검투)
    /// 3/4 왈츠 박자. 늑대 공격에 맞춰 방향키 입력으로 방어/반격.
    /// Phase 1: 발톱 베기, 느린 템포
    /// Phase 2: 발톱 + 연속 물기, 템포 증가
    /// Phase 3: 복합 + 원형 회전, 최고 템포
    /// 판정: Perfect / Good / Miss
    /// </summary>
    public class BossWolf : BossBase
    {
        public enum NoteDirection { Left, Right, Up, Down }
        public enum JudgeResult { Perfect, Good, Miss }

        [System.Serializable]
        public class RhythmNote
        {
            public NoteDirection direction;
            public float beatTime;
            public bool isHold;
            public float holdDuration;
        }

        [Header("리듬 설정")]
        [SerializeField] float baseBPM = 120f;
        [SerializeField] float p2BPMMultiplier = 1.3f;
        [SerializeField] float p3BPMMultiplier = 1.6f;
        [SerializeField] float perfectWindow = 0.08f;
        [SerializeField] float goodWindow = 0.15f;

        [Header("데미지")]
        [SerializeField] int perfectDamage = 8;
        [SerializeField] int goodDamage = 4;
        [SerializeField] int missDamageToPlayer = 1;

        [Header("패턴 — Phase 1 (발톱 베기)")]
        [SerializeField] NoteDirection[] p1Pattern = {
            NoteDirection.Left, NoteDirection.Right, NoteDirection.Left
        };

        [Header("패턴 — Phase 2 (발톱 + 물기)")]
        [SerializeField] NoteDirection[] p2Pattern = {
            NoteDirection.Left, NoteDirection.Right, NoteDirection.Up,
            NoteDirection.Left, NoteDirection.Right
        };

        [Header("패턴 — Phase 3 (복합 + 원형)")]
        [SerializeField] NoteDirection[] p3Pattern = {
            NoteDirection.Up, NoteDirection.Right, NoteDirection.Down,
            NoteDirection.Left, NoteDirection.Up, NoteDirection.Right,
            NoteDirection.Down
        };

        [Header("비주얼")]
        [SerializeField] Transform wolfTransform;
        [SerializeField] Transform playerTransform;

        float _currentBPM;
        float _beatInterval;
        float _songPosition;
        float _lastBeatTime;
        bool _waitingForInput;
        NoteDirection _currentExpectedDir;
        int _comboCount;
        int _patternIndex;

        readonly List<RhythmNote> _activeNotes = new();
        readonly Queue<JudgeResult> _recentResults = new();

        public event System.Action<NoteDirection, float> OnNoteSpawned;
        public event System.Action<JudgeResult, int> OnJudge;
        public event System.Action<int> OnComboChanged;

        protected override void Start()
        {
            base.Start();
            UpdateBPM();
        }

        void Update()
        {
            if (!isBattleActive) return;

            _songPosition += Time.deltaTime;
            CheckPlayerInput();
        }

        void UpdateBPM()
        {
            _currentBPM = currentPhase switch
            {
                1 => baseBPM,
                2 => baseBPM * p2BPMMultiplier,
                _ => baseBPM * p3BPMMultiplier
            };
            _beatInterval = 60f / _currentBPM;
        }

        void CheckPlayerInput()
        {
            if (!_waitingForInput) return;

            NoteDirection? input = null;

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                input = NoteDirection.Left;
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                input = NoteDirection.Right;
            else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                input = NoteDirection.Up;
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                input = NoteDirection.Down;

            if (input.HasValue)
            {
                float timeDiff = Mathf.Abs(_songPosition - _lastBeatTime);
                JudgeResult result;

                if (input.Value != _currentExpectedDir)
                {
                    result = JudgeResult.Miss;
                }
                else if (timeDiff <= perfectWindow)
                {
                    result = JudgeResult.Perfect;
                }
                else if (timeDiff <= goodWindow)
                {
                    result = JudgeResult.Good;
                }
                else
                {
                    result = JudgeResult.Miss;
                }

                ProcessJudgement(result);
                _waitingForInput = false;
            }
        }

        void ProcessJudgement(JudgeResult result)
        {
            switch (result)
            {
                case JudgeResult.Perfect:
                    DamageBoss(perfectDamage);
                    _comboCount++;
                    break;

                case JudgeResult.Good:
                    DamageBoss(goodDamage);
                    _comboCount++;
                    break;

                case JudgeResult.Miss:
                    if (bossAnimator != null)
                    {
                        int atkType = currentPhase >= 2 ? 2 : 1;
                        bossAnimator.SetInteger("AttackType", atkType);
                    }
                    DamagePlayer(missDamageToPlayer);
                    _comboCount = 0;
                    break;
            }

            OnJudge?.Invoke(result, _comboCount);
            OnComboChanged?.Invoke(_comboCount);
        }

        NoteDirection[] GetCurrentPattern()
        {
            return currentPhase switch
            {
                1 => p1Pattern,
                2 => p2Pattern,
                _ => p3Pattern
            };
        }

        #region 배틀 루프

        protected override IEnumerator BattleLoop()
        {
            yield return new WaitForSeconds(1f);

            while (isBattleActive)
            {
                var pattern = GetCurrentPattern();
                _patternIndex = 0;

                while (_patternIndex < pattern.Length && isBattleActive)
                {
                    _currentExpectedDir = pattern[_patternIndex];
                    _lastBeatTime = _songPosition + _beatInterval;

                    OnNoteSpawned?.Invoke(_currentExpectedDir, _beatInterval);

                    yield return new WaitForSeconds(_beatInterval * 0.7f);

                    _waitingForInput = true;
                    _lastBeatTime = _songPosition;

                    yield return new WaitForSeconds(_beatInterval * 0.3f + goodWindow);

                    if (_waitingForInput)
                    {
                        ProcessJudgement(JudgeResult.Miss);
                        _waitingForInput = false;
                    }

                    _patternIndex++;
                }

                if (!isBattleActive) break;

                yield return new WaitForSeconds(_beatInterval * 2f);

                if (currentPhase >= 3 && isBattleActive)
                {
                    yield return StartCoroutine(CircularSpin());
                }
            }
        }

        IEnumerator CircularSpin()
        {
            if (bossAnimator != null) bossAnimator.SetBool("IsSpinning", true);

            NoteDirection[] spin = {
                NoteDirection.Up, NoteDirection.Right,
                NoteDirection.Down, NoteDirection.Left,
                NoteDirection.Up, NoteDirection.Right
            };

            float fastBeat = _beatInterval * 0.6f;

            foreach (var dir in spin)
            {
                if (!isBattleActive) break;

                _currentExpectedDir = dir;
                OnNoteSpawned?.Invoke(dir, fastBeat);

                yield return new WaitForSeconds(fastBeat * 0.7f);

                _waitingForInput = true;
                _lastBeatTime = _songPosition;

                yield return new WaitForSeconds(fastBeat * 0.3f + goodWindow);

                if (_waitingForInput)
                {
                    ProcessJudgement(JudgeResult.Miss);
                    _waitingForInput = false;
                }
            }

            if (bossAnimator != null) bossAnimator.SetBool("IsSpinning", false);
        }

        #endregion

        protected override void CheckPhaseTransition()
        {
            int prevPhase = currentPhase;
            base.CheckPhaseTransition();

            if (currentPhase != prevPhase)
                UpdateBPM();
        }
    }
}
