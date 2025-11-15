using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

// 함선 터릿 제어
public class CargunShipTurretController : MonoBehaviour {
    [SerializeField] private GC_EnumManager.TURRET_TYPE turretType;
    [SerializeField] private Transform turretMuzzle;

    private IObjectPool<GameObject> _bulletPool;
    private GameObject _bulletPrefab;
    private float _fireRate;
    private bool _isControlling;
    
    
    private void Init() {
        CargunShipManager.OnTurretActivate.AddListener(TurretActivate);
        GameManager.OnPlayerJoin.AddListener(TurretAssign);

        this._fireRate = 0.5f;
        this._bulletPool = new ObjectPool<GameObject>(
            createFunc: () => {
                var bullet 
                    = Instantiate(this._bulletPrefab, this.turretMuzzle.position, this.turretMuzzle.localRotation);
                
                return bullet;
            },
            actionOnGet: obj => {
                obj.transform.position = this.turretMuzzle.position;
                obj.transform.rotation = this.turretMuzzle.rotation;
                obj.SetActive(true);
            },
            actionOnRelease: obj => {
                obj.SetActive(false);
                obj.transform.position = this.turretMuzzle.position;
                obj.transform.rotation = this.turretMuzzle.rotation;
            },
            actionOnDestroy: Destroy,
            collectionCheck: true,
            defaultCapacity: 50,
            maxSize: 100
        );
    }

    private void Awake() {
        Init();
    }

    private void Update() {
        TurretRotate();
    }
    
    private void TurretAssign() {
        // TODO: 터릿 할당 구현
    }
    
    private void TurretActivate(bool isActivate) {
        Debug.Log("isActivate: " + isActivate);
        
        if (!isActivate) {
            Debug.Log("Stop TurretFire");
            StopCoroutine(TurretFire());
            return;
        }
        
        
        StartCoroutine(TurretFire());
    }

    private IEnumerator TurretFire() {
        while (true) {
            Debug.Log("Turret Shoot: " + ServerDataManager.Turret_Shoot[(int)this.turretType]);

            // 조이스틱 사용 감지 처리; !! while 조건문으로 기입하면 코루틴 탈출 시, 복귀 불가
            if (ServerDataManager.Turret_Shoot[(int)this.turretType]) {
                Debug.Log("SHOOT!");    // TODO: 총알이 생성되면 발사
                // var bullet = this._bulletPool.Get(); 
            }
            
            yield return new WaitForSeconds(this._fireRate);
        }
    }

    private void TurretRotate() {
        this.transform.rotation 
            = Quaternion.Euler(this.transform.rotation.x, this.transform.rotation.y, 
                ServerDataManager.Turret_Rotation[(int)this.turretType]);
    }
}