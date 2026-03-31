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

        [Header("애니메이션")]
        [SerializeField] Animator npcAnimator;

        bool _playerInRange;
        bool _isTalking;
        static readonly int AnimIsTalking = Animator.StringToHash("IsTalking");

        void Start()
        {
            var col = GetComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = interactRadius;

            if (speechBubbleIndicator != null)
                speechBubbleIndicator.SetActive(false);

            if (npcAnimator == null)
                npcAnimator = GetComponentInChildren<Animator>();
        }

        void OnEnable()
        {
            if (DialogueManager.Instance != null)
                DialogueManager.Instance.OnDialogueEnded += OnDialogueEnded;
        }

        void OnDisable()
        {
            if (DialogueManager.Instance != null)
                DialogueManager.Instance.OnDialogueEnded -= OnDialogueEnded;
        }

        void Update()
        {
            if (!_playerInRange) return;
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive) return;

            if (Input.GetKeyDown(KeyCode.E))
            {
                DialogueManager.Instance?.StartDialogue(npcId);
                SetTalking(true);
            }
        }

        void OnDialogueEnded()
        {
            SetTalking(false);
        }

        void SetTalking(bool talking)
        {
            _isTalking = talking;
            if (npcAnimator != null)
                npcAnimator.SetBool(AnimIsTalking, talking);
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
