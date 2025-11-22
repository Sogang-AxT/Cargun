using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour {
    public static UnityEvent<int> OnPlayerJoin = new();
    public static UnityEvent OnGameOver = new();
    public static UnityEvent<int> OnTurretAssign = new();
    
    private GamePhaseStateManager _gamePhaseStateManager;
    private IGamePhaseState _currentGamePhaseState;
    private bool _isPlayerCountValid;       // true
    private bool _isBaseMoving;             // false

    
    private void Init() {
        OnPlayerJoin.AddListener(PlayerJoin);

        this._gamePhaseStateManager = new GamePhaseStateManager(this);
        this._currentGamePhaseState = this._gamePhaseStateManager.CurrentGamePhaseState;
        this._isPlayerCountValid = false;
        this._isBaseMoving = false;
    }
    
    private void Awake() {
        Init();
    }

    private void Update() {
        // 플레이어 수가 충분한가?
        if (GameData.Instance.CurrentPlayerCount < GameData.Instance.MinPlayer) {
            Debug.Log("Not Enough Players");
            return;
        }
        
        // 웨이브 진행
        GamePhaseRoutine();
    }

    private void PlayerJoin(int playerCount) {
        GameData.Instance.CurrentPlayerCount = playerCount;

        // 터릿 할당
        foreach (var player in ServerDataManager.Turret_Player) {
            if (player != 0) {
                continue;
            }
            
            // ServerDataManager.Turret_Player[VARIABLE] = playerCount;
            OnTurretAssign.Invoke(player);
            
            break;
        }
    }
    
    private void GamePhaseRoutine() {
        if (this._gamePhaseStateManager.IsPhaseRunning) {
            return;
        }

        this._currentGamePhaseState 
            = (this._currentGamePhaseState == this._gamePhaseStateManager.GamePhaseStateCombat) ? 
                this._gamePhaseStateManager.GamePhaseStateReady : this._gamePhaseStateManager.GamePhaseStateCombat;
        
        this._gamePhaseStateManager.TransitionTo(this._currentGamePhaseState);
        // Debug.Log("GameState: " + this._currentGamePhaseState);
    }
    
    private void GameEndingPhase() {
        this._gamePhaseStateManager.TransitionTo(this._gamePhaseStateManager.GamePhaseStateEnding);
    }
}