using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TheSSand.Scene;

namespace TheSSand.UI
{
    /// <summary>
    /// 엔딩 후 크레딧 스크롤 — 스토리북 테마에 맞는 연출
    /// </summary>
    public class CreditsUI : MonoBehaviour
    {
        [Header("크레딧 컨텐츠")]
        [SerializeField] RectTransform creditsContent;
        [SerializeField] float scrollSpeed = 40f;
        [SerializeField] float startDelay = 1f;
        [SerializeField] float endDelay = 3f;

        [Header("텍스트")]
        [SerializeField] TextMeshProUGUI creditsText;

        [Header("배경")]
        [SerializeField] Image backgroundImage;
        [SerializeField] Color backgroundColor = new(0.1f, 0.08f, 0.06f);

        [Header("Skip")]
        [SerializeField] TextMeshProUGUI skipHintText;
        [SerializeField] float skipHoldTime = 1.5f;

        float _skipTimer;
        bool _isScrolling;
        float _totalScrollDistance;

        static readonly string[] CreditLines =
        {
            "<size=48>The SSand</size>",
            "<size=28>사막의 진실</size>",
            "",
            "",
            "<size=24>— 제작 —</size>",
            "",
            "기획 · 시나리오 · 아트",
            "<size=32>noino</size>",
            "",
            "",
            "<size=24>— 프로그래밍 —</size>",
            "",
            "게임 시스템 · 보스 전투 · 카드배틀",
            "대화 시스템 · 퀘스트 · UI",
            "",
            "",
            "<size=24>— 음악 · 사운드 —</size>",
            "",
            "BGM · SFX",
            "",
            "",
            "<size=24>— 스토리 —</size>",
            "",
            "1회차: 할아버지의 동화",
            "2회차: 손자의 진실",
            "",
            "네 개의 오아시스",
            "네 명의 수호신",
            "하나의 거짓말",
            "",
            "",
            "<size=24>— 특별 감사 —</size>",
            "",
            "플레이해주신 모든 분들께",
            "감사합니다.",
            "",
            "",
            "",
            "<size=20>이것이 The SSand의 이야기입니다.</size>",
            "",
            "",
            "",
            "",
        };

        void Start()
        {
            if (backgroundImage != null)
                backgroundImage.color = backgroundColor;

            if (creditsText != null)
                creditsText.text = string.Join("\n", CreditLines);

            if (skipHintText != null)
                skipHintText.text = "Space 길게 누르기 — 건너뛰기";

            StartCoroutine(ScrollCredits());
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.Space))
            {
                _skipTimer += Time.unscaledDeltaTime;
                if (_skipTimer >= skipHoldTime)
                    FinishCredits();
            }
            else
            {
                _skipTimer = 0f;
            }
        }

        IEnumerator ScrollCredits()
        {
            yield return new WaitForSecondsRealtime(startDelay);

            _isScrolling = true;

            if (creditsContent == null) yield break;

            Vector2 startPos = creditsContent.anchoredPosition;
            float contentHeight = creditsContent.rect.height;
            float viewportHeight = 1080f;
            _totalScrollDistance = contentHeight + viewportHeight;

            float elapsed = 0f;
            float duration = _totalScrollDistance / scrollSpeed;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float y = startPos.y + (elapsed / duration) * _totalScrollDistance;
                creditsContent.anchoredPosition = new Vector2(startPos.x, y);
                yield return null;
            }

            _isScrolling = false;
            yield return new WaitForSecondsRealtime(endDelay);
            FinishCredits();
        }

        void FinishCredits()
        {
            StopAllCoroutines();
            SceneTransitionManager.Instance?.LoadScene("SCN_TitleMenu");
        }
    }
}
