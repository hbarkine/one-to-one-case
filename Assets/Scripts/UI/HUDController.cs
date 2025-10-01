using System;
using Configs;
using DG.Tweening;
using Services;
using Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
        private TextMeshProUGUI _difficultyText;

        [SerializeField]
        private TextMeshProUGUI _highScoreText;

        [SerializeField]
        private Button _returnToMenuButton;

        [SerializeField]
        private DOTweenAnimation _showHideAnimation;

        [Inject]
        private SignalBus _signalBus;

        [Inject]
        private GameConfig _gameConfig;

        [Inject]
        private GameDataService _gameDataService;

        private int _currentScore;
        private int _currentRound;
        private int _currentCombo;
        private int _totalRounds;

        public void Initialize()
        {
            _returnToMenuButton.onClick.AddListener(OnReturnToMenuClicked);
            
            _signalBus.Subscribe<GameStartedSignal>(OnGameStarted);
            _signalBus.Subscribe<ScoreUpdatedSignal>(OnScoreUpdated);
            _signalBus.Subscribe<RoundChangedSignal>(OnRoundChanged);
            _signalBus.Subscribe<GameCompletedSignal>(OnGameCompleted);
        }

        private void OnGameStarted(GameStartedSignal signal)
        {
            _difficultyText.text = signal.DifficultyConfig.Difficulty.ToString();
            _currentScore = signal.CurrentScore;
            _currentRound = signal.RoundCount;
            
            // Update high score for current difficulty
            int difficultyIndex = _gameConfig.DifficultyConfigs.IndexOf(signal.DifficultyConfig);
            int highScore = _gameDataService.CurrentGameData.GetHighScore(difficultyIndex);
            _highScoreText.text = $"High Score: {highScore}";
            
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
            _totalRounds = signal.TotalRounds;
            UpdateRoundDisplay();
        }

        private void OnGameCompleted(GameCompletedSignal signal)
        {
            Hide();
        }

        private void OnReturnToMenuClicked()
        {
            _signalBus.Fire<ReturnToMenuSignal>();
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
            _roundText.text = $"Round: {_currentRound}/{_totalRounds}";
        }

        private void UpdateComboDisplay()
        {
            _comboText.text = $"Combo: {_currentCombo}x";
        }

        private void Show()
        {
            _showHideAnimation.DOPlayForward();
        }

        private void Hide()
        {
            _showHideAnimation.DOPlayBackwards();
        }

        public void Dispose()
        {
            _returnToMenuButton.onClick.RemoveListener(OnReturnToMenuClicked);
            
            _signalBus.TryUnsubscribe<GameStartedSignal>(OnGameStarted);
            _signalBus.TryUnsubscribe<ScoreUpdatedSignal>(OnScoreUpdated);
            _signalBus.TryUnsubscribe<RoundChangedSignal>(OnRoundChanged);
            _signalBus.TryUnsubscribe<GameCompletedSignal>(OnGameCompleted);
        }
    }
} 