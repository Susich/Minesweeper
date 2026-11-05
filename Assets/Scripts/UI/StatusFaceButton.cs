using UnityEngine;
using UnityEngine.UI;
using Minesweeper.Core;

namespace Minesweeper.UI
{
    public class StatusFaceButton : MonoBehaviour
    {
        [SerializeField] private Image _faceImage;
        [SerializeField] private Button _button;

        public Sprite FaceNormal;
        public Sprite FaceAlert;
        public Sprite FaceWin;
        public Sprite FaceDefeat;

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

        public void UpdateFaceStatus(GameStatus status)
        {
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
                if (GameController.Instance != null && GameController.Instance.IsMouseDownOnBoard)
                {
                    _faceImage.sprite = FaceAlert != null ? FaceAlert : FaceNormal;
                }
                else
                {
                    _faceImage.sprite = FaceNormal;
                }
            }
        }

        private void OnFaceClicked()
        {
            if (GameController.Instance != null)
            {
                GameController.Instance.RestartGame();
            }
        }
    }
}
