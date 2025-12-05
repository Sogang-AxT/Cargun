using UnityEngine;

public class ProjectileRocketController : Projectile {
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Wall") || other.gameObject.CompareTag("Enemy")) {
            Deactivate();
            // TODO: 적에게 데미지 부여; 이벤트로
        }
        else if (other.gameObject.CompareTag("Item")) {
            // TODO: 아이템 효과 이벤트 Invoke
        }
    }
    
    protected override void Shoot() {
        // TOOD: 로켓 무기 발사 구현
        throw new System.NotImplementedException();
    }
}