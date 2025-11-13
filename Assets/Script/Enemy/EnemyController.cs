using UnityEngine;

public class EnemyController : MonoBehaviour {
    private readonly string _bulletTag = "bullet";
    
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private Transform targetTransform;

    // private GC_EnumManager.ENEMY_TYPE _enemyType;
    // private float _enemyHp;
    // private float _enemyDmg;
    // private float _moveSpeed;
    
    private Vector3 _targetDir;

    
    private void Init() {
        // this._enemyType = this.enemyData.enemyType;
        // this._enemyHp = this.enemyData.hp;
        // this._enemyDmg = this.enemyData.dmg;
        // this._moveSpeed = this.enemyData.moveSpeed;
    }

    private void Awake() {
        Init();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag(this._bulletTag)) {
            var bullet = other.gameObject.GetComponent<Bullet>();
            var bulletDmg = bullet.Dmg;

            this.enemyData.hp -= bulletDmg;
            EnemyReturnToPool();
        }
    }

    private void FixedUpdate() {
        MoveToShip();
    }

    private void MoveToShip() {
        this._targetDir = (this.targetTransform.position - this.gameObject.transform.position).normalized;
        // TODO: Zig-Zag
        this.gameObject.transform.position += this._targetDir * (this.enemyData.speed * Time.deltaTime);
    }

    private void EnemyReturnToPool() {
        if (this.enemyData.hp <= 0f) {
            // TODO: ENEMY KILLED
            return;
        }
    }
}