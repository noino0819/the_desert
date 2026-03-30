using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TheSSand.Scene
{
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        [SerializeField] Image fadeImage;
        [SerializeField] float fadeDuration = 0.5f;
        [SerializeField] Color fadeColor = Color.black;

        Canvas _fadeCanvas;
        bool _isTransitioning;

        public event Action<string> OnSceneLoadStarted;
        public event Action<string> OnSceneLoadCompleted;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitFadeCanvas();
        }

        void InitFadeCanvas()
        {
            if (fadeImage != null) return;

            var canvasObj = new GameObject("FadeCanvas");
            canvasObj.transform.SetParent(transform);

            _fadeCanvas = canvasObj.AddComponent<Canvas>();
            _fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _fadeCanvas.sortingOrder = 9999;

            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            var imgObj = new GameObject("FadeImage");
            imgObj.transform.SetParent(canvasObj.transform, false);

            fadeImage = imgObj.AddComponent<Image>();
            fadeImage.color = fadeColor;
            fadeImage.raycastTarget = false;

            var rect = fadeImage.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            SetFadeAlpha(0f);
        }

        #region 공개 API

        public void LoadScene(string sceneName)
        {
            if (_isTransitioning) return;
            StartCoroutine(TransitionCoroutine(sceneName));
        }

        public void LoadScene(int sceneIndex)
        {
            if (_isTransitioning) return;
            string sceneName = SceneManager.GetSceneByBuildIndex(sceneIndex).name;
            StartCoroutine(TransitionCoroutine(sceneName));
        }

        public void ReloadCurrentScene()
        {
            if (_isTransitioning) return;
            StartCoroutine(TransitionCoroutine(SceneManager.GetActiveScene().name));
        }

        /// <summary>
        /// 즉시 페이드 인 (씬 시작 시 호출용).
        /// </summary>
        public void FadeIn()
        {
            StartCoroutine(FadeCoroutine(1f, 0f));
        }

        #endregion

        #region 코루틴

        IEnumerator TransitionCoroutine(string sceneName)
        {
            _isTransitioning = true;
            OnSceneLoadStarted?.Invoke(sceneName);

            yield return StartCoroutine(FadeCoroutine(0f, 1f));

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;

            while (asyncLoad.progress < 0.9f)
                yield return null;

            asyncLoad.allowSceneActivation = true;

            while (!asyncLoad.isDone)
                yield return null;

            yield return StartCoroutine(FadeCoroutine(1f, 0f));

            _isTransitioning = false;
            OnSceneLoadCompleted?.Invoke(sceneName);
        }

        /// <summary>
        /// CrossFade 코루틴. fromAlpha → toAlpha로 fadeDuration 동안 전환.
        /// </summary>
        IEnumerator FadeCoroutine(float fromAlpha, float toAlpha)
        {
            float elapsed = 0f;
            SetFadeAlpha(fromAlpha);

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                SetFadeAlpha(Mathf.Lerp(fromAlpha, toAlpha, t));
                yield return null;
            }

            SetFadeAlpha(toAlpha);
        }

        #endregion

        void SetFadeAlpha(float alpha)
        {
            if (fadeImage == null) return;
            var c = fadeImage.color;
            c.a = alpha;
            fadeImage.color = c;
        }
    }
}
