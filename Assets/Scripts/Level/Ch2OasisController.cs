using UnityEngine;
using TheSSand.Core;
using TheSSand.Quest;
using TheSSand.Dialogue;
using TheSSand.Audio;
using TheSSand.UI;

namespace TheSSand.Level
{
    public class Ch2OasisController : MonoBehaviour
    {
        [Header("NPC 참조")]
        [SerializeField] GameObject jamJamCraftNPC;
        [SerializeField] GameObject[] childNPCs;
        [SerializeField] GameObject bossZoneTrigger;

        [Header("이벤트 오브젝트")]
        [SerializeField] GameObject scarfHintTrigger;
        [SerializeField] GameObject banditEventTrigger;

        bool _banditEventTriggered;

        void Start()
        {
            AudioManager.Instance?.PlayBGM("BGM_Ch2_Oasis");
            NarrationUI.Instance?.ShowNarration("이번에도 배척받으려나,\n그렇게 생각할 무렵 오아시스에서 한 사람이 나옵니다.");

            if (bossZoneTrigger != null)
                bossZoneTrigger.SetActive(false);

            QuestManager.Instance.OnQuestStateChanged += OnQuestChanged;
        }

        void OnDestroy()
        {
            if (QuestManager.Instance != null)
                QuestManager.Instance.OnQuestStateChanged -= OnQuestChanged;
        }

        void OnQuestChanged(string questId, QuestState state)
        {
            if (questId == "ch2_food" && state == QuestState.Completed)
                OnFoodQuestComplete();

            if (state == QuestState.Completed && IsSubQuest(questId))
                CheckSubQuestCompletion();
        }

        void OnFoodQuestComplete()
        {
            QuestManager.Instance.ActivateQuest("ch2_mudcookie");
            QuestManager.Instance.ActivateQuest("ch2_skill");

            if (GameManager.Instance.CurrentSave.scarfPickedUp)
                QuestManager.Instance.ActivateQuest("ch2_scarfPass");
        }

        void CheckSubQuestCompletion()
        {
            if (!_banditEventTriggered)
            {
                _banditEventTriggered = true;
                TriggerBanditInvasion();
            }
        }

        void TriggerBanditInvasion()
        {
            AudioManager.Instance?.StopBGM(0.5f);
            NarrationUI.Instance?.ShowNarration("갑자기 조용해집니다.", () =>
            {
                AudioManager.Instance?.PlaySFX("SFX_Footsteps_Run");
                QuestManager.Instance.EvaluateChildrenSurvival();
                StartCoroutine(PostBanditSequence());
            });
        }

        System.Collections.IEnumerator PostBanditSequence()
        {
            yield return new WaitForSeconds(2f);
            AudioManager.Instance?.PlayBGM("BGM_Ch2_Oasis");

            if (bossZoneTrigger != null)
                bossZoneTrigger.SetActive(true);
        }

        bool IsSubQuest(string id) =>
            id == "ch2_mudcookie" || id == "ch2_skill" || id == "ch2_scarfPass";

        public void OnLeavingOasis(int farewellChoice)
        {
            switch (farewellChoice)
            {
                case 0:
                    GameManager.Instance?.SetSaidIllComeBack();
                    break;
            }
        }
    }
}
