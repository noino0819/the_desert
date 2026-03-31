using UnityEngine;
using TheSSand.Player;

namespace TheSSand.Level
{
    public enum EnemyState { Patrol, Chase, Attack, Dead }

    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class DesertEnemy : MonoBehaviour
    {
        [Header("스탯")]
        [SerializeField] int maxHP = 2;
        [SerializeField] int contactDamage = 1;
        [SerializeField] float knockbackForce = 6f;

        [Header("순찰")]
        [SerializeField] float patrolSpeed = 2f;
        [SerializeField] float patrolDistance = 4f;

        [Header("추격")]
        [SerializeField] float chaseSpeed = 4f;
        [SerializeField] float detectRange = 6f;
        [SerializeField] float loseRange = 10f;

        [Header("시각")]
        [SerializeField] float flashDuration = 0.12f;

        int _currentHP;
        EnemyState _state = EnemyState.Patrol;
        Rigidbody2D _rb;
        SpriteRenderer _sr;

        Vector3 _spawnPos;
        int _patrolDir = 1;
        Transform _playerTarget;
        float _flashTimer;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _sr = GetComponentInChildren<SpriteRenderer>();
        }

        void Start()
        {
            _currentHP = maxHP;
            _spawnPos = transform.position;
            _rb.gravityScale = 1f;
            _rb.freezeRotation = true;
        }

        void Update()
        {
            if (_state == EnemyState.Dead) return;

            FindPlayer();
            UpdateFlash();
        }

        void FixedUpdate()
        {
            if (_state == EnemyState.Dead) return;

            switch (_state)
            {
                case EnemyState.Patrol:
                    Patrol();
                    break;
                case EnemyState.Chase:
                    Chase();
                    break;
            }
        }

        void FindPlayer()
        {
            if (_playerTarget == null)
            {
                var player = FindAnyObjectByType<PlayerController>();
                if (player != null)
                    _playerTarget = player.transform;
            }

            if (_playerTarget == null) return;

            float dist = Vector2.Distance(transform.position, _playerTarget.position);

            if (_state == EnemyState.Patrol && dist < detectRange)
                _state = EnemyState.Chase;
            else if (_state == EnemyState.Chase && dist > loseRange)
                _state = EnemyState.Patrol;
        }

        void Patrol()
        {
            float distFromSpawn = transform.position.x - _spawnPos.x;
            if (Mathf.Abs(distFromSpawn) > patrolDistance)
                _patrolDir = distFromSpawn > 0 ? -1 : 1;

            _rb.linearVelocity = new Vector2(_patrolDir * patrolSpeed, _rb.linearVelocity.y);
            FlipSprite(_patrolDir);
        }

        void Chase()
        {
            if (_playerTarget == null) { _state = EnemyState.Patrol; return; }

            float dir = Mathf.Sign(_playerTarget.position.x - transform.position.x);
            _rb.linearVelocity = new Vector2(dir * chaseSpeed, _rb.linearVelocity.y);
            FlipSprite((int)dir);
        }

        void FlipSprite(int dir)
        {
            if (_sr == null) return;
            _sr.flipX = dir < 0;
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (_state == EnemyState.Dead) return;
            if (!collision.collider.CompareTag("Player")) return;

            var player = collision.collider.GetComponentInParent<PlayerController>();
            if (player == null) return;

            bool stompedFromAbove = collision.GetContact(0).normal.y < -0.5f;

            if (stompedFromAbove)
            {
                TakeDamage(1);
                var playerRb = collision.collider.GetComponentInParent<Rigidbody2D>();
                if (playerRb != null)
                    playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 8f);
            }
            else
            {
                Vector2 knockDir = (collision.transform.position - transform.position).normalized;
                knockDir.y = 0.5f;
                player.TakeDamage(contactDamage, knockDir.normalized * knockbackForce);
            }
        }

        public void TakeDamage(int damage)
        {
            if (_state == EnemyState.Dead) return;

            _currentHP -= damage;
            _flashTimer = flashDuration;

            Audio.AudioManager.Instance?.PlaySFX("enemy_hit");

            if (_currentHP <= 0)
                Die();
        }

        void Die()
        {
            _state = EnemyState.Dead;
            _rb.linearVelocity = Vector2.zero;
            _rb.bodyType = RigidbodyType2D.Kinematic;

            Audio.AudioManager.Instance?.PlaySFX("enemy_die");
            Destroy(gameObject, 0.5f);
        }

        void UpdateFlash()
        {
            if (_flashTimer <= 0 || _sr == null) return;
            _flashTimer -= Time.deltaTime;
            _sr.color = _flashTimer > 0 ? Color.red : Color.white;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, loseRange);

            Gizmos.color = Color.cyan;
            Vector3 pos = Application.isPlaying ? _spawnPos : transform.position;
            Gizmos.DrawLine(pos + Vector3.left * patrolDistance, pos + Vector3.right * patrolDistance);
        }
    }
}
