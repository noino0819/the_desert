using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TheSSand.Core;
using TheSSand.Scene;
using TheSSand.Dialogue;

namespace TheSSand.Boss
{
    public class Ch3BossController : MonoBehaviour
    {
        [SerializeField] BossWolf bossWolf;
        [SerializeField] DialogueData seedDialogue;
        [SerializeField] string nextSceneName = "SCN_Ch4_Desert";

        [Header("리듬 UI")]
        [SerializeField] RectTransform noteSpawnArea;
        [SerializeField] GameObject noteArrowPrefab;
        [SerializeField] RectTransform judgeLine;
        [SerializeField] TextMeshProUGUI judgeText;
        [SerializeField] TextMeshProUGUI comboText;
        [SerializeField] float noteScrollDuration = 1f;

        [Header("방향 스프라이트")]
        [SerializeField] Sprite arrowLeft;
        [SerializeField] Sprite arrowRight;
        [SerializeField] Sprite arrowUp;
        [SerializeField] Sprite arrowDown;

        [Header("판정 색상")]
        [SerializeField] Color perfectColor = Color.yellow;
        [SerializeField] Color goodColor = Color.green;
        [SerializeField] Color missColor = Color.red;

        void Start()
        {
            if (bossWolf != null)
            {
                bossWolf.OnBattleEnded += OnBattleEnded;
                bossWolf.OnNoteSpawned += SpawnNoteUI;
                bossWolf.OnJudge += ShowJudgeUI;
                bossWolf.OnComboChanged += UpdateCombo;
                bossWolf.StartBattle();
            }

            SceneTransitionManager.Instance?.FadeIn();
        }

        void OnDestroy()
        {
            if (bossWolf != null)
            {
                bossWolf.OnBattleEnded -= OnBattleEnded;
                bossWolf.OnNoteSpawned -= SpawnNoteUI;
                bossWolf.OnJudge -= ShowJudgeUI;
                bossWolf.OnComboChanged -= UpdateCombo;
            }
        }

        void SpawnNoteUI(BossWolf.NoteDirection dir, float beatInterval)
        {
            if (noteArrowPrefab == null || noteSpawnArea == null) return;

            var noteObj = Instantiate(noteArrowPrefab, noteSpawnArea);
            var img = noteObj.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = dir switch
                {
                    BossWolf.NoteDirection.Left => arrowLeft,
                    BossWolf.NoteDirection.Right => arrowRight,
                    BossWolf.NoteDirection.Up => arrowUp,
                    BossWolf.NoteDirection.Down => arrowDown,
                    _ => arrowUp
                };
            }

            var rt = noteObj.GetComponent<RectTransform>();
            if (rt != null && judgeLine != null)
            {
                var mover = noteObj.AddComponent<NoteArrowMover>();
                mover.Setup(rt, judgeLine.anchoredPosition, noteScrollDuration);
            }
        }

        void ShowJudgeUI(BossWolf.JudgeResult result, int combo)
        {
            if (judgeText == null) return;

            judgeText.text = result switch
            {
                BossWolf.JudgeResult.Perfect => "PERFECT!",
                BossWolf.JudgeResult.Good => "GOOD",
                _ => "MISS"
            };

            judgeText.color = result switch
            {
                BossWolf.JudgeResult.Perfect => perfectColor,
                BossWolf.JudgeResult.Good => goodColor,
                _ => missColor
            };

            CancelInvoke(nameof(ClearJudge));
            Invoke(nameof(ClearJudge), 0.5f);
        }

        void ClearJudge()
        {
            if (judgeText != null) judgeText.text = "";
        }

        void UpdateCombo(int combo)
        {
            if (comboText != null)
                comboText.text = combo > 1 ? $"{combo} COMBO" : "";
        }

        void OnBattleEnded(bool playerWon)
        {
            if (!playerWon)
            {
                GameManager.Instance?.OnGameOver();
                return;
            }

            ShowSeedChoice();
        }

        void ShowSeedChoice()
        {
            if (DialogueManager.Instance != null && seedDialogue != null)
            {
                DialogueManager.Instance.OnChoiceSelected += OnSeedChoice;
                DialogueManager.Instance.OnDialogueEnded += OnSeedDialogueEnded;
                DialogueManager.Instance.StartDialogue(seedDialogue);
            }
            else
            {
                ProceedToNextScene();
            }
        }

        void OnSeedChoice(string choiceText, string flag)
        {
            if (flag == "plantSeed")
            {
                GameManager.Instance?.PlantSeed();
                GameManager.Instance?.UnlockAbility(3);
            }
            else if (flag == "refuseSeed")
            {
                GameManager.Instance?.RefuseSeed();
                GameManager.Instance?.UnlockAbility(3);
            }
        }

        void OnSeedDialogueEnded()
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnChoiceSelected -= OnSeedChoice;
                DialogueManager.Instance.OnDialogueEnded -= OnSeedDialogueEnded;
            }

            ProceedToNextScene();
        }

        void ProceedToNextScene()
        {
            SceneTransitionManager.Instance?.LoadScene(nextSceneName);
        }
    }

    public class NoteArrowMover : MonoBehaviour
    {
        RectTransform _rt;
        Vector2 _targetPos;
        Vector2 _startPos;
        float _duration;
        float _elapsed;

        public void Setup(RectTransform rt, Vector2 targetPos, float duration)
        {
            _rt = rt;
            _targetPos = targetPos;
            _startPos = rt.anchoredPosition;
            _duration = duration;
            _elapsed = 0f;
        }

        void Update()
        {
            _elapsed += Time.deltaTime;
            float t = _elapsed / _duration;

            if (_rt != null)
                _rt.anchoredPosition = Vector2.Lerp(_startPos, _targetPos, t);

            if (t >= 1.2f)
                Destroy(gameObject);
        }
    }
}
