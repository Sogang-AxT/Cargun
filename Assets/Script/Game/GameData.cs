using System.Collections.Generic;

public class GameData : GC_SingletonImplementer<GameData> {
    public int MaxWave { get; private set; } = 10;
    public int MinWave { get; private set; } = 1;
    public int CurrentWave { get; set; }
    
    public Dictionary<string, PlayerData> Players { get; private set; }
    public int MaxPlayer { get; private set; } = 4;
    public int MinPlayer { get; private set; } = 1;
    public int CurrentPlayer { get; set; }
    

    private void Init() {
    }

    private void Awake() {
        Init();
    }
    
    
}