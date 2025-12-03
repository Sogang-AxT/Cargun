using UnityEngine;

public class ProjectileDefaultController : Projectile {
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Wall") || other.gameObject.CompareTag("Enemy")) {
            Deactivate();
            // TODO: 적에게 데미지 부여; 이벤트로
        }
    }
    
    protected override void Shoot() {
        this.transform.position += this.transform.up * (this.Velocity * Time.deltaTime);
    }
}