using UnityEngine;

public class BombBox : MonoBehaviour
{
    public GameObject FX;
    public int hp = 10;
    public float speed = 2;

    private Vector2 moveDirection;
    private bool isInsideCamera = false;
    private Camera mainCamera;
    private Vector2 cameraMin;
    private Vector2 cameraMax;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isHit = false;

    // 회전 애니메이션 관련 변수
    private float rotationSpeed = 1.5f; // 기우뚱 속도
    private float rotationAmount = 10f; // 최대 각도 (-10 ~ 10)
    private float rotationTime = 0f;

    void Start()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 원래 색상 저장
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // 대각선에 가까운 방향으로 초기 이동 방향 설정
        // 30~60도, 120~150도, 210~240도, 300~330도 중 랜덤 선택
        int quadrant = Random.Range(0, 4);
        float baseAngle = quadrant * 90f;
        float randomAngle = baseAngle + Random.Range(30f, 60f);

        moveDirection = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad),
                                     Mathf.Sin(randomAngle * Mathf.Deg2Rad)).normalized;
    }

    void Update()
    {
        UpdateCameraBounds();

        if (!isInsideCamera)
        {
            // 카메라 밖에 있을 때: (0,0,0)을 향해 이동
            MoveTowardsCenter();
        }
        else
        {
            // 카메라 안에 있을 때: 등속 직선 운동
            MoveInDirection();
            CheckBoundaryCollision();
        }

        // 기우뚱 회전 애니메이션
        UpdateRotation();
    }

    void UpdateCameraBounds()
    {
        if (mainCamera == null) return;

        // 카메라의 월드 좌표 경계 계산
        float cameraHeight = mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        Vector3 cameraPos = mainCamera.transform.position;
        cameraMin = new Vector2(cameraPos.x - cameraWidth, cameraPos.y - cameraHeight);
        cameraMax = new Vector2(cameraPos.x + cameraWidth, cameraPos.y + cameraHeight);

        // 현재 위치가 카메라 안에 있는지 체크
        Vector2 pos = transform.position;
        if (pos.x >= cameraMin.x && pos.x <= cameraMax.x &&
            pos.y >= cameraMin.y && pos.y <= cameraMax.y)
        {
            isInsideCamera = true;
        }
    }

    void MoveTowardsCenter()
    {
        // (0,0,0)을 향해 직선 이동
        Vector2 currentPos = transform.position;
        Vector2 targetPos = Vector2.zero;
        Vector2 direction = (targetPos - currentPos).normalized;

        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    void MoveInDirection()
    {
        // 현재 방향으로 등속 직선 운동
        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);
    }

    void CheckBoundaryCollision()
    {
        Vector2 pos = transform.position;
        bool collided = false;

        // 좌우 벽 충돌 체크
        if (pos.x <= cameraMin.x || pos.x >= cameraMax.x)
        {
            // X축 반사
            moveDirection.x = -moveDirection.x;

            // -5도 ~ 5도 사이의 랜덤 변화 추가
            float angleVariation = Random.Range(-5f, 5f);
            moveDirection = RotateVector(moveDirection, angleVariation);

            // 수직/수평 방지
            moveDirection = PreventVerticalHorizontal(moveDirection);

            // 경계 안으로 위치 보정
            pos.x = Mathf.Clamp(pos.x, cameraMin.x, cameraMax.x);
            collided = true;
        }

        // 상하 벽 충돌 체크
        if (pos.y <= cameraMin.y || pos.y >= cameraMax.y)
        {
            // Y축 반사
            moveDirection.y = -moveDirection.y;

            // -5도 ~ 5도 사이의 랜덤 변화 추가
            float angleVariation = Random.Range(-5f, 5f);
            moveDirection = RotateVector(moveDirection, angleVariation);

            // 수직/수평 방지
            moveDirection = PreventVerticalHorizontal(moveDirection);

            // 경계 안으로 위치 보정
            pos.y = Mathf.Clamp(pos.y, cameraMin.y, cameraMax.y);
            collided = true;
        }

        if (collided)
        {
            transform.position = pos;
            // 방향 벡터 정규화 유지
            moveDirection = moveDirection.normalized;
        }
    }

    // 벡터를 주어진 각도만큼 회전시키는 함수
    Vector2 RotateVector(Vector2 vector, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        );
    }

    // 방향이 수직/수평에 너무 가까우면 보정
    Vector2 PreventVerticalHorizontal(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 각도를 0~360 범위로 정규화
        if (angle < 0) angle += 360f;

        // 수평/수직에 너무 가까운지 체크 (±15도 이내)
        float minAngle = 15f;
        float[] dangerAngles = { 0f, 90f, 180f, 270f, 360f };

        foreach (float dangerAngle in dangerAngles)
        {
            float diff = Mathf.Abs(angle - dangerAngle);
            if (diff < minAngle)
            {
                // 15도보다 가까우면 30도로 강제 조정
                if (angle < dangerAngle)
                {
                    angle = dangerAngle - 30f;
                }
                else
                {
                    angle = dangerAngle + 30f;
                }
                break;
            }
        }

        // 새로운 방향 벡터 생성
        return new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad),
                          Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
    }

    // 기우뚱 회전 애니메이션
    void UpdateRotation()
    {
        rotationTime += Time.deltaTime * rotationSpeed;

        // Sin 함수를 사용해서 -10 ~ 10도 사이로 부드럽게 왔다갔다
        float angle = Mathf.Sin(rotationTime) * rotationAmount;

        // Z축 회전 적용
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // Bullet과 충돌 처리
    void OnTriggerEnter2D(Collider2D other)
    {
        // 카메라 안에 있을 때만 충돌 처리
        if (!isInsideCamera) return;

        if (other.CompareTag("Bullet"))
        {
            // Bullet 오브젝트 비활성화 (Object Pool로 반환)
            other.gameObject.SetActive(false);

            // HP 감소
            hp--;

            // 피격 효과
            if (!isHit && spriteRenderer != null)
            {
                StartCoroutine(HitEffect());
            }

            // HP가 0이 되면 FX 생성 후 오브젝트 파괴
            if (hp <= 0)
            {
                // FX 생성
                if (FX != null)
                {
                    GameObject fx = Instantiate(FX, transform.position, Quaternion.identity);
                    Destroy(fx, 5f);
                }

                // Bomb 오브젝트 생성
                CreateBombObject();

                Destroy(gameObject);
            }
        }
    }

    // Bomb 오브젝트 생성
    void CreateBombObject()
    {
        // 빈 오브젝트 생성
        GameObject bomb = new GameObject("Bomb");
        bomb.transform.position = transform.position;
        bomb.tag = "Bomb";

        // CircleCollider2D 추가 및 설정
        CircleCollider2D collider = bomb.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 3f;

        // 0.5초 후 파괴
        Destroy(bomb, 0.5f);
    }

    // 피격 효과 코루틴
    System.Collections.IEnumerator HitEffect()
    {
        isHit = true;

        // 빨간색으로 즉시 변경 (1, 0.5, 0.5)
        Color hitColor = new Color(1f, 0.5f, 0.5f);
        Color normalColor = new Color(1f, 1f, 1f);
        spriteRenderer.color = hitColor;

        // 0.2초에 걸쳐서 서서히 원래 색상으로 복구
        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            spriteRenderer.color = Color.Lerp(hitColor, normalColor, t);
            yield return null;
        }

        // 최종적으로 정확히 원래 색상으로 설정
        spriteRenderer.color = normalColor;

        isHit = false;
    }
}