using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TheSSand.Scene;
using TheSSand.Audio;

namespace TheSSand.UI
{
    /// <summary>
    /// ESC 키로 토글하는 일시정지 메뉴.
    /// 재개 / 설정 / 타이틀로 돌아가기
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        public static PauseMenu Instance { get; private set; }

        [Header("패널")]
        [SerializeField] GameObject pausePanel;
        [SerializeField] GameObject settingsPanel;

        [Header("버튼")]
        [SerializeField] Button resumeButton;
        [SerializeField] Button settingsButton;
        [SerializeField] Button titleButton;
        [SerializeField] Button settingsBackButton;

        [Header("설정 — 오디오")]
        [SerializeField] Slider bgmSlider;
        [SerializeField] Slider sfxSlider;
        [SerializeField] TextMeshProUGUI bgmValueText;
        [SerializeField] TextMeshProUGUI sfxValueText;

        [Header("설정 — 화면")]
        [SerializeField] Toggle fullscreenToggle;
        [SerializeField] TMP_Dropdown resolutionDropdown;

        bool _isPaused;
        Resolution[] _resolutions;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);

            resumeButton?.onClick.AddListener(Resume);
            settingsButton?.onClick.AddListener(OpenSettings);
            titleButton?.onClick.AddListener(ReturnToTitle);
            settingsBackButton?.onClick.AddListener(CloseSettings);

            InitAudioSliders();
            InitResolutionDropdown();
            InitFullscreenToggle();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (settingsPanel != null && settingsPanel.activeSelf)
                    CloseSettings();
                else if (_isPaused)
                    Resume();
                else
                    Pause();
            }
        }

        public void Pause()
        {
            if (Dialogue.DialogueManager.Instance != null &&
                Dialogue.DialogueManager.Instance.IsDialogueActive)
                return;

            _isPaused = true;
            Time.timeScale = 0f;
            if (pausePanel != null) pausePanel.SetActive(true);
            AudioManager.Instance?.PauseBGM();
        }

        public void Resume()
        {
            _isPaused = false;
            Time.timeScale = 1f;
            if (pausePanel != null) pausePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            AudioManager.Instance?.ResumeBGM();
        }

        void OpenSettings()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        void CloseSettings()
        {
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(true);
            SaveSettings();
        }

        void ReturnToTitle()
        {
            Resume();
            SceneTransitionManager.Instance?.LoadScene("SCN_TitleMenu");
        }

        #region 오디오 설정

        void InitAudioSliders()
        {
            float savedBGM = PlayerPrefs.GetFloat("BGMVolume", 0.8f);
            float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 1f);

            if (bgmSlider != null)
            {
                bgmSlider.value = savedBGM;
                bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = savedSFX;
                sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }

            OnBGMVolumeChanged(savedBGM);
            OnSFXVolumeChanged(savedSFX);
        }

        void OnBGMVolumeChanged(float value)
        {
            AudioManager.Instance?.SetBGMVolume(value);
            if (bgmValueText != null)
                bgmValueText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }

        void OnSFXVolumeChanged(float value)
        {
            AudioManager.Instance?.SetSFXVolume(value);
            if (sfxValueText != null)
                sfxValueText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }

        #endregion

        #region 화면 설정

        void InitResolutionDropdown()
        {
            if (resolutionDropdown == null) return;

            _resolutions = Screen.resolutions;
            resolutionDropdown.ClearOptions();

            var options = new System.Collections.Generic.List<string>();
            int currentIndex = 0;

            for (int i = 0; i < _resolutions.Length; i++)
            {
                var r = _resolutions[i];
                options.Add($"{r.width} x {r.height}");

                if (r.width == Screen.currentResolution.width &&
                    r.height == Screen.currentResolution.height)
                    currentIndex = i;
            }

            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentIndex;
            resolutionDropdown.RefreshShownValue();
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        }

        void OnResolutionChanged(int index)
        {
            if (_resolutions == null || index >= _resolutions.Length) return;
            var r = _resolutions[index];
            Screen.SetResolution(r.width, r.height, Screen.fullScreen);
        }

        void InitFullscreenToggle()
        {
            if (fullscreenToggle == null) return;
            fullscreenToggle.isOn = Screen.fullScreen;
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);
        }

        void OnFullscreenToggled(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
        }

        #endregion

        void SaveSettings()
        {
            if (bgmSlider != null) PlayerPrefs.SetFloat("BGMVolume", bgmSlider.value);
            if (sfxSlider != null) PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
            PlayerPrefs.Save();
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && !_isPaused)
                Pause();
        }
    }
}
