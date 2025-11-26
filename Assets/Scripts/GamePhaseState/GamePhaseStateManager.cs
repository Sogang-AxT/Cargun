using System.Collections;
using UnityEngine;

public class GamePhaseStateManager {
    public MonoBehaviour Owner { get; }
    public IGamePhaseState CurrentGamePhaseState { get; private set; }
    public GCEnumManager.GAMEPHASE CurrentGamePhase { get; private set; }
    
    public GamePhaseStateReady GamePhaseStateReady { get; }
    public GamePhaseStateCombat GamePhaseStateCombat { get; }
    public GamePhaseStateEnding GamePhaseStateEnding { get; }
    
    public bool IsGameOver { get; private set; } = false;
    public bool IsPhaseRunning { get; set; } = false;
    
    
    public GamePhaseStateManager(MonoBehaviour owner) {
        this.Owner = owner;
        GameManager.OnGameOver.AddListener(() => this.IsGameOver = true);
        
        this.GamePhaseStateReady = new (this);
        this.GamePhaseStateCombat = new (this);
        this.GamePhaseStateEnding = new (this);
        
        this.CurrentGamePhaseState = this.GamePhaseStateCombat;
        this.CurrentGamePhase = GCEnumManager.GAMEPHASE.BATTLE;
    }
    
    public void TransitionTo(IGamePhaseState nextState) {
        this.CurrentGamePhaseState?.Exit();
        this.CurrentGamePhaseState = nextState;
        this.CurrentGamePhaseState?.Enter();
    }

    public void SetGamePhase(GCEnumManager.GAMEPHASE phase) {
        this.CurrentGamePhase = phase;
        // Debug.Log("SetGamePhase: " + phase);
        ServerManager.OnBroadcastPhaseChange.Invoke(phase);
        
        // ServerManager.OnGamePhaseChange.Invoke(phase);
    }
}