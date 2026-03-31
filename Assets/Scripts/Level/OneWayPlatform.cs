using UnityEngine;

namespace TheSSand.Level
{
    [RequireComponent(typeof(PlatformEffector2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class OneWayPlatform : MonoBehaviour
    {
        [SerializeField] float dropDuration = 0.3f;

        PlatformEffector2D _effector;
        BoxCollider2D _collider;
        float _dropTimer;
        bool _isDropping;

        void Awake()
        {
            _effector = GetComponent<PlatformEffector2D>();
            _collider = GetComponent<BoxCollider2D>();
        }

        void Reset()
        {
            var eff = GetComponent<PlatformEffector2D>();
            eff.useOneWay = true;
            eff.surfaceArc = 180f;

            var col = GetComponent<BoxCollider2D>();
            col.usedByEffector = true;
        }

        void Start()
        {
            _effector.useOneWay = true;
            _collider.usedByEffector = true;
        }

        void Update()
        {
            if (_isDropping)
            {
                _dropTimer -= Time.deltaTime;
                if (_dropTimer <= 0f)
                {
                    _isDropping = false;
                    _effector.rotationalOffset = 0f;
                }
            }
        }

        void OnCollisionStay2D(Collision2D col)
        {
            if (!col.collider.CompareTag("Player")) return;
            if (_isDropping) return;

            if (Input.GetAxisRaw("Vertical") < -0.5f && Input.GetKeyDown(KeyCode.Space))
                Drop();
        }

        void Drop()
        {
            _isDropping = true;
            _dropTimer = dropDuration;
            _effector.rotationalOffset = 180f;
        }
    }
}
