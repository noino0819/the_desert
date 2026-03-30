using System;
using System.Collections;
using UnityEngine;
using TMPro;

namespace TheSSand.UI
{
    public class NarrationUI : MonoBehaviour
    {
        public static NarrationUI Instance { get; private set; }

        [SerializeField] GameObject narrationPanel;
        [SerializeField] TextMeshProUGUI narrationText;
        [SerializeField] float typingSpeed = 0.04f;
        [SerializeField] float displayDuration = 3f;
        [SerializeField] CanvasGroup canvasGroup;

        Coroutine _currentRoutine;
        bool _isShowing;
        Action _onComplete;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (narrationPanel != null)
                narrationPanel.SetActive(false);
        }

        public bool IsShowing => _isShowing;

        public void ShowNarration(string text, Action onComplete = null)
        {
            if (_currentRoutine != null)
                StopCoroutine(_currentRoutine);
            _onComplete = onComplete;
            _currentRoutine = StartCoroutine(NarrationRoutine(text));
        }

        public void ShowNarrationImmediate(string text)
        {
            if (narrationPanel != null) narrationPanel.SetActive(true);
            if (narrationText != null) narrationText.text = text;
            _isShowing = true;
        }

        public void HideNarration()
        {
            if (_currentRoutine != null)
                StopCoroutine(_currentRoutine);
            if (narrationPanel != null) narrationPanel.SetActive(false);
            _isShowing = false;
        }

        public void ShowNarrationSequence(string[] texts, Action onComplete = null)
        {
            if (_currentRoutine != null)
                StopCoroutine(_currentRoutine);
            _onComplete = onComplete;
            _currentRoutine = StartCoroutine(SequenceRoutine(texts));
        }

        IEnumerator NarrationRoutine(string text)
        {
            _isShowing = true;
            if (narrationPanel != null) narrationPanel.SetActive(true);
            if (narrationText != null) narrationText.text = "";

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                float fadeIn = 0f;
                while (fadeIn < 0.3f)
                {
                    fadeIn += Time.deltaTime;
                    canvasGroup.alpha = fadeIn / 0.3f;
                    yield return null;
                }
                canvasGroup.alpha = 1f;
            }

            foreach (char c in text)
            {
                narrationText.text += c;
                if (Input.GetKey(KeyCode.Space))
                    yield return null;
                else
                    yield return new WaitForSeconds(typingSpeed);
            }

            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

            if (canvasGroup != null)
            {
                float fadeOut = 0f;
                while (fadeOut < 0.3f)
                {
                    fadeOut += Time.deltaTime;
                    canvasGroup.alpha = 1f - fadeOut / 0.3f;
                    yield return null;
                }
            }

            if (narrationPanel != null) narrationPanel.SetActive(false);
            _isShowing = false;
            _onComplete?.Invoke();
            _onComplete = null;
        }

        IEnumerator SequenceRoutine(string[] texts)
        {
            _isShowing = true;
            if (narrationPanel != null) narrationPanel.SetActive(true);

            foreach (string text in texts)
            {
                if (narrationText != null) narrationText.text = "";

                foreach (char c in text)
                {
                    narrationText.text += c;
                    if (Input.GetKey(KeyCode.Space))
                        yield return null;
                    else
                        yield return new WaitForSeconds(typingSpeed);
                }

                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
                yield return null;
            }

            if (narrationPanel != null) narrationPanel.SetActive(false);
            _isShowing = false;
            _onComplete?.Invoke();
            _onComplete = null;
        }
    }
}
