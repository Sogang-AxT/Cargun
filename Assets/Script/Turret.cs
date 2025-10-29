using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Turret : MonoBehaviour
{
    public static Turret Instance;

    [Header("Turret Objects")]
    public GameObject Bullet;
    public GameObject PivotA, PivotB, PivotC, PivotD;
    public SpriteRenderer TurretA, TurretB, TurretC, TurretD;
    public GameObject AngleA, AngleB, AngleC, AngleD;

    [Header("Turret Sprites")]
    public Sprite Dis, Green, Blue, Purple, Orange;

    [Header("Turret Angles")]
    private float angleA = 45f;
    private float angleB = 315f;
    private float angleC = 225f;
    private float angleD = 135f;

    [Header("Turret Limits")]
    private Vector2 limitA = new Vector2(150f, 0f); // 150~0 (�����δ� 0~150)
    private Vector2 limitB = new Vector2(210f, 360f);
    private Vector2 limitC = new Vector2(180f, 330f);
    private Vector2 limitD = new Vector2(30f, 180f);

    [Header("Fire Rate")]
    public float fireRate = 0.5f; // 0.5�ʸ��� �߻�
    private float[] nextFireTime = new float[4];

    [Header("Shooting")]
    private bool canShoot = false;

    [Header("Player Assignment")]
    private Dictionary<string, string> turretAssignment = new Dictionary<string, string>();
    // turret -> color (A->Green, B->Blue, ...)

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // �ʱ� ����: ��� �ͷ� Disconnect
        SetTurretSprite(TurretA, Dis);
        SetTurretSprite(TurretB, Dis);
        SetTurretSprite(TurretC, Dis);
        SetTurretSprite(TurretD, Dis);

        // Angle ǥ�� ��Ȱ��ȭ
        if (AngleA != null) AngleA.SetActive(false);
        if (AngleB != null) AngleB.SetActive(false);
        if (AngleC != null) AngleC.SetActive(false);
        if (AngleD != null) AngleD.SetActive(false);
    }

    void Update()
    {
        // �÷��̾� �Ҵ� ������Ʈ
        UpdatePlayerAssignment();

        // �ڵ� �߻�
        if (canShoot)
        {
            AutoFire();
        }

        // �ͷ� ȸ�� (�������ϰ�)
        SmoothRotateTurrets();
    }

    // �÷��̾� �Ҵ� ������Ʈ
    void UpdatePlayerAssignment()
    {
        if (Server.Instance == null) return;

        // ���� �Ҵ� �ʱ�ȭ
        turretAssignment.Clear();

        // �÷��̾� ��Ͽ��� �ͷ� �Ҵ� ��������
        foreach (var player in Server.Instance.players.Values)
        {
            turretAssignment[player.turret] = player.color;
        }

        // Sprite ������Ʈ
        UpdateTurretSprites();
    }

    // �ͷ� Sprite ������Ʈ
    void UpdateTurretSprites()
    {
        UpdateSingleTurret("A", TurretA, AngleA);
        UpdateSingleTurret("B", TurretB, AngleB);
        UpdateSingleTurret("C", TurretC, AngleC);
        UpdateSingleTurret("D", TurretD, AngleD);
    }

    void UpdateSingleTurret(string turretName, SpriteRenderer spriteRenderer, GameObject angleObject)
    {
        if (turretAssignment.ContainsKey(turretName))
        {
            // �÷��̾� �Ҵ��
            string color = turretAssignment[turretName];
            Sprite colorSprite = GetSpriteByColor(color);
            SetTurretSprite(spriteRenderer, colorSprite);

            if (angleObject != null) angleObject.SetActive(true);
        }
        else
        {
            // �Ҵ� �ȵ�
            SetTurretSprite(spriteRenderer, Dis);
            if (angleObject != null) angleObject.SetActive(false);
        }
    }

    Sprite GetSpriteByColor(string color)
    {
        switch (color.ToLower())
        {
            case "green": return Green;
            case "blue": return Blue;
            case "purple": return Purple;
            case "orange": return Orange;
            default: return Dis;
        }
    }

    void SetTurretSprite(SpriteRenderer sr, Sprite sprite)
    {
        if (sr != null && sprite != null)
        {
            sr.sprite = sprite;
        }
    }

    // �������� ���� ������ �ͷ� ������Ʈ
    public void UpdateTurretAngle(string turret, float angle)
    {
        switch (turret)
        {
            case "A":
                angleA = ClampAngle(angle, limitA);
                break;
            case "B":
                angleB = ClampAngle(angle, limitB);
                break;
            case "C":
                angleC = ClampAngle(angle, limitC);
                break;
            case "D":
                angleD = ClampAngle(angle, limitD);
                break;
        }
    }

    // ���� ����
    float ClampAngle(float angle, Vector2 limit)
    {
        // limit.x�� �� ū ��� (��: 210~360)
        if (limit.x > limit.y)
        {
            if (angle < limit.x && angle > limit.y)
            {
                // ���� ��: ����� ������
                if (angle - limit.y < limit.x - angle)
                    return limit.y;
                else
                    return limit.x;
            }
        }
        else // limit.x�� �� ���� ��� (��: 30~180)
        {
            angle = Mathf.Clamp(angle, limit.x, limit.y);
        }

        return angle;
    }

    // �ͷ� ������ ȸ��
    void SmoothRotateTurrets()
    {
        if (PivotA != null) RotatePivot(PivotA, angleA);
        if (PivotB != null) RotatePivot(PivotB, angleB);
        if (PivotC != null) RotatePivot(PivotC, angleC);
        if (PivotD != null) RotatePivot(PivotD, angleD);
    }

    void RotatePivot(GameObject pivot, float targetAngle)
    {
        float currentAngle = pivot.transform.eulerAngles.z;
        float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * 5f);
        pivot.transform.rotation = Quaternion.Euler(0, 0, newAngle);
    }

    // �ڵ� �߻�
    void AutoFire()
    {
        if (turretAssignment.ContainsKey("A") && Time.time >= nextFireTime[0])
        {
            FireBullet(PivotA, angleA);
            nextFireTime[0] = Time.time + fireRate;
        }

        if (turretAssignment.ContainsKey("B") && Time.time >= nextFireTime[1])
        {
            FireBullet(PivotB, angleB);
            nextFireTime[1] = Time.time + fireRate;
        }

        if (turretAssignment.ContainsKey("C") && Time.time >= nextFireTime[2])
        {
            FireBullet(PivotC, angleC);
            nextFireTime[2] = Time.time + fireRate;
        }

        if (turretAssignment.ContainsKey("D") && Time.time >= nextFireTime[3])
        {
            FireBullet(PivotD, angleD);
            nextFireTime[3] = Time.time + fireRate;
        }
    }

    // �Ѿ� �߻�
    void FireBullet(GameObject pivot, float angle)
    {
        if (Bullet == null || pivot == null) return;

        GameObject bullet = Instantiate(Bullet, pivot.transform.position, Quaternion.Euler(0, 0, angle));

        // �Ѿ˿� �ӵ� ����
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float angleRad = angle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
            rb.linearVelocity = direction * 10f; // �ӵ� 10
        }

        // 5�� �� �ڵ� ����
        Destroy(bullet, 5f);
    }

    // �߻� Ȱ��ȭ/��Ȱ��ȭ
    public void EnableShooting(bool enable)
    {
        canShoot = enable;
    }
}