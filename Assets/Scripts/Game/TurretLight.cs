using UnityEngine;

public class TurretLight : MonoBehaviour
{
    public GameObject tl1, tl2, tl3, tl4;

    private SpriteRenderer sr1, sr2, sr3, sr4;
    private int[] previousTurretPlayer;

    void Start()
    {
        // SpriteRenderer 컴포넌트 가져오기
        sr1 = tl1.GetComponent<SpriteRenderer>();
        sr2 = tl2.GetComponent<SpriteRenderer>();
        sr3 = tl3.GetComponent<SpriteRenderer>();
        sr4 = tl4.GetComponent<SpriteRenderer>();

        // 초기 배열 설정
        previousTurretPlayer = new int[4];

        // 초기 색상 설정
        UpdateAllLights();
    }

    void Update()
    {
        // Turret_Player 값이 변경되었는지 확인
        if (HasTurretPlayerChanged())
        {
            UpdateAllLights();
            UpdatePreviousValues();
        }
    }

    bool HasTurretPlayerChanged()
    {
        if (ServerDataManager.Turret_Player == null || ServerDataManager.Turret_Player.Length < 4)
            return false;

        for (int i = 0; i < 4; i++)
        {
            if (previousTurretPlayer[i] != ServerDataManager.Turret_Player[i])
                return true;
        }
        return false;
    }

    void UpdatePreviousValues()
    {
        if (ServerDataManager.Turret_Player == null || ServerDataManager.Turret_Player.Length < 4)
            return;

        for (int i = 0; i < 4; i++)
        {
            previousTurretPlayer[i] = ServerDataManager.Turret_Player[i];
        }
    }

    void UpdateAllLights()
    {
        if (ServerDataManager.Turret_Player == null || ServerDataManager.Turret_Player.Length < 4)
        {
            // Turret_Player가 초기화되지 않았으면 모두 투명 흰색
            SetLightColor(sr1, 0);
            SetLightColor(sr2, 0);
            SetLightColor(sr3, 0);
            SetLightColor(sr4, 0);
            return;
        }

        SetLightColor(sr1, ServerDataManager.Turret_Player[0]);
        SetLightColor(sr2, ServerDataManager.Turret_Player[1]);
        SetLightColor(sr3, ServerDataManager.Turret_Player[2]);
        SetLightColor(sr4, ServerDataManager.Turret_Player[3]);
    }

    void SetLightColor(SpriteRenderer sr, int playerValue)
    {
        Color newColor;

        switch (playerValue)
        {
            case 0: // 투명 흰색
                newColor = new Color(1f, 1f, 1f, 0f);
                break;
            case 1: // 반투명 녹색
                newColor = new Color(0f, 1f, 0f, 0.5f);
                break;
            case 2: // 반투명 파랑
                newColor = new Color(0f, 0.7f, 1f, 0.9f);
                break;
            case 3: // 반투명 보라색
                newColor = new Color(0.5f, 0f, 1f, 0.5f);
                break;
            case 4: // 반투명 오렌지색
                newColor = new Color(1f, 0.5f, 0f, 0.5f);
                break;
            default: // 기본값 (투명 흰색)
                newColor = new Color(1f, 1f, 1f, 0f);
                break;
        }

        sr.color = newColor;
    }
}