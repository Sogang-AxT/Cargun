using UnityEngine;

public class ItemSpawn : MonoBehaviour
{
    public GameObject ItemBox, BombBox;
    public float spawnInterval = 5f; // 스폰 간격 (초)

    private Camera mainCamera;
    private float timer;
    private Vector3[] spawnPositions;

    void Start()
    {
        mainCamera = Camera.main;
        timer = spawnInterval; // 초기 타이머 설정
        CalculateSpawnPositions();
    }

    void Update()
    {
        // TotalPlayer가 0이면 스폰하지 않음
        if (ServerDataManager.TotalPlayer <= 0)
        {
            timer = spawnInterval; // 타이머 리셋하여 플레이어가 들어오면 바로 시작
            return;
        }

        // 타이머 감소
        timer -= Time.deltaTime;

        // 타이머가 0 이하가 되면 아이템 스폰
        if (timer <= 0f)
        {
            SpawnRandomItem();
            timer = spawnInterval; // 타이머 리셋
        }
    }

    void CalculateSpawnPositions()
    {
        spawnPositions = new Vector3[4];

        // 카메라 경계 계산
        float cameraHeight = 2f * mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        Vector3 cameraPos = mainCamera.transform.position;

        // 상단 중앙에서 위로 3
        spawnPositions[0] = new Vector3(cameraPos.x, cameraPos.y + (cameraHeight / 2f) + 3f, 0f);

        // 하단 중앙에서 아래로 3
        spawnPositions[1] = new Vector3(cameraPos.x, cameraPos.y - (cameraHeight / 2f) - 3f, 0f);

        // 좌측 중앙에서 왼쪽으로 3
        spawnPositions[2] = new Vector3(cameraPos.x - (cameraWidth / 2f) - 3f, cameraPos.y, 0f);

        // 우측 중앙에서 오른쪽으로 3
        spawnPositions[3] = new Vector3(cameraPos.x + (cameraWidth / 2f) + 3f, cameraPos.y, 0f);
    }

    void SpawnRandomItem()
    {
        // 4개의 위치 중 랜덤 선택
        int randomPosIndex = Random.Range(0, 4);
        Vector3 spawnPos = spawnPositions[randomPosIndex];

        // ItemBox 또는 BombBox 중 랜덤 선택
        GameObject itemToSpawn = Random.Range(0, 2) == 0 ? ItemBox : BombBox;

        // 아이템 생성
        if (itemToSpawn != null)
        {
            Instantiate(itemToSpawn, spawnPos, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("ItemBox 또는 BombBox가 할당되지 않았습니다!");
        }
    }
}