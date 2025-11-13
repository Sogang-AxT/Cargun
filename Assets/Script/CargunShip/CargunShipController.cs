using System;
using UnityEngine;

// 함선 제어
public class CargunShipController : MonoBehaviour {
    private readonly string _enemyTag = "Enemy";
    
    [SerializeField] private CargunShipData shipData;

    private float _shipHp;
    private int _upgradeLv;
    
    
    private void Init() {
        this._shipHp = this.shipData.ShipHp;
        this._upgradeLv = this.shipData.UpgradeLevel;
    }

    private void Awake() {
        Init();
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.CompareTag(this._enemyTag)) {
            var enemy = other.gameObject.GetComponent<EnemyData>();
            var enemyDmg = enemy.dmg;

            this._shipHp -= enemyDmg;
            CargunShipReturnToStation();
        }
    }

    private void CargunShipReturnToStation() {
        if (this.shipData.ShipHp <= 0f) {
            // TODO: Game Over
            GameManager.OnGameOver.Invoke();
            return;
        }
    }
}