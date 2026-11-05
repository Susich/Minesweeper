using System.Collections;
using UnityEngine;

namespace Minesweeper.UI
{
    public class ScreenShake : MonoBehaviour
    {
        public static ScreenShake Instance { get; private set; }

        [SerializeField] private RectTransform _targetTransform;
        private Vector3 _originalPosition;
        private Coroutine _shakeCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            if (_targetTransform == null)
            {
                _targetTransform = GetComponent<RectTransform>();
            }
            if (_targetTransform != null)
            {
                _originalPosition = _targetTransform.localPosition;
            }
        }

        public void Shake(float duration = 0.35f, float magnitude = 12f)
        {
            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
                if (_targetTransform != null) _targetTransform.localPosition = _originalPosition;
            }
            _shakeCoroutine = StartCoroutine(DoShake(duration, magnitude));
        }

        private IEnumerator DoShake(float duration, float magnitude)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float damp = 1f - Mathf.Clamp01(elapsed / duration);
                float x = (Random.value * 2f - 1f) * magnitude * damp;
                float y = (Random.value * 2f - 1f) * magnitude * damp;

                if (_targetTransform != null)
                {
                    _targetTransform.localPosition = _originalPosition + new Vector3(x, y, 0f);
                }
                yield return null;
            }

            if (_targetTransform != null)
            {
                _targetTransform.localPosition = _originalPosition;
            }
            _shakeCoroutine = null;
        }
    }
}
