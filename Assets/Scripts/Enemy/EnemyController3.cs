using UnityEngine;

public class EnemyController3 : MonoBehaviour
{
    public GameObject DieVFX;   //Enemy VFX
    public float moveSpeed = 1f; // 이동 속도

    private enum State { Dashing, Retreating }
    private State currentState = State.Dashing;

    private float dashDistance = 2f; // 전진 거리
    private float retreatDistance = 0.5f; // 후퇴 거리
    private float dashSpeed = 5f; // 전진 속도 (빠르게)
    private float retreatSpeed = 0.5f; // 후퇴 속도 (천천히)
    private float speedMultiplier = 0.5f; // 속도 배율 (다른 적의 절반)

    private Vector2 targetPosition;
    private float randomAngle;

    void Start()
    {
        SetNewDashTarget();
    }

    void Update()
    {
        // ShipManager 찾아서 파괴 여부 체크
        ShipManager shipManager = FindObjectOfType<ShipManager>();
        if (shipManager != null && shipManager.isDestroyed)
        {
            // 배가 파괴되면 움직임 멈춤
            return;
        }

        if (currentState == State.Dashing)
        {
            // 목표 지점으로 빠르게 이동
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, dashSpeed * moveSpeed * speedMultiplier * Time.deltaTime);

            // 목표 지점에 도달하면 후퇴 시작
            if (Vector2.Distance(transform.position, targetPosition) < 0.01f)
            {
                currentState = State.Retreating;
                SetRetreatTarget();
            }
        }
        else if (currentState == State.Retreating)
        {
            // 뒤로 천천히 후퇴
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, retreatSpeed * moveSpeed * speedMultiplier * Time.deltaTime);

            // 후퇴 완료하면 다시 전진 준비
            if (Vector2.Distance(transform.position, targetPosition) < 0.01f)
            {
                currentState = State.Dashing;
                SetNewDashTarget();
            }
        }
    }

    void SetNewDashTarget()
    {
        // (0,0) 방향 계산
        Vector2 directionToCenter = (Vector2.zero - (Vector2)transform.position).normalized;

        // -30 ~ +30도 랜덤 각도
        randomAngle = Random.Range(-30f, 30f);

        // 각도를 라디안으로 변환
        float angleInRadians = randomAngle * Mathf.Deg2Rad;

        // 방향 벡터 회전
        Vector2 rotatedDirection = new Vector2(
            directionToCenter.x * Mathf.Cos(angleInRadians) - directionToCenter.y * Mathf.Sin(angleInRadians),
            directionToCenter.x * Mathf.Sin(angleInRadians) + directionToCenter.y * Mathf.Cos(angleInRadians)
        );

        // 목표 지점 = 현재 위치 + (회전된 방향 * 거리)
        targetPosition = (Vector2)transform.position + rotatedDirection * dashDistance;
    }

    void SetRetreatTarget()
    {
        // (0,0) 반대 방향으로 후퇴
        Vector2 retreatDirection = ((Vector2)transform.position - Vector2.zero).normalized;
        targetPosition = (Vector2)transform.position + retreatDirection * retreatDistance;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Player와 충돌 시
        if (other.CompareTag("Player"))
        {
            // FindObjectOfType으로 ShipManager 찾기
            ShipManager shipManager = FindObjectOfType<ShipManager>();
            if (shipManager != null)
            {
                shipManager.ship_HP -= 1;
            }
            // Enemy 사망
            Die();
        }
        // Bullet과 충돌 시
        else if (other.CompareTag("Bullet"))
        {
            // Bullet 비활성화 (Object Pool로 반환)
            other.gameObject.SetActive(false);
            // Enemy 사망
            Die();
        }
        // Bomb과 충돌 시
        else if (other.CompareTag("Bomb"))
        {
            // Enemy 사망
            Die();
        }
    }

    void Die()
    {
        // DieVFX를 현재 위치에 생성
        if (DieVFX != null)
        {
            GameObject vfx = Instantiate(DieVFX, transform.position, Quaternion.identity);
            // 부모와 모든 자식의 ParticleSystem 재생
            ParticleSystem[] particleSystems = vfx.GetComponentsInChildren<ParticleSystem>();
            float maxDuration = 0f;
            foreach (var ps in particleSystems)
            {
                ps.Play();
                // 가장 긴 파티클 지속시간 찾기
                float duration = ps.main.duration + ps.main.startLifetime.constantMax;
                if (duration > maxDuration)
                {
                    maxDuration = duration;
                }
            }
            // VFX 재생이 끝나면 삭제
            Destroy(vfx, maxDuration);
        }
        // Enemy 오브젝트 삭제
        Destroy(gameObject);
    }
}