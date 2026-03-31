using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TheSSand.Core;
using TheSSand.Scene;
using TheSSand.Audio;

namespace TheSSand.UI
{
    public class TitleMenuController : MonoBehaviour
    {
        [Header("메뉴 버튼")]
        [SerializeField] Button newGameButton;
        [SerializeField] Button loadGameButton;
        [SerializeField] Button newGame2Button;
        [SerializeField] Button settingsButton;
        [SerializeField] Button quitButton;

        [Header("세이브 슬롯 패널")]
        [SerializeField] GameObject loadPanel;
        [SerializeField] Button[] slotButtons;
        [SerializeField] TextMeshProUGUI[] slotInfoTexts;
        [SerializeField] Button loadBackButton;

        [Header("설정 패널")]
        [SerializeField] GameObject settingsPanel;
        [SerializeField] Slider bgmSlider;
        [SerializeField] Slider sfxSlider;
        [SerializeField] TextMeshProUGUI bgmValueText;
        [SerializeField] TextMeshProUGUI sfxValueText;
        [SerializeField] Toggle fullscreenToggle;
        [SerializeField] TMP_Dropdown resolutionDropdown;
        [SerializeField] Button settingsBackButton;

        [Header("연출")]
        [SerializeField] Animator bookAnimator;

        Resolution[] _resolutions;

        void Start()
        {
            if (loadPanel != null)
                loadPanel.SetActive(false);
            if (settingsPanel != null)
                settingsPanel.SetActive(false);

            SetupButtons();
            SetupSettingsPanel();
            UpdateNewGame2Button();
            UpdateLoadSlots();

            AudioManager.Instance?.PlayBGM("BGM_Title");
        }

        void SetupButtons()
        {
            newGameButton?.onClick.AddListener(OnNewGame);
            loadGameButton?.onClick.AddListener(OnLoadGame);
            newGame2Button?.onClick.AddListener(OnNewGame2);
            settingsButton?.onClick.AddListener(OnSettings);
            quitButton?.onClick.AddListener(OnQuit);
            loadBackButton?.onClick.AddListener(() => loadPanel?.SetActive(false));

            for (int i = 0; i < slotButtons.Length; i++)
            {
                int slot = i;
                slotButtons[i]?.onClick.AddListener(() => LoadSlot(slot));
            }
        }

        void UpdateNewGame2Button()
        {
            if (newGame2Button == null) return;

            bool unlocked = false;
            if (SaveManager.Instance != null)
            {
                var global = SaveManager.Instance.LoadGlobalData();
                unlocked = global.hasClearedOnce;
            }
            newGame2Button.gameObject.SetActive(unlocked);
        }

        void UpdateLoadSlots()
        {
            if (SaveManager.Instance == null) return;

            for (int i = 0; i < slotButtons.Length && i < slotInfoTexts.Length; i++)
            {
                var data = SaveManager.Instance.PeekSlot(i);
                if (data != null)
                {
                    int hours = (int)(data.playTime / 3600);
                    int minutes = (int)(data.playTime % 3600 / 60);
                    slotInfoTexts[i].text = $"Ch.{data.currentChapter} | {data.currentSceneId}\n" +
                                            $"{data.playerName} | {hours:00}:{minutes:00}";
                    slotButtons[i].interactable = true;
                }
                else
                {
                    slotInfoTexts[i].text = "비어있음";
                    slotButtons[i].interactable = false;
                }
            }
        }

        void OnNewGame()
        {
            if (bookAnimator != null)
                bookAnimator.SetTrigger("Open");

            SceneTransitionManager.Instance?.LoadScene("SCN_Prologue");
        }

        void OnLoadGame()
        {
            loadPanel?.SetActive(true);
            UpdateLoadSlots();
        }

        void OnNewGame2()
        {
            GameManager.Instance?.NewGame("", true);
            SceneTransitionManager.Instance?.LoadScene("SCN_Prologue");
        }

        void LoadSlot(int slotIndex)
        {
            var data = SaveManager.Instance?.Load(slotIndex);
            if (data != null)
            {
                loadPanel?.SetActive(false);
                SceneTransitionManager.Instance?.LoadScene(data.currentSceneId);
            }
        }

        void OnSettings()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(true);
        }

        void SetupSettingsPanel()
        {
            settingsBackButton?.onClick.AddListener(() =>
            {
                settingsPanel?.SetActive(false);
                SaveTitleSettings();
            });

            float savedBGM = PlayerPrefs.GetFloat("BGMVolume", 0.8f);
            float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 1f);

            if (bgmSlider != null)
            {
                bgmSlider.value = savedBGM;
                bgmSlider.onValueChanged.AddListener(v =>
                {
                    AudioManager.Instance?.SetBGMVolume(v);
                    if (bgmValueText != null) bgmValueText.text = $"{Mathf.RoundToInt(v * 100)}%";
                });
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = savedSFX;
                sfxSlider.onValueChanged.AddListener(v =>
                {
                    AudioManager.Instance?.SetSFXVolume(v);
                    if (sfxValueText != null) sfxValueText.text = $"{Mathf.RoundToInt(v * 100)}%";
                });
            }

            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = Screen.fullScreen;
                fullscreenToggle.onValueChanged.AddListener(v => Screen.fullScreen = v);
            }

            if (resolutionDropdown != null)
            {
                _resolutions = Screen.resolutions;
                resolutionDropdown.ClearOptions();
                var options = new System.Collections.Generic.List<string>();
                int current = 0;
                for (int i = 0; i < _resolutions.Length; i++)
                {
                    var r = _resolutions[i];
                    options.Add($"{r.width} x {r.height}");
                    if (r.width == Screen.currentResolution.width &&
                        r.height == Screen.currentResolution.height)
                        current = i;
                }
                resolutionDropdown.AddOptions(options);
                resolutionDropdown.value = current;
                resolutionDropdown.RefreshShownValue();
                resolutionDropdown.onValueChanged.AddListener(idx =>
                {
                    if (_resolutions != null && idx < _resolutions.Length)
                        Screen.SetResolution(_resolutions[idx].width, _resolutions[idx].height, Screen.fullScreen);
                });
            }

            AudioManager.Instance?.SetBGMVolume(savedBGM);
            AudioManager.Instance?.SetSFXVolume(savedSFX);
        }

        void SaveTitleSettings()
        {
            if (bgmSlider != null) PlayerPrefs.SetFloat("BGMVolume", bgmSlider.value);
            if (sfxSlider != null) PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
            PlayerPrefs.Save();
        }

        void OnQuit()
        {
            if (bookAnimator != null)
                bookAnimator.SetTrigger("Close");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
