using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TheSSand.Core;
using TheSSand.Quest;

namespace TheSSand.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [Header("UI 참조")]
        [SerializeField] GameObject dialoguePanel;
        [SerializeField] Image portraitImage;
        [SerializeField] TextMeshProUGUI speakerNameText;
        [SerializeField] TextMeshProUGUI dialogueText;
        [SerializeField] Transform choicesContainer;
        [SerializeField] GameObject choiceButtonPrefab;

        [Header("타이핑 설정")]
        [SerializeField] float typingSpeed = 0.03f;
        [SerializeField] float fastForwardSpeed = 0.005f;

        DialogueData _currentDialogue;
        int _currentLineIndex;
        bool _isTyping;
        bool _isDialogueActive;
        string _fullText;
        Coroutine _typingCoroutine;

        public event Action OnDialogueStarted;
        public event Action OnDialogueEnded;
        public event Action<string, string> OnChoiceSelected;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
        }

        void Update()
        {
            if (!_isDialogueActive) return;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (_isTyping)
                    FinishTyping();
                else
                    AdvanceLine();
            }

            if (Input.GetKey(KeyCode.Space) && !_isTyping)
                AdvanceLine();
        }

        #region 공개 API

        public void StartDialogue(string npcId)
        {
            TextAsset jsonAsset = LoadDialogueJson(npcId);
            if (jsonAsset == null)
            {
                Debug.LogWarning($"[DialogueManager] '{npcId}' 대화 데이터 없음");
                return;
            }

            var data = JsonUtility.FromJson<DialogueData>(jsonAsset.text);
            if (data == null || data.lines == null || data.lines.Length == 0) return;

            _currentDialogue = data;
            _currentLineIndex = 0;
            _isDialogueActive = true;

            if (dialoguePanel != null)
                dialoguePanel.SetActive(true);

            LockPlayerInput(true);
            OnDialogueStarted?.Invoke();
            ShowCurrentLine();
        }

        public void StartDialogue(DialogueData data)
        {
            if (data == null || data.lines == null || data.lines.Length == 0) return;

            _currentDialogue = data;
            _currentLineIndex = 0;
            _isDialogueActive = true;

            if (dialoguePanel != null)
                dialoguePanel.SetActive(true);

            LockPlayerInput(true);
            OnDialogueStarted?.Invoke();
            ShowCurrentLine();
        }

        public bool IsDialogueActive => _isDialogueActive;

        #endregion

        #region 대화 진행

        void ShowCurrentLine()
        {
            ClearChoices();

            DialogueLine line = FindNextValidLine();
            if (line == null)
            {
                EndDialogue();
                return;
            }

            if (speakerNameText != null)
                speakerNameText.text = line.speaker;

            UpdatePortrait(line.portrait);

            if (line.choices != null && line.choices.Length > 0)
            {
                ShowTextImmediate(line.text);
                ShowChoices(line.choices);
            }
            else
            {
                if (_typingCoroutine != null)
                    StopCoroutine(_typingCoroutine);
                _typingCoroutine = StartCoroutine(TypeText(line.text));
            }
        }

        DialogueLine FindNextValidLine()
        {
            while (_currentLineIndex < _currentDialogue.lines.Length)
            {
                DialogueLine line = _currentDialogue.lines[_currentLineIndex];

                if (!EvaluateCondition(line.condition))
                {
                    _currentLineIndex++;
                    continue;
                }

                if (line.isNG && (GameManager.Instance == null || !GameManager.Instance.CurrentSave.isNewGame2))
                {
                    _currentLineIndex++;
                    continue;
                }

                return line;
            }
            return null;
        }

        void AdvanceLine()
        {
            _currentLineIndex++;
            if (_currentLineIndex >= _currentDialogue.lines.Length)
            {
                EndDialogue();
                return;
            }
            ShowCurrentLine();
        }

        void EndDialogue()
        {
            _isDialogueActive = false;
            _currentDialogue = null;

            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);

            ClearChoices();
            LockPlayerInput(false);
            OnDialogueEnded?.Invoke();
        }

        void LockPlayerInput(bool locked)
        {
            var player = FindAnyObjectByType<Player.PlayerController>();
            if (player != null)
                player.LockInput(locked);
        }

        #endregion

        #region 타이핑 효과

        IEnumerator TypeText(string text)
        {
            _isTyping = true;
            _fullText = text;
            dialogueText.text = "";

            foreach (char c in text)
            {
                dialogueText.text += c;
                yield return new WaitForSecondsRealtime(typingSpeed);
            }
            _isTyping = false;
        }

        void FinishTyping()
        {
            if (_typingCoroutine != null)
                StopCoroutine(_typingCoroutine);

            dialogueText.text = _fullText;
            _isTyping = false;
        }

        void ShowTextImmediate(string text)
        {
            if (_typingCoroutine != null)
                StopCoroutine(_typingCoroutine);

            _fullText = text;
            dialogueText.text = text;
            _isTyping = false;
        }

        #endregion

        #region 선택지

        void ShowChoices(DialogueChoice[] choices)
        {
            if (choicesContainer == null || choiceButtonPrefab == null) return;

            foreach (var choice in choices)
            {
                if (!EvaluateCondition(choice.condition)) continue;

                GameObject btnObj = Instantiate(choiceButtonPrefab, choicesContainer);
                var tmpText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                if (tmpText != null) tmpText.text = choice.text;

                var button = btnObj.GetComponent<Button>();
                if (button != null)
                {
                    var captured = choice;
                    button.onClick.AddListener(() => OnChoiceClicked(captured));
                }
            }
        }

        void OnChoiceClicked(DialogueChoice choice)
        {
            if (!string.IsNullOrEmpty(choice.flagToSet))
                ApplyFlag(choice.flagToSet);

            if (!string.IsNullOrEmpty(choice.questToProgress))
                QuestManager.Instance?.AdvanceProgress(choice.questToProgress);

            OnChoiceSelected?.Invoke(choice.text, choice.flagToSet);

            ClearChoices();

            if (!string.IsNullOrEmpty(choice.nextLineId))
            {
                int idx = FindLineById(choice.nextLineId);
                if (idx >= 0)
                {
                    _currentLineIndex = idx;
                    ShowCurrentLine();
                    return;
                }
            }

            AdvanceLine();
        }

        void ClearChoices()
        {
            if (choicesContainer == null) return;
            for (int i = choicesContainer.childCount - 1; i >= 0; i--)
                Destroy(choicesContainer.GetChild(i).gameObject);
        }

        #endregion

        #region 조건 평가

        bool EvaluateCondition(DialogueCondition condition)
        {
            if (condition == null || string.IsNullOrEmpty(condition.questId)) return true;

            if (QuestManager.Instance == null) return true;

            var currentState = QuestManager.Instance.GetState(condition.questId);
            QuestState requiredState = ParseQuestState(condition.state);

            return currentState == requiredState;
        }

        QuestState ParseQuestState(string stateStr)
        {
            return stateStr switch
            {
                "Active" => QuestState.Active,
                "Completed" => QuestState.Completed,
                "Failed" => QuestState.Failed,
                _ => QuestState.Inactive
            };
        }

        #endregion

        #region 플래그 적용

        void ApplyFlag(string flag)
        {
            if (GameManager.Instance == null) return;

            switch (flag)
            {
                case "saidIllComeBack":
                    GameManager.Instance.SetSaidIllComeBack();
                    break;
                case "scarfPickedUp":
                    GameManager.Instance.SetScarfPickedUp();
                    break;
                case "scarfGiven":
                    GameManager.Instance.SetScarfGiven();
                    break;
                case "mudCookieDelivered":
                    GameManager.Instance.SetMudCookieDelivered();
                    break;
                case "isDoubleAgent":
                    GameManager.Instance.SetDoubleAgent(true);
                    break;
                case "plantSeed":
                    GameManager.Instance.PlantSeed();
                    break;
                case "refuseSeed":
                    GameManager.Instance.RefuseSeed();
                    break;
            }
        }

        #endregion

        #region 유틸

        TextAsset LoadDialogueJson(string npcId)
        {
            bool isNG = GameManager.Instance != null &&
                        GameManager.Instance.CurrentSave.isNewGame2;

            TextAsset asset = null;
            if (isNG)
                asset = Resources.Load<TextAsset>($"Dialogues/ng/{npcId}");

            return asset != null ? asset : Resources.Load<TextAsset>($"Dialogues/{npcId}");
        }

        int FindLineById(string lineId)
        {
            for (int i = 0; i < _currentDialogue.lines.Length; i++)
            {
                if (_currentDialogue.lines[i].lineId == lineId)
                    return i;
            }
            return -1;
        }

        void UpdatePortrait(string portraitName)
        {
            if (portraitImage == null || string.IsNullOrEmpty(portraitName)) return;

            Sprite sprite = Resources.Load<Sprite>($"Portraits/{portraitName}");
            if (sprite != null)
                portraitImage.sprite = sprite;
        }

        #endregion
    }

    #region 대화 데이터 구조체

    [Serializable]
    public class DialogueData
    {
        public string npcId;
        public DialogueLine[] lines;
    }

    [Serializable]
    public class DialogueLine
    {
        public string lineId;
        public string speaker;
        public string portrait;
        public string text;
        public bool isNG;
        public DialogueCondition condition;
        public DialogueChoice[] choices;
    }

    [Serializable]
    public class DialogueCondition
    {
        public string questId;
        public string state;
    }

    [Serializable]
    public class DialogueChoice
    {
        public string text;
        public string nextLineId;
        public string flagToSet;
        public string questToProgress;
        public DialogueCondition condition;
    }

    #endregion
}
