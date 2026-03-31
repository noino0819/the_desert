using System;
using System.IO;
using UnityEngine;

namespace TheSSand.Core
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        const int MaxSlots = 3;
        const string SaveFilePrefix = "save_slot_";
        const string GlobalFileName = "global.json";

        string SaveDirectory => Path.Combine(Application.persistentDataPath, "SaveData");

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (!Directory.Exists(SaveDirectory))
                Directory.CreateDirectory(SaveDirectory);
        }

        string GetSlotPath(int slot) =>
            Path.Combine(SaveDirectory, $"{SaveFilePrefix}{slot}.json");

        string GlobalPath => Path.Combine(SaveDirectory, GlobalFileName);

        #region 슬롯 저장 / 로드

        public void Save(int slotIndex)
        {
            var save = GameManager.Instance.CurrentSave;
            save.saveSlotIndex = slotIndex;
            save.playTime += Time.timeSinceLevelLoad;
            save.currentSceneId = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            if (Quest.QuestManager.Instance != null)
                save.questStates = Quest.QuestManager.Instance.GetSaveEntries();

            if (UI.InventoryManager.Instance != null)
                UI.InventoryManager.Instance.SyncToSave();

            var player = UnityEngine.Object.FindAnyObjectByType<Player.PlayerController>();
            if (player != null)
            {
                save.playerPosX = player.transform.position.x;
                save.playerPosY = player.transform.position.y;
            }

            string json = JsonUtility.ToJson(save, true);
            File.WriteAllText(GetSlotPath(slotIndex), json);
            Debug.Log($"[SaveManager] 슬롯 {slotIndex} 저장 완료");
        }

        public SaveData Load(int slotIndex)
        {
            string path = GetSlotPath(slotIndex);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[SaveManager] 슬롯 {slotIndex} 세이브 없음");
                return null;
            }

            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            GameManager.Instance.LoadGame(data);

            if (Quest.QuestManager.Instance != null)
                Quest.QuestManager.Instance.RestoreFromSave(data.questStates);

            if (UI.InventoryManager.Instance != null)
                UI.InventoryManager.Instance.RestoreFromSave();

            Debug.Log($"[SaveManager] 슬롯 {slotIndex} 로드 완료");
            return data;
        }

        public bool HasSave(int slotIndex) => File.Exists(GetSlotPath(slotIndex));

        public SaveData LoadLastUsedSlot()
        {
            if (GameManager.Instance != null)
            {
                int lastSlot = GameManager.Instance.CurrentSave.saveSlotIndex;
                if (HasSave(lastSlot))
                    return Load(lastSlot);
            }

            for (int i = 0; i < MaxSlots; i++)
            {
                if (HasSave(i)) return Load(i);
            }
            return null;
        }

        /// <summary>
        /// 메뉴 슬롯 미리보기용. GameManager 상태를 변경하지 않음.
        /// </summary>
        public SaveData PeekSlot(int slotIndex)
        {
            string path = GetSlotPath(slotIndex);
            if (!File.Exists(path)) return null;
            return JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
        }

        #endregion

        #region 글로벌 데이터 (1회차 클리어 여부)

        public void SaveGlobalData()
        {
            var global = new GlobalSaveData
            {
                hasClearedOnce = GameManager.Instance.HasClearedOnce
            };
            File.WriteAllText(GlobalPath, JsonUtility.ToJson(global, true));
        }

        public GlobalSaveData LoadGlobalData()
        {
            if (!File.Exists(GlobalPath)) return new GlobalSaveData();
            return JsonUtility.FromJson<GlobalSaveData>(File.ReadAllText(GlobalPath));
        }

        #endregion

        #region 삭제

        public void DeleteSlot(int slotIndex)
        {
            string path = GetSlotPath(slotIndex);
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"[SaveManager] 슬롯 {slotIndex} 삭제");
            }
        }

        /// <summary>
        /// Ed.5 "책 불태우기" — 모든 세이브 + 글로벌 데이터 삭제.
        /// </summary>
        public void DeleteAllSaves()
        {
            for (int i = 0; i < MaxSlots; i++)
                DeleteSlot(i);

            if (File.Exists(GlobalPath))
                File.Delete(GlobalPath);

            GameManager.Instance.HasClearedOnce = false;
            Debug.Log("[SaveManager] 모든 세이브 삭제 (책 불태우기)");
        }

        #endregion
    }

    [Serializable]
    public class GlobalSaveData
    {
        public bool hasClearedOnce;
    }
}
