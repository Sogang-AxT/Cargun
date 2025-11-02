using UnityEngine;

public class EnemyUnit : MonoBehaviour {
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float zigzagAmplitude = 1f; // ������� ��
    public float zigzagFrequency = 2f; // ������� ��

    private int direction = 1; // 1 = ����������, -1 = ��������
    private Vector3 startPosition;
    private float timeOffset;
    private GameObject cargoTarget;

    void Start() {
        startPosition = transform.position;
        timeOffset = Random.Range(0f, 100f); // ������� ������ ����

        // Cargo Ÿ�� ã��
        if (CargoHitController.Instance != null) {
            cargoTarget = CargoHitController.Instance.CargoHitBox;
        }
    }

    void Update() {
        MoveToCargo();
    }

    // Cargo�� ���� ������׷� �̵�
    void MoveToCargo() {
        if (cargoTarget == null) return;

        // Cargo ����
        Vector3 targetDirection = (cargoTarget.transform.position - transform.position).normalized;

        // ������� ȿ�� (Sin �ĵ�)
        float zigzag = Mathf.Sin((Time.time + timeOffset) * zigzagFrequency) * zigzagAmplitude;

        // ���� �������� �������
        Vector3 perpendicular = new Vector3(-targetDirection.y, targetDirection.x, 0);
        Vector3 zigzagOffset = perpendicular * zigzag;

        // ���� �̵�
        Vector3 moveDirection = targetDirection + zigzagOffset.normalized * 0.3f;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        // ȭ�� ������ ������ ����
        if (Mathf.Abs(transform.position.x) > 15f || Mathf.Abs(transform.position.y) > 10f) {
            DestroyEnemy();
        }
    }

    // ���� ����
    public void SetDirection(int dir) {
        direction = dir;
    }

    // Bullet�� �浹
    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Bullet")) {
            // Bullet�� ����
            Destroy(other.gameObject);

            // Enemy ����
            DestroyEnemy();
        }
    }

    // Enemy ����
    void DestroyEnemy() {
        // Enemy ����Ʈ���� ����
        if (Enemy.Instance != null) {
            Enemy.Instance.RemoveEnemy(gameObject);
        }

        Destroy(gameObject);
    }
}