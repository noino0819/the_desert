using UnityEngine;
using TheSSand.Core;
using TheSSand.Scene;
using TheSSand.Dialogue;

namespace TheSSand.Boss
{
    public class Ch2BossController : MonoBehaviour
    {
        [SerializeField] BossEagle bossEagle;
        [SerializeField] DialogueData seedDialogue;
        [SerializeField] string nextSceneName = "SCN_Ch3_Desert";

        [Header("자동 스크롤 배경")]
        [SerializeField] Transform[] backgroundLayers;
        [SerializeField] float[] layerSpeedMultipliers;

        void Start()
        {
            if (bossEagle != null)
            {
                bossEagle.OnBattleEnded += OnBattleEnded;
                bossEagle.StartBattle();
            }

            SceneTransitionManager.Instance?.FadeIn();
        }

        void Update()
        {
            if (bossEagle == null || backgroundLayers == null) return;

            float scrollSpeed = bossEagle.GetScrollSpeed();
            for (int i = 0; i < backgroundLayers.Length; i++)
            {
                if (backgroundLayers[i] == null) continue;
                float mult = i < layerSpeedMultipliers.Length ? layerSpeedMultipliers[i] : 1f;
                backgroundLayers[i].position += Vector3.left * scrollSpeed * mult * Time.deltaTime;
            }
        }

        void OnDestroy()
        {
            if (bossEagle != null)
                bossEagle.OnBattleEnded -= OnBattleEnded;
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
                GameManager.Instance?.UnlockAbility(2);
            }
            else if (flag == "refuseSeed")
            {
                GameManager.Instance?.RefuseSeed();
                GameManager.Instance?.UnlockAbility(2);
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
