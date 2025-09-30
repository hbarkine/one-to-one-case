using System.Collections.Generic;
using System.Linq;
using Configs;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class MainMenuController : MonoBehaviour, IInitializable
    {
        [SerializeField]
        private Dropdown _difficultyDropdown;

        [Inject]
        private GameConfig _gameConfig;

        public void Initialize()
        {
            List<string> difficulties =
                _gameConfig.DifficultyConfigs.Select(config => config.Difficulty.ToString()).ToList();
            _difficultyDropdown.AddOptions(difficulties);
        }
        
        public void OnStartGameButtonClicked()
        {
            
        }
    }
}