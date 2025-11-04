using System.Collections.Generic;

public class GameData : GC_SingletonImplementer<GameData> {
    public int MaxWave { get; private set; }
    public int MinWave { get; private set; }
    public int CurrentWave { get; set; }
    
    
    public Dictionary<string, PlayerData> Players { get; private set; }
    public int MaxPlayer { get; private set; }
    public int MinPlayer { get; private set; }
    public int CurrentPlayer { get; set; }
    

    private void Init() {
        
    }

    private void Awake() {
        Init();
    }
    
    
}