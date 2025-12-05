using UnityEngine;
using UnityEngine.Pool;

public class EnemyController : MonoBehaviour {
    [SerializeField] private EnemyData enemyData;

    public IObjectPool<EnemyController> EnemySpawnPool { get; set; }
    
    
    private GCEnumManager.ENEMY_TYPE _enemyType;
    
    private Vector3 _initPos;
    private float _initHp;
    
    private Vector3 _targetDir;
    private Transform _targetTransform;
    private float _currentHp;
    private float _currentSpeed;
    private bool _isDead;
    public float Damage { get; private set; }
    
    
    private void Init() {
        this._initPos = this.gameObject.transform.position;
        this._isDead = false;
        
        this._enemyType = this.enemyData.enemyType;
        this._currentHp = this.enemyData.hp;
        this._currentSpeed = this.enemyData.speed;

        this._initHp = this._currentHp;
        this.Damage = this.enemyData.dmg;
        
        this._targetTransform = EnemyManager.PlayerShip.transform;
    }

    private void Awake() {
        Init();
    }

    private void OnEnable() {
        Init();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (this._isDead) {
            return;
        }
        
        if (other.gameObject.CompareTag("Bullet")) {
            if (other.gameObject.TryGetComponent<Projectile>(out var projectile)) {
                var bulletDmg = projectile.Damage;
                this._currentHp -= bulletDmg;
            }
            
            EnemyReturnToPool();
        }
        else if (other.gameObject.CompareTag("Player")) {
            this._currentHp = 0f;
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
        if (this._currentHp > 0f || this._isDead) {
            return;
        }
        
        this._isDead = true;
        this.gameObject.transform.position = this._initPos;
        
        this.EnemySpawnPool.Release(this);
    }
}