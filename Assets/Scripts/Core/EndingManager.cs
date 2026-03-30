using System;
using System.Collections;
using UnityEngine;
using TheSSand.Audio;
using TheSSand.Scene;

namespace TheSSand.Core
{
    public class EndingManager : MonoBehaviour
    {
        public static EndingManager Instance { get; private set; }

        public event Action<int> OnEndingDetermined;

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

        public int EvaluateAndTriggerEnding()
        {
            if (GameManager.Instance == null) return 1;

            var save = GameManager.Instance.CurrentSave;

            if (save.isNewGame2)
            {
                bool isEd5 = GameManager.Instance.EvaluateNGEnding();
                int ending = isEd5 ? 5 : 0;
                OnEndingDetermined?.Invoke(ending);
                return ending;
            }

            int ed = GameManager.Instance.EvaluateEnding();
            OnEndingDetermined?.Invoke(ed);

            GameManager.Instance.HasClearedOnce = true;
            SaveManager.Instance?.SaveGlobalData();

            return ed;
        }

        public void TriggerBookBurn()
        {
            StartCoroutine(BookBurnSequence());
        }

        IEnumerator BookBurnSequence()
        {
            AudioManager.Instance?.PlaySFX("SFX_BookBurn");
            AudioManager.Instance?.StopBGM(2f);

            yield return new WaitForSeconds(5f);

            SaveManager.Instance?.DeleteAllSaves();

            yield return new WaitForSeconds(2f);

            SceneTransitionManager.Instance?.LoadScene("SCN_TitleMenu");
        }
    }
}
