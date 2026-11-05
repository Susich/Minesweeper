using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Minesweeper.Core;

namespace Minesweeper.UI
{
    public class StatusFaceButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Image _faceImage;
        [SerializeField] private Button _button;

        public Sprite FaceNormal;
        public Sprite FaceAlert;
        public Sprite FaceWin;
        public Sprite FaceDefeat;

        private bool _isHovered = false;
        private bool _isPressed = false;
        private GameStatus _currentStatus = GameStatus.WaitingForStart;

        private void Awake()
        {
            if (_button == null) _button = GetComponent<Button>();
            if (_button != null)
            {
                _button.onClick.AddListener(OnFaceClicked);
            }
        }

        private void Start()
        {
            if (GameController.Instance != null)
            {
                GameController.Instance.OnGameStatusChanged += UpdateFaceStatus;
            }
            UpdateFaceStatus(GameStatus.WaitingForStart);
        }

        private void OnDestroy()
        {
            if (GameController.Instance != null)
            {
                GameController.Instance.OnGameStatusChanged -= UpdateFaceStatus;
            }
        }

        private void Update()
        {
            // Idle hover wobble and gentle breathing pulse
            if (_faceImage != null)
            {
                float wobbleY = Mathf.Sin(Time.time * 2.8f) * 1.8f;
                _faceImage.rectTransform.anchoredPosition = new Vector2(0f, wobbleY);
            }

            if (!_isHovered && !_isPressed)
            {
                float breathe = 1f + Mathf.Sin(Time.time * 2.2f) * 0.025f;
                transform.localScale = new Vector3(breathe, breathe, 1f);
            }

            // Real-time mouse down check during game play
            if (_currentStatus == GameStatus.InProgress || _currentStatus == GameStatus.WaitingForStart)
            {
                if (GameController.Instance != null && GameController.Instance.IsMouseDownOnBoard)
                {
                    if (_faceImage != null && _faceImage.sprite != FaceAlert)
                    {
                        _faceImage.sprite = FaceAlert != null ? FaceAlert : FaceNormal;
                    }
                }
                else
                {
                    if (_faceImage != null && _faceImage.sprite != FaceNormal && _faceImage.sprite != FaceWin && _faceImage.sprite != FaceDefeat)
                    {
                        _faceImage.sprite = FaceNormal;
                    }
                }
            }
        }

        public void UpdateFaceStatus(GameStatus status)
        {
            _currentStatus = status;
            if (_faceImage == null) return;

            if (status == GameStatus.Victory)
            {
                _faceImage.sprite = FaceWin != null ? FaceWin : FaceNormal;
            }
            else if (status == GameStatus.Defeat)
            {
                _faceImage.sprite = FaceDefeat != null ? FaceDefeat : FaceNormal;
            }
            else
            {
                _faceImage.sprite = FaceNormal;
            }
        }

        private void OnFaceClicked()
        {
            if (GameController.Instance != null)
            {
                GameController.Instance.RestartGame();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            transform.localScale = Vector3.one * 1.08f;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            transform.localScale = Vector3.one;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
            transform.localScale = Vector3.one * 0.92f;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
            transform.localScale = _isHovered ? Vector3.one * 1.08f : Vector3.one;
        }
    }
}
