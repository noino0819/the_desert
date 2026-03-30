using System.Collections;
using UnityEngine;

namespace TheSSand.Boss
{
    /// <summary>
    /// Ch.2 보스 — 독수리 추격전 (자동 스크롤 사이드 체이스)
    /// Phase 1: 급강하 발톱
    /// Phase 2: 급강하 + 깃털 탄막 + 장애물 증가
    /// Phase 3: 급강하 + 깃털 + 바람 밀기 + 스크롤 속도 증가
    /// 반격: 독수리 급강하 후 떨어뜨리는 아이템 → 위로 투척
    /// </summary>
    public class BossEagle : BossBase
    {
        [Header("독수리 설정")]
        [SerializeField] Transform eagleTransform;
        [SerializeField] Transform playerTransform;
        [SerializeField] float eagleBaseY = 4f;
        [SerializeField] float eagleHoverAmplitude = 0.5f;

        [Header("자동 스크롤")]
        [SerializeField] float baseScrollSpeed = 4f;
        [SerializeField] float p2ScrollMultiplier = 1.3f;
        [SerializeField] float p3ScrollMultiplier = 1.6f;
        [SerializeField] float arenaMinY = -3f;
        [SerializeField] float arenaMaxY = 1f;

        [Header("급강하")]
        [SerializeField] float diveSpeed = 12f;
        [SerializeField] float diveReturnSpeed = 6f;
        [SerializeField] float diveCooldown = 3f;
        [SerializeField] float p2DiveCooldown = 2f;
        [SerializeField] float p3DiveCooldown = 1.5f;
        [SerializeField] float diveWarningDuration = 0.8f;

        [Header("깃털 탄막")]
        [SerializeField] GameObject featherPrefab;
        [SerializeField] int featherPoolSize = 20;
        [SerializeField] float featherSpeed = 5f;
        [SerializeField] float featherSpread = 30f;
        [SerializeField] int p2FeatherCount = 3;
        [SerializeField] int p3FeatherCount = 5;

        [Header("바람 밀기 (Phase 3)")]
        [SerializeField] float windPushForce = 3f;
        [SerializeField] float windDuration = 2f;

        [Header("장애물")]
        [SerializeField] GameObject[] obstaclePrefabs;
        [SerializeField] float obstacleSpawnInterval = 4f;
        [SerializeField] float p2ObstacleInterval = 2.5f;
        [SerializeField] float p3ObstacleInterval = 1.8f;
        [SerializeField] Transform obstacleSpawnPoint;
        [SerializeField] int obstaclePoolSize = 10;

        [Header("반격 아이템")]
        [SerializeField] GameObject counterItemPrefab;
        [SerializeField] float counterItemLifetime = 3f;
        [SerializeField] int counterDamage = 15;

        [Header("플레이어 이동")]
        [SerializeField] float playerMoveSpeed = 6f;
        [SerializeField] float playerJumpForce = 10f;
        [SerializeField] float groundY = -2.5f;

        GameObject[] _featherPool;
        GameObject[] _obstaclePool;
        int _featherIndex;
        int _obstacleIndex;
        bool _isDiving;
        bool _isWindActive;
        float _scrollSpeed;
        float _playerVelocityY;
        bool _isGrounded;

        protected override void Start()
        {
            base.Start();
            InitPools();
        }

        void Update()
        {
            if (!isBattleActive) return;

            HandlePlayerMovement();
            HandleAutoScroll();
            AnimateEagleHover();
        }

        void HandlePlayerMovement()
        {
            if (playerTransform == null) return;

            Vector3 pos = playerTransform.position;

            float moveY = Input.GetAxisRaw("Vertical");
            float moveX = Input.GetAxisRaw("Horizontal");

            _isGrounded = pos.y <= groundY + 0.05f;

            if (_isGrounded && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)))
                _playerVelocityY = playerJumpForce;

