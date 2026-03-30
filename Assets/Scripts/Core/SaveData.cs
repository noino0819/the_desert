using System;
using System.Collections.Generic;

namespace TheSSand.Core
{
    [Serializable]
    public class SaveData
    {
        public int saveSlotIndex;
        public string playerName;
        public string currentSceneId;
        public float playerPosX;
        public float playerPosY;
        public int currentChapter;
        public float playTime;

        // 엔딩 판정 플래그
        public int seedPlantedCount;    // 씨앗 심은 횟수 (0~4)
        public int seedRefuseCount;     // 씨앗 거절 누적 횟수
        public int npcKillCount;        // 강압 선택지 선택 횟수 (0~4)
        public int goldenIdolCount;     // 황금우상 획득 수 (0~4)
        public int gold;                // 소지금

        // 챕터별 킬 플래그
        public bool ch1Killed;
        public bool ch2Killed;
        public bool ch3Killed;
        public bool ch4Killed;

        // Ed.2 조건 플래그
        public bool mudCookieDelivered; // Ch.1 진흙쿠키 배달 완료
        public bool scarfGiven;         // Ch.1 스카프 전달 완료
        public bool ch2ChildrenAlive;   // Ch.2 아이들 생존
        public bool saidIllComeBack;    // Ch.2 "꼭 돌아올게" 선택

        // 2회차 플래그
        public bool isNewGame2;
        public int ngSeedRecovered;     // 가짜 씨앗 회수 수 (0~4)
        public int ngTrueSeedPlanted;   // 진짜 씨앗 설치 수 (0~4)

        // 능력 해금 플래그
        public bool hasDoubleJump;      // Ch.1 클리어 후 해금
        public bool hasWallJump;        // Ch.2 클리어 후 해금
        public bool hasDash;            // Ch.3 클리어 후 해금

        // Ch.3 이중스파이
        public bool isDoubleAgent;

        // 퀘스트 상태
        public List<QuestSaveEntry> questStates = new List<QuestSaveEntry>();

        // 인벤토리
        public List<string> inventoryItems = new List<string>();

        public SaveData()
        {
            saveSlotIndex = 0;
            playerName = "";
            currentSceneId = "SCN_Prologue";
            currentChapter = 0;
        }
    }

    [Serializable]
    public class QuestSaveEntry
    {
        public string questId;
        public int state;
        public int progress;

        public QuestSaveEntry() { }

        public QuestSaveEntry(string questId, int state, int progress)
        {
            this.questId = questId;
            this.state = state;
            this.progress = progress;
        }
    }
}
