using System;
using System.Collections;
using System.Collections.Generic;
using Configs;
using DG.Tweening;
using Managers;
using Services;
using Signals;
using Stateless;
using UnityEngine;
using Zenject;

namespace Controllers
{
    public class GameController : IInitializable, IDisposable
    {
        [Inject]
        private SignalBus _signalBus;

        [Inject]
        private LevelManager _levelManager;

        [Inject]
        private GameConfig _gameConfig;

        [Inject]
        private GameDataService _gameDataService;

        [Inject]
        private SoundManager _soundManager;

        private enum GameControllerState
        {
            InitialState,
            WaitingForGameStart,
            InitializeRound,
            ChoosingCards,
            MatchSuccess,
            MatchFail
        }

        private enum GameControllerTrigger
        {
            StartWaitingTrigger,
            GameStarted,
            RoundInitialized,
            FirstCardSelected,
            SecondCardSelected,
            CardsMatched,
            CardsMismatched,
            AnimationCompleted,
            AllCardsCompleted,
            AllRoundsCompleted,
            ReturnToMenuTrigger
        }

        private StateMachine<GameControllerState, GameControllerTrigger> _stateMachine;
        
        private int _currentRound;
        private int _currentScore;
        private int _comboCounter;
        private int _successfulMatches;
        private int _totalPairsInRound;
        
        private CardComponent _firstSelectedCard;
        private CardComponent _secondSelectedCard;
        
        private DifficultyConfig _currentDifficulty;

        public void Initialize()
        {
            _stateMachine = new StateMachine<GameControllerState, GameControllerTrigger>(GameControllerState.InitialState);

            _stateMachine.Configure(GameControllerState.InitialState)
                .OnActivate(() => _stateMachine.Fire(GameControllerTrigger.StartWaitingTrigger))
                .Permit(GameControllerTrigger.StartWaitingTrigger, GameControllerState.WaitingForGameStart);

            _stateMachine.Configure(GameControllerState.WaitingForGameStart)
                .OnEntry(OnWaitingForGameStartEntry)
                .OnExit(OnWaitingForGameStartExit)
                .Permit(GameControllerTrigger.GameStarted, GameControllerState.InitializeRound);

            _stateMachine.Configure(GameControllerState.InitializeRound)
                .OnEntry(OnInitializeRoundEntry)
                .OnExit(OnInitializeRoundExit)
                .Permit(GameControllerTrigger.RoundInitialized, GameControllerState.ChoosingCards)
                .Permit(GameControllerTrigger.ReturnToMenuTrigger, GameControllerState.WaitingForGameStart);

            _stateMachine.Configure(GameControllerState.ChoosingCards)
                .OnEntry(OnChoosingCardsEntry)
                .OnExit(OnChoosingCardsExit)
                .Permit(GameControllerTrigger.CardsMatched, GameControllerState.MatchSuccess)
                .Permit(GameControllerTrigger.CardsMismatched, GameControllerState.MatchFail)
                .Permit(GameControllerTrigger.ReturnToMenuTrigger, GameControllerState.WaitingForGameStart);

            _stateMachine.Configure(GameControllerState.MatchSuccess)
                .OnEntry(OnMatchSuccessEntry)
                .OnExit(OnMatchSuccessExit)
                .Permit(GameControllerTrigger.AllCardsCompleted, GameControllerState.InitializeRound)
                .Permit(GameControllerTrigger.AllRoundsCompleted, GameControllerState.WaitingForGameStart)
                .Permit(GameControllerTrigger.AnimationCompleted, GameControllerState.ChoosingCards)
                .Permit(GameControllerTrigger.ReturnToMenuTrigger, GameControllerState.WaitingForGameStart);

            _stateMachine.Configure(GameControllerState.MatchFail)
                .OnEntry(OnMatchFailEntry)
                .OnExit(OnMatchFailExit)
                .Permit(GameControllerTrigger.AnimationCompleted, GameControllerState.ChoosingCards)
                .Permit(GameControllerTrigger.ReturnToMenuTrigger, GameControllerState.WaitingForGameStart);

            _stateMachine.Activate();
        }

