using UnityEngine;
using TheSSand.Core;

namespace TheSSand.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class PlayerController : MonoBehaviour
    {
        #region 이동 파라미터

        [Header("기본 이동")]
        [SerializeField] float walkSpeed = 6f;
        [SerializeField] float runSpeed = 10f;

        [Header("점프")]
        [SerializeField] float jumpForce = 12f;
        [SerializeField] float coyoteTime = 0.12f;
        [SerializeField] float jumpBufferTime = 0.1f;
        [SerializeField] float fallGravityMultiplier = 2.5f;
        [SerializeField] float lowJumpGravityMultiplier = 2f;

        [Header("벽타기")]
        [SerializeField] float wallSlideSpeed = 2f;
        [SerializeField] float wallJumpForceX = 8f;
        [SerializeField] float wallJumpForceY = 12f;
        [SerializeField] float wallCheckDistance = 0.3f;

        [Header("대시")]
        [SerializeField] float dashDistance = 4.5f;
        [SerializeField] float dashDuration = 0.15f;
        [SerializeField] float dashCooldown = 0.6f;
        [SerializeField] float dashInvincibleTime = 0.12f;

        [Header("지면 / 벽 감지")]
        [SerializeField] Transform groundCheck;
        [SerializeField] Vector2 groundCheckSize = new Vector2(0.4f, 0.05f);
        [SerializeField] LayerMask groundLayer;
        [SerializeField] LayerMask wallLayer;

        #endregion

        Rigidbody2D _rb;
        BoxCollider2D _col;

        // 입력
        float _moveInput;
        bool _isRunning;

        // 지면 / 벽
        bool _isGrounded;
        bool _isTouchingWall;
        int _wallDirection;
        int _facingDirection = 1;

        // 코요테 타임 & 입력 버퍼
        float _coyoteTimer;
        float _jumpBufferTimer;

        // 점프
        bool _isJumping;
        int _jumpCount;
        int MaxJumpCount => HasDoubleJump ? 2 : 1;

        // 벽 점프
        bool _isWallSliding;

        // 대시
        bool _isDashing;
        float _dashTimer;
        float _dashCooldownTimer;
        Vector2 _dashDirection;

        // 체력
        [Header("체력")]
        [SerializeField] int maxHP = 3;
        int _currentHP;

        // 무적
        bool _isInvincible;
        float _invincibleTimer;
        [SerializeField] float hitInvincibleTime = 1.5f;

        // 피격 비주얼
        SpriteRenderer _spriteRenderer;
        float _flashTimer;
        bool _isFlashing;

        // 애니메이션
        Animator _animator;
        static readonly int AnimSpeed = Animator.StringToHash("Speed");
        static readonly int AnimIsGrounded = Animator.StringToHash("IsGrounded");
        static readonly int AnimVelocityY = Animator.StringToHash("VelocityY");
        static readonly int AnimIsHit = Animator.StringToHash("IsHit");
        static readonly int AnimIsDashing = Animator.StringToHash("IsDashing");
        static readonly int AnimIsWallSliding = Animator.StringToHash("IsWallSliding");
        static readonly int AnimIsDead = Animator.StringToHash("IsDead");

        // 입력 잠금 (대화, 컷씬 등)
        bool _inputLocked;

        public event System.Action<int, int> OnHPChanged;
        public event System.Action OnPlayerDied;

        #region 능력 해금 플래그

        public bool HasDoubleJump { get; set; }
        public bool HasWallJump { get; set; }
        public bool HasDash { get; set; }

        #endregion

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<BoxCollider2D>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _animator = GetComponentInChildren<Animator>();
        }

        void Start()
        {
            _currentHP = maxHP;
            SyncAbilitiesFromSave();
            RestorePositionIfNeeded();
            OnHPChanged?.Invoke(_currentHP, maxHP);
        }

        void RestorePositionIfNeeded()
        {
            var gm = Core.GameManager.Instance;
            if (gm == null || !gm.PendingPositionRestore) return;

            var save = gm.CurrentSave;
            if (save.playerPosX != 0f || save.playerPosY != 0f)
                transform.position = new Vector3(save.playerPosX, save.playerPosY, 0f);

            gm.PendingPositionRestore = false;
        }

        void Update()
        {
            if (_inputLocked)
            {
                _moveInput = 0f;
                return;
            }

            GatherInput();
            UpdateTimers();
            CheckGround();
            CheckWall();

            if (!_isDashing)
            {
                HandleJump();
                HandleWallSlide();
                HandleDash();
            }

            UpdateFlash();
            UpdateAnimator();
        }

        void FixedUpdate()
        {
            if (_isDashing)
            {
                DashMovement();
                return;
            }

            HorizontalMovement();
            ApplyGravityModifiers();
        }

        #region 입력

        void GatherInput()
        {
            _moveInput = Input.GetAxisRaw("Horizontal");
            _isRunning = Input.GetKey(KeyCode.LeftShift);

            if (Input.GetKeyDown(KeyCode.Space))
                _jumpBufferTimer = jumpBufferTime;

            if (_moveInput != 0)
                _facingDirection = _moveInput > 0 ? 1 : -1;
        }

        #endregion

        #region 타이머

        void UpdateTimers()
        {
            if (_coyoteTimer > 0) _coyoteTimer -= Time.deltaTime;
            if (_jumpBufferTimer > 0) _jumpBufferTimer -= Time.deltaTime;
            if (_dashCooldownTimer > 0) _dashCooldownTimer -= Time.deltaTime;

            if (_isInvincible)
            {
                _invincibleTimer -= Time.deltaTime;
                if (_invincibleTimer <= 0)
                    _isInvincible = false;
            }

            if (_isDashing)
            {
                _dashTimer -= Time.deltaTime;
                if (_dashTimer <= 0)
                    EndDash();
            }
        }

        #endregion

        #region 지면 / 벽 감지

        void CheckGround()
        {
            bool wasGrounded = _isGrounded;
            _isGrounded = groundCheck != null &&
                          Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);

            if (_isGrounded)
            {
                _coyoteTimer = coyoteTime;
                if (_jumpCount > 0 || (wasGrounded == false && _rb.linearVelocity.y <= 0.1f))
                    Audio.AudioManager.Instance?.PlaySFX("player_land", 0.08f);
                _jumpCount = 0;
                _isJumping = false;
            }
            else if (wasGrounded)
            {
                // 플랫폼에서 떨어진 직후 — 코요테 타임 시작
            }
        }

        void CheckWall()
        {
            if (!HasWallJump)
            {
                _isTouchingWall = false;
                return;
            }

            RaycastHit2D hitRight = Physics2D.Raycast(
                transform.position, Vector2.right, wallCheckDistance, wallLayer);
            RaycastHit2D hitLeft = Physics2D.Raycast(
                transform.position, Vector2.left, wallCheckDistance, wallLayer);

            if (hitRight.collider != null)
            {
                _isTouchingWall = true;
                _wallDirection = 1;
            }
            else if (hitLeft.collider != null)
            {
                _isTouchingWall = true;
                _wallDirection = -1;
            }
            else
            {
                _isTouchingWall = false;
            }
        }

        #endregion

        #region 수평 이동

        void HorizontalMovement()
        {
            if (_isWallSliding) return;

            float speed = _isRunning ? runSpeed : walkSpeed;
            _rb.linearVelocity = new Vector2(_moveInput * speed, _rb.linearVelocity.y);
        }

        #endregion

        #region 점프

        void HandleJump()
        {
            if (_jumpBufferTimer <= 0) return;

            // 벽 점프
            if (_isWallSliding && HasWallJump)
            {
                WallJump();
                return;
            }

            // 일반 점프 (코요테 타임 포함)
            if (_coyoteTimer > 0 && _jumpCount == 0)
            {
                ExecuteJump();
                return;
            }

            // 더블 점프
            if (HasDoubleJump && _jumpCount < MaxJumpCount && !_isGrounded)
            {
                ExecuteJump();
            }
        }

        void ExecuteJump()
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
            _jumpCount++;
            _isJumping = true;
            _jumpBufferTimer = 0;
            _coyoteTimer = 0;
            Audio.AudioManager.Instance?.PlaySFX("player_jump", 0.05f);
        }

        void WallJump()
        {
            float jumpDirX = -_wallDirection * wallJumpForceX;
            _rb.linearVelocity = new Vector2(jumpDirX, wallJumpForceY);
            _facingDirection = -_wallDirection;
            _isWallSliding = false;
            _jumpBufferTimer = 0;
            _jumpCount = 1;
            Audio.AudioManager.Instance?.PlaySFX("player_jump", 0.05f);
        }

        #endregion

        #region 중력 보정

        void ApplyGravityModifiers()
        {
            if (_isWallSliding) return;

            if (_rb.linearVelocity.y < 0)
            {
                _rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallGravityMultiplier - 1) * Time.fixedDeltaTime;
            }
            else if (_rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
            {
                _rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpGravityMultiplier - 1) * Time.fixedDeltaTime;
            }
        }

        #endregion

        #region 벽 슬라이드

        void HandleWallSlide()
        {
            if (!HasWallJump) return;

            bool shouldSlide = _isTouchingWall && !_isGrounded && _moveInput == _wallDirection;
            _isWallSliding = shouldSlide;

            if (_isWallSliding)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x,
                    Mathf.Max(_rb.linearVelocity.y, -wallSlideSpeed));
                _jumpCount = 0;
            }
        }

        #endregion

        #region 대시

        void HandleDash()
        {
            if (!HasDash) return;
            if (_dashCooldownTimer > 0) return;
            if (!Input.GetKeyDown(KeyCode.LeftShift)) return;
            if (_moveInput == 0) return;

            StartDash();
        }

        void StartDash()
        {
            _isDashing = true;
            _dashTimer = dashDuration;
            _dashCooldownTimer = dashCooldown;
            _dashDirection = new Vector2(_facingDirection, 0f).normalized;
            _rb.gravityScale = 0f;

            _isInvincible = true;
            _invincibleTimer = dashInvincibleTime;
        }

        void DashMovement()
        {
            float dashSpeed = dashDistance / dashDuration;
            _rb.linearVelocity = _dashDirection * dashSpeed;
        }

        void EndDash()
        {
            _isDashing = false;
            _rb.gravityScale = 1f;
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x * 0.5f, 0f);
        }

        #endregion

        #region 외부 API

        public int CurrentHP => _currentHP;
        public int MaxHP => maxHP;

        public void LockInput(bool locked)
        {
            _inputLocked = locked;
            if (locked)
                _rb.linearVelocity = Vector2.zero;
        }

        public bool IsInvincible => _isInvincible;

        public void TakeDamage(int damage = 1, Vector2 knockbackDir = default)
        {
            if (_isInvincible || _currentHP <= 0) return;

            _currentHP = Mathf.Max(0, _currentHP - damage);
            OnHPChanged?.Invoke(_currentHP, maxHP);

            _isInvincible = true;
            _invincibleTimer = hitInvincibleTime;
            StartFlash();

            if (_animator != null)
                _animator.SetTrigger(AnimIsHit);

            if (knockbackDir != default)
                _rb.linearVelocity = knockbackDir * 6f;

            Audio.AudioManager.Instance?.PlaySFX("player_hit");
            UI.DamageNumber.Spawn(transform.position + Vector3.up * 0.8f, damage);

            if (_currentHP <= 0)
            {
                Die();
            }
        }

        public void Heal(int amount = 1)
        {
            if (_currentHP >= maxHP) return;
            _currentHP = Mathf.Min(maxHP, _currentHP + amount);
            OnHPChanged?.Invoke(_currentHP, maxHP);
            UI.DamageNumber.Spawn(transform.position + Vector3.up * 0.8f, amount, true);
        }

        public void ResetHP()
        {
            _currentHP = maxHP;
            OnHPChanged?.Invoke(_currentHP, maxHP);
        }

        void Die()
        {
            LockInput(true);

            if (_animator != null)
                _animator.SetTrigger(AnimIsDead);

            OnPlayerDied?.Invoke();
            GameManager.Instance?.OnGameOver();
        }

        void StartFlash()
        {
            _isFlashing = true;
            _flashTimer = hitInvincibleTime;
        }

        void UpdateAnimator()
        {
            if (_animator == null) return;

            float speed = Mathf.Abs(_moveInput) * (_isRunning ? runSpeed : walkSpeed);
            _animator.SetFloat(AnimSpeed, speed);
            _animator.SetBool(AnimIsGrounded, _isGrounded);
            _animator.SetFloat(AnimVelocityY, _rb.linearVelocity.y);
            _animator.SetBool(AnimIsDashing, _isDashing);
            _animator.SetBool(AnimIsWallSliding, _isWallSliding);

            if (_spriteRenderer != null && _moveInput != 0)
                _spriteRenderer.flipX = _moveInput < 0;
        }

        void UpdateFlash()
        {
            if (!_isFlashing || _spriteRenderer == null) return;

            _flashTimer -= Time.deltaTime;
            bool visible = Mathf.FloorToInt(_flashTimer * 10f) % 2 == 0;
            _spriteRenderer.enabled = visible;

            if (_flashTimer <= 0)
            {
                _isFlashing = false;
                _spriteRenderer.enabled = true;
            }
        }

        public void SyncAbilitiesFromSave()
        {
            if (GameManager.Instance == null) return;
            var save = GameManager.Instance.CurrentSave;
            HasDoubleJump = save.hasDoubleJump;
            HasWallJump = save.hasWallJump;
            HasDash = save.hasDash;
        }

        #endregion

        #region 기즈모

        void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, Vector2.right * wallCheckDistance);
            Gizmos.DrawRay(transform.position, Vector2.left * wallCheckDistance);
        }

        #endregion
    }
}
