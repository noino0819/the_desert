using UnityEngine;
using TMPro;

namespace TheSSand.UI
{
    public class DamageNumber : MonoBehaviour
    {
        static DamageNumber _prefabInstance;
        static Transform _poolParent;

        [SerializeField] TextMeshPro text;
        [SerializeField] float floatSpeed = 1.5f;
        [SerializeField] float lifetime = 0.8f;
        [SerializeField] float scaleStart = 0.5f;
        [SerializeField] float scalePeak = 1.2f;
        [SerializeField] AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        float _timer;
        Color _baseColor;

        void Awake()
        {
            if (text == null) text = GetComponent<TextMeshPro>();
        }

        public static void Spawn(Vector3 worldPos, int amount, bool isHeal = false)
        {
            if (_prefabInstance == null)
            {
                var prefab = Resources.Load<DamageNumber>("Prefabs/DamageNumber");
                if (prefab == null)
                {
                    SpawnFallback(worldPos, amount, isHeal);
                    return;
                }
                _prefabInstance = prefab;
            }

            if (_poolParent == null)
            {
                _poolParent = new GameObject("DamageNumbers").transform;
            }

            var instance = Instantiate(_prefabInstance, worldPos + Random.insideUnitSphere * 0.2f,
                Quaternion.identity, _poolParent);
            instance.Init(amount, isHeal);
        }

        void Init(int amount, bool isHeal)
        {
            if (text == null) return;

            text.text = isHeal ? $"+{amount}" : $"{amount}";
            _baseColor = isHeal ? new Color(0.2f, 1f, 0.3f) : new Color(1f, 0.3f, 0.2f);
            text.color = _baseColor;

            _timer = 0f;
            transform.localScale = Vector3.one * scaleStart;
        }

        void Update()
        {
            _timer += Time.deltaTime;
            float t = _timer / lifetime;

            transform.position += Vector3.up * floatSpeed * Time.deltaTime;

            float curveVal = scaleCurve.Evaluate(t);
            float scale = Mathf.Lerp(scaleStart, scalePeak, curveVal);
            if (t > 0.6f) scale *= Mathf.Lerp(1f, 0.5f, (t - 0.6f) / 0.4f);
            transform.localScale = Vector3.one * scale;

            if (text != null)
            {
                var c = _baseColor;
                c.a = 1f - Mathf.Clamp01((t - 0.5f) / 0.5f);
                text.color = c;
            }

            if (_timer >= lifetime)
                Destroy(gameObject);
        }

        static void SpawnFallback(Vector3 worldPos, int amount, bool isHeal)
        {
            var go = new GameObject("DmgNum");
            go.transform.position = worldPos + Vector3.up * 0.5f;

            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text = isHeal ? $"+{amount}" : $"{amount}";
            tmp.color = isHeal ? Color.green : Color.red;
            tmp.fontSize = 5f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.sortingOrder = 100;

            var dn = go.AddComponent<DamageNumber>();
            dn.text = tmp;
            dn._baseColor = tmp.color;
            dn._timer = 0f;
        }
    }
}
