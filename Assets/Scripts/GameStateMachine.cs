using System;
using Signals;
using Stateless;
using Zenject;

public class GameStateMachine : IInitializable, IDisposable
{
    [Inject]
    private SignalBus _signalBus;
    
    private enum GameState
    {
        InitialState,
        Menu,
        Playing
        
    }

    private enum GameStateMachineTrigger
    {
        LoadMenuTrigger,
        PlayTrigger,
        GameCompletedTrigger
    }
    
    private StateMachine<GameState, GameStateMachineTrigger> _gameStateMachine = new StateMachine<GameState, GameStateMachineTrigger>(GameState.InitialState);

    public void Initialize()
    {
        _gameStateMachine.Configure(GameState.InitialState)
            .OnActivate(() => _gameStateMachine.Fire(GameStateMachineTrigger.LoadMenuTrigger))
            .Permit(GameStateMachineTrigger.LoadMenuTrigger, GameState.Menu);

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

    private void OnMenuEntry()
    {
        _signalBus.Subscribe<GameStartedSignal>(OnGameStarted);
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
