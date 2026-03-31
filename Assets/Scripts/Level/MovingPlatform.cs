using UnityEngine;

namespace TheSSand.Level
{
    public class MovingPlatform : MonoBehaviour
    {
        [SerializeField] Vector3[] waypoints;
        [SerializeField] float speed = 2f;
        [SerializeField] float waitTime = 0.5f;
        [SerializeField] bool loop = true;

        int _currentIndex;
        int _direction = 1;
        float _waitTimer;
        Vector3 _lastPosition;
        Vector3 _startPos;

        void Start()
        {
            _startPos = transform.position;
            _lastPosition = transform.position;

            if (waypoints == null || waypoints.Length == 0)
                waypoints = new[] { Vector3.zero, Vector3.right * 4f };
        }

        void FixedUpdate()
        {
            if (waypoints == null || waypoints.Length < 2) return;

            if (_waitTimer > 0)
            {
                _waitTimer -= Time.fixedDeltaTime;
                _lastPosition = transform.position;
                return;
            }

            Vector3 target = _startPos + waypoints[_currentIndex];
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.fixedDeltaTime);

            if (Vector3.Distance(transform.position, target) < 0.01f)
            {
                _waitTimer = waitTime;
                AdvanceWaypoint();
            }

            _lastPosition = transform.position;
        }

        void AdvanceWaypoint()
        {
            if (loop)
            {
                _currentIndex = (_currentIndex + 1) % waypoints.Length;
            }
            else
            {
                _currentIndex += _direction;
                if (_currentIndex >= waypoints.Length || _currentIndex < 0)
                {
                    _direction *= -1;
                    _currentIndex += _direction * 2;
                }
            }
        }

        void OnCollisionEnter2D(Collision2D col)
        {
            if (col.collider.CompareTag("Player"))
                col.transform.SetParent(transform);
        }

        void OnCollisionExit2D(Collision2D col)
        {
            if (col.collider.CompareTag("Player"))
                col.transform.SetParent(null);
        }

        void OnDrawGizmosSelected()
        {
            if (waypoints == null || waypoints.Length == 0) return;

            Vector3 origin = Application.isPlaying ? _startPos : transform.position;
            Gizmos.color = Color.cyan;
            for (int i = 0; i < waypoints.Length; i++)
            {
                Vector3 wp = origin + waypoints[i];
                Gizmos.DrawSphere(wp, 0.15f);
                int next = (i + 1) % waypoints.Length;
                Gizmos.DrawLine(wp, origin + waypoints[next]);
            }
        }
    }
}
