using UnityEngine;

// 함선 제어
public class CargunShipController : MonoBehaviour {
    private readonly string _enemyTag = "Enemy";
    
    [SerializeField] private CargunShipData shipData;
    
    
    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.CompareTag(this._enemyTag)) {
            var enemy = other.gameObject.GetComponent<EnemyData>();
            var enemyDmg = enemy.dmg;

            this.shipData.hp -= enemyDmg;
            CargunShipReturnToStation();
        }
    }

    private void CargunShipReturnToStation() {
        if (this.shipData.hp > 0f) {
            return;
        }
        
        GameManager.OnGameOver.Invoke();    // TODO: Game Over
    }
}