        private void OnWaitingForGameStartEntry()
        {
            _signalBus.Subscribe<GameStartedSignal>(OnGameStarted);
        }
        
        private void OnWaitingForGameStartExit()
        {
            _signalBus.Unsubscribe<GameStartedSignal>(OnGameStarted);
        }

        private void OnGameStarted(GameStartedSignal signal)
        {
            _currentRound = signal.RoundCount;
            _currentDifficulty = signal.DifficultyConfig;
            _currentScore = signal.CurrentScore;
            _comboCounter = 0;
            
            // Save game state
            SaveGameProgress();
            
            _signalBus.Fire(new RoundChangedSignal 
            { 
                CurrentRound = _currentRound,
                TotalRounds = _gameConfig.TotalRounds
            });
            
            // If loading a saved game, update the score display
            if (_currentScore > 0)
            {
                _signalBus.Fire(new ScoreUpdatedSignal { Score = _currentScore, Combo = _comboCounter });
            }
            
            _stateMachine.Fire(GameControllerTrigger.GameStarted);
        }

        private void OnInitializeRoundEntry()
        {
            _successfulMatches = 0;
            
            int gridX = (int)_currentDifficulty.LevelLayout.x;
            int gridY = (int)_currentDifficulty.LevelLayout.y;
            _totalPairsInRound = (gridX * gridY) / 2;
            
            _levelManager.InitializeLevel(_currentDifficulty);
            
            _signalBus.Subscribe<ReturnToMenuSignal>(OnReturnToMenu);
            
            _levelManager.StartCoroutine(ShowAndHideCardsRoutine());
        }

        private void OnInitializeRoundExit()
        {
            _signalBus.TryUnsubscribe<ReturnToMenuSignal>(OnReturnToMenu);
        }

        private IEnumerator ShowAndHideCardsRoutine()
        {
            foreach (CardComponent card in _levelManager.ActiveCards)
            {
                card.Show();
            }

            yield return new WaitForSeconds(_gameConfig.CardShowDuration);

            foreach (CardComponent card in _levelManager.ActiveCards)
            {
                card.Hide();
            }

            _stateMachine.Fire(GameControllerTrigger.RoundInitialized);
        }

        private void OnChoosingCardsEntry()
        {
            _firstSelectedCard = null;
            _secondSelectedCard = null;
            
            _signalBus.Subscribe<CardSelectedSignal>(OnCardSelected);
            _signalBus.Subscribe<ReturnToMenuSignal>(OnReturnToMenu);
        }

        private void OnChoosingCardsExit()
        {
            _signalBus.TryUnsubscribe<CardSelectedSignal>(OnCardSelected);
            _signalBus.TryUnsubscribe<ReturnToMenuSignal>(OnReturnToMenu);
        }

        private void OnCardSelected(CardSelectedSignal signal)
        {
            if (_firstSelectedCard == null)
            {
                _firstSelectedCard = signal.CardComponent;
                _firstSelectedCard.Show();
            }
            else if (_secondSelectedCard == null && signal.CardComponent != _firstSelectedCard)
            {
                _secondSelectedCard = signal.CardComponent;
                _secondSelectedCard.Show();

                DOVirtual.DelayedCall(1f, () => 
                {
                    if (_firstSelectedCard.AssignedCardConfig.CardId == _secondSelectedCard.AssignedCardConfig.CardId)
                    {
                        _stateMachine.Fire(GameControllerTrigger.CardsMatched);
                    }
                    else
                    {
                        _stateMachine.Fire(GameControllerTrigger.CardsMismatched);
                    } 
                });
                
            }
        }

        private void OnMatchSuccessEntry()
        {
            _successfulMatches++;
            _comboCounter++;
            
            _currentScore += _comboCounter;
            
            _soundManager.PlayCardMatch();
            
            _signalBus.Fire(new ScoreUpdatedSignal { Score = _currentScore, Combo = _comboCounter });
            
            _signalBus.Subscribe<ReturnToMenuSignal>(OnReturnToMenu);

            _levelManager.StartCoroutine(MatchSuccessAnimationRoutine());
        }

