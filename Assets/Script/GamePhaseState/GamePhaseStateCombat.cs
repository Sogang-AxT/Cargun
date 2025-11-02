using System.Collections;
using UnityEngine;

public class GamePhaseStateCombat : MonoBehaviour, IGamePhaseState {
    private readonly GC_EnumManager.GAMEPHASE _gamePhase;
    
    private GamePhaseStateManager _gamePhaseStateManager;
    private int _phaseTimer;

    
    public GamePhaseStateCombat(GamePhaseStateManager manager) {
        this._gamePhaseStateManager = manager;
        this._gamePhase = GC_EnumManager.GAMEPHASE.COMBAT;
        this._phaseTimer = 0;
    }

    public void Enter() {
        this._phaseTimer = this._gamePhaseStateManager.CombatTimer;
        this._gamePhaseStateManager.SetGamePhase(this._gamePhase);
        
        // TODO: 전투 단계 진입 시 수행할 작업 구현: 이벤트 호출 방식
        ShipStationManager.OnStationMoveOut.Invoke();           // Station Move Out
        CargunShipManager.OnTurretActivate.Invoke(true);        // Turret ON
        EnemySpawnManager.OnEnemySpawnActivate.Invoke(true);    // Enemy Spawn ON
        // TODO: View 업데이트; Interface.Instance.ShowCombatPhase()
        ServerManager.OnBroadcastPhaseChange.Invoke(GC_EnumManager.GAMEPHASE.COMBAT);   // 모바일로 Phase 변경 전송
        
        Execute();
    }

    public void Execute() {
        // TODO: 전투 단계에서 수행할 작업 구현; 이벤트 호출 방식
        StartCoroutine(CombatPhase());
    }

    public void Exit() {
        this._phaseTimer = 0;        
        
        // TODO: 준비 단계 탈출 시 수행할 작업 구현: 이벤트 호출 방식
        CargunShipManager.OnTurretActivate.Invoke(false);        // Turret OFF
        EnemySpawnManager.OnEnemySpawnActivate.Invoke(false);    // Enemy Spawn ON
    }

    private IEnumerator CombatPhase() {
        while (this._phaseTimer > 0) {
            this._phaseTimer -= 1;
            
            if (CargunShip.Instance.CargoShipHP <= 0) {
                break;
            }
            
            yield return new WaitForSeconds(1f);
        }
    }
}