using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TheSSand.Core;
using TheSSand.Scene;
using TheSSand.Player;

namespace TheSSand.UI
{
    /// <summary>
    /// 게임 오버 화면 — 재시도 / 마지막 저장 지점 / 타이틀
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [Header("패널")]
        [SerializeField] GameObject gameOverPanel;
        [SerializeField] CanvasGroup canvasGroup;

        [Header("텍스트")]
        [SerializeField] TextMeshProUGUI titleText;
        [SerializeField] TextMeshProUGUI subtitleText;

        [Header("버튼")]
        [SerializeField] Button retryButton;
        [SerializeField] Button loadButton;
        [SerializeField] Button titleButton;

        [Header("연출")]
        [SerializeField] float fadeInDuration = 1f;

        float _fadeTimer;
        bool _isFading;

        void Start()
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            retryButton?.onClick.AddListener(OnRetry);
            loadButton?.onClick.AddListener(OnLoadSave);
            titleButton?.onClick.AddListener(OnReturnToTitle);

            var player = FindAnyObjectByType<PlayerController>();
            if (player != null)
                player.OnPlayerDied += ShowGameOver;
        }

        void Update()
        {
            if (!_isFading || canvasGroup == null) return;

            _fadeTimer += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(_fadeTimer / fadeInDuration);

            if (_fadeTimer >= fadeInDuration)
            {
                _isFading = false;
                EnableButtons();
            }
        }

        public void ShowGameOver()
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);

            if (titleText != null)
                titleText.text = "여행이 끝나다...";
            if (subtitleText != null)
                subtitleText.text = "골드의 10%를 잃었습니다.";

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                _fadeTimer = 0f;
                _isFading = true;
            }

            DisableButtons();
            Time.timeScale = 0f;
        }

        void DisableButtons()
        {
            if (retryButton != null) retryButton.interactable = false;
            if (loadButton != null) loadButton.interactable = false;
            if (titleButton != null) titleButton.interactable = false;
        }

        void EnableButtons()
        {
            if (retryButton != null) retryButton.interactable = true;
            if (loadButton != null) loadButton.interactable = true;
            if (titleButton != null) titleButton.interactable = true;
        }

        void OnRetry()
        {
            Time.timeScale = 1f;

            if (Level.Checkpoint.LastCheckpointPosition != null)
            {
                Level.Checkpoint.RespawnPlayer();
                if (gameOverPanel != null) gameOverPanel.SetActive(false);
                return;
            }

            SceneTransitionManager.Instance?.ReloadCurrentScene();
        }

        void OnLoadSave()
        {
            Time.timeScale = 1f;
            var data = SaveManager.Instance?.LoadLastUsedSlot();
            if (data != null)
                SceneTransitionManager.Instance?.LoadScene(data.currentSceneId);
            else
                SceneTransitionManager.Instance?.ReloadCurrentScene();
        }

        void OnReturnToTitle()
        {
            Time.timeScale = 1f;
            SceneTransitionManager.Instance?.LoadScene("SCN_TitleMenu");
        }

        void OnDestroy()
        {
            var player = FindAnyObjectByType<PlayerController>();
            if (player != null)
                player.OnPlayerDied -= ShowGameOver;
        }
    }
}
