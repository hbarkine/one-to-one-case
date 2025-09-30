using System.Collections.Generic;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "MatchGame/Configs/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        public List<DifficultyConfig> DifficultyConfigs;
        public List<CardConfig> AllCardsConfigs;
    }
}