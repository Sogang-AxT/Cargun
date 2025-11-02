using UnityEngine;

public class GamePhaseStateEnding : MonoBehaviour, IGamePhaseState {
    private readonly GC_EnumManager.GAMEPHASE _gamePhase;

    private GamePhaseStateController _gamePhaseStateController;
    

    public GamePhaseStateEnding(GamePhaseStateController controller) {
        this._gamePhaseStateController = controller;
        this._gamePhase = GC_EnumManager.GAMEPHASE.ENDING;
    }
    
    public void Enter() {
        
    }

    public void Execute() {
        
    }

    public void Exit() {
        
    }
}