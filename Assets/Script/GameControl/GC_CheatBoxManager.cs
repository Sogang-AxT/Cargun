using UnityEngine;

public class GC_CheatBoxManager : GC_SingletonImplementer<GC_CheatBoxManager> {
    public static GC_CheatBoxManager Instance;
    private int _goldAmount = 1000;
    

    private void Update() {
        // G키: 모든 플레이어에게 골드 지급
        if (Input.GetKeyDown(KeyCode.G)) {
            GiveGoldToAllPlayers();
        }

        // R키: 즉시 Ready Phase로
        if (Input.GetKeyDown(KeyCode.R)) {
            ForceReadyPhase();
        }

        // C키: 즉시 Combat Phase로
        if (Input.GetKeyDown(KeyCode.C)) {
            ForceCombatPhase();
        }

        // W키: Wave 증가
        if (Input.GetKeyDown(KeyCode.W)) {
            IncreaseWave();
        }
    }

    // 모든 플레이어에게 골드 지급
    private void GiveGoldToAllPlayers() {
        Debug.Log($"Cheat: GiveGoldToAllPlayers() = {this._goldAmount}");

        // 서버로 골드 지급 이벤트 전송
        if (ServerManager.Instance != null) {
            var jsonData = JsonUtility.ToJson(new { gold = this._goldAmount });
            ServerManager.Instance.SendToServer("giveGold", jsonData);
        }
    }

    // 강제로 Ready Phase로
    private void ForceReadyPhase() {
        Debug.Log("Cheat: ForceReadyPhase()");

        if (GameManager.Instance != null) {
            GameManager.Instance.currentPhase = "ready";
        }

        if (InterfaceManager.Instance != null) {
            InterfaceManager.Instance.ShowReadyPhase();
        }
    }

    // 강제로 Combat Phase로
    private void ForceCombatPhase() {
        Debug.Log("Cheat: ForceCombatPhase()");

        if (GameManager.Instance != null) {
            GameManager.Instance.currentPhase = "combat";
        }

        if (InterfaceManager.Instance != null) {
            InterfaceManager.Instance.ShowCombatPhase();
        }
    }

    // Wave 증가
    private void IncreaseWave() {
        if (GameManager.Instance != null) {
            Debug.Log($"Cheat: IncreaseWave() = {GameManager.Instance.currentWave}");
            GameManager.Instance.currentWave++;
        }
    }
}