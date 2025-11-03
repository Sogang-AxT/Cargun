using System.Collections;
using UnityEngine;

public class GamePhaseStateReady : MonoBehaviour, IGamePhaseState {
    private readonly GC_EnumManager.GAMEPHASE _gamePhase;
    
    private GamePhaseStateManager _gamePhaseStateManager;
    private int _phaseTimer;

    
    public GamePhaseStateReady(GamePhaseStateManager manager) {
        this._gamePhase = GC_EnumManager.GAMEPHASE.READY;
        this._gamePhaseStateManager = manager;
        this._phaseTimer = 0;
    }

    public void Enter() {
        this._gamePhaseStateManager.SetGamePhase(this._gamePhase);  // ENUM 설정; 외부 판별용
        this._phaseTimer = this._gamePhaseStateManager.ReadyTimer;  // 타이머 초기화; Ready
        
        // TODO: 준비 단계 진입 시 수행할 작업 구현; 이벤트 호출 방식
        ShipStationManager.OnStationMoveIn.Invoke();                // Station Move In
        CargunShipManager.OnTurretActivate.Invoke(false);           // Turret OFF
        EnemySpawnManager.OnEnemySpawnActivate.Invoke(false);       // Enemy Spawn OFF
        CargunShipManager.OnCargoActivate.Invoke(true);             // Cargo Reset ON
        
        // View 업데이트; Interface.Instance.ShowCombatPhase()
        
        Execute();
    }

    public void Execute() {
        // TODO: 준비 단계에서 수행할 작업 구현;
        StartCoroutine(ReadyPhase());
    }

    public void Exit() {
        this._phaseTimer = 0;
        
        // TODO: 준비 단계 탈출 시 수행할 작업 구현; 이벤트 호출 방식
        ShipStationManager.OnStationMoveOut.Invoke();           // Station Move Out
        CargunShipManager.OnTurretActivate.Invoke(true);        // Turret ON
        EnemySpawnManager.OnEnemySpawnActivate.Invoke(true);    // Enemy Spawn ON
    }
    
    private IEnumerator ReadyPhase() {
        while (this._phaseTimer > 0) {
            this._phaseTimer -= 1;
            // TODO: 준비 단계에서 수행할 작업 구현;
            yield return new WaitForSeconds(1f);
        }
    }
}