using UnityEngine;
using UnityEngine.Serialization;

// 함선 제어
public class CargunShipController : MonoBehaviour {
    private readonly string _enemyTag = "Enemy";
    public float Hp { get; private set; }

    
    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.CompareTag(this._enemyTag)) {
            var enemy = other.gameObject.GetComponent<EnemyData>();
            var enemyDmg = enemy.dmg;

            this.Hp -= enemyDmg;
            CargunShipReturnToStation();
        }
    }

    private void CargunShipReturnToStation() {
        if (this.Hp > 0f) {
            return;
        }
        
        GameManager.OnGameOver.Invoke();    // TODO: Game Over
    }
}