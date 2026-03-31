using UnityEngine;
using TheSSand.Player;

namespace TheSSand.Level
{
    public enum HazardType { Spike, Lava, Pit, Poison, Custom }

    [RequireComponent(typeof(Collider2D))]
    public class HazardZone : MonoBehaviour
    {
        [SerializeField] HazardType hazardType = HazardType.Spike;
        [SerializeField] int damage = 1;
        [SerializeField] float knockbackForce = 8f;
        [SerializeField] float damageCooldown = 1f;
        [SerializeField] bool instantKill;
        [SerializeField] bool continuousDamage;

        float _lastDamageTime = -999f;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            TryDamage(other);
        }

        void OnTriggerStay2D(Collider2D other)
        {
            if (!continuousDamage) return;
            if (!other.CompareTag("Player")) return;
            TryDamage(other);
        }

        void TryDamage(Collider2D playerCol)
        {
            if (Time.time - _lastDamageTime < damageCooldown) return;

            var player = playerCol.GetComponentInParent<PlayerController>();
            if (player == null || player.IsInvincible) return;

            _lastDamageTime = Time.time;

            int dmg = instantKill ? player.MaxHP : damage;

            Vector2 knockback = Vector2.zero;
            if (knockbackForce > 0)
            {
                Vector2 dir = (playerCol.transform.position - transform.position).normalized;
                dir.y = Mathf.Max(dir.y, 0.4f);
                knockback = dir.normalized * knockbackForce;
            }

            player.TakeDamage(dmg, knockback);

            string sfx = hazardType switch
            {
                HazardType.Spike => "hazard_spike",
                HazardType.Lava => "hazard_lava",
                HazardType.Poison => "hazard_poison",
                _ => "hazard_hit"
            };
            Audio.AudioManager.Instance?.PlaySFX(sfx);
        }

        void Reset()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }
    }
}
