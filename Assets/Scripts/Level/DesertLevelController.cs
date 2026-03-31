using UnityEngine;
using TheSSand.Audio;
using TheSSand.Scene;

namespace TheSSand.Level
{
    public class DesertLevelController : MonoBehaviour
    {
        [Header("레벨 설정")]
        [SerializeField] int chapter = 1;
        [SerializeField] string nextSceneName = "SCN_Ch1_Oasis";

        [Header("카메라")]
        [SerializeField] Camera mainCamera;
        [SerializeField] float cameraFollowSpeed = 5f;
        [SerializeField] Vector2 cameraOffset = new Vector2(3f, 1f);
        [SerializeField] float cameraBoundsMinX;
        [SerializeField] float cameraBoundsMaxX;

        [Header("패럴랙스 배경")]
        [SerializeField] Transform[] backgroundLayers;
        [SerializeField] float[] parallaxFactors;

        Transform _player;
        float[] _bgStartX;

        void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (mainCamera == null)
                mainCamera = Camera.main;

            InitParallax();

            AudioManager.Instance?.PlayBGM($"BGM_Ch{chapter}_Desert");
            SceneTransitionManager.Instance?.FadeIn();
        }

        void LateUpdate()
        {
            if (_player == null) return;

            FollowCamera();
            UpdateParallax();
        }

        void FollowCamera()
        {
            Vector3 targetPos = new Vector3(
                _player.position.x + cameraOffset.x,
                _player.position.y + cameraOffset.y,
                mainCamera.transform.position.z
            );

            targetPos.x = Mathf.Clamp(targetPos.x, cameraBoundsMinX, cameraBoundsMaxX);

            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position,
                targetPos,
                cameraFollowSpeed * Time.deltaTime
            );
        }

        #region 패럴랙스

        void InitParallax()
        {
            if (backgroundLayers == null) return;
            _bgStartX = new float[backgroundLayers.Length];
            for (int i = 0; i < backgroundLayers.Length; i++)
            {
                if (backgroundLayers[i] != null)
                    _bgStartX[i] = backgroundLayers[i].position.x;
            }
        }

        void UpdateParallax()
        {
            if (backgroundLayers == null || mainCamera == null) return;

            for (int i = 0; i < backgroundLayers.Length; i++)
            {
                if (backgroundLayers[i] == null) continue;
                if (i >= parallaxFactors.Length) break;

                float camDelta = mainCamera.transform.position.x * parallaxFactors[i];
                backgroundLayers[i].position = new Vector3(
                    _bgStartX[i] + camDelta,
                    backgroundLayers[i].position.y,
                    backgroundLayers[i].position.z
                );
            }
        }

        #endregion
    }
}
