using System.Collections;
using UnityEngine;

public class GamePhaseStateManager {
    public IGamePhaseState CurrentGamePhaseState { get; private set; }
    public GC_EnumManager.GAMEPHASE CurrentGamePhase { get; private set; }
    
    public GamePhaseStateReady GamePhaseStateReady { get; }
    public GamePhaseStateCombat GamePhaseStateCombat { get; }
    public GamePhaseStateEnding GamePhaseStateEnding { get; }
    
    public int ReadyPhaseTimer { get; private set; } = 30;
    public int CombatPhaseTimer { get; private set; } = 90;
    
    
    public GamePhaseStateManager(GamePhaseStateManager manager) {
        this.GamePhaseStateReady = new (manager);
        this.GamePhaseStateCombat = new (manager);
        this.GamePhaseStateEnding = new (manager);
        
        this.CurrentGamePhaseState = this.GamePhaseStateCombat;
        this.CurrentGamePhase = GC_EnumManager.GAMEPHASE.COMBAT;
    }
    
    public void TransitionTo(IGamePhaseState nextState) {
        this.CurrentGamePhaseState?.Exit();
        this.CurrentGamePhaseState = nextState;
        this.CurrentGamePhaseState?.Enter();
    }

    public void SetGamePhase(GC_EnumManager.GAMEPHASE phase) {
        this.CurrentGamePhase = phase;
        // ServerManager.OnGamePhaseChange.Invoke(phase);
    }
}