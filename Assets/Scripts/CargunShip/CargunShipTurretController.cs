using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

// 함선 터릿 제어
public class CargunShipTurretController : MonoBehaviour {
    [SerializeField] private Sprite offlineTurretSprite;
    [SerializeField] private Sprite onlineTurretSprite;
    [Space(10f)]
    [SerializeField] private GC_EnumManager.TURRET_TYPE turretType;
    [SerializeField] private Transform turretMuzzle;
    
    private Sprite _turretSprite;
    private IObjectPool<GameObject> _bulletPool;
    private GameObject _bulletPrefab;
    private Coroutine _turretFireCoroutine;
    private float _fireRate;
    private bool _isControlling;
    private bool _isAssigned;
    private int _turretPlayer;
    
    
    private void Init() {
        CargunShipManager.OnTurretActivate.AddListener(TurretActivate);
        GameManager.OnTurretAssign.AddListener(TurretAssign);

        this._turretSprite = gameObject.GetComponent<SpriteRenderer>().sprite;
        this._isAssigned = false;
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
            // Debug.Log(this.turretType + " - " + ServerDataManager.Turret_Shoot[(int)this.turretType]);

            // 조이스틱 사용 감지 처리; while 조건문으로 기입하면 코루틴 탈출 시 복귀 불가
            if (ServerDataManager.Turret_Shoot[(int)this.turretType]) {
                Debug.Log($"{this.turretType} - SHOOT!");    // TODO: 총알 생성, 발사
            }
            
            yield return new WaitForSeconds(this._fireRate);
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
}