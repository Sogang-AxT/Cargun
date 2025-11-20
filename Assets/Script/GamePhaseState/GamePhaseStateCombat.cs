using System.Collections;
using UnityEngine;

public class GamePhaseStateCombat : IGamePhaseState {
    private readonly GC_EnumManager.GAMEPHASE _gamePhase;
    
    private GamePhaseStateManager _gamePhaseStateManager;

    
    public GamePhaseStateCombat(GamePhaseStateManager manager) {
        this._gamePhase = GC_EnumManager.GAMEPHASE.COMBAT;
        this._gamePhaseStateManager = manager;
    }

    public void Enter() {
        this._gamePhaseStateManager.SetGamePhase(this._gamePhase);            // ENUM 설정; 외부 판별용
        
        // TODO: 전투 단계 진입 시 수행할 작업 구현;
        ShipStationManager.OnStationMoveOut.Invoke();           // Station Move Out
        CargunShipManager.OnTurretActivate.Invoke(true);        // Turret ON
        EnemyManager.OnEnemySpawnActivate.Invoke(true);         // Enemy Spawn ON
        
        Execute();
    }

    public void Execute() {
        this._gamePhaseStateManager.IsPhaseRunning = true;

        while (true) {
            if (this._gamePhaseStateManager.IsGameOver) {
                return;
            }
            // TODO: 전투 단계에서 수행할 작업 구현;
        }
        
        this._gamePhaseStateManager.IsPhaseRunning = false;
    }

    public void Exit() {
        // TODO: 전투 단계 탈출 시 수행할 작업 구현;
    }
}