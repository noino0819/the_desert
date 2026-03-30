using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheSSand.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("BGM")]
        [SerializeField] AudioSource bgmSourceA;
        [SerializeField] AudioSource bgmSourceB;
        [SerializeField] float crossFadeDuration = 1.5f;

        [Header("SFX")]
        [SerializeField] AudioSource sfxSource;
        [SerializeField] int sfxPoolSize = 8;

        [Header("볼륨")]
        [SerializeField, Range(0, 1)] float masterVolume = 1f;
        [SerializeField, Range(0, 1)] float bgmVolume = 0.7f;
        [SerializeField, Range(0, 1)] float sfxVolume = 1f;

        readonly Dictionary<string, AudioClip> _clipCache = new();
        AudioSource _activeBgmSource;
        AudioSource _inactiveBgmSource;
        List<AudioSource> _sfxPool;
        Coroutine _crossFadeCoroutine;

        public float MasterVolume
        {
            get => masterVolume;
            set { masterVolume = Mathf.Clamp01(value); UpdateVolumes(); }
        }
        public float BgmVolume
        {
            get => bgmVolume;
            set { bgmVolume = Mathf.Clamp01(value); UpdateVolumes(); }
        }
        public float SfxVolume
        {
            get => sfxVolume;
            set { sfxVolume = Mathf.Clamp01(value); UpdateVolumes(); }
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitAudioSources();
        }

        void InitAudioSources()
        {
            if (bgmSourceA == null)
            {
                var objA = new GameObject("BGM_A");
                objA.transform.SetParent(transform);
                bgmSourceA = objA.AddComponent<AudioSource>();
                bgmSourceA.loop = true;
                bgmSourceA.playOnAwake = false;
            }
            if (bgmSourceB == null)
            {
                var objB = new GameObject("BGM_B");
                objB.transform.SetParent(transform);
                bgmSourceB = objB.AddComponent<AudioSource>();
                bgmSourceB.loop = true;
                bgmSourceB.playOnAwake = false;
            }

            _activeBgmSource = bgmSourceA;
            _inactiveBgmSource = bgmSourceB;

            _sfxPool = new List<AudioSource>();
            if (sfxSource == null)
            {
                var sfxObj = new GameObject("SFX_Main");
                sfxObj.transform.SetParent(transform);
                sfxSource = sfxObj.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }
            _sfxPool.Add(sfxSource);

            for (int i = 1; i < sfxPoolSize; i++)
            {
                var obj = new GameObject($"SFX_{i}");
                obj.transform.SetParent(transform);
                var src = obj.AddComponent<AudioSource>();
                src.playOnAwake = false;
                _sfxPool.Add(src);
            }

            UpdateVolumes();
        }

        #region BGM

        public void PlayBGM(string clipId)
        {
            var clip = LoadClip($"Audio/BGM/{clipId}");
            if (clip == null) return;

            if (_activeBgmSource.clip == clip && _activeBgmSource.isPlaying)
                return;

            CrossFade(clip);
        }

        public void CrossFade(AudioClip newClip, float duration = -1)
        {
            if (duration < 0) duration = crossFadeDuration;

            if (_crossFadeCoroutine != null)
                StopCoroutine(_crossFadeCoroutine);

            _crossFadeCoroutine = StartCoroutine(CrossFadeCoroutine(newClip, duration));
        }

        IEnumerator CrossFadeCoroutine(AudioClip newClip, float duration)
        {
            var fadeOut = _activeBgmSource;
            var fadeIn = _inactiveBgmSource;

            fadeIn.clip = newClip;
            fadeIn.volume = 0f;
            fadeIn.Play();

            float startVolOut = fadeOut.volume;
            float targetVol = bgmVolume * masterVolume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                fadeOut.volume = Mathf.Lerp(startVolOut, 0f, t);
                fadeIn.volume = Mathf.Lerp(0f, targetVol, t);
                yield return null;
            }

            fadeOut.Stop();
            fadeOut.volume = 0f;
            fadeIn.volume = targetVol;

            _activeBgmSource = fadeIn;
            _inactiveBgmSource = fadeOut;
            _crossFadeCoroutine = null;
        }

        public void StopBGM(float fadeOutDuration = 1f)
        {
            if (_crossFadeCoroutine != null)
                StopCoroutine(_crossFadeCoroutine);
            _crossFadeCoroutine = StartCoroutine(FadeOutBGM(fadeOutDuration));
        }

        IEnumerator FadeOutBGM(float duration)
        {
            float start = _activeBgmSource.volume;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _activeBgmSource.volume = Mathf.Lerp(start, 0f, elapsed / duration);
                yield return null;
            }
            _activeBgmSource.Stop();
            _crossFadeCoroutine = null;
        }

        public void PauseBGM() => _activeBgmSource.Pause();
        public void ResumeBGM() => _activeBgmSource.UnPause();

        #endregion

        #region SFX

        public void PlaySFX(string clipId, float pitchVariation = 0f)
        {
            var clip = LoadClip($"Audio/SFX/{clipId}");
            if (clip == null) return;

            var source = GetAvailableSfxSource();
            source.pitch = 1f + UnityEngine.Random.Range(-pitchVariation, pitchVariation);
            source.volume = sfxVolume * masterVolume;
            source.PlayOneShot(clip);
        }

        public void PlaySFX(AudioClip clip, float pitchVariation = 0f)
        {
            if (clip == null) return;
            var source = GetAvailableSfxSource();
            source.pitch = 1f + UnityEngine.Random.Range(-pitchVariation, pitchVariation);
            source.volume = sfxVolume * masterVolume;
            source.PlayOneShot(clip);
        }

        AudioSource GetAvailableSfxSource()
        {
            foreach (var src in _sfxPool)
            {
                if (!src.isPlaying) return src;
            }
            return _sfxPool[0];
        }

        #endregion

        #region 볼륨

        void UpdateVolumes()
        {
            if (_activeBgmSource != null && _activeBgmSource.isPlaying)
                _activeBgmSource.volume = bgmVolume * masterVolume;

            foreach (var src in _sfxPool)
                src.volume = sfxVolume * masterVolume;
        }

        #endregion

        #region 캐시

        AudioClip LoadClip(string path)
        {
            if (_clipCache.TryGetValue(path, out var cached))
                return cached;

            var clip = Resources.Load<AudioClip>(path);
            if (clip != null)
                _clipCache[path] = clip;
            else
                Debug.LogWarning($"[AudioManager] 오디오 클립 없음: {path}");

            return clip;
        }

        #endregion
    }
}
