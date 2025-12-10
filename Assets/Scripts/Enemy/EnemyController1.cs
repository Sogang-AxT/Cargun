using UnityEngine;
using System.Collections;
public class EnemyController1 : MonoBehaviour
{
    public GameObject DieVFX;   //Enemy VFX
    public float moveSpeed = 1f; // 이동 속도
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
        // (0, 0)을 향해 이동
        Vector2 direction = (Vector2.zero - (Vector2)transform.position).normalized;
        transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);
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
        // Hit 사운드 재생
        if (SFX_Manager.instance != null)
        {
            SFX_Manager.instance.PlayRandomHitSound();
        }

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