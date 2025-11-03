using System.Collections;
using UnityEngine;

public class GamePhaseStateManager {
    public IGamePhaseState CurrentGamePhaseState { get; private set; }
    
    public GamePhaseStateReady GamePhaseStateReady { get; }
    public GamePhaseStateCombat GamePhaseStateCombat { get; }
    public GamePhaseStateEnding GamePhaseStateEnding { get; }
    
    public int ReadyTimer { get; private set; }   // 30f
    public int CombatTimer { get; private set; }  // 90f
    

    public GamePhaseStateManager(GamePhaseStateManager manager) {
        this.GamePhaseStateReady = new (manager);
        this.GamePhaseStateCombat = new (manager);
        this.GamePhaseStateEnding = new (manager);
        
        this.CurrentGamePhaseState = this.GamePhaseStateCombat;
        
        this.ReadyTimer = 30;
        this.CombatTimer = 90;
        
        this.CurrentGamePhase = GC_EnumManager.GAMEPHASE.COMBAT;
    }
    
    public void TransitionTo(IGamePhaseState nextState) {
        this.CurrentGamePhaseState?.Exit();
        this.CurrentGamePhaseState = nextState;
        this.CurrentGamePhaseState?.Enter();
    }

    public void SetGamePhase(GC_EnumManager.GAMEPHASE phase) {
        this.CurrentGamePhase = phase;
        ServerManager.OnBroadcastPhaseChange.Invoke(this.CurrentGamePhase);
    }
}