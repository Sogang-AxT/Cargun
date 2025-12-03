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
    private Coroutine _projectileChangeCoroutine;
    private bool _isAssigned;
    private int _turretPlayer;
    private float _turretFireRate;
    
    
    private void Init() {
        CargunShipManager.OnTurretActivate.AddListener(TurretActivate);
        CargunShipManager.OnProjectileItemGet.AddListener(ProjectileTypeChange);
        GameManager.OnTurretAssign.AddListener(TurretAssign);
        
        this._currentProjectileType = GCEnumManager.PROJECTILE_TYPE.DEFAULT;    // TODO: 플레이어의 아이템 항목 로드; 어디서?
        this._turretFireCoroutine = null;
        this._projectileChangeCoroutine = null;
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
    
    // TODO: 터릿이 아니라 사용자를 따라가게
    private void ProjectileTypeChange(GCEnumManager.PROJECTILE_TYPE projectileType, float usableTime) {
        if (this._projectileChangeCoroutine != null) {
            StopCoroutine(this._projectileChangeCoroutine);
        }
        
        this._projectileChangeCoroutine = StartCoroutine(ProjectileActivate(projectileType, usableTime));
    }
    
    private IEnumerator ProjectileActivate(GCEnumManager.PROJECTILE_TYPE projectileType, float usableTime) {
        var t = usableTime;
        this._currentProjectileType = projectileType;
        
        Debug.Log(this._currentProjectileType);

        while (t > 0) {
            t -= 1;
            Debug.Log(t);
            yield return new WaitForSeconds(1f);
        }

        this._currentProjectileType = GCEnumManager.PROJECTILE_TYPE.DEFAULT;
    }
    
    private void TurretAssign(int player) {
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
                
                Debug.Log(this._currentProjectileType);
                
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