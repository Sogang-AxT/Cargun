using UnityEngine;

public class Cargo : MonoBehaviour
{
    public static Cargo Instance;

    [Header("Cargo Objects")]
    public GameObject CargoHitBox;
    public GameObject[] CargoObject; // 4���� ȭ��

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
        // �ʱ� ����: ��� Cargo Ȱ��ȭ
        ResetCargo();
    }

    // Cargo ���� (��� Ȱ��ȭ)
    public void ResetCargo()
    {
        foreach (GameObject cargo in CargoObject)
        {
            if (cargo != null)
            {
                cargo.SetActive(true);
            }
        }
    }

    // Cargo 1�� ����
    public void DecreaseCargo()
    {
        // Ȱ��ȭ�� Cargo ã�Ƽ� ��Ȱ��ȭ
        for (int i = CargoObject.Length - 1; i >= 0; i--)
        {
            if (CargoObject[i] != null && CargoObject[i].activeSelf)
            {
                CargoObject[i].SetActive(false);

                // GameManager�� �˸�
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.DecreaseCargo();
                }

                break;
            }
        }
    }
}