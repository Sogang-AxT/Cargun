using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

// TODO: View
public class InterfaceManager : MonoBehaviour {
    public static InterfaceManager Instance;

    [Header("QR Code")]
    public GameObject QRCode;

    [Header("Ready Phase UI")]
    public GameObject ReadyPhase;
    public Text Timer_Ready;
    public Text PlayerNum_Ready;
    public Text Wave_Ready;
    public Text[] PlayerName_Ready; // 4개 (플레이어 1~4)

    [Header("Combat Phase UI")]
    public GameObject CombatPhase;
    public Text Wave_Combat;
    public Text CargoNum_Combat;
    public Text Timer_Combat;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 초기 상태: QR만 표시
        ShowQROnly();
    }

    void Update()
    {
        // 타이머 업데이트
        if (GameManager.Instance != null)
        {
            float timer = GameManager.Instance.GetCurrentTimer();
            int minutes = Mathf.FloorToInt(timer / 60);
            int seconds = Mathf.FloorToInt(timer % 60);

            if (GameManager.Instance.currentPhase == "ready" && Timer_Ready != null)
            {
                Timer_Ready.text = $"{minutes}:{seconds:00}";
            }
            else if (GameManager.Instance.currentPhase == "combat" && Timer_Combat != null)
            {
                Timer_Combat.text = $"{minutes}:{seconds:00}";

                // ✅ 디버깅: 타이머 값 확인 (5초마다)
                if (Time.frameCount % 300 == 0)
                {
                    Debug.Log($"[Interface Timer] Combat Timer: {minutes}:{seconds:00} (raw: {timer:F2})");
                }
            }
        }
        else
        {
            // ✅ GameManager가 없으면 경고
            if (Time.frameCount % 300 == 0)
            {
                Debug.LogWarning("[Interface] GameManager.Instance is NULL!");
            }
        }

        // Wave 업데이트
        if (GameManager.Instance != null)
        {
            int wave = GameManager.Instance.currentWave;
            int maxWave = GameManager.Instance.maxWave;

            if (Wave_Ready != null)
            {
                Wave_Ready.text = $"WAVE {wave}/{maxWave}";
            }

            if (Wave_Combat != null)
            {
                Wave_Combat.text = $"WAVE {wave}/{maxWave}";
            }
        }

        // 플레이어 수 업데이트
        UpdatePlayerCount();
    }

    // QR 코드만 표시
    void ShowQROnly()
    {
        if (QRCode != null) QRCode.SetActive(true);
        if (ReadyPhase != null) ReadyPhase.SetActive(false);
        if (CombatPhase != null) CombatPhase.SetActive(false);
    }

    // Ready Phase 표시
    public void ShowReadyPhase()
    {
        int playerCount = ServerManager.Instance != null ? ServerManager.Instance.playerCount : 0;

        if (playerCount == 0)
        {
            // 플레이어 0명: QR만
            ShowQROnly();
        }
        else if (playerCount >= 1 && playerCount <= 3)
        {
            // 플레이어 1~3명: QR + Ready
            if (QRCode != null) QRCode.SetActive(true);
            if (ReadyPhase != null) ReadyPhase.SetActive(true);
            if (CombatPhase != null) CombatPhase.SetActive(false);
        }
        else if (playerCount >= 4)
        {
            // 플레이어 4명: QR 숨김, Ready만
            if (QRCode != null) QRCode.SetActive(false);
            if (ReadyPhase != null) ReadyPhase.SetActive(true);
            if (CombatPhase != null) CombatPhase.SetActive(false);
        }

        UpdatePlayerList();
    }

    // Combat Phase 표시
    public void ShowCombatPhase()
    {
        if (QRCode != null) QRCode.SetActive(false);
        if (ReadyPhase != null) ReadyPhase.SetActive(false);
        if (CombatPhase != null) CombatPhase.SetActive(true);

        // Cargo 초기화
        UpdateCargoCount(4);
    }

    // ✅ 플레이어 목록 업데이트 (슬롯 기반)
    public void UpdatePlayerList()
    {
        if (ServerManager.Instance == null || PlayerName_Ready == null) return;

        // ✅ 4개의 고정 슬롯을 만들어서 관리
        string[] slotNames = new string[4] { "", "", "", "" };

        // ✅ 플레이어를 슬롯 번호에 맞춰 배치
        Debug.Log($"=== UpdatePlayerList: 총 {ServerManager.Instance.players.Count}명 ===");
        foreach (var player in ServerManager.Instance.players.Values)
        {
            Debug.Log($"Player: {player.nickname}, Slot: {player.slot}, Color: {player.color}");

            if (player.slot >= 1 && player.slot <= 4)
            {
                slotNames[player.slot - 1] = player.nickname;
            }
        }

        // ✅ UI에 표시 (빈 슬롯은 공란)
        for (int i = 0; i < PlayerName_Ready.Length && i < 4; i++)
        {
            if (PlayerName_Ready[i] != null)
            {
                PlayerName_Ready[i].text = slotNames[i];
                Debug.Log($"Slot {i + 1}: {slotNames[i]}");
            }
        }
    }

    // 플레이어 수 업데이트
    void UpdatePlayerCount()
    {
        if (PlayerNum_Ready != null && ServerManager.Instance != null)
        {
            int count = ServerManager.Instance.playerCount;
            PlayerNum_Ready.text = $"({count}/4)";
        }
    }

    // Cargo 수량 업데이트
    public void UpdateCargoCount(int count)
    {
        if (CargoNum_Combat != null)
        {
            CargoNum_Combat.text = $"{count}/4";
        }
    }
}