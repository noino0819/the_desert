using UnityEngine;
using TheSSand.Scene;

namespace TheSSand.Level
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class SceneTransitionTrigger : MonoBehaviour
    {
        [SerializeField] string targetSceneName;
        [SerializeField] bool requireInteraction;

        bool _playerInRange;

        void Update()
        {
            if (!requireInteraction) return;
            if (_playerInRange && Input.GetKeyDown(KeyCode.E))
                Transition();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            if (requireInteraction)
            {
                _playerInRange = true;
                return;
            }

            Transition();
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                _playerInRange = false;
        }

        void Transition()
        {
            if (string.IsNullOrEmpty(targetSceneName)) return;
            SceneTransitionManager.Instance?.LoadScene(targetSceneName);
        }
    }
}
