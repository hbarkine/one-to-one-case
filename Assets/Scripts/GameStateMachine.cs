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
        Level
        
    }

    private enum GameStateMachineTrigger
    {
        LoadMenuTrigger,
        LoadLevelTrigger,
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
            .Permit(GameStateMachineTrigger.LoadLevelTrigger, GameState.Level);
    }
    
    private void OnMenuEntry()
    {
        
    }
    
    private void OnMenuExit()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }
}
