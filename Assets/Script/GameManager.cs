using UnityEngine;
using System.Collections;

public class GameManager : GC_SingletonImplementer<GameManager> {
    private GamePhaseStateManager _gamePhaseStateManager;
    
    [SerializeField] private ShipStationManager _shipStationManager;
    
    public int CurrentplayerCount { get; private set; } // 0
    public int CurrentWave { get; private set; }        // 1
    public int MaxWave { get; private set; }            // 10

    private bool _isBaseMoving;             // false
    private bool _isPhaseRunning;           // false
    private IGamePhaseState _nextGameState; // null
    
    private readonly int minPlayersToStart = 1;


    private void Init() {
        this._gamePhaseStateManager = new GamePhaseStateManager(this._gamePhaseStateManager);
        
        this._nextGameState = null;
        this.CurrentplayerCount = 0;
        this.CurrentWave = 0;
        this.MaxWave = 10;

        this._isBaseMoving = false;
        this._isPhaseRunning = false;
    }
    
    private void Awake() {
        Init();
    }
    
    private void Update() {
        // Check Player count from Server
        if (!CurrentPlayerCountCheck()) {
            return;
        }

        if (this.CurrentWave == this.MaxWave) {
            GameEndingPhase();
        }

        GamePhaseUpdate();

        // 시간이 다 지나면 승리 (Cargo보다 먼저 체크)
        // if (_phaseTimer <= 0)
        // {
        //     Debug.Log("⏰ 시간 종료! 승리!");
        //     EndCombatPhase(true); // 승리
        //     return; // ✅ 즉시 종료
        // }
        //
        // // 화물이 0이 되면 패배 (DecreaseCargo에서도 처리하지만 안전장치)
        // if (_remainingCargo <= 0)
        // {
        //     Debug.Log("📦 Cargo 0개! 패배!");
        //     EndCombatPhase(false); // 패배
        //     return; // ✅ 즉시 종료
        // }
        //
        // // 플레이어 0명이면 즉시 Ready로
        // if (playerCount == 0)
        // {
        //     Debug.Log("👥 플레이어 0명! Ready로 복귀");
        //     ForceReturnToReady();
        //     return; // ✅ 즉시 종료
        // }
    }

    private bool CurrentPlayerCountCheck() {
        var count = (ServerManager.Instance != null) ? ServerManager.Instance.playerCount : 0;
        return (count >= this.minPlayersToStart);
    }

    private void GamePhaseUpdate() {
        if (!this._isPhaseRunning) {
            this._isPhaseRunning = true;
            this.CurrentWave += 1;    
            
            this._nextGameState = (this._gamePhaseStateManager.CurrentGamePhase == GC_EnumManager.GAMEPHASE.READY) ? 
                this._gamePhaseStateManager.GamePhaseStateReady : this._gamePhaseStateManager.GamePhaseStateCombat;
                
            this._gamePhaseStateManager.TransitionTo(this._nextGameState);
        }

        this._isPhaseRunning = false;
    }

    private void GameEndingPhase() {
        this._nextGameState = this._gamePhaseStateManager.GamePhaseStateEnding;
        this._gamePhaseStateManager.TransitionTo(this._nextGameState);
    }

    
    
    // // Combat Phase 종료
    // void EndCombatPhase(bool victory)
    // {
    //     // ✅ 중복 호출 방지
    //     if (_currentPhase != "combat") return;
    //
    //     Debug.Log($"=== Combat Phase 종료 ({(victory ? "승리" : "패배")}) ===");
    //
    //     // ✅ 즉시 Phase 변경하여 Update에서 더 이상 처리하지 않도록
    //     _currentPhase = "ending"; // combat도 ready도 아닌 중간 상태
    //     _phaseTimer = 0f; // 타이머 정지
    //
    //     // Enemy 스폰 중지 및 제거
    //     if (Enemy.Instance != null)
    //     {
    //         Enemy.Instance.StopSpawning();
    //         Enemy.Instance.ClearAllEnemies();
    //     }
    //
    //     // Turret 발사 중지
    //     if (Turret.Instance != null)
    //     {
    //         Turret.Instance.EnableShooting(false);
    //     }
    //
    //     // Wave 증가 (승리 시에만)
    //     if (victory)
    //     {
    //         _currentWave++;
    //         if (_currentWave > maxWave)
    //         {
    //             Debug.Log("🎉 게임 클리어!");
    //             _currentWave = maxWave;
    //         }
    //     }
    //
    //     // Base 복귀 시작
    //     if (ShipStationMovementController.Instance != null)
    //     {
    //         StartCoroutine(ShipStationMovementController.Instance.MoveIn());
    //     }
    //
    //     // 5초 후 Ready Phase로
    //     StartCoroutine(ReturnToReadyPhase());
    // }
    //
    // // 플레이어 0명일 때 즉시 Ready로
    // void ForceReturnToReady()
    // {
    //
    //     // Enemy 제거
    //     if (Enemy.Instance != null)
    //     {
    //         Enemy.Instance.StopSpawning();
    //         Enemy.Instance.ClearAllEnemies();
    //     }
    //
    //     // Turret 발사 중지
    //     if (Turret.Instance != null)
    //     {
    //         Turret.Instance.EnableShooting(false);
    //     }
    //
    //     // Base 즉시 원위치
    //     if (ShipStationMovementController.Instance != null)
    //     {
    //         ShipStationMovementController.Instance.ResetPosition();
    //     }
    //
    //     // Ready Phase로
    //     _currentPhase = "ready";
    //     _phaseTimer = _readyTimer;
    //     _gameStarted = false;
    //     _remainingCargo = 4;
    //
    //     // Cargo 리셋
    //     if (CargoHitController.Instance != null)
    //     {
    //         CargoHitController.Instance.ResetCargo();
    //     }
    //
    //     // Interface 업데이트
    //     if (Interface.Instance != null)
    //     {
    //         Interface.Instance.ShowReadyPhase();
    //     }
    //
    //     // 모바일로 Phase 변경 전송
    //     if (Server.Instance != null)
    //     {
    //         Server.Instance.BroadcastPhaseChange("ready");
    //     }
    // }
    //
    // IEnumerator ReturnToReadyPhase()
    // {
    //     // 5초 대기 (Base가 돌아오는 동안)
    //     yield return new WaitForSeconds(5f);
    //
    //     _currentPhase = "ready";
    //     _phaseTimer = _readyTimer;
    //     _gameStarted = false;
    //     _remainingCargo = 4;
    //
    //     // Cargo 리셋
    //     if (CargoHitController.Instance != null)
    //     {
    //         CargoHitController.Instance.ResetCargo();
    //     }
    //
    //     // Interface 업데이트
    //     if (Interface.Instance != null)
    //     {
    //         Interface.Instance.ShowReadyPhase();
    //     }
    //
    //     // 모바일로 Phase 변경 전송
    //     if (Server.Instance != null)
    //     {
    //         Server.Instance.BroadcastPhaseChange("ready");
    //     }
    // }
    //
    // // Cargo 감소
    // public void DecreaseCargo()
    // {
    //     _remainingCargo--;
    //     Debug.Log($"💥 Cargo 감소: {_remainingCargo}/4");
    //
    //     if (Interface.Instance != null)
    //     {
    //         Interface.Instance.UpdateCargoCount(_remainingCargo);
    //     }
    //
    //     // ✅ 즉시 체크: Cargo가 0이면 패배
    //     if (_remainingCargo <= 0)
    //     {
    //         Debug.Log("🚨 Cargo 0개! 패배 처리 시작");
    //         EndCombatPhase(false);
    //     }
    // }
}