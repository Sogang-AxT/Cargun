using UnityEngine;
using UnityEngine.UI;

public class UpgradeInfo : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject InfoPanel; // 업그레이드 정보 패널
    public Text InfoText; // 정보 표시 텍스트

    void Start()
    {
        // 초기 상태: 패널 숨김
        if (InfoPanel != null)
        {
            InfoPanel.SetActive(false);
        }
    }

    void Update()
    {
        // I키: 업그레이드 정보 토글
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInfo();
        }

        // ESC키: 정보 닫기
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HideInfo();
        }
    }

    // 정보 토글
    void ToggleInfo()
    {
        if (InfoPanel != null)
        {
            bool isActive = InfoPanel.activeSelf;
            InfoPanel.SetActive(!isActive);

            if (!isActive)
            {
                // 정보 업데이트
                UpdateInfo();
            }
        }
    }

    // 정보 숨김
    void HideInfo()
    {
        if (InfoPanel != null)
        {
            InfoPanel.SetActive(false);
        }
    }

    // 업그레이드 정보 업데이트
    void UpdateInfo()
    {
        if (InfoText == null || Server.Instance == null) return;

        string info = "=== 업그레이드 정보 ===\n\n";

        // 각 터렛별 업그레이드 상태 표시
        info += GetTurretInfo("A");
        info += GetTurretInfo("B");
        info += GetTurretInfo("C");
        info += GetTurretInfo("D");

        InfoText.text = info;
    }

    // 특정 터렛의 업그레이드 정보
    string GetTurretInfo(string turret)
    {
        if (Server.Instance.upgradeStates.ContainsKey(turret))
        {
            int[] upgrades = Server.Instance.upgradeStates[turret];

            string info = $"Turret {turret}: ";
            info += $"{upgrades[0]}, {upgrades[1]}, {upgrades[2]}, {upgrades[3]}\n";

            return info;
        }

        return $"Turret {turret}: 0, 0, 0, 0\n";
    }
}