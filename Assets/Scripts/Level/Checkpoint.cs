using UnityEngine;
using TheSSand.Player;

namespace TheSSand.Level
{
    public class Checkpoint : MonoBehaviour
    {
        public static Vector3? LastCheckpointPosition { get; private set; }
        public static string LastCheckpointScene { get; private set; }

        [SerializeField] Sprite activeSprite;
        [SerializeField] Sprite inactiveSprite;
        [SerializeField] GameObject activateEffect;
        [SerializeField] bool healOnActivate = true;

        SpriteRenderer _sr;
        bool _activated;

        static Checkpoint _currentCheckpoint;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        void Start()
        {
            if (_sr != null && inactiveSprite != null)
                _sr.sprite = inactiveSprite;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (_activated) return;
            if (!other.CompareTag("Player")) return;

            Activate(other);
        }

        void Activate(Collider2D playerCol)
        {
            if (_currentCheckpoint != null && _currentCheckpoint != this)
                _currentCheckpoint.Deactivate();

            _activated = true;
            _currentCheckpoint = this;

            LastCheckpointPosition = transform.position;
            LastCheckpointScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            if (_sr != null && activeSprite != null)
                _sr.sprite = activeSprite;

            if (activateEffect != null)
                Instantiate(activateEffect, transform.position, Quaternion.identity);

            if (healOnActivate)
            {
                var player = playerCol.GetComponentInParent<PlayerController>();
                player?.Heal(1);
            }

            Audio.AudioManager.Instance?.PlaySFX("checkpoint");
        }

        void Deactivate()
        {
            _activated = false;
            if (_sr != null && inactiveSprite != null)
                _sr.sprite = inactiveSprite;
        }

        public static void RespawnPlayer()
        {
            if (LastCheckpointPosition == null) return;

            var player = FindFirstObjectByType<PlayerController>();
            if (player == null) return;

            player.transform.position = LastCheckpointPosition.Value;
            player.ResetHP();
        }

        public static void ClearCheckpoint()
        {
            LastCheckpointPosition = null;
            LastCheckpointScene = null;
            _currentCheckpoint = null;
        }
    }
}
