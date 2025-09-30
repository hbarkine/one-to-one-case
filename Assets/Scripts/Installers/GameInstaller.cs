
using Configs;
using UnityEngine;
using Zenject;


namespace Installers
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField]
        private GameConfig _gameConfig;
        
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);
            // Container.BindInterfacesAndSelfTo<GameConfig>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<GameConfig>().FromInstance(_gameConfig);
            DeclareSignals();
        }

        public void DeclareSignals()
        {
            // Container.DeclareSignal<HardResetGameSignal>();

        }
    }
}

