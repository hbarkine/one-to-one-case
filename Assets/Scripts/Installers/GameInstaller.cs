

using UnityEngine;
using Zenject;



namespace Installers
{
    public class GlobalInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);
            DeclareSignals();
        }

        public void DeclareSignals()
        {
            // Container.DeclareSignal<HardResetGameSignal>();

        }
    }
}

