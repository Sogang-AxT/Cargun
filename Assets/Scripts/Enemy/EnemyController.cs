using UnityEngine;
using UnityEngine.Pool;

public class EnemyController : MonoBehaviour {
    [SerializeField] private EnemyData enemyData;

    public IObjectPool<EnemyController> EnemySpawnPool { get; set; }
    public Vector3 InitPos { get; private set; }
    
    private GCEnumManager.ENEMY_TYPE _enemyType;
    private Vector3 _targetDir;
    private Transform _targetTransform;
    private float _currentHp;
    private float _currentSpeed;
    
    
    
    private void Init() {
        this.InitPos = this.gameObject.transform.position;
        
        this._enemyType = this.enemyData.enemyType;
        this._currentHp = this.enemyData.hp;
        this._currentSpeed = this.enemyData.speed;

        this._targetTransform = EnemyManager.PlayerShip.transform;
    }

    private void Awake() {
        Init();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Bullet")) {
            var bullet = other.gameObject.GetComponent<Projectile>();
            // var bulletDmg = bullet.Dmg;
            // TODO: 적에게 데미지 부여; 이벤트로
            // this._currentHp -= bulletDmg;
            EnemyReturnToPool();
        }
        else if (other.gameObject.CompareTag("Player")) {
            // TODO: 함선 공격
            this._currentHp = 0;
            EnemyReturnToPool();
        }
    }

    private void FixedUpdate() {
        MoveToShip();
    }

    private void MoveToShip() { // TODO: 4종으로 늘리기; state pattern
        this._targetDir = (this._targetTransform.position - this.gameObject.transform.position).normalized;
        this.gameObject.transform.position += this._targetDir * (this._currentSpeed * Time.deltaTime);
    }

    private void EnemyReturnToPool() {
        if (this._currentHp > 0f) {
            return;
        }
        
        // TODO: ENEMY KILLED
        this.EnemySpawnPool.Release(this);
    }
}