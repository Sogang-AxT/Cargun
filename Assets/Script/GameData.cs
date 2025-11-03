using System.Collections.Generic;

public class GameData : GC_SingletonImplementer<GameData> {
    public GC_EnumManager.GAMEPHASE CurrentGamePhase { get; private set; }
    
    public int MaxWave { get; private set; }
    public int MinWave { get; private set; }
    public int CurrentWave { get; set; }
    
    
    public Dictionary<string, PlayerData> Players { get; private set; }
    public int MaxPlayer { get; private set; }
    public int MinPlayer { get; private set; }
    public int CurrentPlayer { get; private set; }
    
    

    private void Init() {
    }

    private void Awake() {
        Init();
    }

}