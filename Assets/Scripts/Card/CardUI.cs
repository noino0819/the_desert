using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace TheSSand.Card
{
    /// <summary>
    /// 개별 카드 UI — 클릭으로 플레이, 호버 시 확대
    /// </summary>
    public class CardUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("카드 요소")]
        [SerializeField] Image artworkImage;
        [SerializeField] Image cardFrame;
        [SerializeField] TextMeshProUGUI nameText;
        [SerializeField] TextMeshProUGUI descText;
        [SerializeField] TextMeshProUGUI costText;
        [SerializeField] Image typeIcon;

        [Header("타입별 프레임 색상")]
        [SerializeField] Color attackColor = new(0.9f, 0.3f, 0.3f);
        [SerializeField] Color defenseColor = new(0.3f, 0.6f, 0.9f);
        [SerializeField] Color specialColor = new(0.9f, 0.8f, 0.3f);

        [Header("호버 효과")]
        [SerializeField] float hoverScale = 1.15f;
        [SerializeField] float hoverYOffset = 30f;

        CardData _data;
        CardBattleManager _manager;
        RectTransform _rt;
        Vector2 _originalPos;
        Vector3 _originalScale;
        bool _isHovered;

        public void Setup(CardData data, CardBattleManager manager)
        {
            _data = data;
            _manager = manager;
            _rt = GetComponent<RectTransform>();
            _originalScale = transform.localScale;

            if (nameText != null) nameText.text = data.cardName;
            if (costText != null) costText.text = data.energyCost.ToString();

            bool isNG = TheSSand.Core.GameManager.Instance != null &&
                        TheSSand.Core.GameManager.Instance.CurrentSave.isNewGame2;
            if (descText != null) descText.text = data.GetActiveDescription(isNG);

            if (artworkImage != null && data.artwork != null)
                artworkImage.sprite = data.artwork;

            if (cardFrame != null)
            {
                cardFrame.color = data.cardType switch
                {
                    CardType.Attack => attackColor,
                    CardType.Defense => defenseColor,
                    CardType.Special => specialColor,
                    _ => Color.white
                };
            }

            UpdateInteractable();
        }

        void Update()
        {
            UpdateInteractable();
        }

        void UpdateInteractable()
        {
            if (_data == null || _manager == null) return;

            bool canPlay = _manager.IsPlayerTurn &&
                           _manager.CurrentEnergy >= _data.energyCost;

            var group = GetComponent<CanvasGroup>();
            if (group != null)
                group.alpha = canPlay ? 1f : 0.5f;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_data == null || _manager == null) return;
            _manager.TryPlayCard(_data);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            if (_rt == null) return;

            if (_originalPos == Vector2.zero)
                _originalPos = _rt.anchoredPosition;

            transform.localScale = _originalScale * hoverScale;
            _rt.anchoredPosition = _originalPos + new Vector2(0, hoverYOffset);
            transform.SetAsLastSibling();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            if (_rt == null) return;

            transform.localScale = _originalScale;
            _rt.anchoredPosition = _originalPos;
        }
    }
}
