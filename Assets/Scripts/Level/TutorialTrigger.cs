using UnityEngine;
using TMPro;

namespace TheSSand.Level
{
    public class TutorialTrigger : MonoBehaviour
    {
        [SerializeField] string tutorialKey;
        [SerializeField] string message;
        [SerializeField] KeyCode requiredKey = KeyCode.None;
        [SerializeField] bool showOnce = true;

        [Header("UI 참조")]
        [SerializeField] GameObject tutorialPopup;
        [SerializeField] TextMeshProUGUI tutorialText;

        bool _triggered;
        bool _playerInRange;

        void Start()
        {
            if (tutorialPopup != null)
                tutorialPopup.SetActive(false);
        }

        void Update()
        {
            if (!_playerInRange) return;

            if (requiredKey != KeyCode.None && Input.GetKeyDown(requiredKey))
                HideTutorial();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            if (showOnce && _triggered) return;

            _playerInRange = true;
            _triggered = true;
            ShowTutorial();
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = false;
            HideTutorial();
        }

        void ShowTutorial()
        {
            if (tutorialPopup != null)
                tutorialPopup.SetActive(true);
            if (tutorialText != null)
                tutorialText.text = message;
        }

        void HideTutorial()
        {
            if (tutorialPopup != null)
                tutorialPopup.SetActive(false);
            _playerInRange = false;
        }
    }
}
