using System.Collections;
using UnityEngine;

public class GamePhaseStateCombat : IGamePhaseState {
    private readonly GC_EnumManager.GAMEPHASE _gamePhase;
    
    private GamePhaseStateManager _gamePhaseStateManager;

    
    public GamePhaseStateCombat(GamePhaseStateManager manager) {
        this._gamePhase = GC_EnumManager.GAMEPHASE.BATTLE;
        this._gamePhaseStateManager = manager;
    }

    public void Enter() {
        this._gamePhaseStateManager.SetGamePhase(this._gamePhase);            // ENUM 설정; 외부 판별용
        
        ShipStationManager.OnStationMoveOut.Invoke();           // Station Move Out
        CargunShipManager.OnTurretActivate.Invoke(true);        // Turret ON
        EnemyManager.OnEnemySpawnActivate.Invoke(true);         // Enemy Spawn ON
        
        Execute();
    }

    public void Execute() {
        if (this._gamePhaseStateManager.IsGameOver) {
            this._gamePhaseStateManager.IsPhaseRunning = false;
            return;
        }
        
        this._gamePhaseStateManager.IsPhaseRunning = true;

        // TODO: 비용 문제 발생 중
        if ((GameData.Instance.CurrentScore % GameData.Instance.LevelUpScoreStep) == 0 
            && GameData.Instance.CurrentScore >= 500) {
            GameData.Instance.CurrentLevel += 1;
        }
    }

    public void Exit() {
        // TODO: 전투 단계 탈출 시 수행할 작업 구현;
        CargunShipManager.OnTurretActivate.Invoke(false);        // Turret OFF
        EnemyManager.OnEnemySpawnActivate.Invoke(false);         // Enemy Spawn OFF
    }
}