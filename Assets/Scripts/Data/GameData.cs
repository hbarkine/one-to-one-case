using System;
using System.Collections.Generic;

namespace Data
{
    [Serializable]
    public class GameData
    {
        public int CurrentRoundCount;
        public int CurrentDifficulty;
        public Dictionary<int, int> HighScoreDictionary;

        public void Init()
        {
            HighScoreDictionary = new Dictionary<int, int>();
            CurrentDifficulty = -1;
            CurrentRoundCount = 0;
        }
    }
}