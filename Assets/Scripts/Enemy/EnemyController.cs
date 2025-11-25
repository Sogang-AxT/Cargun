using UnityEngine;

public class EnemyController : MonoBehaviour {
    // TODO: 하나로 묶어서 관리
    private readonly string _bulletTag = "Bullet";
    private readonly string _shipTag = "Player";
    
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private Transform targetTransform;

    private GCEnumManager.ENEMY_TYPE _enemyType;
    private Vector3 _targetDir;
    private float _currentHp;
    private float _currentSpeed;
    
    
    private void Init() {
        this._enemyType = this.enemyData.enemyType;
        this._currentHp = this.enemyData.hp;
        this._currentSpeed = this.enemyData.speed;
    }

    private void Awake() {
        Init();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag(this._bulletTag)) {
            var bullet = other.gameObject.GetComponent<Bullet>();
            var bulletDmg = bullet.Dmg;

            this._currentHp -= bulletDmg;
            EnemyReturnToPool();
        }
        else if (other.gameObject.CompareTag(this._shipTag)) {
            // TODO: 함선 공격
            this._currentHp = 0;
            EnemyReturnToPool();
        }
    }

    private void FixedUpdate() {
        MoveToShip();
    }

    private void MoveToShip() {
        this._targetDir = (this.targetTransform.position - this.gameObject.transform.position).normalized;
        this.gameObject.transform.position += this._targetDir * (this._currentSpeed * Time.deltaTime);
    }

    private void EnemyReturnToPool() {
        if (this._currentHp <= 0f) {
            // TODO: ENEMY KILLED
            Destroy(this.gameObject);
            return;
        }
    }
}