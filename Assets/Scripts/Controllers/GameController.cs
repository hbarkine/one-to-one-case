using System;
using System.Collections;
using System.Collections.Generic;
using Configs;
using Managers;
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

        private enum GameControllerState
        {
            WaitingForGameStart,
            InitializeRound,
            ChoosingCards,
            MatchSuccess,
            MatchFail
        }

        private enum GameControllerTrigger
        {
            GameStarted,
            RoundInitialized,
            FirstCardSelected,
            SecondCardSelected,
            CardsMatched,
            CardsMismatched,
            AnimationCompleted,
            AllCardsCompleted,
            AllRoundsCompleted
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
            _stateMachine = new StateMachine<GameControllerState, GameControllerTrigger>(GameControllerState.WaitingForGameStart);

            _stateMachine.Configure(GameControllerState.WaitingForGameStart)
                .OnEntry(OnWaitingForGameStartEntry)
                .Permit(GameControllerTrigger.GameStarted, GameControllerState.InitializeRound);

            _stateMachine.Configure(GameControllerState.InitializeRound)
                .OnEntry(OnInitializeRoundEntry)
                .Permit(GameControllerTrigger.RoundInitialized, GameControllerState.ChoosingCards);

            _stateMachine.Configure(GameControllerState.ChoosingCards)
                .OnEntry(OnChoosingCardsEntry)
                .OnExit(OnChoosingCardsExit)
                .Permit(GameControllerTrigger.CardsMatched, GameControllerState.MatchSuccess)
                .Permit(GameControllerTrigger.CardsMismatched, GameControllerState.MatchFail);

            _stateMachine.Configure(GameControllerState.MatchSuccess)
                .OnEntry(OnMatchSuccessEntry)
                .Permit(GameControllerTrigger.AllCardsCompleted, GameControllerState.InitializeRound)
                .Permit(GameControllerTrigger.AllRoundsCompleted, GameControllerState.WaitingForGameStart)
                .Permit(GameControllerTrigger.AnimationCompleted, GameControllerState.ChoosingCards);

            _stateMachine.Configure(GameControllerState.MatchFail)
                .OnEntry(OnMatchFailEntry)
                .Permit(GameControllerTrigger.AnimationCompleted, GameControllerState.ChoosingCards);

            _stateMachine.Activate();
        }

        private void OnWaitingForGameStartEntry()
        {
            _signalBus.Subscribe<GameStartedSignal>(OnGameStarted);
        }

        private void OnGameStarted(GameStartedSignal signal)
        {
            _currentRound = signal.RoundCount;
            _currentDifficulty = signal.DifficultyConfig;
            _currentScore = 0;
            _comboCounter = 0;
            _signalBus.Fire(new RoundChangedSignal 
            { 
                CurrentRound = _currentRound,
                TotalRounds = _gameConfig.TotalRounds
            });
            _stateMachine.Fire(GameControllerTrigger.GameStarted);
        }

        private void OnInitializeRoundEntry()
        {
            _successfulMatches = 0;
            
            int gridX = (int)_currentDifficulty.LevelLayout.x;
            int gridY = (int)_currentDifficulty.LevelLayout.y;
            _totalPairsInRound = (gridX * gridY) / 2;
            
            _levelManager.InitializeLevel(_currentDifficulty);
            
            _levelManager.StartCoroutine(ShowAndHideCardsRoutine());
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
        }

        private void OnChoosingCardsExit()
        {
            _signalBus.TryUnsubscribe<CardSelectedSignal>(OnCardSelected);
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
                
                if (_firstSelectedCard.AssignedCardConfig.CardId == _secondSelectedCard.AssignedCardConfig.CardId)
                {
                    _stateMachine.Fire(GameControllerTrigger.CardsMatched);
                }
                else
                {
                    _stateMachine.Fire(GameControllerTrigger.CardsMismatched);
                }
            }
        }

        private void OnMatchSuccessEntry()
        {
            _successfulMatches++;
            _comboCounter++;
            
            _currentScore += _comboCounter;
            
            _signalBus.Fire(new ScoreUpdatedSignal { Score = _currentScore, Combo = _comboCounter });

            _levelManager.StartCoroutine(MatchSuccessAnimationRoutine());
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
                    _signalBus.Fire(new GameCompletedSignal { Score = _currentScore });
                    _stateMachine.Fire(GameControllerTrigger.AllRoundsCompleted);
                }
                else
                {
                    _currentRound++;
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

            _signalBus.Fire(new ScoreUpdatedSignal { Score = _currentScore, Combo = _comboCounter });
            
            _levelManager.StartCoroutine(MatchFailAnimationRoutine());
        }

        private IEnumerator MatchFailAnimationRoutine()
        {
            // TODO: Play match fail animations
            yield return new WaitForSeconds(0.5f);
            
            _firstSelectedCard.Hide();
            _secondSelectedCard.Hide();
            
            _stateMachine.Fire(GameControllerTrigger.AnimationCompleted);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<GameStartedSignal>(OnGameStarted);
            _signalBus.TryUnsubscribe<CardSelectedSignal>(OnCardSelected);
        }
    }
} 