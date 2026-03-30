using System.Collections;
using UnityEngine;
using TheSSand.Core;
using TheSSand.Audio;
using TheSSand.UI;
using TheSSand.Quest;

namespace TheSSand.Level
{
    public class NGChapterController : MonoBehaviour
    {
        [Header("2회차 설정")]
        [SerializeField] int chapter;
        [SerializeField] GameObject[] desolationOverlays;
        [SerializeField] GameObject[] npcOriginals;
        [SerializeField] GameObject[] npcDescendants;
        [SerializeField] GameObject mutantBossObject;
        [SerializeField] GameObject seedRecoveryPoint;

        [Header("단서 오브젝트")]
        [SerializeField] GameObject[] clueObjects;

        [Header("포스트 프로세싱")]
        [SerializeField] UnityEngine.Rendering.Volume postProcessVolume;

        void Start()
        {
            if (!GameManager.Instance.CurrentSave.isNewGame2)
            {
                Debug.LogWarning("[NGController] 1회차에서 2회차 씬 로드됨");
                return;
            }

            ApplyNGVisuals();
            SwapNPCs();
            SetupChapter();
        }

        void ApplyNGVisuals()
        {
            foreach (var overlay in desolationOverlays)
            {
                if (overlay != null) overlay.SetActive(true);
            }

            var cam = Camera.main;
            if (cam != null)
            {
                var light = FindFirstObjectByType<UnityEngine.Rendering.Universal.Light2D>();
                if (light != null)
                    light.intensity *= 0.7f;
            }
        }

        void SwapNPCs()
        {
            foreach (var npc in npcOriginals)
            {
                if (npc != null) npc.SetActive(false);
            }
            foreach (var desc in npcDescendants)
            {
                if (desc != null) desc.SetActive(true);
            }
        }

        void SetupChapter()
        {
            switch (chapter)
            {
                case 4: SetupNGCh4(); break;
                case 3: SetupNGCh3(); break;
                case 2: SetupNGCh2(); break;
                case 1: SetupNGCh1(); break;
            }
        }

        void SetupNGCh4()
        {
            string bgm = "BGM_Ch4_Oasis";
            AudioManager.Instance?.PlayBGM(bgm);

            NarrationUI.Instance?.ShowNarrationSequence(new[]
            {
                "여기서부터 시작하는군요.",
                "할아버지가 마지막으로 씨앗을 심은 곳.",
                "가장 최근에 망가진 곳."
            });
        }

        void SetupNGCh3()
        {
            AudioManager.Instance?.PlayBGM("BGM_Ch3_Desert");

            NarrationUI.Instance?.ShowNarrationSequence(new[]
            {
                "나무벽이 없네요.",
                "1회차에서 무너졌으니까요.",
                "그런데… 통합된 마을치고는 너무 조용합니다.",
                "사람이 많지 않아요."
            });
        }

        void SetupNGCh2()
        {
            AudioManager.Instance?.PlayBGM("BGM_Ch2_Oasis");

            NarrationUI.Instance?.ShowNarrationSequence(new[]
            {
                "아이들이 많지 않네요.",
                "예전에는 왁자지껄했을 텐데."
            });
        }

        void SetupNGCh1()
        {
            AudioManager.Instance?.PlayBGM("BGM_Ch1_Oasis");

            NarrationUI.Instance?.ShowNarrationSequence(new[]
            {
                "여기가 마지막이네요.",
                "할아버지가 처음으로 씨앗을 심은 곳.",
                "가장 오래된 피해.",
                "오아시스가 거의 사라졌습니다.",
                "물이 없어요."
            });
        }

        public void OnBossDefeated()
        {
            StartCoroutine(SeedRecoverySequence());
        }

        IEnumerator SeedRecoverySequence()
        {
            AudioManager.Instance?.PlaySFX("SFX_SeedPlant");

            NarrationUI.Instance?.ShowNarrationSequence(new[]
            {
                "씨앗을 뽑아냅니다.",
                "그리고 진짜 씨앗을 심습니다."
            });

            yield return new WaitForSeconds(2f);

            GameManager.Instance?.RecoverFakeSeed();
            GameManager.Instance?.PlantTrueSeed();

            if (chapter == 1)
            {
                yield return new WaitForSeconds(2f);
                NarrationUI.Instance?.ShowNarrationSequence(new[]
                {
                    "진짜 씨앗이 심어집니다.",
                    "네 곳 모두."
                });

                yield return new WaitForSeconds(3f);

                int ending = EndingManager.Instance?.EvaluateAndTriggerEnding() ?? 0;
                if (ending == 5)
                    Scene.SceneTransitionManager.Instance?.LoadScene("SCN_Ending");
                else
                    Scene.SceneTransitionManager.Instance?.LoadScene("SCN_Ending");
            }
        }
    }
}