        private void OnMatchSuccessExit()
        {
            _signalBus.TryUnsubscribe<ReturnToMenuSignal>(OnReturnToMenu);
        }

        private IEnumerator MatchSuccessAnimationRoutine()
        {
            // TODO: Play match success animations
            _firstSelectedCard.Complete();
            _secondSelectedCard.Complete();
            
            yield return new WaitForSeconds(0.5f);
            
            if (_successfulMatches >= _totalPairsInRound)
            {
                if (_currentRound >= _gameConfig.TotalRounds)
                {
                    // Reset the level and destroy all cards
                    _levelManager.Reset();
                    
                    // Game completed - update high score and clear saved game
                    int difficultyIndex = _gameConfig.DifficultyConfigs.IndexOf(_currentDifficulty);
                    _gameDataService.CurrentGameData.UpdateHighScore(difficultyIndex, _currentScore);
                    _gameDataService.CurrentGameData.ClearCurrentGame();
                    _gameDataService.SaveGameData();
                    
                    _soundManager.PlayGameOver();
                    
                    _signalBus.Fire(new GameCompletedSignal { Score = _currentScore });
                    _stateMachine.Fire(GameControllerTrigger.AllRoundsCompleted);
                }
                else
                {
                    _currentRound++;
                    
                    // Save progress for next round
                    SaveGameProgress();
                    
                    _signalBus.Fire(new RoundChangedSignal 
                    { 
                        CurrentRound = _currentRound,
                        TotalRounds = _gameConfig.TotalRounds
                    });
                    _stateMachine.Fire(GameControllerTrigger.AllCardsCompleted);
                }
            }
            else
            {
                _stateMachine.Fire(GameControllerTrigger.AnimationCompleted);
            }
        }

        private void OnMatchFailEntry()
        {
            _comboCounter = 0;
            
            _currentScore = Mathf.Max(0, _currentScore - 1);

            _soundManager.PlayCardMismatch();

            _signalBus.Fire(new ScoreUpdatedSignal { Score = _currentScore, Combo = _comboCounter });
            
            _signalBus.Subscribe<ReturnToMenuSignal>(OnReturnToMenu);
            
            _levelManager.StartCoroutine(MatchFailAnimationRoutine());
        }

        private void OnMatchFailExit()
        {
            _signalBus.TryUnsubscribe<ReturnToMenuSignal>(OnReturnToMenu);
        }

        private IEnumerator MatchFailAnimationRoutine()
        {
            // TODO: Play match fail animations
            yield return new WaitForSeconds(0.5f);
            
            _firstSelectedCard.Hide();
            _secondSelectedCard.Hide();
            
            _stateMachine.Fire(GameControllerTrigger.AnimationCompleted);
        }

        private void OnReturnToMenu()
        {
            // Stop all coroutines
            _levelManager.StopAllCoroutines();
            
            // Reset the level and destroy all cards
            _levelManager.Reset();
            
            // Clear saved game progress
            _gameDataService.CurrentGameData.ClearCurrentGame();
            _gameDataService.SaveGameData();
            
            // Transition state machine back to waiting state
            _stateMachine.Fire(GameControllerTrigger.ReturnToMenuTrigger);
            
            // Hide HUD and go back to menu
            _signalBus.Fire(new GameCompletedSignal { Score = _currentScore });
        }

        private void SaveGameProgress()
        {
            int difficultyIndex = _gameConfig.DifficultyConfigs.IndexOf(_currentDifficulty);
            _gameDataService.CurrentGameData.CurrentDifficulty = difficultyIndex;
            _gameDataService.CurrentGameData.CurrentRoundCount = _currentRound;
            _gameDataService.CurrentGameData.CurrentScore = _currentScore;
            _gameDataService.SaveGameData();
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<GameStartedSignal>(OnGameStarted);
            _signalBus.TryUnsubscribe<CardSelectedSignal>(OnCardSelected);
            _signalBus.TryUnsubscribe<ReturnToMenuSignal>(OnReturnToMenu);
        }
    }
} 