using UnityEngine;

public class Cargo : MonoBehaviour
{
    public static Cargo Instance;

    [Header("Cargo Objects")]
    public GameObject CargoHitBox;
    public GameObject[] CargoObject; // 4개의 화물

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
        // 초기 상태: 모든 Cargo 활성화
        ResetCargo();
    }

    // Cargo 리셋 (모두 활성화)
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

    // Cargo 1개 제거
    public void DecreaseCargo()
    {
        // 활성화된 Cargo 찾아서 비활성화
        for (int i = CargoObject.Length - 1; i >= 0; i--)
        {
            if (CargoObject[i] != null && CargoObject[i].activeSelf)
            {
                CargoObject[i].SetActive(false);

                // GameManager에 알림
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.DecreaseCargo();
                }

                break;
            }
        }
    }
}