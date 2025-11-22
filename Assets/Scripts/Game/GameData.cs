public class GameData : GC_SingletonImplementer<GameData> {
    public int CurrentScore { get; set; }
    public int LevelUpScoreStep { get; private set; } = 500;

    public int CurrentLevel { get; set; } = 1;
    
    public int MaxPlayer { get; private set; } = 4;
    public int MinPlayer { get; private set; } = 1;
    public int CurrentPlayerCount { get; set; }
}