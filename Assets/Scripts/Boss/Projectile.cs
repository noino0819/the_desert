using UnityEngine;

namespace TheSSand.Boss
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] float lifetime = 5f;

        float _timer;

        void OnEnable()
        {
            _timer = 0f;
        }

        void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= lifetime)
                gameObject.SetActive(false);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                var boss = GetComponentInParent<BossBase>();
                boss?.DamagePlayer();
                gameObject.SetActive(false);
            }

            if (other.CompareTag("Ground"))
                gameObject.SetActive(false);
        }

        void OnBecameInvisible()
        {
            gameObject.SetActive(false);
        }
    }
}
