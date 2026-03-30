using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TheSSand.Core;
using TheSSand.Scene;

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

        [Header("연출")]
        [SerializeField] Animator bookAnimator;

        void Start()
        {
            if (loadPanel != null)
                loadPanel.SetActive(false);

            SetupButtons();
            UpdateNewGame2Button();
            UpdateLoadSlots();
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
            Debug.Log("[TitleMenu] 설정 열기 — 미구현");
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
