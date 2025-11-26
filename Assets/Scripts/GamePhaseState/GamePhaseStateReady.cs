using System.Collections;
using UnityEngine;

public class GamePhaseStateReady : IGamePhaseState {
    private readonly GCEnumManager.GAMEPHASE _gamePhase;
    
    private GamePhaseStateManager _gamePhaseStateManager;
    private int _phaseTimer;

    
    public GamePhaseStateReady(GamePhaseStateManager manager) {
        this._gamePhase = GCEnumManager.GAMEPHASE.PREPARE;
        this._gamePhaseStateManager = manager;
        this._phaseTimer = 1;
    }

    public void Enter() {
        this._gamePhaseStateManager.SetGamePhase(this._gamePhase);              // ENUM 설정; 외부 판별용
        
        // TODO: 준비 단계 진입 시 수행할 작업 구현;
        ShipStationManager.OnStationMoveIn.Invoke();                // Station Move In; OK
        CargunShipManager.OnTurretActivate.Invoke(false);           // Turret OFF; OK
        EnemyManager.OnEnemySpawnActivate.Invoke(false);            // Enemy Spawn OFF; OK
        
        Execute();
    }

    public void Execute() {
        // TODO: 준비 단계에서 수행할 작업 구현;
        this._gamePhaseStateManager.Owner.StartCoroutine(ReadyPhase());
    }

    public void Exit() {
        // TODO: 준비 단계 탈출 시 수행할 작업 구현;
        this._phaseTimer = 1;
    }
    
    private IEnumerator ReadyPhase() {
        this._gamePhaseStateManager.IsPhaseRunning = true;
        
        while (this._phaseTimer > 0) {
            this._phaseTimer -= 1;
            // Debug.Log("Ready: " + this._phaseTimer);
            
            yield return new WaitForSeconds(1f);
        }

        this._gamePhaseStateManager.IsPhaseRunning = false;
    }
}