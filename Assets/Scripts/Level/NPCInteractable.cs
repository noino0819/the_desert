using UnityEngine;
using TheSSand.Dialogue;

namespace TheSSand.Level
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class NPCInteractable : MonoBehaviour
    {
        [SerializeField] string npcId;
        [SerializeField] float interactRadius = 2f;

        [Header("말풍선 UI")]
        [SerializeField] GameObject speechBubbleIndicator;

        bool _playerInRange;

        void Start()
        {
            var col = GetComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = interactRadius;

            if (speechBubbleIndicator != null)
                speechBubbleIndicator.SetActive(false);
        }

        void Update()
        {
            if (!_playerInRange) return;
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive) return;

            if (Input.GetKeyDown(KeyCode.E))
            {
                DialogueManager.Instance?.StartDialogue(npcId);
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = true;
            if (speechBubbleIndicator != null)
                speechBubbleIndicator.SetActive(true);
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = false;
            if (speechBubbleIndicator != null)
                speechBubbleIndicator.SetActive(false);
        }
    }
}
