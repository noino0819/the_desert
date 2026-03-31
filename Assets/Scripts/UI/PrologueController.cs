using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TheSSand.Core;
using TheSSand.Scene;
using TheSSand.Audio;

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
        bool _isNG;

        readonly string[] _storyLines1P = new[]
        {
            "오랜만이구나, 얘야.\n오늘도 이야기 들으러 왔니?",
            "그래, 이 할아버지가 오늘은 한 용사 이야기를 해주도록 하마.",
            "오늘의 이야기는 뭐가 좋을까...\n그래, 이게 좋겠구나.",
            "{name}, 씩씩한 이름이구나.\n자, 그럼 시작해볼까.",
            "오늘의 이야기는 세상을 구한 용사의 이야기란다.\n배경은 사막이지.",
            "이 용사는 사막으로 바뀌어버린 세상을 구하기 위해 여행길에 올랐지.",
            "녹색 나라에서 살던 용사가 있었습니다.\n용사는 아주 현명하고 생명을 사랑하는 사람이었습니다.",
            "그는 어릴 적부터 자연과 지냈으며, 식물의 사랑을 듬뿍 받았습니다.\n식물의 정령들은 그에게 끊임없이 말을 걸었고, 항상 축복을 내렸습니다.",
            "용사, 있잖아.\n너가 사는 이 세상 밖에.\n모래로 가득찬 곳에서 살아가는 사람들이 있어.",
            "거기에 네 개의 오아시스가 있거든.\n거기 사람들을 도와줄 수 있다면 좋겠어.\n이 씨앗을 심으면 그들의 땅이 다시 살아날 수 있어.",
            "용사는 정령의 말을 듣고 결심했습니다.\n내가 직접 가겠다고.\n그렇게 네 개의 씨앗을 들고 여행길에 올랐습니다.",
            "와! 할아버지, 그래서 다음엔 어떻게 되었어요?",
            "하하, 얘야. 그건 말이다…"
        };

        readonly string[] _storyLinesNG = new[]
        {
            "할아버지는 죽었습니다.\n평온하게, 인공녹지 안에서.\n참회도 처벌도 없이.\n편하게.",
            "당신은 할아버지가 들려준 이야기를 기억합니다.\n그 이야기가 동화가 아니라는 것도.\n이제는.",
            "할아버지.\n이게 진짜 이야기였던 거잖아요.",
            "손자는 커서 할아버지의 이야기가 정말 할아버지의 이야기였다는 걸 깨닫습니다.",
            "그리고 사막인권운동가가 되어 사막을 구하기 위해 떠납니다.\n이번엔 반대 방향으로.\nCh.4에서 Ch.1으로."
        };

        void Start()
        {
            _isNG = GameManager.Instance != null &&
                    GameManager.Instance.CurrentSave.isNewGame2;

            AudioManager.Instance?.PlayBGM("BGM_Prologue");

            if (promptText != null)
                promptText.text = _isNG
                    ? "당신의 이름을 입력해주세요"
                    : "용사의 이름을 지어주세요";

            if (storyPanel != null) storyPanel.SetActive(false);
            if (nameInputPanel != null) nameInputPanel.SetActive(true);

            confirmButton?.onClick.AddListener(OnNameConfirmed);
            nameInputField?.onSubmit.AddListener(_ => OnNameConfirmed());
        }

        void Update()
        {
            if (storyPanel == null || !storyPanel.activeSelf) return;

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
            AudioManager.Instance?.PlaySFX("SFX_Confirm");

            if (GameManager.Instance != null)
            {
                if (_isNG)
                    GameManager.Instance.CurrentSave.playerName = _playerName;
                else
                    GameManager.Instance.NewGame(_playerName);
            }

            if (nameInputPanel != null) nameInputPanel.SetActive(false);
            if (storyPanel != null) storyPanel.SetActive(true);
            _storyIndex = 0;
            ShowCurrentStoryLine();
        }

        string[] ActiveStoryLines => _isNG ? _storyLinesNG : _storyLines1P;

        void ShowCurrentStoryLine()
        {
            var lines = ActiveStoryLines;
            if (_storyIndex >= lines.Length)
            {
                EndPrologue();
                return;
            }

            string line = lines[_storyIndex].Replace("{name}", _playerName);

            AudioManager.Instance?.PlaySFX("SFX_BookPageTurn");

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
            string target = _isNG ? "SCN_NG_Ch4_Oasis" : nextSceneName;
            SceneTransitionManager.Instance?.LoadScene(target);
        }
    }
}
