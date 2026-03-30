using System.Collections;
using UnityEngine;

namespace TheSSand.Camera
{
    /// <summary>
    /// 범용 카메라 컨트롤러 — 플레이어 추적, 줌, 셰이크, 포커스, 바운드
    /// 모든 씬에서 재사용 가능. 대본 [CAM] 태그 구현용.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        public static CameraController Instance { get; private set; }

        [Header("추적 대상")]
        [SerializeField] Transform target;
        [SerializeField] float followSpeed = 5f;
        [SerializeField] Vector3 offset = new(0, 1f, -10f);

        [Header("바운드 (맵 경계)")]
        [SerializeField] bool useBounds;
        [SerializeField] float minX = -10f;
        [SerializeField] float maxX = 100f;
        [SerializeField] float minY = -5f;
        [SerializeField] float maxY = 10f;

        [Header("줌")]
        [SerializeField] float defaultOrthoSize = 5f;
        [SerializeField] float zoomSpeed = 3f;

        [Header("데드존")]
        [SerializeField] float deadZoneX = 0.5f;
        [SerializeField] float deadZoneY = 0.3f;

        [Header("Look Ahead")]
        [SerializeField] float lookAheadDistance = 2f;
        [SerializeField] float lookAheadSpeed = 2f;

        UnityEngine.Camera _cam;
        float _targetOrthoSize;
        float _currentLookAhead;
        Vector3 _shakeOffset;
        bool _isShaking;
        bool _isFocusing;
        Vector3 _focusTarget;
        float _focusSpeed;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _cam = GetComponent<UnityEngine.Camera>();
            if (_cam == null) _cam = UnityEngine.Camera.main;
            _targetOrthoSize = defaultOrthoSize;
        }

        void Start()
        {
            if (target == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) target = player.transform;
            }

            if (target != null)
                transform.position = GetTargetPosition();
        }

        void LateUpdate()
        {
            if (_isFocusing)
            {
                Vector3 focusPos = new Vector3(_focusTarget.x, _focusTarget.y, offset.z);
                transform.position = Vector3.Lerp(transform.position, focusPos + _shakeOffset, _focusSpeed * Time.deltaTime);
            }
            else if (target != null)
            {
                Vector3 targetPos = GetTargetPosition();
                Vector3 current = transform.position;

                float dx = targetPos.x - current.x;
                float dy = targetPos.y - current.y;

                if (Mathf.Abs(dx) > deadZoneX)
                    current.x = Mathf.Lerp(current.x, targetPos.x, followSpeed * Time.deltaTime);
                if (Mathf.Abs(dy) > deadZoneY)
                    current.y = Mathf.Lerp(current.y, targetPos.y, followSpeed * Time.deltaTime);

                current.z = offset.z;
                current += _shakeOffset;

                if (useBounds)
                {
                    float halfH = _cam.orthographicSize;
                    float halfW = halfH * _cam.aspect;
                    current.x = Mathf.Clamp(current.x, minX + halfW, maxX - halfW);
                    current.y = Mathf.Clamp(current.y, minY + halfH, maxY - halfH);
                }

                transform.position = current;
            }

            if (_cam != null && !Mathf.Approximately(_cam.orthographicSize, _targetOrthoSize))
                _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, _targetOrthoSize, zoomSpeed * Time.deltaTime);
        }

        Vector3 GetTargetPosition()
        {
            if (target == null) return transform.position;

            float inputX = Input.GetAxisRaw("Horizontal");
            float targetLookAhead = inputX * lookAheadDistance;
            _currentLookAhead = Mathf.Lerp(_currentLookAhead, targetLookAhead, lookAheadSpeed * Time.deltaTime);

            return new Vector3(
                target.position.x + offset.x + _currentLookAhead,
                target.position.y + offset.y,
                offset.z
            );
        }

        #region 공개 API — [CAM] 태그 구현용

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            _isFocusing = false;
        }

        public void SetBounds(float xMin, float xMax, float yMin, float yMax)
        {
            useBounds = true;
            minX = xMin; maxX = xMax;
            minY = yMin; maxY = yMax;
        }

        public void ZoomTo(float orthoSize, float speed = -1f)
        {
            _targetOrthoSize = orthoSize;
            if (speed > 0f) zoomSpeed = speed;
        }

        public void ResetZoom()
        {
            _targetOrthoSize = defaultOrthoSize;
        }

        /// <summary>
        /// 특정 위치로 카메라 포커스 (컷씬용)
        /// </summary>
        public void FocusOn(Vector3 position, float speed = 5f)
        {
            _isFocusing = true;
            _focusTarget = position;
            _focusSpeed = speed;
        }

        /// <summary>
        /// 포커스 해제 → 플레이어 추적으로 복귀
        /// </summary>
        public void ReleaseFocus()
        {
            _isFocusing = false;
        }

        /// <summary>
        /// 화면 흔들기 (보스 페이즈, 충격 등)
        /// </summary>
        public void Shake(float intensity = 0.3f, float duration = 0.3f)
        {
            if (_isShaking) return;
            StartCoroutine(ShakeCoroutine(intensity, duration));
        }

        IEnumerator ShakeCoroutine(float intensity, float duration)
        {
            _isShaking = true;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float decay = 1f - (elapsed / duration);
                _shakeOffset = new Vector3(
                    Random.Range(-1f, 1f) * intensity * decay,
                    Random.Range(-1f, 1f) * intensity * decay,
                    0f
                );
                yield return null;
            }

            _shakeOffset = Vector3.zero;
            _isShaking = false;
        }

        /// <summary>
        /// 즉시 이동 (씬 시작 시 등)
        /// </summary>
        public void SnapToTarget()
        {
            if (target != null)
                transform.position = GetTargetPosition();
        }

        #endregion
    }
}
