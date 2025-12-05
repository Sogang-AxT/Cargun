using UnityEngine;

public class ProjectileLaserController : Projectile {
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Wall")) {
            Deactivate();
        }
        else if (other.gameObject.CompareTag("Item")) {
            // TODO: 아이템 효과 이벤트 Invoke
        }
    }
    
    protected override void Shoot() {
        this.transform.position += this.transform.up * (this.Velocity * Time.deltaTime);
    }
}