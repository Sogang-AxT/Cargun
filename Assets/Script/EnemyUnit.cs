using UnityEngine;

public class EnemyUnit : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float zigzagAmplitude = 1f; // 지그재그 폭
    public float zigzagFrequency = 2f; // 지그재그 빈도

    private int direction = 1; // 1 = 오른쪽으로, -1 = 왼쪽으로
    private Vector3 startPosition;
    private float timeOffset;
    private GameObject cargoTarget;

    void Start()
    {
        startPosition = transform.position;
        timeOffset = Random.Range(0f, 100f); // 지그재그 시작점 랜덤

        // Cargo 타겟 찾기
        if (Cargo.Instance != null)
        {
            cargoTarget = Cargo.Instance.CargoHitBox;
        }
    }

    void Update()
    {
        MoveToCargo();
    }

    // Cargo를 향해 지그재그로 이동
    void MoveToCargo()
    {
        if (cargoTarget == null) return;

        // Cargo 방향
        Vector3 targetDirection = (cargoTarget.transform.position - transform.position).normalized;

        // 지그재그 효과 (Sin 파동)
        float zigzag = Mathf.Sin((Time.time + timeOffset) * zigzagFrequency) * zigzagAmplitude;

        // 수직 방향으로 지그재그
        Vector3 perpendicular = new Vector3(-targetDirection.y, targetDirection.x, 0);
        Vector3 zigzagOffset = perpendicular * zigzag;

        // 최종 이동
        Vector3 moveDirection = targetDirection + zigzagOffset.normalized * 0.3f;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        // 화면 밖으로 나가면 제거
        if (Mathf.Abs(transform.position.x) > 15f || Mathf.Abs(transform.position.y) > 10f)
        {
            DestroyEnemy();
        }
    }

    // 방향 설정
    public void SetDirection(int dir)
    {
        direction = dir;
    }

    // Bullet과 충돌
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            // Bullet도 제거
            Destroy(other.gameObject);

            // Enemy 제거
            DestroyEnemy();
        }
    }

    // Enemy 제거
    void DestroyEnemy()
    {
        // Enemy 리스트에서 제거
        if (Enemy.Instance != null)
        {
            Enemy.Instance.RemoveEnemy(gameObject);
        }

        Destroy(gameObject);
    }
}