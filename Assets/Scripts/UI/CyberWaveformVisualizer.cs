using UnityEngine;

namespace Minesweeper.UI
{
    public class CyberWaveformVisualizer : MonoBehaviour
    {
        public enum WaveformMode
        {
            MinesHazard,
            TimerClock
        }

        [SerializeField] private WaveformMode _mode = WaveformMode.MinesHazard;
        [SerializeField] private RectTransform[] _bars;
        [SerializeField] private float _minHeight = 4f;
        [SerializeField] private float _maxHeight = 22f;
        [SerializeField] private float _speed = 5f;

        private float[] _phases;

        private void Awake()
        {
            if (_bars == null || _bars.Length == 0)
            {
                int childCount = transform.childCount;
                _bars = new RectTransform[childCount];
                for (int i = 0; i < childCount; i++)
                {
                    _bars[i] = transform.GetChild(i) as RectTransform;
                }
            }

            if (_bars != null)
            {
                _phases = new float[_bars.Length];
                for (int i = 0; i < _bars.Length; i++)
                {
                    _phases[i] = i * 0.85f + Random.Range(0f, 0.5f);
                }
            }
        }

        private void Update()
        {
            if (_bars == null) return;

            float time = Time.time * _speed;
            for (int i = 0; i < _bars.Length; i++)
            {
                if (_bars[i] == null) continue;

                float wave = Mathf.Sin(time + _phases[i]);
                if (_mode == WaveformMode.MinesHazard)
                {
                    wave = (wave + Mathf.Sin(time * 2.3f + _phases[i] * 1.5f)) * 0.5f;
                }

                float normalized = Mathf.Clamp01((wave + 1f) * 0.5f);
                float h = Mathf.Lerp(_minHeight, _maxHeight, normalized);

                var size = _bars[i].sizeDelta;
                size.y = h;
                _bars[i].sizeDelta = size;
            }
        }
    }
}
