using UnityEngine;
using TheSSand.Core;
using TheSSand.Scene;
using TheSSand.Dialogue;
using TheSSand.Card;

namespace TheSSand.Boss
{
    public class Ch4BossController : MonoBehaviour
    {
        [SerializeField] BossTurtle bossTurtle;
        [SerializeField] CardBattleManager cardBattleManager;
        [SerializeField] DialogueData seedDialogue;
        [SerializeField] string nextSceneName = "SCN_Ending";

        void Start()
        {
            if (bossTurtle != null)
            {
                bossTurtle.OnBattleEnded += OnBattleEnded;
                bossTurtle.StartBattle();
            }

            SceneTransitionManager.Instance?.FadeIn();
        }

        void OnDestroy()
        {
            if (bossTurtle != null)
                bossTurtle.OnBattleEnded -= OnBattleEnded;
        }

        void OnBattleEnded(bool playerWon)
        {
            if (!playerWon)
            {
                GameManager.Instance?.OnGameOver();
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
            }
            else if (flag == "refuseSeed")
            {
                GameManager.Instance?.RefuseSeed();
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

        void ProceedToNextScene()
        {
            SceneTransitionManager.Instance?.LoadScene(nextSceneName);
        }
    }
}
