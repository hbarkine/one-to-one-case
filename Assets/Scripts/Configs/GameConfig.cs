using System.Collections.Generic;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "MatchGame/Configs/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        public List<DifficultyConfig> DifficultyConfigs;
        public List<CardConfig> AllCardsConfigs;
        public int TotalRounds = 5;
        public float CardShowDuration = 2f;
    }
}