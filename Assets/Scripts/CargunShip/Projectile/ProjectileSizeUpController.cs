using UnityEngine;

public class ProjectileSizeUpController : Projectile {
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Wall")) {
            Deactivate();
        }
    }
    
    protected override void Shoot() {
        this.transform.position += this.transform.up * (this.Velocity * Time.deltaTime);
    }
}