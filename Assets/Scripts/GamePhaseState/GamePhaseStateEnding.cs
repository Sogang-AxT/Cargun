using UnityEngine;

public class GamePhaseStateEnding : MonoBehaviour, IGamePhaseState {
    private readonly GCEnumManager.GAMEPHASE _gamePhase;

    private GamePhaseStateManager _gamePhaseStateManager;
    

    public GamePhaseStateEnding(GamePhaseStateManager manager) {
        this._gamePhaseStateManager = manager;
        this._gamePhase = GCEnumManager.GAMEPHASE.ENDING;
    }
    
    public void Enter() {
        
    }

    public void Execute() {
        
    }

    public void Exit() {
        
    }
}