using Configs;

namespace Signals
{
    public class GameStartedSignal
    {
        public int RoundCount { get; set; }
        public int CurrentScore { get; set; }
        public DifficultyConfig DifficultyConfig { get; set; }
    }
} 