using System.Collections;
using UnityEngine;

// 함선 터릿 제어
public class CargunShipTurretController : MonoBehaviour {
    // private CargunShipManager _cargunShipManager;
    
    private float _fireAngle;
    private float _fireRate; 
    
    
    private void Init() {
        CargunShipManager.OnTurretActivate.AddListener(TurretActivate);
        GameManager.OnPlayerJoin.AddListener(TurretAssign);

        this._fireAngle = 360f;
        this._fireRate = 0.5f;
    }

    private void Awake() {
        Init();
    }

    private void TurretAssign() {
        // TODO: 터릿 할당 구현
    }
    
    private void TurretActivate(bool isActivate) {
        if (!isActivate) {
            return;
        }
        
        StartCoroutine(TurretFire());
    }

    private IEnumerator TurretFire() {
        // TODO: 터릿 발사 구현
        yield return new WaitForSeconds(this._fireRate);
    }
    
    
    // [Header("Turret Objects")]
    // public GameObject Bullet;
    // public GameObject PivotA, PivotB, PivotC, PivotD;
    // public SpriteRenderer TurretA, TurretB, TurretC, TurretD;
    // public GameObject AngleA, AngleB, AngleC, AngleD;
    //
    // [Header("Turret Sprites")]
    // public Sprite Dis, Green, Blue, Purple, Orange;
    //
    // [Header("Turret Angles")]
    // private float angleA = 45f;
    // private float angleB = 315f;
    // private float angleC = 225f;
    // private float angleD = 135f;
    //
    // [Header("Turret Limits")]
    // private Vector2 limitA = new Vector2(0f, 150f);     // 0~150도 (12시 → 4시 반시계)
    // private Vector2 limitB = new Vector2(210f, 360f);   // 210~360도 (5시 → 12시 반시계, 360은 0과 같음)
    // private Vector2 limitC = new Vector2(180f, 330f);   // 180~330도 (6시 → 9시 반시계)
    // private Vector2 limitD = new Vector2(30f, 180f);    // 30~180도 (1시 → 6시 반시계)
    //
    // [Header("Fire Rate")]
    // public float fireRate = 0.5f;                       // 0.5초마다 발사
    // private float[] nextFireTime = new float[4];
    //
    // [Header("Shooting")]
    // private bool canShoot = false;
    //
    // [Header("Player Assignment")]
    // private Dictionary<string, string> turretAssignment = new Dictionary<string, string>();
    // // turret -> color (A->Green, B->Blue, ...)
    //
    //
    // private void Init() {
    //     // 초기 상태: 모든 터렛 Disconnect
    //     SetTurretSprite(TurretA, Dis);
    //     SetTurretSprite(TurretB, Dis);
    //     SetTurretSprite(TurretC, Dis);
    //     SetTurretSprite(TurretD, Dis);
    //
    //     // Angle 표시 비활성화
    //     if (AngleA != null) AngleA.SetActive(false);
    //     if (AngleB != null) AngleB.SetActive(false);
    //     if (AngleC != null) AngleC.SetActive(false);
    //     if (AngleD != null) AngleD.SetActive(false);
    // }
    //
    // private void Awake() {
    //     Init();
    // }
    //
    // void Update() {
    //     // 플레이어 할당 업데이트
    //     UpdatePlayerAssignment();
    //
    //     // 자동 발사
    //     if (canShoot) {
    //         AutoFire();
    //     }
    //
    //     // 터렛 회전 (스무스하게)
    //     SmoothRotateTurrets();
    // }
    //
    // // 플레이어 할당 업데이트
    // void UpdatePlayerAssignment() {
    //     if (ServerManager.Instance == null) return;
    //
    //     // 기존 할당 초기화
    //     turretAssignment.Clear();
    //
    //     // 플레이어 목록에서 터렛 할당 가져오기
    //     foreach (var player in ServerManager.Instance.players.Values) {
    //         turretAssignment[player.turret] = player.color;
    //     }
    //
    //     // Sprite 업데이트
    //     UpdateTurretSprites();
    // }
    //
    // // 터렛 Sprite 업데이트
    // void UpdateTurretSprites() {
    //     UpdateSingleTurret("A", TurretA, AngleA);
    //     UpdateSingleTurret("B", TurretB, AngleB);
    //     UpdateSingleTurret("C", TurretC, AngleC);
    //     UpdateSingleTurret("D", TurretD, AngleD);
    // }
    //
    // void UpdateSingleTurret(string turretName, SpriteRenderer spriteRenderer, GameObject angleObject) {
    //     if (turretAssignment.ContainsKey(turretName)) {
    //         // 플레이어 할당됨
    //         string color = turretAssignment[turretName];
    //         Sprite colorSprite = GetSpriteByColor(color);
    //         SetTurretSprite(spriteRenderer, colorSprite);
    //
    //         if (angleObject != null) angleObject.SetActive(true);
    //     }
    //     else {
    //         // 할당 안됨
    //         SetTurretSprite(spriteRenderer, Dis);
    //         if (angleObject != null) angleObject.SetActive(false);
    //     }
    // }
    //
    // Sprite GetSpriteByColor(string color) {
    //     switch (color.ToLower()) {
    //         case "green": return Green;
    //         case "blue": return Blue;
    //         case "purple": return Purple;
    //         case "orange": return Orange;
    //         default: return Dis;
    //     }
    // }
    //
    // void SetTurretSprite(SpriteRenderer sr, Sprite sprite) {
    //     if (sr != null && sprite != null) {
    //         sr.sprite = sprite;
    //     }
    // }
    //
    // // ✅ 모바일에서 받은 조이스틱 각도를 Unity RotationZ로 변환 후 업데이트
    // public void UpdateTurretAngle(string turret, float joystickAngle) {
    //     // 조이스틱 각도 → Unity RotationZ 변환
    //     // 조이스틱: 0° = 3시, 90° = 6시, 180° = 9시, 270° = 12시 (시계방향)
    //     // Unity Z: 0° = 12시, 90° = 9시, 180° = 6시, 270° = 3시 (반시계방향)
    //     // 변환 공식: unityAngle = (joystickAngle - 90 + 360) % 360
    //     // 또는: unityAngle = (270 - joystickAngle + 360) % 360
    //     float unityAngle = (270f - joystickAngle + 360f) % 360f;
    //
    //     switch (turret) {
    //         case "A":
    //             angleA = ClampAngle(unityAngle, limitA);
    //             break;
    //         case "B":
    //             angleB = ClampAngle(unityAngle, limitB);
    //             break;
    //         case "C":
    //             angleC = ClampAngle(unityAngle, limitC);
    //             break;
    //         case "D":
    //             angleD = ClampAngle(unityAngle, limitD);
    //             break;
    //     }
    // }
    //
    // // ✅ 각도 제한 (Clamp)
    // float ClampAngle(float angle, Vector2 limit) {
    //     // 각도를 0~360 범위로 정규화
    //     angle = NormalizeAngle(angle);
    //
    //     float min = limit.x;
    //     float max = limit.y;
    //
    //     // 범위가 360도를 넘는 경우 (예: 210~360, 실제로는 210~0)
    //     if (min > max) {
    //         // max를 실제 각도로 변환 (360 = 0)
    //         // 예: 210~360 → 210~0 범위
    //         // 각도가 범위 안에 있는지 확인
    //         if (angle >= min || angle <= max) {
    //             // 범위 안: 그대로 반환
    //             return angle;
    //         }
    //         else {
    //             // 범위 밖: 가장 가까운 경계로
    //             float distToMin = AngleDifference(angle, min);
    //             float distToMax = AngleDifference(angle, max);
    //             return distToMin < distToMax ? min : max;
    //         }
    //     }
    //     else {
    //         // 일반 범위 (예: 30~180)
    //         if (angle >= min && angle <= max) {
    //             return angle;
    //         }
    //         else {
    //             // 범위 밖: 가장 가까운 경계로
    //             float distToMin = AngleDifference(angle, min);
    //             float distToMax = AngleDifference(angle, max);
    //             return distToMin < distToMax ? min : max;
    //         }
    //     }
    // }
    //
    // // 두 각도 간의 최단 거리 계산
    // float AngleDifference(float from, float to) {
    //     float diff = Mathf.Abs(to - from);
    //     
    //     if (diff > 180f) {
    //         diff = 360f - diff;
    //     }
    //     return diff;
    // }
    //
    // // 각도를 0~360 범위로 정규화
    // float NormalizeAngle(float angle) {
    //     angle = angle % 360f;
    //     
    //     if (angle < 0f) {
    //         angle += 360f;
    //     }
    //     return angle;
    // }
    //
    // // ✅ 터렛 스무스하게 회전 (최단 거리로)
    // void SmoothRotateTurrets() {
    //     if (PivotA != null) RotatePivot(PivotA, angleA);
    //     if (PivotB != null) RotatePivot(PivotB, angleB);
    //     if (PivotC != null) RotatePivot(PivotC, angleC);
    //     if (PivotD != null) RotatePivot(PivotD, angleD);
    // }
    //
    // void RotatePivot(GameObject pivot, float targetAngle) {
    //     float currentAngle = pivot.transform.eulerAngles.z;
    //
    //     // Mathf.LerpAngle은 자동으로 최단 경로로 회전
    //     float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * 5f);
    //     pivot.transform.rotation = Quaternion.Euler(0, 0, newAngle);
    // }
    //
    // // 자동 발사
    // void AutoFire() {
    //     if (turretAssignment.ContainsKey("A") && Time.time >= nextFireTime[0]) {
    //         FireBullet(PivotA, angleA);
    //         nextFireTime[0] = Time.time + fireRate;
    //     }
    //
    //     if (turretAssignment.ContainsKey("B") && Time.time >= nextFireTime[1]) {
    //         FireBullet(PivotB, angleB);
    //         nextFireTime[1] = Time.time + fireRate;
    //     }
    //
    //     if (turretAssignment.ContainsKey("C") && Time.time >= nextFireTime[2]) {
    //         FireBullet(PivotC, angleC);
    //         nextFireTime[2] = Time.time + fireRate;
    //     }
    //
    //     if (turretAssignment.ContainsKey("D") && Time.time >= nextFireTime[3]) {
    //         FireBullet(PivotD, angleD);
    //         nextFireTime[3] = Time.time + fireRate;
    //     }
    // }
    //
    // // 총알 발사
    // void FireBullet(GameObject pivot, float angle) {
    //     if (Bullet == null || pivot == null) return;
    //
    //     GameObject bullet = Instantiate(Bullet, pivot.transform.position, Quaternion.Euler(0, 0, angle));
    //
    //     // 총알에 속도 적용
    //     Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
    //     
    //     if (rb != null) {
    //         // Unity RotationZ를 방향 벡터로 변환
    //         // RotationZ 0° = 위(12시) 이므로 +90도 보정 필요
    //         float angleRad = (angle + 90f) * Mathf.Deg2Rad;
    //         Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    //         rb.linearVelocity = direction * 20f; // 속도 10
    //     }
    //
    //     // 5초 후 총알 제거
    //     Destroy(bullet, 5f);
    // }
    //
    // // 발사 활성화/비활성화
    // public void EnableShooting(bool enable) {
    //     canShoot = enable;
    // }
}