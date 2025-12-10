using UnityEngine;
using System.Collections;

public class ItemSample : MonoBehaviour
{
    private Vector2 moveDirection;
    public float moveSpeed = 3f;
    private Camera mainCamera;
    private bool isOutsideCamera = false;
    private float outsideTimer = 0f;
    private float destroyDelay = 3f;

    public bool IsRepairItem = false; // 수리 아이템 여부

    void Start()
    {
        mainCamera = Camera.main;
        // 0~360도 랜덤 방향 설정
        float randomAngle = Random.Range(0f, 360f);
        moveDirection = new Vector2(
            Mathf.Cos(randomAngle * Mathf.Deg2Rad),
            Mathf.Sin(randomAngle * Mathf.Deg2Rad)
        ).normalized;
    }

    void Update()
    {
        // 설정된 방향으로 이동
        if (moveDirection != Vector2.zero)
        {
            transform.position += (Vector3)(moveDirection * moveSpeed * Time.deltaTime);
        }
        // 카메라 밖 체크
        CheckCameraBounds();
    }

    // ItemBox에서 호출할 이동 방향 설정 함수
    public void SetMoveDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
    }

    // 카메라 경계 체크
    void CheckCameraBounds()
    {
        if (mainCamera == null) return;
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);
        // 카메라 밖에 있는지 체크 (viewport 좌표: 0~1 범위)
        if (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1)
        {
            if (!isOutsideCamera)
            {
                // 처음 밖으로 나갔을 때
                isOutsideCamera = true;
                outsideTimer = 0f;
            }
            // 밖에 있는 시간 누적
            outsideTimer += Time.deltaTime;
            // 3초가 지나면 파괴
            if (outsideTimer >= destroyDelay)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            // 다시 카메라 안으로 들어오면 타이머 리셋
            isOutsideCamera = false;
            outsideTimer = 0f;
        }
    }

    // Bullet과 충돌 처리
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            // Bullet 오브젝트 비활성화 (Object Pool로 반환)
            other.gameObject.SetActive(false);

            // Item 사운드 재생 추가
            if (SFX_Manager.instance != null)
            {
                SFX_Manager.instance.PlayItemSound();
            }

            // 수리 아이템이면 ShipManager의 HP 복구
            if (IsRepairItem)
            {
                ShipManager shipManager = FindObjectOfType<ShipManager>();
                if (shipManager != null)
                {
                    shipManager.RestoreHP();
                }
            }

            // 자신은 파괴
            Destroy(gameObject);
        }
    }
}