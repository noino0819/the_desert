using System;
using UnityEngine;

namespace TheSSand.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public SaveData CurrentSave { get; private set; }
        public bool HasClearedOnce { get; set; }

        public event Action<string> OnFlagChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CurrentSave = new SaveData();
        }

        public void NewGame(string playerName, bool isNewGame2 = false)
        {
            CurrentSave = new SaveData
            {
                playerName = playerName,
                isNewGame2 = isNewGame2,
                currentSceneId = isNewGame2 ? "SCN_NG_Ch4_Oasis" : "SCN_Ch1_Desert",
                currentChapter = isNewGame2 ? 4 : 1
            };
        }

        public void LoadGame(SaveData data)
        {
            CurrentSave = data;
        }

        #region 씨앗 시스템

        public void PlantSeed()
        {
            CurrentSave.seedPlantedCount++;
            OnFlagChanged?.Invoke("seedPlantedCount");
        }

        public void RefuseSeed()
        {
            CurrentSave.seedRefuseCount++;
            OnFlagChanged?.Invoke("seedRefuseCount");

            if (CurrentSave.seedRefuseCount >= 10 && !CurrentSave.isNewGame2)
                ForcePlantSeed();
        }

        void ForcePlantSeed()
        {
            CurrentSave.seedPlantedCount++;
            OnFlagChanged?.Invoke("seedForcePlanted");
        }

        #endregion

        #region NPC 킬 시스템

        public void KillNPC(int chapter)
        {
            CurrentSave.npcKillCount++;
            switch (chapter)
            {
                case 1: CurrentSave.ch1Killed = true; break;
                case 2: CurrentSave.ch2Killed = true; break;
                case 3: CurrentSave.ch3Killed = true; break;
                case 4: CurrentSave.ch4Killed = true; break;
            }
            OnFlagChanged?.Invoke("npcKillCount");
        }

        #endregion

        #region 아이템 / 재화

        public void AddGoldenIdol()
        {
            CurrentSave.goldenIdolCount++;
            OnFlagChanged?.Invoke("goldenIdolCount");
        }

        public void AddGold(int amount)
        {
            CurrentSave.gold += amount;
            OnFlagChanged?.Invoke("gold");
        }

        public void DeductGold(int amount)
        {
            CurrentSave.gold = Mathf.Max(0, CurrentSave.gold - amount);
            OnFlagChanged?.Invoke("gold");
        }

        #endregion

        #region Ed.2 조건 플래그

        public void SetMudCookieDelivered()
        {
            CurrentSave.mudCookieDelivered = true;
            OnFlagChanged?.Invoke("mudCookieDelivered");
        }

        public void SetScarfGiven()
        {
            CurrentSave.scarfGiven = true;
            OnFlagChanged?.Invoke("scarfGiven");
        }

        public void SetChildrenAlive(bool alive)
        {
            CurrentSave.ch2ChildrenAlive = alive;
            OnFlagChanged?.Invoke("ch2ChildrenAlive");
        }

        public void SetSaidIllComeBack()
        {
            CurrentSave.saidIllComeBack = true;
            OnFlagChanged?.Invoke("saidIllComeBack");
        }

        #endregion

        #region 능력 해금

        public void UnlockAbility(int clearedChapter)
        {
            switch (clearedChapter)
            {
                case 1:
                    CurrentSave.hasDoubleJump = true;
                    OnFlagChanged?.Invoke("hasDoubleJump");
                    break;
                case 2:
                    CurrentSave.hasWallJump = true;
                    OnFlagChanged?.Invoke("hasWallJump");
                    break;
                case 3:
                    CurrentSave.hasDash = true;
                    OnFlagChanged?.Invoke("hasDash");
                    break;
            }
        }

        #endregion

        #region 2회차

        public void RecoverFakeSeed()
        {
            CurrentSave.ngSeedRecovered++;
            OnFlagChanged?.Invoke("ngSeedRecovered");
        }

        public void PlantTrueSeed()
        {
            CurrentSave.ngTrueSeedPlanted++;
            OnFlagChanged?.Invoke("ngTrueSeedPlanted");
        }

        public void SetDoubleAgent(bool value)
        {
            CurrentSave.isDoubleAgent = value;
            OnFlagChanged?.Invoke("isDoubleAgent");
        }

        #endregion

        #region 엔딩 판정

        /// <summary>
        /// 1회차 엔딩 판정. Ed.3 > Ed.4 > Ed.2 > Ed.1 우선순위.
        /// </summary>
        public int EvaluateEnding()
        {
            if (CurrentSave.npcKillCount >= 2)
                return 3;

            if (CurrentSave.goldenIdolCount >= 3 && CurrentSave.gold >= 1000)
                return 4;

            if (CurrentSave.mudCookieDelivered &&
                CurrentSave.scarfGiven &&
                CurrentSave.ch2ChildrenAlive &&
                CurrentSave.saidIllComeBack)
                return 2;

            return 1;
        }

        /// <summary>
        /// 2회차 Ed.5 (진엔딩) 판정.
        /// </summary>
        public bool EvaluateNGEnding()
        {
            return CurrentSave.isNewGame2 &&
                   CurrentSave.ngSeedRecovered >= 4 &&
                   CurrentSave.ngTrueSeedPlanted >= 4;
        }

        #endregion

        #region 게임 오버

        public void OnGameOver()
        {
            CurrentSave.gold = Mathf.Max(0, CurrentSave.gold - CurrentSave.gold / 10);
        }

        #endregion
    }
}
