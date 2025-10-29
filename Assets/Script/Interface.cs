using UnityEngine;
using UnityEngine.UI;

public class Interface : MonoBehaviour
{
    public static Interface Instance;

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
        int playerCount = Server.Instance != null ? Server.Instance.playerCount : 0;

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

    // 플레이어 목록 업데이트
    public void UpdatePlayerList()
    {
        if (Server.Instance == null) return;

        int count = 0;
        foreach (var player in Server.Instance.players.Values)
        {
            if (count < PlayerName_Ready.Length && PlayerName_Ready[count] != null)
            {
                PlayerName_Ready[count].text = player.nickname;
            }
            count++;
        }

        // 나머지 칸은 비움
        for (int i = count; i < PlayerName_Ready.Length; i++)
        {
            if (PlayerName_Ready[i] != null)
            {
                PlayerName_Ready[i].text = "";
            }
        }
    }

    // 플레이어 수 업데이트
    void UpdatePlayerCount()
    {
        if (PlayerNum_Ready != null && Server.Instance != null)
        {
            int count = Server.Instance.playerCount;
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