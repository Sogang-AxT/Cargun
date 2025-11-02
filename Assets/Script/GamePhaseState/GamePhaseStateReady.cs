using System.Collections;
using UnityEngine;

public class GamePhaseStateReady : MonoBehaviour, IGamePhaseState {
    private readonly GC_EnumManager.GAMEPHASE _gamePhase;
    
    private GamePhaseStateManager _gamePhaseStateManager;
    private int _phaseTimer;

    
    public GamePhaseStateReady(GamePhaseStateManager manager) {
        this._gamePhaseStateManager = manager;
        this._gamePhase = GC_EnumManager.GAMEPHASE.READY;
        this._phaseTimer = 0;
    }

    public void Enter() {
        this._phaseTimer = this._gamePhaseStateManager.ReadyTimer;
        this._gamePhaseStateManager.SetGamePhase(this._gamePhase);
        
        // TODO: 준비 단계 진입 시 수행할 작업 구현: 이벤트 호출 방식
        CargunShipManager.OnCargoActivate.Invoke(true);         // Cargo ON
        
        Execute();
    }

    public void Execute() {
        // TODO: 준비 단계에서 수행할 작업 구현; 이벤트 호출 방식
        StartCoroutine(ReadyPhase());
    }

    public void Exit() {
        this._phaseTimer = 0;
        // TODO: 준비 단계 탈출 시 수행할 작업 구현: 이벤트 호출 방식
    }
    
    private IEnumerator ReadyPhase() {
        while (this._phaseTimer > 0) {
            this._phaseTimer -= 1;
            
            
            yield return new WaitForSeconds(1f);
        }
    }
}