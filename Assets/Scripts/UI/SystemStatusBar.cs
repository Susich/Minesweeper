using UnityEngine;
using TMPro;

namespace Minesweeper.UI
{
    /// <summary>
    /// Bottom system status bar of the HUD: engine tag on the left, live FPS readout on the right.
    /// </summary>
    public class SystemStatusBar : MonoBehaviour
    {
        [Header("Telemetry")]
        [SerializeField] private TextMeshProUGUI _fpsText;
        [SerializeField] private string _engineLabel = "Движок: CyberSweeper 2.4";

        private float _fpsTimer = 0f;
        private int _fpsFrameCount = 0;

        private void Update()
        {
            _fpsFrameCount++;
            _fpsTimer += Time.unscaledDeltaTime;
            if (_fpsTimer < 0.5f) return;

            int fps = Mathf.RoundToInt(_fpsFrameCount / _fpsTimer);
            _fpsFrameCount = 0;
            _fpsTimer = 0f;

            if (_fpsText != null)
            {
                _fpsText.text = $"{_engineLabel}    FPS: {fps}";
            }
        }
    }
}
