using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

// 함선 터릿 제어
public class CargunShipTurretController : MonoBehaviour {
    [SerializeField] private Sprite offlineTurretSprite;
    [SerializeField] private Sprite onlineTurretSprite;
    [Space(10f)]
    [SerializeField] private GCEnumManager.TURRET_TYPE turretType;
    [SerializeField] private Transform turretMuzzle;
    
    private Sprite _turretSprite;
    private Coroutine _turretFireCoroutine;
    private float _fireRate;    // TODO: 스폰 타임으로 접근해야
    private bool _isControlling;
    private bool _isAssigned;
    private int _turretPlayer;
    
    [Space(25f)]
    
    [Header("Projectile")]
    [SerializeField] private ProjectileController projectilePrefab;
    [SerializeField] private bool collectionCheck;
    [SerializeField] private int defaultPoolCapacity;
    [SerializeField] private int maxSize;
    [SerializeField] private float muzzleVelocity;
    
    private IObjectPool<ProjectileController> _projectileSpawnPool;
    
    
    private void Init() {
        CargunShipManager.OnTurretActivate.AddListener(TurretActivate);
        GameManager.OnTurretAssign.AddListener(TurretAssign);

        this._turretSprite = gameObject.GetComponent<SpriteRenderer>().sprite;
        this._isAssigned = false;
        this._fireRate = 0.5f;

        this._projectileSpawnPool = new ObjectPool<ProjectileController>(
            BulletSpawn, OnGetFromPool, OnReleaseToPool, OnDestroyPooledBullet,
            this.collectionCheck, this.defaultPoolCapacity, this.maxSize);
    }
    
    private void Awake() {
        Init();
    }

    private void Update() {
        TurretRotate();
    }

    private void TurretAssign(int player) {
        if (ServerDataManager.Turret_Player[(int)this.turretType] != player) {
            // Debug.Log(this.turretType + " - UnAssigned!");
            this._turretSprite = this.offlineTurretSprite;
            return;
        }
        
        this._turretPlayer = player;
        this._turretSprite = this.onlineTurretSprite;
        this._isAssigned = true;
    }
    
    private void TurretActivate(bool isActivate) {
        // Debug.Log("isActivate: " + isActivate);

        if (!this._isAssigned) {
            return;
        }
        
        if (!isActivate) {
            // Debug.Log(this.turretType + " - Stop TurretFire");

            if (this._turretFireCoroutine == null) {
                return;
            }
            
            StopCoroutine(this._turretFireCoroutine);
            this._turretFireCoroutine = null;
            
            return;
        }
        
        this._turretFireCoroutine = StartCoroutine(TurretFire());
    }

    private IEnumerator TurretFire() {
        while (true) {
            // 조이스틱 사용 감지 처리; while 조건문으로 기입하면 코루틴 탈출 시 복귀 불가
            if (ServerDataManager.Turret_Shoot[(int)this.turretType]) {
                Debug.Log($"{this.turretType} - SHOOT!");    // TODO: 총알 생성 시 게임이 죽어버림.
                var projectile = this._projectileSpawnPool.Get();

                if (!projectile) {
                    break;
                }
                
                projectile.transform.SetPositionAndRotation(this.turretMuzzle.position, this.turretMuzzle.rotation);
                projectile.GetComponent<Rigidbody>().AddForce(
                    projectile.transform.forward * this.muzzleVelocity, ForceMode.Acceleration);
                
                yield return new WaitForSeconds(this._fireRate);

                // projectile.Deactivate();
                projectile.ProjectileSpawnPool.Release(projectile);
            }
        }
    }

    private void TurretRotate() {
        if (ServerDataManager.Turret_Player[(int)this.turretType] == 0) {
            return;
        }
        
        this.transform.rotation 
            = Quaternion.Euler(this.transform.rotation.x, this.transform.rotation.y, 
                -ServerDataManager.Turret_Rotation[(int)this.turretType]);
    }
    
    private void OnDestroyPooledBullet(ProjectileController obj) {
        Destroy(obj.gameObject);
    }

    private void OnReleaseToPool(ProjectileController obj) {
        obj.gameObject.SetActive(false);
    }

    private void OnGetFromPool(ProjectileController obj) {
        obj.gameObject.SetActive(this);
    }

    private ProjectileController BulletSpawn() {
        var projectileInstance = Instantiate(this.projectilePrefab);
        projectileInstance.ProjectileSpawnPool = this._projectileSpawnPool;
        
        return projectileInstance;
    }
}