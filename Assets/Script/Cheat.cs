using UnityEngine;

public class Cheat : MonoBehaviour
{
    [Header("Cheat Keys")]
    public int goldAmount = 1000;

    void Update()
    {
        // G키: 모든 플레이어에게 골드 지급
        if (Input.GetKeyDown(KeyCode.G))
        {
            GiveGoldToAllPlayers();
        }

        // R키: 즉시 Ready Phase로
        if (Input.GetKeyDown(KeyCode.R))
        {
            ForceReadyPhase();
        }

        // C키: 즉시 Combat Phase로
        if (Input.GetKeyDown(KeyCode.C))
        {
            ForceCombatPhase();
        }

        // W키: Wave 증가
        if (Input.GetKeyDown(KeyCode.W))
        {
            IncreaseWave();
        }
    }

    // 모든 플레이어에게 골드 지급
    void GiveGoldToAllPlayers()
    {
        Debug.Log($"💰 치트: 모든 플레이어에게 {goldAmount} 골드 지급");

        // 서버로 골드 지급 이벤트 전송
        if (Server.Instance != null)
        {
            string jsonData = JsonUtility.ToJson(new { gold = goldAmount });
            Server.Instance.SendToServer("giveGold", jsonData);
        }
    }

    // 강제로 Ready Phase로
    void ForceReadyPhase()
    {
        Debug.Log("🔄 치트: 강제 Ready Phase");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentPhase = "ready";
        }

        if (Interface.Instance != null)
        {
            Interface.Instance.ShowReadyPhase();
        }
    }

    // 강제로 Combat Phase로
    void ForceCombatPhase()
    {
        Debug.Log("⚔️ 치트: 강제 Combat Phase");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentPhase = "combat";
        }

        if (Interface.Instance != null)
        {
            Interface.Instance.ShowCombatPhase();
        }
    }

    // Wave 증가
    void IncreaseWave()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentWave++;
            Debug.Log($"📈 치트: Wave {GameManager.Instance.currentWave}");
        }
    }
}