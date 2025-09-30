using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName = "DifficultyConfig", menuName = "MatchGame/Configs/DifficultyConfig")]
    public class DifficultyConfig : ScriptableObject
    {
        public enum DifficultyEnum
        {
            Easy,
            Normal,
            Hard,
            Extreme,
            God
        }

        public DifficultyEnum Difficulty;
        public Vector2 LevelLayout;
    }
}