            _playerVelocityY += Physics2D.gravity.y * Time.deltaTime;
            pos.y += _playerVelocityY * Time.deltaTime;
            pos.x += moveX * playerMoveSpeed * 0.3f * Time.deltaTime;

            if (_isWindActive)
                pos.x -= windPushForce * Time.deltaTime;

            pos.y = Mathf.Clamp(pos.y, groundY, arenaMaxY);
            if (pos.y <= groundY)
            {
                pos.y = groundY;
                _playerVelocityY = 0f;
            }

            playerTransform.position = pos;
        }

        void HandleAutoScroll()
        {
            _scrollSpeed = currentPhase switch
            {
                1 => baseScrollSpeed,
                2 => baseScrollSpeed * p2ScrollMultiplier,
                _ => baseScrollSpeed * p3ScrollMultiplier
            };
        }

        void AnimateEagleHover()
        {
            if (eagleTransform == null || _isDiving) return;

            float hover = Mathf.Sin(Time.time * 2f) * eagleHoverAmplitude;
            eagleTransform.position = new Vector3(
                eagleTransform.position.x,
                eagleBaseY + hover,
                eagleTransform.position.z
            );
        }

        public float GetScrollSpeed() => _scrollSpeed;

        #region 배틀 루프

        protected override IEnumerator BattleLoop()
        {
            StartCoroutine(ObstacleSpawnLoop());

            while (isBattleActive)
            {
                float cooldown = currentPhase switch
                {
                    1 => diveCooldown,
                    2 => p2DiveCooldown,
                    _ => p3DiveCooldown
                };

                yield return new WaitForSeconds(cooldown);
                if (!isBattleActive) break;

                yield return StartCoroutine(DiveAttack());

                if (currentPhase >= 2 && isBattleActive)
                {
                    yield return new WaitForSeconds(0.5f);
                    SpawnFeathers();
                }

                if (currentPhase >= 3 && isBattleActive)
                {
                    yield return new WaitForSeconds(0.3f);
                    StartCoroutine(WindPush());
                }
            }
        }

        IEnumerator DiveAttack()
        {
            if (eagleTransform == null || playerTransform == null) yield break;

            _isDiving = true;

            float targetX = playerTransform.position.x;
            float targetY = playerTransform.position.y + 0.5f;

            yield return new WaitForSeconds(diveWarningDuration);
            if (!isBattleActive) { _isDiving = false; yield break; }

            Vector3 startPos = eagleTransform.position;
            Vector3 diveTarget = new Vector3(targetX, targetY, 0);
            float dist = Vector3.Distance(startPos, diveTarget);
            float diveTime = dist / diveSpeed;
            float elapsed = 0f;

            while (elapsed < diveTime && isBattleActive)
            {
                elapsed += Time.deltaTime;
                eagleTransform.position = Vector3.Lerp(startPos, diveTarget, elapsed / diveTime);
                yield return null;
            }

            if (isBattleActive)
                SpawnCounterItem(eagleTransform.position);

            Vector3 returnTarget = new Vector3(startPos.x, eagleBaseY, 0);
            elapsed = 0f;
            float returnTime = Vector3.Distance(eagleTransform.position, returnTarget) / diveReturnSpeed;

            while (elapsed < returnTime && isBattleActive)
            {
                elapsed += Time.deltaTime;
                eagleTransform.position = Vector3.Lerp(diveTarget, returnTarget, elapsed / returnTime);
                yield return null;
            }

            _isDiving = false;
        }

        void SpawnFeathers()
        {
            int count = currentPhase >= 3 ? p3FeatherCount : p2FeatherCount;
            float startAngle = -featherSpread * 0.5f;
            float step = count > 1 ? featherSpread / (count - 1) : 0f;

            for (int i = 0; i < count; i++)
            {
                var feather = GetFromPool(_featherPool, ref _featherIndex);
                if (feather == null) continue;

                feather.transform.position = eagleTransform != null
                    ? eagleTransform.position
                    : transform.position;
                feather.SetActive(true);

                float angle = startAngle + step * i - 90f;
                Vector2 dir = new Vector2(
                    Mathf.Cos(angle * Mathf.Deg2Rad),
                    Mathf.Sin(angle * Mathf.Deg2Rad)
                );

                var rb = feather.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.gravityScale = 0.3f;
                    rb.linearVelocity = dir * featherSpeed;
                }
            }
        }

        IEnumerator WindPush()
        {
            _isWindActive = true;
            yield return new WaitForSeconds(windDuration);
            _isWindActive = false;
        }

        void SpawnCounterItem(Vector3 position)
        {
            if (counterItemPrefab == null) return;

            var item = Instantiate(counterItemPrefab, position, Quaternion.identity);
            var pickup = item.GetComponent<EagleCounterItem>();
            if (pickup != null)
            {
                pickup.Setup(this, counterDamage, counterItemLifetime);
            }
            else
            {
                Destroy(item, counterItemLifetime);
            }
        }

        IEnumerator ObstacleSpawnLoop()
        {
            while (isBattleActive)
            {
                float interval = currentPhase switch
                {
                    1 => obstacleSpawnInterval,
                    2 => p2ObstacleInterval,
                    _ => p3ObstacleInterval
                };

                yield return new WaitForSeconds(interval);
                if (!isBattleActive) break;

                SpawnObstacle();
            }
        }

        void SpawnObstacle()
        {
            if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;

            var obstacle = GetFromPool(_obstaclePool, ref _obstacleIndex);
            if (obstacle == null) return;

            if (obstacleSpawnPoint != null)
                obstacle.transform.position = obstacleSpawnPoint.position +
                    new Vector3(0, Random.Range(-1f, 1f), 0);

            obstacle.SetActive(true);

            var rb = obstacle.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0;
                rb.linearVelocity = Vector2.left * _scrollSpeed;
            }
        }

        #endregion

        #region 오브젝트 풀

        void InitPools()
        {
            _featherPool = CreatePool(featherPrefab, "FeatherPool", featherPoolSize);

            if (obstaclePrefabs != null && obstaclePrefabs.Length > 0)
                _obstaclePool = CreatePool(obstaclePrefabs[0], "ObstaclePool", obstaclePoolSize);
            else
                _obstaclePool = new GameObject[0];
        }

        GameObject[] CreatePool(GameObject prefab, string parentName, int size)
        {
            if (prefab == null) return new GameObject[0];

            var parent = new GameObject(parentName);
            parent.transform.SetParent(transform);

            var pool = new GameObject[size];
            for (int i = 0; i < size; i++)
            {
                pool[i] = Instantiate(prefab, parent.transform);
                pool[i].SetActive(false);
            }
            return pool;
        }

        GameObject GetFromPool(GameObject[] pool, ref int index)
        {
            if (pool == null || pool.Length == 0) return null;

            for (int i = 0; i < pool.Length; i++)
            {
                int idx = (index + i) % pool.Length;
                if (!pool[idx].activeInHierarchy)
                {
                    index = (idx + 1) % pool.Length;
                    return pool[idx];
                }
            }

            index = (index + 1) % pool.Length;
            pool[index].SetActive(false);
            return pool[index];
        }

        #endregion
    }

    public class EagleCounterItem : MonoBehaviour
    {
        BossEagle _boss;
        int _damage;
        float _lifetime;
        bool _pickedUp;

        public void Setup(BossEagle boss, int damage, float lifetime)
        {
            _boss = boss;
            _damage = damage;
            _lifetime = lifetime;
            _pickedUp = false;
            Invoke(nameof(AutoDestroy), lifetime);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (_pickedUp || !other.CompareTag("Player")) return;

            _pickedUp = true;
            _boss?.DamageBoss(_damage);
            Destroy(gameObject);
        }

        void AutoDestroy() => Destroy(gameObject);
    }
}
