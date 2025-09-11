using UnityEngine;
using UnityEngine.UI;
using YARG.Core.Engine;
using YARG.Gameplay.Player;
using YARG.Gameplay.Visuals;
using YARG.Player;
using YARG.Helpers.UI;

namespace YARG.Gameplay.HUD
{
    public class TrackView : MonoBehaviour
    {
        [SerializeField]
        private AspectRatioFitter _aspectRatioFitter;
        [SerializeField]
        private ScaleByParentSize _UIScaler;
        [SerializeField]
        private RectTransform _topElementContainer;

        [Space]
        [SerializeField]
        private SoloBox _soloBox;
        [SerializeField]
        private TextNotifications _textNotifications;
        [SerializeField]
        private CountdownDisplay _countdownDisplay;
        [SerializeField]
        private PlayerNameDisplay _playerNameDisplay;

        private TrackPlayer _trackPlayer;

        private void Start()
        {
            _aspectRatioFitter.aspectRatio = (float) Screen.width / Screen.height;
            _UIScaler.Initialize();
        }

        public void Initialize(TrackPlayer trackPlayer)
        {
            _trackPlayer = trackPlayer;
        }

        public void UpdateHUDPosition(int playerCount)
        {
            var rect = GetComponent<RectTransform>();
            var viewportPos = _trackPlayer.HUDViewportPosition;

            // Caching this is faster
            var rectRect = rect.rect;

            // Adjust the screen's viewport position to the rect's viewport position
            // -0.5f as our position is relative to center, not the corner
            _topElementContainer.localPosition = _topElementContainer.localPosition.WithY(rect.rect.height * (viewportPos.y - 0.5f));
            //
            // // Account for the initial camera and canvas offset
            // int player = _trackPlayer.PlayerIndex;
            // float cameraOffset = HighwayCameraRendering.GetMultiplayerXOffset(player, playerCount, -0.5f) / playerCount;
            // // TODO: Get canvas offset
            //
            // // Gets value from [0, 1] with respect to each individual player
            // // Each highway is 100 units apart
            // // TODO: Investigate why the y value seems to do what you'd expect z to do
            // // TODO: Replace 10f and 20f with real world z points
            // float countdownXOffsetMagnitude = HighwayCameraRendering.WorldToViewport(new Vector3(100f * player, 10f, 0f), player).x;
            // float soloBoxXOffsetMagnitude = HighwayCameraRendering.WorldToViewport(new Vector3(100f * player, 20f, 0f), player).x;
            //
            // // Convert to individual screen space and then to screen position
            // // Bounds are [-0.5f * Screen.width / playerCount, 0.5f * Screen.width / playerCount]
            // countdownXOffsetMagnitude = (cameraOffset + countdownXOffsetMagnitude - 0.5f) * Screen.width / playerCount;
            // soloBoxXOffsetMagnitude = (cameraOffset + soloBoxXOffsetMagnitude - 0.5f) * Screen.width / playerCount;
            //
            // // Offset bottom elements
            // HighwayCameraRendering.OffsetLocalPosition(_playerNameDisplay.transform, countdownXOffsetMagnitude);
            // HighwayCameraRendering.OffsetLocalPosition(_countdownDisplay.transform, countdownXOffsetMagnitude);
            //
            // // Offset top elements; text notifications are under a parent container GameObject
            // HighwayCameraRendering.OffsetLocalPosition(_soloBox.transform, soloBoxXOffsetMagnitude);
            // HighwayCameraRendering.OffsetLocalPosition(_textNotifications.transform.parent.transform, soloBoxXOffsetMagnitude);
        }

        public void UpdateCountdown(double countdownLength, double endTime)
        {
            _countdownDisplay.UpdateCountdown(countdownLength, endTime);
        }

        public void StartSolo(SoloSection solo)
        {
            _soloBox.StartSolo(solo);

            // No text notifications during the solo
            _textNotifications.SetActive(false);
        }

        public void EndSolo(int soloBonus)
        {
            _soloBox.EndSolo(soloBonus, () =>
            {
                // Show text notifications again
                _textNotifications.SetActive(true);
            });
        }

        public void UpdateNoteStreak(int streak)
        {
            _textNotifications.UpdateNoteStreak(streak);
        }

        public void ShowNewHighScore()
        {
            _textNotifications.ShowNewHighScore();
        }

        public void ShowFullCombo()
        {
            _textNotifications.ShowFullCombo();
        }

        public void ShowHotStart()
        {
            _textNotifications.ShowHotStart();
        }

        public void ShowBassGroove()
        {
            _textNotifications.ShowBassGroove();
        }

        public void ShowStarPowerReady()
        {
            _textNotifications.ShowStarPowerReady();
        }

        public void ShowStrongFinish()
        {
            _textNotifications.ShowStrongFinish();
        }

        public void ShowPlayerName(YargPlayer player)
        {
            _playerNameDisplay.ShowPlayer(player);
        }

        public void ForceReset()
        {
            _textNotifications.SetActive(true);

            _soloBox.ForceReset();
            _textNotifications.ForceReset();
            _countdownDisplay.ForceReset();
        }
    }
}
