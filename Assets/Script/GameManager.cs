using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Phase")]
    public string currentPhase = "ready"; // "ready" or "combat"
    public int currentWave = 1;
    public int maxWave = 10;

    [Header("Timers")]
    public float readyTimer = 30f;
    public float combatTimer = 90f;
    private float currentTimer = 0f;

    [Header("Player Count")]
    public int minPlayersToStart = 1;

    [Header("Combat State")]
    public bool isBaseMoving = false;
    public int remainingCargo = 4;

    private bool gameStarted = false;

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
        currentPhase = "ready";
        currentTimer = readyTimer;
    }

    void Update()
    {
        // 플레이어 수 확인
        int playerCount = Server.Instance != null ? Server.Instance.playerCount : 0;

        if (currentPhase == "ready")
        {
            // 1명 이상 접속 시 타이머 시작
            if (playerCount >= minPlayersToStart && !gameStarted)
            {
                gameStarted = true;
                currentTimer = readyTimer;
            }

            // 타이머 카운트다운
            if (gameStarted && playerCount >= minPlayersToStart)
            {
                currentTimer -= Time.deltaTime;

                if (currentTimer <= 0)
                {
                    StartCombatPhase();
                }
            }

            // 플레이어 0명이면 타이머 리셋
            if (playerCount == 0)
            {
                gameStarted = false;
                currentTimer = readyTimer;
            }
        }
        else if (currentPhase == "combat")
        {
            // Combat 타이머
            currentTimer -= Time.deltaTime;

            // 화물이 0이 되면 패배
            if (remainingCargo <= 0)
            {
                EndCombatPhase(false); // 패배
            }

            // 시간이 다 지나면 승리
            if (currentTimer <= 0)
            {
                EndCombatPhase(true); // 승리
            }

            // 플레이어 0명이면 즉시 Ready로
            if (playerCount == 0)
            {
                ForceReturnToReady();
            }
        }
    }

    public float GetCurrentTimer()
    {
        return currentTimer;
    }

    // Combat Phase 시작
    void StartCombatPhase()
    {
        Debug.Log("=== Combat Phase 시작 ===");
        currentPhase = "combat";
        currentTimer = combatTimer;
        remainingCargo = 4;

        // Base 이동 시작
        if (Base.Instance != null)
        {
            Base.Instance.MoveOut();
        }

        // Cargo 활성화
        if (Cargo.Instance != null)
        {
            Cargo.Instance.ResetCargo();
        }

        // Enemy 스폰 시작
        if (Enemy.Instance != null)
        {
            Enemy.Instance.StartSpawning();
        }

        // Turret 활성화
        if (Turret.Instance != null)
        {
            Turret.Instance.EnableShooting(true);
        }

        // Interface 업데이트
        if (Interface.Instance != null)
        {
            Interface.Instance.ShowCombatPhase();
        }

        // 모바일로 Phase 변경 전송
        if (Server.Instance != null)
        {
            Server.Instance.BroadcastPhaseChange("combat");
        }
    }

    // Combat Phase 종료
    void EndCombatPhase(bool victory)
    {
        Debug.Log($"=== Combat Phase 종료 ({(victory ? "승리" : "패배")}) ===");

        // Enemy 스폰 중지 및 제거
        if (Enemy.Instance != null)
        {
            Enemy.Instance.StopSpawning();
            Enemy.Instance.ClearAllEnemies();
        }

        // Turret 발사 중지
        if (Turret.Instance != null)
        {
            Turret.Instance.EnableShooting(false);
        }

        // Wave 증가 (승리 시에만)
        if (victory)
        {
            currentWave++;
            if (currentWave > maxWave)
            {
                Debug.Log("🎉 게임 클리어!");
                currentWave = maxWave;
            }
        }

        // Base 복귀 시작
        if (Base.Instance != null)
        {
            StartCoroutine(Base.Instance.MoveIn());
        }

        // 5초 후 Ready Phase로
        StartCoroutine(ReturnToReadyPhase());
    }

    // 플레이어 0명일 때 즉시 Ready로
    void ForceReturnToReady()
    {
        Debug.Log("=== 플레이어 0명 - 강제 Ready ===");

        // Enemy 제거
        if (Enemy.Instance != null)
        {
            Enemy.Instance.StopSpawning();
            Enemy.Instance.ClearAllEnemies();
        }

        // Turret 발사 중지
        if (Turret.Instance != null)
        {
            Turret.Instance.EnableShooting(false);
        }

        // Base 즉시 원위치
        if (Base.Instance != null)
        {
            Base.Instance.ResetPosition();
        }

        // Ready Phase로
        currentPhase = "ready";
        currentTimer = readyTimer;
        gameStarted = false;
        remainingCargo = 4;

        // Cargo 리셋
        if (Cargo.Instance != null)
        {
            Cargo.Instance.ResetCargo();
        }

        // Interface 업데이트
        if (Interface.Instance != null)
        {
            Interface.Instance.ShowReadyPhase();
        }

        // 모바일로 Phase 변경 전송
        if (Server.Instance != null)
        {
            Server.Instance.BroadcastPhaseChange("ready");
        }
    }

    IEnumerator ReturnToReadyPhase()
    {
        // 5초 대기 (Base가 돌아오는 동안)
        yield return new WaitForSeconds(5f);

        currentPhase = "ready";
        currentTimer = readyTimer;
        gameStarted = false;
        remainingCargo = 4;

        // Cargo 리셋
        if (Cargo.Instance != null)
        {
            Cargo.Instance.ResetCargo();
        }

        // Interface 업데이트
        if (Interface.Instance != null)
        {
            Interface.Instance.ShowReadyPhase();
        }

        // 모바일로 Phase 변경 전송
        if (Server.Instance != null)
        {
            Server.Instance.BroadcastPhaseChange("ready");
        }
    }

    // Cargo 감소
    public void DecreaseCargo()
    {
        remainingCargo--;
        Debug.Log($"Cargo 감소: {remainingCargo}/4");

        if (Interface.Instance != null)
        {
            Interface.Instance.UpdateCargoCount(remainingCargo);
        }
    }
}