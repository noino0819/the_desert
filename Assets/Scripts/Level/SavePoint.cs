using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TheSSand.Core;

namespace TheSSand.Level
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class SavePoint : MonoBehaviour
    {
        [SerializeField] float interactRadius = 1.5f;
        [SerializeField] Animator fairyAnimator;

        [Header("저장 UI")]
        [SerializeField] GameObject savePanel;
        [SerializeField] Button[] slotButtons;
        [SerializeField] TextMeshProUGUI[] slotTexts;
        [SerializeField] Button cancelButton;

        [Header("상호작용 표시")]
        [SerializeField] GameObject interactPrompt;

        bool _playerInRange;

        void Start()
        {
            var col = GetComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = interactRadius;

            if (savePanel != null) savePanel.SetActive(false);
            if (interactPrompt != null) interactPrompt.SetActive(false);

            cancelButton?.onClick.AddListener(CloseSavePanel);

            for (int i = 0; i < slotButtons.Length; i++)
            {
                int slot = i;
                slotButtons[i]?.onClick.AddListener(() => SaveToSlot(slot));
            }
        }

        void Update()
        {
            if (!_playerInRange) return;
            if (savePanel != null && savePanel.activeSelf) return;

            if (Input.GetKeyDown(KeyCode.E))
                OpenSavePanel();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = true;
            if (interactPrompt != null) interactPrompt.SetActive(true);
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = false;
            if (interactPrompt != null) interactPrompt.SetActive(false);
            CloseSavePanel();
        }

        void OpenSavePanel()
        {
            if (savePanel == null || SaveManager.Instance == null) return;

            savePanel.SetActive(true);
            RefreshSlotInfo();

            if (fairyAnimator != null)
                fairyAnimator.SetTrigger("Talk");
        }

        void RefreshSlotInfo()
        {
            for (int i = 0; i < slotTexts.Length; i++)
            {
                var data = SaveManager.Instance.PeekSlot(i);
                slotTexts[i].text = data != null
                    ? $"슬롯 {i + 1}: Ch.{data.currentChapter} | {data.playerName}"
                    : $"슬롯 {i + 1}: 비어있음";
            }
        }

        void SaveToSlot(int slot)
        {
            var gm = GameManager.Instance;
            if (gm == null || SaveManager.Instance == null) return;

            gm.CurrentSave.playerPosX = transform.position.x;
            gm.CurrentSave.playerPosY = transform.position.y;
            gm.CurrentSave.currentSceneId =
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            SaveManager.Instance.Save(slot);
            RefreshSlotInfo();

            Debug.Log($"[SavePoint] 슬롯 {slot} 저장 완료");
        }

        void CloseSavePanel()
        {
            if (savePanel != null)
                savePanel.SetActive(false);
        }
    }
}
