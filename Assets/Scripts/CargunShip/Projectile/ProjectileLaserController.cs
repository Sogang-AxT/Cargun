using UnityEngine;

public class ProjectileLaserController : Projectile {
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Wall")) {
            Deactivate();
        }
        else if (other.gameObject.CompareTag("Enemy")) {
            // TODO: 적에게 데미지 부여; 이벤트로
        }
        else if (other.gameObject.CompareTag("Item")) {
            // TODO: 아이템 효과 이벤트 Invoke
        }
    }
    
    protected override void Shoot() {
        // TODO: 레이저 무기 발사 구현
        throw new System.NotImplementedException();
    }
}