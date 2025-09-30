
using Configs;
using Controllers;
using Managers;
using Signals;
using UI;
using UnityEngine;
using Zenject;


namespace Installers
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField]
        private GameConfig _gameConfig;
        
        [SerializeField]
        private LevelManager _levelManager;
        
        [SerializeField]
        private PlayerController _playerController;

        [SerializeField]
        private MainMenuController _mainMenuController;

        [SerializeField]
        private HUDController _hudController;
        
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);
            DeclareSignals();

            // Bind GameConfig
            Container.Bind<GameConfig>().FromInstance(_gameConfig);
            
            // Bind LevelManager from scene
            Container.BindInterfacesAndSelfTo<LevelManager>().FromInstance(_levelManager);
            
            // Bind PlayerController from scene
            Container.BindInterfacesAndSelfTo<PlayerController>().FromInstance(_playerController);

            // Bind UI Controllers from scene
            Container.BindInterfacesAndSelfTo<MainMenuController>().FromInstance(_mainMenuController);

            // Bind HUD Controllers from scene
            Container.BindInterfacesAndSelfTo<HUDController>().FromInstance(_hudController);
            
            // Bind GameController
            Container.BindInterfacesAndSelfTo<GameController>().AsSingle().NonLazy();
            
            Container.BindInterfacesAndSelfTo<GameStateMachine>().AsSingle().NonLazy();
        }

        public void DeclareSignals()
        {
            Container.DeclareSignal<GameStartedSignal>();
            Container.DeclareSignal<CardSelectedSignal>();
            Container.DeclareSignal<ScoreUpdatedSignal>();
            Container.DeclareSignal<GameCompletedSignal>();
            Container.DeclareSignal<RoundChangedSignal>();
            Container.DeclareSignal<StartLevelSignal>();
        }
    }
}

