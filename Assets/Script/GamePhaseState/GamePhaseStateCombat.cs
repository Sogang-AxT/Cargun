using System.Collections;
using UnityEngine;

public class GamePhaseStateCombat : IGamePhaseState {
    private readonly GC_EnumManager.GAMEPHASE _gamePhase;
    
    private GamePhaseStateManager _gamePhaseStateManager;
    private int _phaseTimer;

    
    public GamePhaseStateCombat(GamePhaseStateManager manager) {
        this._gamePhase = GC_EnumManager.GAMEPHASE.COMBAT;
        this._gamePhaseStateManager = manager;
        this._phaseTimer = 0;
    }

    public void Enter() {
        this._gamePhaseStateManager.SetGamePhase(this._gamePhase);  // ENUM 설정; 외부 판별용
        this._phaseTimer = this._gamePhaseStateManager.CombatPhaseTimer;      // 타이머 초기화; Combat
        
        // TODO: 전투 단계 진입 시 수행할 작업 구현; 이벤트 호출 방식
        ShipStationManager.OnStationMoveOut.Invoke();           // Station Move Out
        CargunShipManager.OnTurretActivate.Invoke(true);        // Turret ON
        EnemyManager.OnEnemySpawnActivate.Invoke(true);    // Enemy Spawn ON
        
        Execute();
    }

    public void Execute() {
        // TODO: 전투 단계에서 수행할 작업 구현;
        this._gamePhaseStateManager.Owner.StartCoroutine(CombatPhase());
    }

    public void Exit() {
        this._phaseTimer = 0;
        // TODO: 준비 단계 탈출 시 수행할 작업 구현; 이벤트 호출 방식
    }

    private IEnumerator CombatPhase() {
        while (this._phaseTimer > 0) {
            this._phaseTimer -= 1;
            // TODO: 전투 단계에서 수행할 작업 구현;
            yield return new WaitForSeconds(1f);
        }
    }
}