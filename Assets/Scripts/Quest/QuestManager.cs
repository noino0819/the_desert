using System;
using System.Collections.Generic;
using UnityEngine;
using TheSSand.Core;

namespace TheSSand.Quest
{
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        [SerializeField] TextAsset questDatabaseJson;

        readonly Dictionary<string, QuestData> _quests = new Dictionary<string, QuestData>();

        public event Action<string, QuestState> OnQuestStateChanged;
        public event Action<string, int> OnQuestProgressChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            LoadQuestDatabase();
        }

        #region DB 로드

        void LoadQuestDatabase()
        {
            if (questDatabaseJson == null)
            {
                LoadDefaultQuests();
                return;
            }

            var wrapper = JsonUtility.FromJson<QuestDatabaseWrapper>(questDatabaseJson.text);
            if (wrapper?.quests == null) return;

            foreach (var q in wrapper.quests)
            {
                q.state = QuestState.Inactive;
                q.progress = 0;
                _quests[q.questId] = q;
            }
        }

        void LoadDefaultQuests()
        {
            RegisterQuest(new QuestData
            {
                questId = "ch1_water", questName = "물 문제 해결",
                goal = 1, flagsOnComplete = new string[0]
            });
            RegisterQuest(new QuestData
            {
                questId = "ch1_mudcookie", questName = "진흙쿠키 배달",
                goal = 3, flagsOnComplete = new[] { "mudCookieDelivered" }
            });
            RegisterQuest(new QuestData
            {
                questId = "ch1_scarf", questName = "스카프 전달",
                goal = 1, flagsOnComplete = new[] { "scarfGiven" }
            });
            RegisterQuest(new QuestData
            {
                questId = "ch2_food", questName = "식량 조달",
                goal = 1, flagsOnComplete = new string[0]
            });
            RegisterQuest(new QuestData
            {
                questId = "ch2_survive", questName = "생존 기술 전수",
                goal = 1, flagsOnComplete = new string[0]
            });
            RegisterQuest(new QuestData
            {
                questId = "ch2_skill", questName = "아이들 도움",
                goal = 1, flagsOnComplete = new string[0]
            });
            RegisterQuest(new QuestData
            {
                questId = "ch2_scarfPass", questName = "스카프 전달 (Ch.2)",
                goal = 1, flagsOnComplete = new string[0]
            });
            RegisterQuest(new QuestData
            {
                questId = "ch3_doubleAgent", questName = "이중스파이",
                goal = 2, flagsOnComplete = new string[0]
            });
            RegisterQuest(new QuestData
            {
                questId = "ch4_chem", questName = "화학 실험",
                goal = 1, flagsOnComplete = new string[0]
            });
            RegisterQuest(new QuestData
            {
                questId = "ch4_bio", questName = "생물 실험",
                goal = 1, flagsOnComplete = new string[0]
            });
            RegisterQuest(new QuestData
            {
                questId = "ch4_phys", questName = "물리 실험",
                goal = 1, flagsOnComplete = new string[0]
            });
        }

        public void RegisterQuest(QuestData quest)
        {
            _quests[quest.questId] = quest;
        }

        #endregion

        #region 퀘스트 상태 관리

        public QuestState GetState(string questId)
        {
            return _quests.TryGetValue(questId, out var q) ? q.state : QuestState.Inactive;
        }

        public QuestData GetQuest(string questId)
        {
            return _quests.TryGetValue(questId, out var q) ? q : null;
        }

        public void ActivateQuest(string questId)
        {
            if (!_quests.TryGetValue(questId, out var q)) return;
            if (q.state != QuestState.Inactive) return;

            q.state = QuestState.Active;
            OnQuestStateChanged?.Invoke(questId, QuestState.Active);
        }

        public void AdvanceProgress(string questId, int amount = 1)
        {
            if (!_quests.TryGetValue(questId, out var q)) return;
            if (q.state != QuestState.Active) return;

            q.Advance(amount);
            OnQuestProgressChanged?.Invoke(questId, q.progress);

            if (q.IsGoalReached)
                CompleteQuest(questId);
        }

        public void CompleteQuest(string questId)
        {
            if (!_quests.TryGetValue(questId, out var q)) return;
            if (q.state == QuestState.Completed) return;

            q.state = QuestState.Completed;
            ApplyCompletionFlags(q);
            OnQuestStateChanged?.Invoke(questId, QuestState.Completed);
        }

        public void FailQuest(string questId)
        {
            if (!_quests.TryGetValue(questId, out var q)) return;

            q.state = QuestState.Failed;
            OnQuestStateChanged?.Invoke(questId, QuestState.Failed);
        }

        #endregion

        #region 플래그 자동 세트

        void ApplyCompletionFlags(QuestData quest)
        {
            if (quest.flagsOnComplete == null || GameManager.Instance == null) return;

            foreach (string flag in quest.flagsOnComplete)
            {
                switch (flag)
                {
                    case "mudCookieDelivered":
                        GameManager.Instance.SetMudCookieDelivered();
                        break;
                    case "scarfGiven":
                        GameManager.Instance.SetScarfGiven();
                        break;
                    case "saidIllComeBack":
                        GameManager.Instance.SetSaidIllComeBack();
                        break;
                    case "isDoubleAgent":
                        GameManager.Instance.SetDoubleAgent(true);
                        break;
                }
            }
        }

        #endregion

        #region 세이브 / 로드 연동

        public List<QuestSaveEntry> GetSaveEntries()
        {
            var entries = new List<QuestSaveEntry>();
            foreach (var kv in _quests)
            {
                entries.Add(new QuestSaveEntry(
                    kv.Key, (int)kv.Value.state, kv.Value.progress));
            }
            return entries;
        }

        public void RestoreFromSave(List<QuestSaveEntry> entries)
        {
            if (entries == null) return;
            foreach (var entry in entries)
            {
                if (_quests.TryGetValue(entry.questId, out var q))
                {
                    q.state = (QuestState)entry.state;
                    q.progress = entry.progress;
                }
            }
        }

        #endregion

        #region 유틸

        public bool AreAllCompleted(params string[] questIds)
        {
            foreach (var id in questIds)
            {
                if (GetState(id) != QuestState.Completed) return false;
            }
            return true;
        }

        /// <summary>
        /// Ch.2 아이들 생존 판정: 3개 서브퀘스트 모두 완료 시 true.
        /// </summary>
        public void EvaluateChildrenSurvival()
        {
            bool allDone = AreAllCompleted("ch2_survive", "ch2_skill", "ch2_scarfPass");
            GameManager.Instance?.SetChildrenAlive(allDone);
        }

        #endregion
    }

    [Serializable]
    public class QuestDatabaseWrapper
    {
        public QuestData[] quests;
    }
}
