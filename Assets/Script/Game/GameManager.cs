using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour {
    public static UnityEvent OnPlayerJoin = new();
    public static UnityEvent OnGameOver = new();
    
    private GamePhaseStateManager _gamePhaseStateManager;
    private IGamePhaseState _nextGameState; // null
    private bool _isPlayerCountValid;       // true
    private bool _isBaseMoving;             // false
    private bool _isPhaseRunning;           // false 
    
    
    private void Init() {
        OnPlayerJoin.AddListener(PlayerJoin);

        this._gamePhaseStateManager = new GamePhaseStateManager(this);
        this._nextGameState = null;
        this._isPlayerCountValid = false;
        this._isBaseMoving = false;
        this._isPhaseRunning = false;
    }
    
    private void Awake() {
        Init();
    }

    private void Update() {
        // 플레이어 수가 충분한가?
        if (GameData.Instance.CurrentPlayerCount < GameData.Instance.MinPlayer) {
            Debug.Log("GameManager; player num");
            return;
        }
        
        // 마지막 웨이브 달성?
        if (GameData.Instance.CurrentWave >= GameData.Instance.MaxWave) { 
            Debug.Log("GameManger; wave fin");
            GameEndingPhase();
            return;
        }
        
        // Debug.Log("GameManager; Update()");
        GamePhaseUpdate();
    }

    private void PlayerJoin() {
        GameData.Instance.CurrentPlayerCount = ServerDataManager.TotalPlayer;
        // TODO: 플레이어 접속 관련 처리
    }
    
    private void GamePhaseUpdate() {
        if (!this._isPhaseRunning) {
            this._isPhaseRunning = true;
            
            // 초기값 = COMBAT
            var currentGamePhase = this._gamePhaseStateManager.CurrentGamePhase;
            
            // TODO: 코루틴을 잘못 사용함; 전투 페이스 진입 시, 1프레임 단위로 계속 증가 중
            // if (currentGamePhase == GC_EnumManager.GAMEPHASE.COMBAT) {
            //     GameData.Instance.CurrentWave += 1; // TODO: 비용 문제 해결
            // }
            
            // COMBAT -> (NEXT) READY; READY -> (NEXT) COMBAT
            this._nextGameState = (currentGamePhase == GC_EnumManager.GAMEPHASE.COMBAT) ? 
                this._gamePhaseStateManager.GamePhaseStateReady : this._gamePhaseStateManager.GamePhaseStateCombat;
            this._gamePhaseStateManager.TransitionTo(this._nextGameState);
            
            Debug.Log("GameState: " + this._nextGameState);
        }
        
        // TODO: 코루틴을 잘못 사용함; 코루틴 호출 명령만 내린 후, 바로 복귀
        if (Input.GetKeyDown(KeyCode.C)) {
            this._isPhaseRunning = false; 
        }
        
        // this._isPhaseRunning = false;
    }

    private void GameEndingPhase() {
        this._nextGameState = this._gamePhaseStateManager.GamePhaseStateEnding;
        this._gamePhaseStateManager.TransitionTo(this._nextGameState);
    }
}