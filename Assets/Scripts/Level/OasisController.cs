using UnityEngine;
using TheSSand.Audio;
using TheSSand.Core;
using TheSSand.Quest;
using TheSSand.Scene;

namespace TheSSand.Level
{
    public class OasisController : MonoBehaviour
    {
        [Header("챕터 설정")]
        [SerializeField] int chapter = 1;
        [SerializeField] string bossSceneName = "SCN_Ch1_Boss";

        [Header("보스전 트리거")]
        [SerializeField] GameObject bossZoneTrigger;

        [Header("2회차 오버레이")]
        [SerializeField] GameObject ngOverlayObjects;
        [SerializeField] GameObject normalObjects;

        void Start()
        {
            bool isNG = GameManager.Instance != null &&
                        GameManager.Instance.CurrentSave.isNewGame2;

            if (ngOverlayObjects != null)
                ngOverlayObjects.SetActive(isNG);
            if (normalObjects != null)
                normalObjects.SetActive(!isNG);

            if (bossZoneTrigger != null)
                bossZoneTrigger.SetActive(false);

            if (QuestManager.Instance != null)
                QuestManager.Instance.OnQuestStateChanged += OnQuestChanged;

            AudioManager.Instance?.PlayBGM($"BGM_Ch{chapter}_Oasis");
            SceneTransitionManager.Instance?.FadeIn();
        }

        void OnDestroy()
        {
            if (QuestManager.Instance != null)
                QuestManager.Instance.OnQuestStateChanged -= OnQuestChanged;
        }

        void OnQuestChanged(string questId, QuestState state)
        {
            if (chapter == 1 && questId == "ch1_water" && state == QuestState.Completed)
                UnlockBossZone();
        }

        void UnlockBossZone()
        {
            if (bossZoneTrigger != null)
                bossZoneTrigger.SetActive(true);

            Debug.Log($"[Oasis] Ch.{chapter} 보스전 해금");
        }

        public void EnterBoss()
        {
            SceneTransitionManager.Instance?.LoadScene(bossSceneName);
        }
    }
}
