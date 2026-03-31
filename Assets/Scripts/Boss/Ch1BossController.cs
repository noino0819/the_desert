using UnityEngine;
using TheSSand.Audio;
using TheSSand.Core;
using TheSSand.Scene;
using TheSSand.Dialogue;

namespace TheSSand.Boss
{
    public class Ch1BossController : MonoBehaviour
    {
        [SerializeField] BossHippo bossHippo;

        [Header("씨앗 선택지")]
        [SerializeField] DialogueData seedDialogue;

        [Header("씬 전환")]
        [SerializeField] string nextSceneName = "SCN_Ch2_Desert";

        void Start()
        {
            if (bossHippo != null)
            {
                bossHippo.OnBattleEnded += OnBattleEnded;
                bossHippo.StartBattle();
            }

            AudioManager.Instance?.PlayBGM("BGM_Ch1_Boss");
            SceneTransitionManager.Instance?.FadeIn();
        }

        void OnDestroy()
        {
            if (bossHippo != null)
                bossHippo.OnBattleEnded -= OnBattleEnded;
        }

        void OnBattleEnded(bool playerWon)
        {
            if (!playerWon)
            {
                HandleDefeat();
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
                GameManager.Instance?.UnlockAbility(1);
            }
            else if (flag == "refuseSeed")
            {
                GameManager.Instance?.RefuseSeed();
                GameManager.Instance?.UnlockAbility(1);
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

        void HandleDefeat()
        {
            GameManager.Instance?.OnGameOver();
        }

        void ProceedToNextScene()
        {
            SceneTransitionManager.Instance?.LoadScene(nextSceneName);
        }
    }
}
