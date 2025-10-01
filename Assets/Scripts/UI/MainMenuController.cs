using System;
using System.Collections.Generic;
using System.Linq;
using Configs;
using DG.Tweening;
using Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class MainMenuController : MonoBehaviour, IInitializable, IDisposable
    {
        [SerializeField]
        private TMP_Dropdown _difficultyDropdown;

        [SerializeField]
        private Button _startGameButton;

        [SerializeField]
        private DOTweenAnimation _showHideAnimation;

        [Inject]
        private GameConfig _gameConfig;

        [Inject]
        private SignalBus _signalBus;

        private int _selectedDifficultyIndex;

        public void Initialize()
        {
            List<string> difficulties =
                _gameConfig.DifficultyConfigs.Select(config => config.Difficulty.ToString()).ToList();
            _difficultyDropdown.AddOptions(difficulties);
            
            _difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
            _startGameButton.onClick.AddListener(OnStartGameButtonClicked);
            
            _signalBus.Subscribe<MenuLoadedSignal>(OnMenuLoaded);
            _signalBus.Subscribe<GameCompletedSignal>(OnGameCompleted);
        }

        private void OnDifficultyChanged(int index)
        {
            _selectedDifficultyIndex = index;
        }

        public void OnStartGameButtonClicked()
        {
            DifficultyConfig selectedDifficulty = _gameConfig.DifficultyConfigs[_selectedDifficultyIndex];

            Hide();
            _signalBus.Fire(new GameStartedSignal 
            { 
                RoundCount = 1,
                CurrentScore = 0,
                DifficultyConfig = selectedDifficulty
            });
        }

        private void OnMenuLoaded()
        {
            Show();
        }

        private void OnGameCompleted(GameCompletedSignal signal)
        {
            Show();
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
            _difficultyDropdown.onValueChanged.RemoveListener(OnDifficultyChanged);
            _startGameButton.onClick.RemoveListener(OnStartGameButtonClicked);
            _signalBus.TryUnsubscribe<MenuLoadedSignal>(OnMenuLoaded);
            _signalBus.TryUnsubscribe<GameCompletedSignal>(OnGameCompleted);
        }
    }
}