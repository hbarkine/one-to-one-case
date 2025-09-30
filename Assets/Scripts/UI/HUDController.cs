using System;
using DG.Tweening;
using Signals;
using TMPro;
using UnityEngine;
using Zenject;

namespace UI
{
    public class HUDController : MonoBehaviour, IInitializable, IDisposable
    {
        [SerializeField]
        private TextMeshProUGUI _scoreText;

        [SerializeField]
        private TextMeshProUGUI _roundText;

        [SerializeField]
        private TextMeshProUGUI _comboText;

        [SerializeField]
        private DOTweenAnimation _showHideAnimation;

        [Inject]
        private SignalBus _signalBus;

        private int _currentScore;
        private int _currentRound;
        private int _currentCombo;

        public void Initialize()
        {
            _signalBus.Subscribe<GameStartedSignal>(OnGameStarted);
            _signalBus.Subscribe<ScoreUpdatedSignal>(OnScoreUpdated);
            _signalBus.Subscribe<RoundChangedSignal>(OnRoundChanged);
            _signalBus.Subscribe<GameCompletedSignal>(OnGameCompleted);

            gameObject.SetActive(false);
        }

        private void OnGameStarted(GameStartedSignal signal)
        {
            _currentScore = 0;
            _currentRound = signal.RoundCount;
            _currentCombo = 0;
            UpdateDisplay();
            Show();
        }

        private void OnScoreUpdated(ScoreUpdatedSignal signal)
        {
            _currentScore = signal.Score;
            _currentCombo = signal.Combo;
            UpdateScoreDisplay();
            UpdateComboDisplay();
        }

        private void OnRoundChanged(RoundChangedSignal signal)
        {
            _currentRound = signal.CurrentRound;
            UpdateRoundDisplay();
        }

        private void OnGameCompleted(GameCompletedSignal signal)
        {
            Hide();
        }

        private void UpdateDisplay()
        {
            UpdateScoreDisplay();
            UpdateRoundDisplay();
            UpdateComboDisplay();
        }

        private void UpdateScoreDisplay()
        {
            _scoreText.text = $"Score: {_currentScore}";
        }

        private void UpdateRoundDisplay()
        {
            _roundText.text = $"Round: {_currentRound}";
        }

        private void UpdateComboDisplay()
        {
            _comboText.text = $"Combo: {_currentCombo}x";
        }

        private void Show()
        {
            gameObject.SetActive(true);
            _showHideAnimation.DOPlayForward();
        }

        private void Hide()
        {
            _showHideAnimation.DOPlayBackwards();
            _showHideAnimation.tween.OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<GameStartedSignal>(OnGameStarted);
            _signalBus.TryUnsubscribe<ScoreUpdatedSignal>(OnScoreUpdated);
            _signalBus.TryUnsubscribe<RoundChangedSignal>(OnRoundChanged);
            _signalBus.TryUnsubscribe<GameCompletedSignal>(OnGameCompleted);
        }
    }
} 