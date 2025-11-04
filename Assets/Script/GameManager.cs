using UnityEngine.Events;

public class GameManager : GC_SingletonImplementer<GameManager> {
    private GamePhaseStateManager _gamePhaseStateManager;
    
    private bool _isPlayerCountValid;       // true
    private bool _isBaseMoving;             // false
    private bool _isPhaseRunning;           // false 
    private IGamePhaseState _nextGameState; // null
    
    private readonly int minPlayersToStart = 1;


    private void Init() {
        this._gamePhaseStateManager = new GamePhaseStateManager(this._gamePhaseStateManager);
        
        // TODO: 데이터 모델에서 참조하도록 변경
        this._nextGameState = null;
        // this.CurrentPlayer = 0;
        // this.CurrentWave = 0;
        // this.MaxWave = 10;

        this._isPlayerCountValid = false;
        this._isBaseMoving = false;
        this._isPhaseRunning = false;
    }
    
    private void Awake() {
        Init();
    }

    private void Update() {
        // 플레이어 수가 충분한가?
        if (GameData.Instance.CurrentPlayer < GameData.Instance.MinPlayer) {
            // TODO: Game Reset Event Call
            return;
        }
        
        // 마지막 웨이브 달성?
        if (GameData.Instance.CurrentWave >= GameData.Instance.MaxWave) { 
            GameEndingPhase();
            return;
        }

        GamePhaseUpdate();
    }

    private void GamePhaseUpdate() {
        if (!this._isPhaseRunning) {
            this._isPhaseRunning = true;
            
            // 초기값 = COMBAT
            var currentGamePhase = this._gamePhaseStateManager.CurrentGamePhase;
            
            // 전투 페이스일 때만 웨이브 값 증가
            if (currentGamePhase == GC_EnumManager.GAMEPHASE.COMBAT) {
                GameData.Instance.CurrentWave += 1;
            }
            
            // COMBAT -> (NEXT) READY; READY -> (NEXT) COMBAT
            this._nextGameState = (currentGamePhase == GC_EnumManager.GAMEPHASE.COMBAT) ? 
                this._gamePhaseStateManager.GamePhaseStateReady : this._gamePhaseStateManager.GamePhaseStateCombat;
            this._gamePhaseStateManager.TransitionTo(this._nextGameState);
        }

        this._isPhaseRunning = false;
    }

    private void GameEndingPhase() {
        this._nextGameState = this._gamePhaseStateManager.GamePhaseStateEnding;
        // TODO: 엔딩 페이스 구현
        this._gamePhaseStateManager.TransitionTo(this._nextGameState);
    }
}