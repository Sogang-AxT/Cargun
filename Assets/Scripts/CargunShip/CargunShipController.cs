using UnityEngine;


public class CargunShipController : MonoBehaviour {
    private float _hp;

    
    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.CompareTag("Enemy")) {
            if (other.gameObject.TryGetComponent<EnemyController>(out var enemy)) {
                this._hp -= enemy.Damage;
                // TODO: VFX
            }
            
            if (this._hp <= 0f) {
                GameManager.OnGameOver.Invoke();    // TODO: Game Over
            }
        }
    }
}