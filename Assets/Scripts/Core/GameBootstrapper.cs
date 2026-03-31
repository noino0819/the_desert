using UnityEngine;

namespace TheSSand.Core
{
    public class GameBootstrapper : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            if (GameManager.Instance != null) return;

            var prefab = Resources.Load<GameObject>("GameBootstrapper");
            if (prefab != null)
            {
                var obj = Instantiate(prefab);
                obj.name = "GameBootstrapper";
                DontDestroyOnLoad(obj);
            }
            else
            {
                CreateManagersManually();
            }
        }

        static void CreateManagersManually()
        {
            var root = new GameObject("GameBootstrapper");
            DontDestroyOnLoad(root);

            root.AddComponent<GameManager>();
            root.AddComponent<SaveManager>();
            root.AddComponent<EndingManager>();

            var transitionObj = new GameObject("SceneTransitionManager");
            transitionObj.transform.SetParent(root.transform);
            transitionObj.AddComponent<Scene.SceneTransitionManager>();

            var dialogueObj = new GameObject("DialogueManager");
            dialogueObj.transform.SetParent(root.transform);
            dialogueObj.AddComponent<Dialogue.DialogueManager>();

            var questObj = new GameObject("QuestManager");
            questObj.transform.SetParent(root.transform);
            questObj.AddComponent<Quest.QuestManager>();

            var audioObj = new GameObject("AudioManager");
            audioObj.transform.SetParent(root.transform);
            audioObj.AddComponent<Audio.AudioManager>();

            var itemDBObj = new GameObject("ItemDatabase");
            itemDBObj.transform.SetParent(root.transform);
            itemDBObj.AddComponent<Item.ItemDatabase>();

            var inventoryObj = new GameObject("InventoryManager");
            inventoryObj.transform.SetParent(root.transform);
            inventoryObj.AddComponent<UI.InventoryManager>();

            var loadingObj = new GameObject("LoadingScreen");
            loadingObj.transform.SetParent(root.transform);
            loadingObj.AddComponent<UI.LoadingScreen>();
        }
    }
}
