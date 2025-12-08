using UnityEngine;

public class EnemyController2 : MonoBehaviour
{
    public GameObject DieVFX;   //Enemy VFX
    public float moveSpeed = 1f; // 이동 속도

    private float zigzagTimer = 0f;
    private float zigzagFrequency = 2f; // 지그재그 주파수 (높을수록 빠르게 좌우 전환)
    private float zigzagAmplitude = 3f; // 지그재그 진폭 (좌우 이동 거리)
    private float forwardSpeedMultiplier = 0.5f; // 전진 속도 배율

    void Start()
    {
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

        // 타이머 업데이트 (고정 속도로 지그재그)
        zigzagTimer += Time.deltaTime;

        // (0, 0)을 향한 전진 방향
        Vector2 forwardDirection = (Vector2.zero - (Vector2)transform.position).normalized;

        // 전진 방향에 수직인 방향 (지그재그용)
        Vector2 perpendicular = new Vector2(-forwardDirection.y, forwardDirection.x);

        // 사인 함수로 부드러운 좌우 이동 (-1 ~ 1 사이 값)
        float sineWave = Mathf.Sin(zigzagTimer * zigzagFrequency);

        // 전진 이동 (moveSpeed에 비례) + 사인파 기반 지그재그 이동 (고정)
        Vector2 forwardMovement = forwardDirection * moveSpeed * forwardSpeedMultiplier;
        Vector2 zigzagMovement = perpendicular * sineWave * zigzagAmplitude;

        transform.position += (Vector3)((forwardMovement + zigzagMovement) * Time.deltaTime);
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