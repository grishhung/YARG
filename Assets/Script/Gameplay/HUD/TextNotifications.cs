using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using YARG.Core.Engine;
using YARG.Settings;

namespace YARG.Gameplay.HUD
{
    public class TextNotifications : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _text;

        private int _nextNoteStreakThreshold;

        private int _prevCombo;
        private int _currCombo;

        private double _prevStarPowerAmount;
        private double _currStarPowerAmount;

        private bool _isFc;
        private bool _isSongOver;
        private bool _isFullComboChecked;
        private bool _isStrongFinishChecked;

        private List<string> _notifications;

        private readonly PerformanceTextScaler _scaler = new(2f);
        private Coroutine _coroutine;

        private void OnEnable()
        {
            _text.text = string.Empty;
            _notifications = new List<string>();
            _coroutine = null;
        }

        private void OnDisable()
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
            }
        }

        public void UpdateTextNotification(BaseStats stats, bool isFc, int totalNotes, bool isBass)
        {
            _currCombo = stats.Combo;
            _currStarPowerAmount = stats.StarPowerAmount;

            _isFc = isFc;
            _isSongOver = stats.NotesHit + stats.NotesMissed == totalNotes;

            UpdateNoteStreak(isBass);
            if (isBass) UpdateBassGroove();
            UpdateHotStart();
            UpdateOverdriveReady();
            UpdateFullCombo();
            UpdateStrongFinish();

            _prevCombo = _currCombo;
            _prevStarPowerAmount = _currStarPowerAmount;
        }

        private void UpdateNoteStreak(bool isBass)
        {
            // If the streak is less than before, then reset
            if (_currCombo < _prevCombo || _prevCombo == 0)
            {
                _nextNoteStreakThreshold = 0;
                ChangeNextNoteStreakThreshold(isBass);
            }

            // Queue the note streak notification
            if (_currCombo >= _nextNoteStreakThreshold)
            {
                PruneNoteStreakNotifications(@"\bNOTE STREAK\b");
                _notifications.Add($"{_nextNoteStreakThreshold}-NOTE STREAK");
                ChangeNextNoteStreakThreshold(isBass);
            }
        }

        private void UpdateBassGroove()
        {
            if (_prevCombo < 50 && _currCombo >= 50)
            {
                PruneNoteStreakNotifications(@"\bBASS GROOVE\b");
                _notifications.Add("BASS GROOVE");
            }
        }

        private void UpdateHotStart()
        {
            if (_prevCombo < 30 && _currCombo >= 30 && _isFc)
            {
                _notifications.Add("HOT START");
            }
        }

        private void UpdateOverdriveReady()
        {
            if (_prevStarPowerAmount < 0.5 && _currStarPowerAmount >= 0.5)
            {
                PruneNoteStreakNotifications(@"\bOVERDRIVE READY\b");
                _notifications.Add("OVERDRIVE READY");
            }
        }

        private void UpdateFullCombo()
        {
            if (!_isSongOver || _isFullComboChecked) return;

            if (_isFc)
            {
                _notifications.Clear();
                _notifications.Add("FULL COMBO");
            }

            _isFullComboChecked = true;
        }

        private void UpdateStrongFinish()
        {
            if (!_isSongOver || _isStrongFinishChecked) return;

            if (_currCombo >= 30)
            {
                PruneNoteStreakNotifications(@"^(?!FULL COMBO$).*$");
                _notifications.Add("STRONG FINISH");
            }

            _isStrongFinishChecked = true;
        }

        private void Update()
        {
            // Never update this if text notifications are disabled
            if (SettingsManager.Settings.DisableTextNotifications.Data) return;

            if (_coroutine == null && _notifications.Count > 0)
            {
                _coroutine = StartCoroutine(ShowNextNotification());
            }
        }

        private IEnumerator ShowNextNotification()
        {
            _text.text = _notifications[0];

            _scaler.ResetAnimationTime();

            while (_scaler.AnimTimeRemaining > 0f)
            {
                _scaler.AnimTimeRemaining -= Time.deltaTime;
                float scale = _scaler.PerformanceTextScale();

                _text.transform.localScale = new Vector3(scale, scale, scale);
                yield return null;
            }

            _text.text = string.Empty;
            _coroutine = null;
            _notifications.RemoveAt(0);
        }

        private void ChangeNextNoteStreakThreshold(bool isBass)
        {
            if (SettingsManager.Settings.EnableSparseNoteStreaks.Data)
            {
                if (isBass)
                {
                    // Excluding 50 to prevent collision with 50-NOTE STREAK
                    switch (_nextNoteStreakThreshold)
                    {
                        case 0:
                            _nextNoteStreakThreshold = 100;
                            break;
                        case 100:
                            _nextNoteStreakThreshold = 250;
                            break;
                        case >= 250:
                            _nextNoteStreakThreshold += 250;
                            break;
                    }
                }
                else
                {
                    switch (_nextNoteStreakThreshold)
                    {
                        case 0:
                            _nextNoteStreakThreshold = 50;
                            break;
                        case 50:
                            _nextNoteStreakThreshold = 100;
                            break;
                        case 100:
                            _nextNoteStreakThreshold = 250;
                            break;
                        case >= 250:
                            _nextNoteStreakThreshold += 250;
                            break;
                    }
                }
            }
            else
            {
                if (isBass)
                {
                    // Excluding 50 to prevent collision with 50-NOTE STREAK
                    _nextNoteStreakThreshold += 100;
                }
                else
                {
                    switch (_nextNoteStreakThreshold)
                    {
                        case 0:
                            _nextNoteStreakThreshold = 50;
                            break;
                        case 50:
                            _nextNoteStreakThreshold = 100;
                            break;
                        case >= 100:
                            _nextNoteStreakThreshold += 100;
                            break;
                    }
                }
            }
        }

        public void ForceReset()
        {
            // Reset everything when practice section repeats
            _nextNoteStreakThreshold = 0;

            _prevCombo = 0;
            _currCombo = 0;

            _prevStarPowerAmount = 0.0;
            _currStarPowerAmount = 0.0;

            _isFc = true;
            _isSongOver = false;
            _isFullComboChecked = false;
            _isStrongFinishChecked = false;

            _notifications.Clear();
        }

        private void PruneNoteStreakNotifications(string pattern)
        {
            // Used for preventing build up of notifications during high NPS sections and solos
            List<int> noteStreakIndices = Enumerable.Range(0, _notifications.Count)
                .Where(i => Regex.IsMatch(_notifications[i], pattern))
                .ToList();

            for (int i = noteStreakIndices.Count - 1; i >= 0; i--)
            {
                _notifications.RemoveAt(noteStreakIndices[i]);
            }
        }
    }
}