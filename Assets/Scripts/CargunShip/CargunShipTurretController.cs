using System.Collections;
using UnityEngine;

// 함선 터릿 제어
public class CargunShipTurretController : MonoBehaviour {
    [SerializeField] private ProjectileSpawnController projectileSpawnController;
    
    [Space(25f)]
    
    [SerializeField] private GCEnumManager.TURRET_TYPE turretNumber;
    [SerializeField] private Transform turretMuzzleTransform;
    
    private GCEnumManager.PROJECTILE_TYPE _currentProjectileType;
    private Coroutine _turretFireCoroutine;
    private bool _isAssigned;
    private int _turretPlayer;
    private float _turretFireRate;
    
    
    private void Init() {
        CargunShipManager.OnTurretActivate.AddListener(TurretActivate);
        GameManager.OnTurretAssign.AddListener(TurretAssign);

        this._currentProjectileType = GCEnumManager.PROJECTILE_TYPE.DEFAULT;    // TODO: 플레이어의 아이템 항목 로드; 어디서?
        this._turretFireCoroutine = null;
        this._isAssigned = false;
        this._turretPlayer = 0;
        this._turretFireRate = 1f;
    }
    
    private void Awake() {
        Init();
    }

    private void Update() {
        TurretRotate();
    }

    private void TurretAssign(int player) {
        if (ServerDataManager.Turret_Player[(int)this.turretNumber] != player) {
            // Debug.Log(this.turretType + " - UnAssigned!");
            return;
        }
        
        this._turretPlayer = player;
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
            if (ServerDataManager.Turret_Shoot[(int)this.turretNumber]) {
                // Debug.Log($"{this.turretType} - SHOOT!");
                
                // TODO: Projectile type change
                var projectile = this.projectileSpawnController.GetProjectile(this._currentProjectileType, 
                    this.turretMuzzleTransform.position, this.turretMuzzleTransform.rotation);
                
                if (!projectile) {
                    yield break;
                }
                
                this._turretFireRate = projectile.FireRate;
            }

            yield return new WaitForSeconds(this._turretFireRate);    // if 문 안으로 이동 금지; 무한루프
        }
    }

    private void TurretRotate() {
        if (ServerDataManager.Turret_Player[(int)this.turretNumber] == 0) {
            return;
        }
        
        this.transform.rotation 
            = Quaternion.Euler(0, 0, -ServerDataManager.Turret_Rotation[(int)this.turretNumber]);
    }
}