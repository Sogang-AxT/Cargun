using System.Collections;
using UnityEngine;

public class GamePhaseStateCombat : MonoBehaviour, IGamePhaseState {
    private readonly GC_EnumManager.GAMEPHASE _gamePhase;
    
    private GamePhaseStateController _gamePhaseStateController;
    private int _phaseTimer;

    
    public GamePhaseStateCombat(GamePhaseStateController controller) {
        this._gamePhaseStateController = controller;
        this._gamePhase = GC_EnumManager.GAMEPHASE.COMBAT;
        this._phaseTimer = 0;
    }

    public void Enter() {
        this._phaseTimer = this._gamePhaseStateController.CombatTimer;
        this._gamePhaseStateController.SetGamePhase(this._gamePhase);
        
        Execute();
    }

    public void Execute() {
        StartCoroutine(CombatPhase());
    }

    public void Exit() {
        this._phaseTimer = 0;
    }

    private IEnumerator CombatPhase() {
        while (this._phaseTimer > 0) {
            this._phaseTimer -= 1;
            
            if (CargoShip.Instance.CargoShipHP <= 0) {
                break;
            }
            
            ShipStationManager.OnStationMoveOut.Invoke();
            
            
            // TODO: 전투 단계에서 수행할 작업 구현; 이벤트 호출 방식
            yield return new WaitForSeconds(1f);
        }
    }
}