using System;
using Configs;
using Services;
using Signals;
using Stateless;
using Zenject;

public class GameStateMachine : IInitializable, IDisposable
{
    [Inject]
    private SignalBus _signalBus;

    [Inject]
    private GameDataService _gameDataService;

    [Inject]
    private GameConfig _gameConfig;
    
    private enum GameState
    {
        InitialState,
        CheckGameState,
        Menu,
        Playing
        
    }

    private enum GameStateMachineTrigger
    {
        CheckStateTrigger,
        LoadMenuTrigger,
        PlayTrigger,
        GameCompletedTrigger
    }
    
    private StateMachine<GameState, GameStateMachineTrigger> _gameStateMachine = new StateMachine<GameState, GameStateMachineTrigger>(GameState.InitialState);

    public void Initialize()
    {
        _gameStateMachine.Configure(GameState.InitialState)
            .OnActivate(() => _gameStateMachine.Fire(GameStateMachineTrigger.CheckStateTrigger))
            .Permit(GameStateMachineTrigger.CheckStateTrigger, GameState.CheckGameState);

        _gameStateMachine.Configure(GameState.CheckGameState)
            .OnEntry(OnCheckGameStateEntry)
            .Permit(GameStateMachineTrigger.LoadMenuTrigger, GameState.Menu)
            .Permit(GameStateMachineTrigger.PlayTrigger, GameState.Playing);

        _gameStateMachine.Configure(GameState.Menu)
            .OnEntry(OnMenuEntry)
            .OnExit(OnMenuExit)
            .Permit(GameStateMachineTrigger.PlayTrigger, GameState.Playing);

        _gameStateMachine.Configure(GameState.Playing)
            .OnEntry(OnPlayingEntry)
            .OnExit(OnPlayingExit)
            .Permit(GameStateMachineTrigger.GameCompletedTrigger, GameState.Menu);
        
        _gameStateMachine.Activate();
    }

    private void OnCheckGameStateEntry()
    {
        if (_gameDataService.CurrentGameData.HasGameInProgress())
        {
            // Load saved game
            int savedDifficulty = _gameDataService.CurrentGameData.CurrentDifficulty;
            DifficultyConfig difficultyConfig = _gameConfig.DifficultyConfigs[savedDifficulty];
            
            _signalBus.Fire(new GameStartedSignal
            {
                RoundCount = _gameDataService.CurrentGameData.CurrentRoundCount,
                CurrentScore = _gameDataService.CurrentGameData.CurrentScore,
                DifficultyConfig = difficultyConfig
            });
            
            _gameStateMachine.Fire(GameStateMachineTrigger.PlayTrigger);
        }
        else
        {
            // No saved game, go to menu
            _gameStateMachine.Fire(GameStateMachineTrigger.LoadMenuTrigger);
        }
    }

    private void OnMenuEntry()
    {
        _signalBus.Subscribe<GameStartedSignal>(OnGameStarted);
        _signalBus.Fire<MenuLoadedSignal>();
    }
    
    private void OnMenuExit()
    {
        _signalBus.TryUnsubscribe<GameStartedSignal>(OnGameStarted);
    }

    private void OnPlayingEntry()
    {
        _signalBus.Subscribe<GameCompletedSignal>(OnGameCompleted);
    }

    private void OnPlayingExit()
    {
        _signalBus.TryUnsubscribe<GameCompletedSignal>(OnGameCompleted);
    }

    private void OnGameStarted(GameStartedSignal signal)
    {
        _gameStateMachine.Fire(GameStateMachineTrigger.PlayTrigger);
    }

    private void OnGameCompleted(GameCompletedSignal signal)
    {
        _gameStateMachine.Fire(GameStateMachineTrigger.GameCompletedTrigger);
    }

    public void Dispose()
    {
        _signalBus.TryUnsubscribe<GameStartedSignal>(OnGameStarted);
        _signalBus.TryUnsubscribe<GameCompletedSignal>(OnGameCompleted);
    }
}
