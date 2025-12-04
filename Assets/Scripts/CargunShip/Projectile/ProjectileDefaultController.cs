using UnityEngine;

public class ProjectileDefaultController : Projectile {
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Wall") || other.gameObject.CompareTag("Enemy")) {
            Deactivate();
        }
    }
    
    protected override void Shoot() {
        this.transform.position += this.transform.up * (this.Velocity * Time.deltaTime);
    }
}