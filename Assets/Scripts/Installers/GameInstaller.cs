
using Configs;
using Controllers;
using Managers;
using Signals;
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
        
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);
            
            // Bind GameConfig
            Container.BindInterfacesAndSelfTo<GameConfig>().FromInstance(_gameConfig);
            
            // Bind LevelManager from scene
            Container.Bind<LevelManager>().FromInstance(_levelManager).AsSingle();
            
            // Bind GameController
            Container.BindInterfacesAndSelfTo<GameController>().AsSingle().NonLazy();
            
            DeclareSignals();
        }

        public void DeclareSignals()
        {
            Container.DeclareSignal<GameStartedSignal>();
            Container.DeclareSignal<CardSelectedSignal>();
            Container.DeclareSignal<ScoreUpdatedSignal>();
            Container.DeclareSignal<GameCompletedSignal>();
            Container.DeclareSignal<StartLevelSignal>();
        }
    }
}

