using UnityEngine;
using UnityEngine.UI;

namespace DeepTransit.UI
{
    [RequireComponent(typeof(Slider))]
    public class SliderPulse : MonoBehaviour
    {
        [SerializeField] Color baseColor  = new Color(0.10f, 0.72f, 0.96f);
        [SerializeField] Color peakColor  = new Color(0.55f, 0.90f, 1.00f);
        [SerializeField] float speed      = 0.9f;

        Image _fill;

        void Start()
        {
            var slider = GetComponent<Slider>();
            if (slider.fillRect != null)
                _fill = slider.fillRect.GetComponent<Image>();
            if (_fill != null) _fill.color = baseColor;
        }

        void Update()
        {
            if (_fill == null) return;
            // Subtle oscillation — amplitude 35% of range so it stays identifiably blue
            float t = (Mathf.Sin(Time.time * speed * Mathf.PI) + 1f) * 0.5f;
            _fill.color = Color.Lerp(baseColor, peakColor, t * 0.35f);
        }
    }
}
