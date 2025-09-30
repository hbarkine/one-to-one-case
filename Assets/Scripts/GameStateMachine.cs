using System;
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
    }
    
    private StateMachine<GameState, GameStateMachineTrigger> _gameStateMachine;

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
            .Permit(GameStateMachineTrigger.PlayTrigger, GameState.Playing);
        
        _gameStateMachine.Activate();
    }

    private void OnPlayingExit()
    {
    }

    private void OnPlayingEntry()
    {
    }

    private void OnMenuEntry()
    {
        
    }
    
    private void OnMenuExit()
    {
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }
}
