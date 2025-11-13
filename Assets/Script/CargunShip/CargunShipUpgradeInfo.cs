using UnityEngine;
using UnityEngine.UI;

public class CargunShipUpgradeInfo : MonoBehaviour {
    // [Header("UI Elements")]
    // public GameObject InfoPanel; // ���׷��̵� ���� �г�
    // public Text InfoText; // ���� ǥ�� �ؽ�Ʈ
    //
    // void Start()
    // {
    //     // �ʱ� ����: �г� ����
    //     if (InfoPanel != null)
    //     {
    //         InfoPanel.SetActive(false);
    //     }
    // }
    //
    // void Update()
    // {
    //     // IŰ: ���׷��̵� ���� ���
    //     if (Input.GetKeyDown(KeyCode.I))
    //     {
    //         ToggleInfo();
    //     }
    //
    //     // ESCŰ: ���� �ݱ�
    //     if (Input.GetKeyDown(KeyCode.Escape))
    //     {
    //         HideInfo();
    //     }
    // }
    //
    // // ���� ���
    // void ToggleInfo()
    // {
    //     if (InfoPanel != null)
    //     {
    //         bool isActive = InfoPanel.activeSelf;
    //         InfoPanel.SetActive(!isActive);
    //
    //         if (!isActive)
    //         {
    //             // ���� ������Ʈ
    //             UpdateInfo();
    //         }
    //     }
    // }
    //
    // // ���� ����
    // void HideInfo()
    // {
    //     if (InfoPanel != null)
    //     {
    //         InfoPanel.SetActive(false);
    //     }
    // }
    //
    // // ���׷��̵� ���� ������Ʈ
    // void UpdateInfo()
    // {
    //     if (InfoText == null || ServerManager.Instance == null) return;
    //
    //     string info = "=== ���׷��̵� ���� ===\n\n";
    //
    //     // �� �ͷ��� ���׷��̵� ���� ǥ��
    //     info += GetTurretInfo("A");
    //     info += GetTurretInfo("B");
    //     info += GetTurretInfo("C");
    //     info += GetTurretInfo("D");
    //
    //     InfoText.text = info;
    // }
    //
    // // Ư�� �ͷ��� ���׷��̵� ����
    // string GetTurretInfo(string turret)
    // {
    //     if (ServerManager.Instance.upgradeStates.ContainsKey(turret))
    //     {
    //         int[] upgrades = ServerManager.Instance.upgradeStates[turret];
    //
    //         string info = $"Turret {turret}: ";
    //         info += $"{upgrades[0]}, {upgrades[1]}, {upgrades[2]}, {upgrades[3]}\n";
    //
    //         return info;
    //     }
    //
    //     return $"Turret {turret}: 0, 0, 0, 0\n";
    // }
}