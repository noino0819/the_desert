using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TheSSand.Core;
using TheSSand.Scene;

namespace TheSSand.UI
{
    public class PrologueController : MonoBehaviour
    {
        [Header("이름 입력 UI")]
        [SerializeField] GameObject nameInputPanel;
        [SerializeField] TMP_InputField nameInputField;
        [SerializeField] Button confirmButton;
        [SerializeField] TextMeshProUGUI promptText;

        [Header("동화 연출 UI")]
        [SerializeField] GameObject storyPanel;
        [SerializeField] TextMeshProUGUI storyText;
        [SerializeField] Image grandpaPortrait;
        [SerializeField] Image pageBackground;

        [Header("설정")]
        [SerializeField] float typingSpeed = 0.04f;
        [SerializeField] string nextSceneName = "SCN_Ch1_Desert";

        string _playerName;
        int _storyIndex;
        bool _isTyping;
        string _fullText;
        Coroutine _typingCoroutine;

        readonly string[] _storyLines = new[]
        {
            "옛날 옛적, 푸른 숲이 온 세상을 뒤덮던 시절이 있었단다.",
            "하지만 어느 날부터 숲이 사라지기 시작했어.",
            "초록빛 나무들이 모래로 변하고, 강물이 말라버렸지.",
            "사람들은 슬퍼했단다. 이대로는 모두 사라져버릴 거라고.",
            "그때, 한 용감한 용사가 나타났어.",
            "작은 정령이 용사에게 말했지. \"이 씨앗을 사막에 심으면 다시 녹색 세상이 돌아올 거예요.\"",
            "용사는 씨앗 네 개를 들고 머나먼 사막으로 떠났단다.",
            "자, 이제 그 용사의 이야기를 들려줄게."
        };

        void Start()
        {
            bool isNG = GameManager.Instance != null &&
                        GameManager.Instance.CurrentSave.isNewGame2;

            if (promptText != null)
                promptText.text = isNG
                    ? "손자의 이름을 알려주렴."
                    : "이 이야기의 주인공 이름을 지어줄래?";

            storyPanel?.SetActive(false);
            nameInputPanel?.SetActive(true);

            confirmButton?.onClick.AddListener(OnNameConfirmed);
            nameInputField?.onSubmit.AddListener(_ => OnNameConfirmed());
        }

        void Update()
        {
            if (!storyPanel.activeSelf) return;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (_isTyping)
                    FinishTyping();
                else
                    AdvanceStory();
            }
        }

        void OnNameConfirmed()
        {
            string inputName = nameInputField?.text?.Trim();
            if (string.IsNullOrEmpty(inputName)) return;

            _playerName = inputName;

            bool isNG = GameManager.Instance != null &&
                        GameManager.Instance.CurrentSave.isNewGame2;

            if (GameManager.Instance != null)
            {
                if (isNG)
                    GameManager.Instance.CurrentSave.playerName = _playerName;
                else
                    GameManager.Instance.NewGame(_playerName);
            }

            nameInputPanel?.SetActive(false);
            storyPanel?.SetActive(true);
            _storyIndex = 0;
            ShowCurrentStoryLine();
        }

        void ShowCurrentStoryLine()
        {
            if (_storyIndex >= _storyLines.Length)
            {
                EndPrologue();
                return;
            }

            string line = _storyLines[_storyIndex].Replace("{name}", _playerName);

            if (_typingCoroutine != null)
                StopCoroutine(_typingCoroutine);
            _typingCoroutine = StartCoroutine(TypeText(line));
        }

        IEnumerator TypeText(string text)
        {
            _isTyping = true;
            _fullText = text;
            storyText.text = "";

            foreach (char c in text)
            {
                storyText.text += c;
                yield return new WaitForSecondsRealtime(typingSpeed);
            }
            _isTyping = false;
        }

        void FinishTyping()
        {
            if (_typingCoroutine != null)
                StopCoroutine(_typingCoroutine);
            storyText.text = _fullText;
            _isTyping = false;
        }

        void AdvanceStory()
        {
            _storyIndex++;
            ShowCurrentStoryLine();
        }

        void EndPrologue()
        {
            bool isNG = GameManager.Instance != null &&
                        GameManager.Instance.CurrentSave.isNewGame2;

            string target = isNG ? "SCN_NG_Ch4_Oasis" : nextSceneName;
            SceneTransitionManager.Instance?.LoadScene(target);
        }
    }
}
