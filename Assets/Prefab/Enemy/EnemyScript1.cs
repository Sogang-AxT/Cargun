using UnityEngine;

public class EnemyMovementSimple : MonoBehaviour
{
    // --- 공통 능력치 ---
    public float HP = 10f;
    public float Size = 0.1f;

    // --- 최소 필수 설정 변수 ---
    [Header("Movement Settings")]
    public float maxSpeed = 3f; // 최대 순간 가속 속력
    public float accelerationTime = 0.1f; // 0에서 maxSpeed까지 걸리는 시간
    public float decelerationTime = 0.5f; // maxSpeed에서 0까지 걸리는 시간
    public float pauseDuration = 0f; // 정지 상태 유지 시간

    private Vector2 targetPosition = Vector2.zero;
    private float currentSpeed = 0f;
    private float timer = 0f;
    private float totalCycleTime;

    // 0: 정지 대기, 1: 가속, 2: 감속 상태를 나타내는 단순 카운터
    private int currentPhase = 0;

    void Start()
    {
        targetPosition = Vector2.zero;
        transform.localScale = new Vector3(Size, Size, 1f);

        // 전체 사이클 시간 계산 (가속 시간은 매우 짧아 무시 가능)
        // 정지 대기 (1s) + 가속 (0.1s) + 감속 (1s) -> 대략 2.1초
        totalCycleTime = pauseDuration + accelerationTime + decelerationTime;

        // 시작은 정지 대기 상태 (currentPhase = 0)
        currentPhase = 0;
        timer = 0f;
    }

    void Update()
    {
        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            Destroy(gameObject);
            return;
        }

        HandleMovementPatternSimple();

        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;

        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPosition,
            currentSpeed * Time.deltaTime
        );
    }

    private void HandleMovementPatternSimple()
    {
        timer += Time.deltaTime;

        if (currentPhase == 0) // 정지 대기 (PauseDuration 만큼)
        {
            currentSpeed = 0f;
            if (timer >= pauseDuration)
            {
                currentPhase = 1; // 가속으로 전환
                timer = 0f; // 타이머 리셋
            }
        }
        else if (currentPhase == 1) // 가속 (AccelerationTime 동안 0에서 maxSpeed까지)
        {
            // timer / accelerationTime 은 0에서 1까지 증가하는 비율
            currentSpeed = Mathf.Lerp(0f, maxSpeed, timer / accelerationTime);

            if (timer >= accelerationTime)
            {
                currentPhase = 2; // 감속으로 전환
                timer = 0f; // 타이머 리셋
            }
        }
        else if (currentPhase == 2) // 감속 (DecelerationTime 동안 maxSpeed에서 0까지)
        {
            // timer / decelerationTime 은 0에서 1까지 증가하는 비율
            // Mathf.Lerp(maxSpeed, 0f, 비율)로 서서히 0에 도달하게 함
            currentSpeed = Mathf.Lerp(maxSpeed, 0f, timer / decelerationTime);

            // 속도가 거의 0이 되거나 감속 시간이 완료되면
            if (currentSpeed <= 0.01f || timer >= decelerationTime)
            {
                currentPhase = 0; // 정지 대기로 전환
                timer = 0f; // 타이머 리셋
            }
        }
    }
}