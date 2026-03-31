using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace TheSSand.UI
{
    public class LoadingScreen : MonoBehaviour
    {
        public static LoadingScreen Instance { get; private set; }

        [SerializeField] GameObject loadingPanel;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Slider progressBar;
        [SerializeField] TextMeshProUGUI progressText;
        [SerializeField] TextMeshProUGUI tipText;
        [SerializeField] float fadeDuration = 0.3f;
        [SerializeField] float minimumLoadTime = 1f;

        static readonly string[] Tips =
        {
            "사막에는 숨겨진 오아시스가 존재합니다...",
            "요정과 대화하면 게임을 저장할 수 있습니다.",
            "황금 우상을 모두 모으면 특별한 일이 일어납니다.",
            "E키를 눌러 NPC와 대화할 수 있습니다.",
            "달빛 아래 늑대의 검이 가장 날카롭습니다.",
            "씨앗의 선택이 사막의 미래를 결정합니다.",
            "2회차에서는 새로운 진실이 밝혀집니다.",
            "각 챕터에서 새로운 이동 능력이 해금됩니다.",
            "인벤토리(I)에서 수집한 기록을 다시 읽을 수 있습니다."
        };

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (loadingPanel != null)
                loadingPanel.SetActive(false);
        }

        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadRoutine(sceneName));
        }

        IEnumerator LoadRoutine(string sceneName)
        {
            if (loadingPanel != null) loadingPanel.SetActive(true);
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            SetProgress(0f);
            SetRandomTip();

            yield return StartCoroutine(Fade(0f, 1f));

            float startTime = Time.unscaledTime;
            AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
            async.allowSceneActivation = false;

            while (async.progress < 0.9f)
            {
                SetProgress(async.progress / 0.9f);
                yield return null;
            }

            SetProgress(1f);

            float elapsed = Time.unscaledTime - startTime;
            if (elapsed < minimumLoadTime)
                yield return new WaitForSecondsRealtime(minimumLoadTime - elapsed);

            async.allowSceneActivation = true;
            while (!async.isDone) yield return null;

            yield return StartCoroutine(Fade(1f, 0f));

            if (loadingPanel != null) loadingPanel.SetActive(false);
        }

        void SetProgress(float t)
        {
            if (progressBar != null) progressBar.value = t;
            if (progressText != null) progressText.text = $"{Mathf.FloorToInt(t * 100)}%";
        }

        void SetRandomTip()
        {
            if (tipText != null)
                tipText.text = Tips[Random.Range(0, Tips.Length)];
        }

        IEnumerator Fade(float from, float to)
        {
            if (canvasGroup == null) yield break;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = to;
        }
    }
}
