using System.Collections;
using UnityEngine;

public class GamePhaseStateReady : MonoBehaviour, IGamePhaseState {
    private readonly GC_EnumManager.GAMEPHASE _gamePhase;
    
    private GamePhaseStateController _gamePhaseStateController;
    private int _phaseTimer;

    
    public GamePhaseStateReady(GamePhaseStateController controller) {
        this._gamePhaseStateController = controller;
        this._gamePhase = GC_EnumManager.GAMEPHASE.READY;
        this._phaseTimer = 0;
    }

    public void Enter() {
        this._phaseTimer = this._gamePhaseStateController.ReadyTimer;
        this._gamePhaseStateController.SetGamePhase(this._gamePhase);
        Execute();
    }

    public void Execute() {
        StartCoroutine(ReadyPhase());
    }

    public void Exit() {
        this._phaseTimer = 0;
        // TODO: 준비 단계 탈출 시 수행할 작업 구현: 이벤트 호출 방식
    }
    
    private IEnumerator ReadyPhase() {
        while (this._phaseTimer > 0) {
            this._phaseTimer -= 1;
            // TODO: 준비 단계에서 수행할 작업 구현; 이벤트 호출 방식
            yield return new WaitForSeconds(1f);
        }
    }
}