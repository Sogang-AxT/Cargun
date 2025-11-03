public class GameData : GC_SingletonImplementer<GameData> {
    public int MaxWave { get; private set; }
    public int MinWave { get; private set; }
    public int CurrentWave { get; private set; }
    
    public int MaxPlayer { get; private set; }
    public int MinPlayer { get; private set; }
    public int CurrentPlayer { get; private set; }


    private void Init() {
        
    }

    private void Awake() {
        Init();
    }

    private void SetCurrentPlayer(int playerCount) {
        this.CurrentPlayer = playerCount;
    }
}