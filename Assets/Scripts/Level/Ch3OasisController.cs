using UnityEngine;
using TheSSand.Core;
using TheSSand.Quest;
using TheSSand.Dialogue;
using TheSSand.Audio;
using TheSSand.UI;

namespace TheSSand.Level
{
    public class Ch3OasisController : MonoBehaviour
    {
        [Header("NPC 참조")]
        [SerializeField] GameObject solNPC;
        [SerializeField] GameObject lunaNPC;
        [SerializeField] GameObject[] sentryNPCs;

        [Header("맵 오브젝트")]
        [SerializeField] GameObject wallGap;
        [SerializeField] Collider2D wallGapCollider;
        [SerializeField] GameObject bossZoneTrigger;

        [Header("분열 선택")]
        [SerializeField] bool enteredFromA;

        void Start()
        {
            AudioManager.Instance?.PlayBGM("BGM_Ch3_Oasis");

            NarrationUI.Instance?.ShowNarrationSequence(new[]
            {
                "와, 이번에는 정말 큰 오아시스네요.",
                "이 정도 오아시스가 존재할 줄은 상상도 못했는걸요.",
                "그런데…",
                "누가 억지로 하나를 둘로 갈라놓은 것 같네요."
            });

            if (wallGapCollider != null)
                wallGapCollider.enabled = true;

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
            if (questId == "ch3_doubleAgent" && state == QuestState.Active)
                OpenWallGap();

            if (questId == "ch3_doubleAgent" && state == QuestState.Completed)
                EnableBossZone();
        }

        void OpenWallGap()
        {
            GameManager.Instance?.SetDoubleAgent(true);
            if (wallGapCollider != null)
                wallGapCollider.enabled = false;
        }

        void EnableBossZone()
        {
            if (bossZoneTrigger != null)
                bossZoneTrigger.SetActive(true);
        }

        public void OnEntranceChoice(int choice)
        {
            enteredFromA = choice == 0;
        }
    }
}
