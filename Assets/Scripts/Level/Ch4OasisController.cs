using System.Collections;
using UnityEngine;
using TheSSand.Core;
using TheSSand.Quest;
using TheSSand.Dialogue;
using TheSSand.Audio;
using TheSSand.UI;

namespace TheSSand.Level
{
    public class Ch4OasisController : MonoBehaviour
    {
        [Header("NPC 참조")]
        [SerializeField] GameObject researcherNPC;
        [SerializeField] GameObject fruitTurtleNPC;

        [Header("오브젝트")]
        [SerializeField] GameObject intercomObject;
        [SerializeField] GameObject bossZoneTrigger;
        [SerializeField] GameObject labArea;
        [SerializeField] GameObject rooftopGarden;

        bool _discoveryTriggered;

        void Start()
        {
            AudioManager.Instance?.PlayBGM("BGM_Ch4_Oasis");

            NarrationUI.Instance?.ShowNarrationSequence(new[]
            {
                "와, 이거 참…",
                "아까 봤던 사막들과는 썩 다른 광경이네요.",
                "이 오아시스는… 마치 도시입니다."
            });

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
            if (state != QuestState.Completed) return;

            if (AllExperimentsComplete() && !_discoveryTriggered)
            {
                _discoveryTriggered = true;
                StartCoroutine(ResearcherDiscoverySequence());
            }
        }

        bool AllExperimentsComplete()
        {
            return QuestManager.Instance.AreAllCompleted("ch4_chem", "ch4_bio", "ch4_phys");
        }

        IEnumerator ResearcherDiscoverySequence()
        {
            AudioManager.Instance?.StopBGM(1f);

            yield return new WaitForSeconds(1f);

            NarrationUI.Instance?.ShowNarrationSequence(new[]
            {
                "연구자가 말을 잇지 않습니다.",
                "한참 동안."
            }, () =>
            {
                if (bossZoneTrigger != null)
                    bossZoneTrigger.SetActive(true);
            });
        }
    }
}
