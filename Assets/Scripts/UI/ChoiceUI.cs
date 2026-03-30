using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TheSSand.UI
{
    public class ChoiceUI : MonoBehaviour
    {
        public static ChoiceUI Instance { get; private set; }

        [SerializeField] GameObject choicePanel;
        [SerializeField] Transform choiceButtonContainer;
        [SerializeField] GameObject choiceButtonPrefab;
        [SerializeField] TextMeshProUGUI timerText;
        [SerializeField] Image timerFillImage;

        Coroutine _timerCoroutine;
        List<GameObject> _spawnedButtons = new();
        Action<int> _onChoiceSelected;
        int _defaultChoice;
        bool _isActive;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (choicePanel != null)
                choicePanel.SetActive(false);
        }

        public bool IsActive => _isActive;

        public void ShowChoices(string[] options, Action<int> onSelected, float timer = -1, int defaultChoice = 0)
        {
            _isActive = true;
            _onChoiceSelected = onSelected;
            _defaultChoice = defaultChoice;

            ClearButtons();
            if (choicePanel != null) choicePanel.SetActive(true);

            for (int i = 0; i < options.Length; i++)
            {
                GameObject btnObj;
                if (choiceButtonPrefab != null)
                    btnObj = Instantiate(choiceButtonPrefab, choiceButtonContainer);
                else
                {
                    btnObj = new GameObject($"Choice_{i}");
                    btnObj.transform.SetParent(choiceButtonContainer, false);
                    btnObj.AddComponent<RectTransform>();
                    btnObj.AddComponent<Image>().color = new Color(0.65f, 0.5f, 0.3f);
                    btnObj.AddComponent<Button>();
                    var textObj = new GameObject("Label");
                    textObj.transform.SetParent(btnObj.transform, false);
                    var tmp = textObj.AddComponent<TextMeshProUGUI>();
                    tmp.alignment = TextAlignmentOptions.Center;
                    var rt = tmp.rectTransform;
                    rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                }

                var label = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null) label.text = options[i];

                int captured = i;
                var button = btnObj.GetComponent<Button>();
                if (button != null)
                    button.onClick.AddListener(() => SelectChoice(captured));

                _spawnedButtons.Add(btnObj);
            }

            if (timer > 0)
                _timerCoroutine = StartCoroutine(TimerCoroutine(timer));
            else
            {
                if (timerText != null) timerText.gameObject.SetActive(false);
                if (timerFillImage != null) timerFillImage.gameObject.SetActive(false);
            }
        }

        void SelectChoice(int index)
        {
            if (!_isActive) return;
            _isActive = false;

            if (_timerCoroutine != null)
                StopCoroutine(_timerCoroutine);

            ClearButtons();
            if (choicePanel != null) choicePanel.SetActive(false);

            _onChoiceSelected?.Invoke(index);
            _onChoiceSelected = null;
        }

        IEnumerator TimerCoroutine(float duration)
        {
            if (timerText != null) timerText.gameObject.SetActive(true);
            if (timerFillImage != null) timerFillImage.gameObject.SetActive(true);

            float remaining = duration;
            while (remaining > 0)
            {
                remaining -= Time.deltaTime;
                if (timerText != null)
                    timerText.text = Mathf.CeilToInt(remaining).ToString();
                if (timerFillImage != null)
                    timerFillImage.fillAmount = remaining / duration;
                yield return null;
            }

            SelectChoice(_defaultChoice);
        }

        void ClearButtons()
        {
            foreach (var btn in _spawnedButtons)
            {
                if (btn != null) Destroy(btn);
            }
            _spawnedButtons.Clear();
        }
    }
}
