using System;
using System.Collections.Generic;

namespace Data
{
    [Serializable]
    public class GameData
    {
        public int CurrentRoundCount;
        public int CurrentDifficulty;
        public int CurrentScore;
        public Dictionary<int, int> HighScoreDictionary;

        public void Init()
        {
            HighScoreDictionary = new Dictionary<int, int>();
            CurrentDifficulty = -1;
            CurrentRoundCount = 0;
            CurrentScore = 0;
        }

        public bool HasGameInProgress()
        {
            return CurrentDifficulty >= 0 && CurrentRoundCount > 0;
        }

        public void ClearCurrentGame()
        {
            CurrentDifficulty = -1;
            CurrentRoundCount = 0;
            CurrentScore = 0;
        }

        public void UpdateHighScore(int difficulty, int score)
        {
            if (!HighScoreDictionary.ContainsKey(difficulty))
            {
                HighScoreDictionary[difficulty] = score;
            }
            else if (score > HighScoreDictionary[difficulty])
            {
                HighScoreDictionary[difficulty] = score;
            }
        }

        public int GetHighScore(int difficulty)
        {
            return HighScoreDictionary.ContainsKey(difficulty) ? HighScoreDictionary[difficulty] : 0;
        }
    }
}