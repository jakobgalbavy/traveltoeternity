using UnityEngine;
using UnityEngine.UI;

namespace DeepTransit.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class StarfieldBackground : MonoBehaviour
    {
        [SerializeField] int   starCount  = 130;
        [SerializeField] float minSize    = 1.5f;
        [SerializeField] float maxSize    = 4.5f;
        [SerializeField] float driftSpeed = 10f; // reference-resolution pixels per second

        RectTransform[] _stars;

        void Awake()
        {
            var rt = GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            _stars = new RectTransform[starCount];
            for (int i = 0; i < starCount; i++)
            {
                var go = new GameObject($"Star{i}");
                go.transform.SetParent(transform, false);

                var img = go.AddComponent<Image>();
                float b = Random.Range(0.45f, 1f);
                // slight blue-white tint for distant stars
                img.color = new Color(b * 0.82f, b * 0.90f, b, Random.Range(0.35f, 0.95f));
                img.raycastTarget = false;

                var starRt = go.GetComponent<RectTransform>();
                float size = Random.Range(minSize, maxSize);
                starRt.sizeDelta        = new Vector2(size, size);
                starRt.anchorMin        = starRt.anchorMax = new Vector2(0.5f, 0.5f);
                starRt.anchoredPosition = new Vector2(
                    Random.Range(-540f, 540f),
                    Random.Range(-960f, 960f)
                );
                _stars[i] = starRt;
            }
        }

        void Update()
        {
            float delta = driftSpeed * Time.deltaTime;
            for (int i = 0; i < _stars.Length; i++)
            {
                var pos = _stars[i].anchoredPosition;
                pos.y -= delta;
                if (pos.y < -960f) pos.y += 1920f;
                _stars[i].anchoredPosition = pos;
            }
        }
    }
}
