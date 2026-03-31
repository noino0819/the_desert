using System.Collections;
using UnityEngine;

namespace TheSSand.Boss
{
    public class BossHippo : BossBase
    {
        [Header("사막하마 설정")]
        [SerializeField] Transform hippoTransform;
        [SerializeField] Transform playerTransform;
        [SerializeField] float arenaMinX = -6f;
        [SerializeField] float arenaMaxX = 6f;

        [Header("투사체")]
        [SerializeField] GameObject mudProjectilePrefab;
        [SerializeField] GameObject waterProjectilePrefab;
        [SerializeField] Transform[] spawnPoints;
        [SerializeField] int projectilePoolSize = 30;

        [Header("페이즈별 파라미터")]
        [SerializeField] float p1FireRate = 1.2f;
        [SerializeField] float p2FireRate = 0.8f;
        [SerializeField] float p3FireRate = 0.5f;
        [SerializeField] float p1ProjectileSpeed = 4f;
        [SerializeField] float p2ProjectileSpeed = 6f;
        [SerializeField] float p3ProjectileSpeed = 8f;

        [Header("경직")]
        [SerializeField] float p1StunDuration = 2f;
        [SerializeField] float p2StunDuration = 1.5f;
        [SerializeField] float p3StunDuration = 1f;
        [SerializeField] int attacksBeforeStun = 5;

        [Header("반격")]
        [SerializeField] float counterAttackRange = 1.5f;
        [SerializeField] int counterDamage = 12;

        GameObject[] _mudPool;
        GameObject[] _waterPool;
        int _mudPoolIndex;
        int _waterPoolIndex;
        int _attackCount;

        protected override void Start()
        {
            base.Start();
            InitObjectPools();
        }

        void Update()
        {
            if (!isBattleActive) return;
            HandlePlayerInput();
        }

        void HandlePlayerInput()
        {
            if (playerTransform == null) return;

            float moveInput = Input.GetAxisRaw("Horizontal");
            float speed = 8f;
            Vector3 pos = playerTransform.position;
            pos.x = Mathf.Clamp(pos.x + moveInput * speed * Time.deltaTime, arenaMinX, arenaMaxX);
            playerTransform.position = pos;

            if (isStunned && Input.GetKeyDown(KeyCode.Space))
                TryCounterAttack();
        }

        void TryCounterAttack()
        {
            if (hippoTransform == null || playerTransform == null) return;

            float dist = Mathf.Abs(hippoTransform.position.x - playerTransform.position.x);
            if (dist <= counterAttackRange)
            {
                DamageBoss(counterDamage);
                isStunned = false;
            }
        }

        #region 배틀 루프

        protected override IEnumerator BattleLoop()
        {
            while (isBattleActive)
            {
                _attackCount = 0;

                for (int i = 0; i < attacksBeforeStun && isBattleActive; i++)
                {
                    yield return StartCoroutine(ExecuteAttackPattern());
                    _attackCount++;

                    float delay = currentPhase switch
                    {
                        1 => p1FireRate,
                        2 => p2FireRate,
                        _ => p3FireRate
                    };
                    yield return new WaitForSeconds(delay);
                }

                if (!isBattleActive) break;

                yield return StartCoroutine(StunPhase());
            }
        }

        IEnumerator ExecuteAttackPattern()
        {
            switch (currentPhase)
            {
                case 1:
                    if (bossAnimator != null) bossAnimator.SetInteger("AttackType", 1);
                    SpawnMudProjectile();
                    break;
                case 2:
                    if (_attackCount % 2 == 0)
                    {
                        if (bossAnimator != null) bossAnimator.SetInteger("AttackType", 1);
                        SpawnMudProjectile();
                    }
                    else
                    {
                        if (bossAnimator != null) bossAnimator.SetInteger("AttackType", 2);
                        SpawnWaterProjectile();
                    }
                    break;
                default:
                    if (bossAnimator != null) bossAnimator.SetInteger("AttackType", 1);
                    SpawnMudProjectile();
                    yield return new WaitForSeconds(0.2f);
                    if (bossAnimator != null) bossAnimator.SetInteger("AttackType", 2);
                    SpawnWaterProjectile();
                    break;
            }

            yield return new WaitForSeconds(0.3f);
            if (bossAnimator != null) bossAnimator.SetInteger("AttackType", 0);
        }

        IEnumerator StunPhase()
        {
            isStunned = true;
            if (bossAnimator != null) bossAnimator.SetBool("IsStunned", true);

            float duration = currentPhase switch
            {
                1 => p1StunDuration,
                2 => p2StunDuration,
                _ => p3StunDuration
            };

            yield return new WaitForSeconds(duration);
            isStunned = false;
            if (bossAnimator != null) bossAnimator.SetBool("IsStunned", false);
        }

        #endregion

        #region 투사체

        void SpawnMudProjectile()
        {
            var proj = GetFromPool(_mudPool, ref _mudPoolIndex);
            if (proj == null) return;

            Transform sp = spawnPoints.Length > 0
                ? spawnPoints[Random.Range(0, spawnPoints.Length)]
                : hippoTransform;

            proj.transform.position = sp.position;
            proj.SetActive(true);

            var rb = proj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 1f;

                float speed = currentPhase switch
                {
                    1 => p1ProjectileSpeed,
                    2 => p2ProjectileSpeed,
                    _ => p3ProjectileSpeed
                };

                float dirX = playerTransform != null
                    ? (playerTransform.position.x - sp.position.x) * 0.3f
                    : 0f;
                rb.linearVelocity = new Vector2(dirX, -speed * 0.5f);
            }
        }

        void SpawnWaterProjectile()
        {
            var proj = GetFromPool(_waterPool, ref _waterPoolIndex);
            if (proj == null) return;

            Vector3 spawnPos = hippoTransform != null
                ? hippoTransform.position
                : transform.position;
            spawnPos.y -= 0.5f;

            proj.transform.position = spawnPos;
            proj.SetActive(true);

            var rb = proj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0f;

                float speed = currentPhase switch
                {
                    1 => p1ProjectileSpeed,
                    2 => p2ProjectileSpeed,
                    _ => p3ProjectileSpeed
                };

                float dir = playerTransform != null
                    ? Mathf.Sign(playerTransform.position.x - spawnPos.x)
                    : (Random.value > 0.5f ? 1f : -1f);

                rb.linearVelocity = new Vector2(dir * speed, 0f);
            }
        }

        #endregion

        #region 오브젝트 풀

        void InitObjectPools()
        {
            _mudPool = CreatePool(mudProjectilePrefab, "MudPool");
            _waterPool = CreatePool(waterProjectilePrefab, "WaterPool");
        }

        GameObject[] CreatePool(GameObject prefab, string parentName)
        {
            if (prefab == null) return new GameObject[0];

            var parent = new GameObject(parentName);
            parent.transform.SetParent(transform);

            var pool = new GameObject[projectilePoolSize];
            for (int i = 0; i < projectilePoolSize; i++)
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
}
