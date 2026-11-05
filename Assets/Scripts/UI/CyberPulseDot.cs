using UnityEngine;
using UnityEngine.UI;

namespace Minesweeper.UI
{
    public class CyberPulseDot : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private float _pulseSpeed = 3f;
        [SerializeField] private float _minAlpha = 0.35f;
        [SerializeField] private float _maxAlpha = 1.0f;
        [SerializeField] private bool _scalePulse = true;
        [SerializeField] private float _minScale = 0.9f;
        [SerializeField] private float _maxScale = 1.15f;

        private Color _baseColor;

        private void Awake()
        {
            if (_image == null) _image = GetComponent<Image>();
            if (_image != null) _baseColor = _image.color;
        }

        private void Update()
        {
            if (_image == null) return;

            float t = (Mathf.Sin(Time.time * _pulseSpeed) + 1f) * 0.5f;
            float alpha = Mathf.Lerp(_minAlpha, _maxAlpha, t);
            _image.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, alpha);

            if (_scalePulse)
            {
                float s = Mathf.Lerp(_minScale, _maxScale, t);
                transform.localScale = new Vector3(s, s, 1f);
            }
        }
    }
}
