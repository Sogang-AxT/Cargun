using UnityEngine;
using System.Collections;

public class EnemySpawnManager : MonoBehaviour
{
    public GameObject Warning;
    public GameObject Enemy1, Enemy2, Enemy3;
    private Camera mainCamera;
    private float spawnInterval = 2f; // 2초마다 생성
    private float spawnTimer = 0f;
    private float extraDistance = 1f; // 카메라 경계에서 추가로 더 먼 거리

    private float gameTimer = 0f; // 게임 경과 시간
    private float currentMoveSpeed = 1f; // 현재 moveSpeed

    private float waveTimer = 0f; // 물량 웨이브 타이머
    private float waveInterval = 30f; // 30초마다 물량
    private int waveCount = 0; // 웨이브 카운트 (물량 수 계산용)
    private Vector3 waveSpawnCenter; // 웨이브 생성 중심점

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // TotalPlayer가 1 이상일 때만 스폰
        if (ServerDataManager.TotalPlayer >= 1)
        {
            // 게임 타이머 업데이트
            gameTimer += Time.deltaTime;

            // 10초마다 moveSpeed 0.1씩 증가
            currentMoveSpeed = 1f + (Mathf.Floor(gameTimer / 10f) * 0.1f);

            // 일반 스폰 (3초마다)
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                SpawnRandomEnemy();
                spawnTimer = 0f;
            }

            // 물량 웨이브 (30초마다)
            waveTimer += Time.deltaTime;
            if (waveTimer >= waveInterval)
            {
                StartCoroutine(SpawnWave());
                waveTimer = 0f;
            }
        }
        else
        {
            // 플레이어가 없으면 타이머 리셋
            spawnTimer = 0f;
            gameTimer = 0f;
            currentMoveSpeed = 1f;
            waveTimer = 0f;
            waveCount = 0;
        }
    }

    IEnumerator SpawnWave()
    {
        waveCount++;
        int enemiesToSpawn = 10 + ((waveCount - 1) * 2); // 10, 12, 14, 16, 18...

        // 랜덤 방향 선택 (0: 왼쪽, 1: 오른쪽, 2: 위, 3: 아래)
        int side = Random.Range(0, 4);

        // Warning 생성 위치 계산 (카메라 경계 안쪽 (0,0,0) 방향으로 1만큼)
        Vector3 warningPosition = GetWarningPosition(side);
        Instantiate(Warning, warningPosition, Quaternion.identity);

        // Warning 사운드 재생 추가
        if (SFX_Manager.instance != null)
        {
            SFX_Manager.instance.PlayWarningSound();
        }

        // 웨이브 생성 중심점 설정 (Warning 위치를 기준으로!)
        waveSpawnCenter = GetWaveSpawnCenterFromWarning(warningPosition);

        // 5초 대기
        yield return new WaitForSeconds(5f);

        // 랜덤 적 타입 선택
        GameObject[] enemies = { Enemy1, Enemy2, Enemy3 };
        GameObject selectedEnemy = enemies[Random.Range(0, enemies.Length)];

        if (selectedEnemy == null) yield break;

        // 0.1초마다 적 생성
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Vector3 spawnPosition = GetRandomPositionInCircle(waveSpawnCenter, 2.5f); // 반지름 2.5 (지름 5)
            GameObject spawnedEnemy = Instantiate(selectedEnemy, spawnPosition, Quaternion.identity);

            // moveSpeed 설정
            SetEnemyMoveSpeed(spawnedEnemy);

            yield return new WaitForSeconds(0.1f);
        }
    }

    Vector3 GetWarningPosition(int side)
    {
        if (mainCamera == null) return Vector3.zero;

        float cameraHeight = mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;
        Vector3 cameraPos = mainCamera.transform.position;

        Vector3 warningPos = Vector3.zero;

        switch (side)
        {
            case 0: // 왼쪽
                warningPos = new Vector3(cameraPos.x - cameraWidth, Random.Range(cameraPos.y - cameraHeight, cameraPos.y + cameraHeight), 0);
                break;
            case 1: // 오른쪽
                warningPos = new Vector3(cameraPos.x + cameraWidth, Random.Range(cameraPos.y - cameraHeight, cameraPos.y + cameraHeight), 0);
                break;
            case 2: // 위
                warningPos = new Vector3(Random.Range(cameraPos.x - cameraWidth, cameraPos.x + cameraWidth), cameraPos.y + cameraHeight, 0);
                break;
            case 3: // 아래
                warningPos = new Vector3(Random.Range(cameraPos.x - cameraWidth, cameraPos.x + cameraWidth), cameraPos.y - cameraHeight, 0);
                break;
        }

        // (0,0,0) 방향으로 1만큼 안쪽으로
        Vector3 directionToCenter = (Vector3.zero - warningPos).normalized;
        warningPos += directionToCenter * 1f;

        return warningPos;
    }

    Vector3 GetWaveSpawnCenterFromWarning(Vector3 warningPos)
    {
        // Warning 위치에서 (0,0) 반대 방향으로 2만큼 밀기
        // (카메라 경계 안쪽 1 + 카메라 경계 밖 1 = 총 2)
        Vector3 directionFromCenter = (warningPos - Vector3.zero).normalized;
        return warningPos + directionFromCenter * 5f;
    }

    Vector3 GetRandomPositionInCircle(Vector3 center, float radius)
    {
        // 원 안의 랜덤 위치 생성
        Vector2 randomInCircle = Random.insideUnitCircle * radius;
        return center + new Vector3(randomInCircle.x, randomInCircle.y, 0);
    }

    void SpawnRandomEnemy()
    {
        // Enemy1, Enemy2, Enemy3 중 랜덤 선택
        GameObject[] enemies = { Enemy1, Enemy2, Enemy3 };
        GameObject selectedEnemy = enemies[Random.Range(0, enemies.Length)];
        if (selectedEnemy == null) return;

        // 카메라 경계 밖 위치 계산
        Vector3 spawnPosition = GetSpawnPositionOutsideCamera();

        // Enemy 생성
        GameObject spawnedEnemy = Instantiate(selectedEnemy, spawnPosition, Quaternion.identity);

        // moveSpeed 설정
        SetEnemyMoveSpeed(spawnedEnemy);
    }

    void SetEnemyMoveSpeed(GameObject enemy)
    {
        // 생성된 Enemy의 moveSpeed 설정
        if (enemy.TryGetComponent<EnemyController1>(out EnemyController1 enemy1))
        {
            enemy1.moveSpeed = currentMoveSpeed;
        }
        else if (enemy.TryGetComponent<EnemyController2>(out EnemyController2 enemy2))
        {
            enemy2.moveSpeed = currentMoveSpeed;
        }
        else if (enemy.TryGetComponent<EnemyController3>(out EnemyController3 enemy3))
        {
            enemy3.moveSpeed = currentMoveSpeed;
        }
    }

    Vector3 GetSpawnPositionOutsideCamera()
    {
        if (mainCamera == null) return Vector3.zero;

        // 카메라 경계 계산
        float cameraHeight = mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;
        Vector3 cameraPos = mainCamera.transform.position;

        // 4개의 면(왼쪽, 오른쪽, 위, 아래) 중 랜덤 선택
        int side = Random.Range(0, 4);
        Vector3 spawnPos = Vector3.zero;

        switch (side)
        {
            case 0: // 왼쪽 면
                {
                    float x = cameraPos.x - cameraWidth;
                    float y = Random.Range(cameraPos.y - cameraHeight, cameraPos.y + cameraHeight);
                    spawnPos = new Vector3(x, y, 0);
                    // (0,0,0) 방향으로 extraDistance만큼 더 밀기
                    Vector3 directionFromCenter = (spawnPos - Vector3.zero).normalized;
                    spawnPos += directionFromCenter * extraDistance;
                    break;
                }
            case 1: // 오른쪽 면
                {
                    float x = cameraPos.x + cameraWidth;
                    float y = Random.Range(cameraPos.y - cameraHeight, cameraPos.y + cameraHeight);
                    spawnPos = new Vector3(x, y, 0);
                    Vector3 directionFromCenter = (spawnPos - Vector3.zero).normalized;
                    spawnPos += directionFromCenter * extraDistance;
                    break;
                }
            case 2: // 위쪽 면
                {
                    float x = Random.Range(cameraPos.x - cameraWidth, cameraPos.x + cameraWidth);
                    float y = cameraPos.y + cameraHeight;
                    spawnPos = new Vector3(x, y, 0);
                    Vector3 directionFromCenter = (spawnPos - Vector3.zero).normalized;
                    spawnPos += directionFromCenter * extraDistance;
                    break;
                }
            case 3: // 아래쪽 면
                {
                    float x = Random.Range(cameraPos.x - cameraWidth, cameraPos.x + cameraWidth);
                    float y = cameraPos.y - cameraHeight;
                    spawnPos = new Vector3(x, y, 0);
                    Vector3 directionFromCenter = (spawnPos - Vector3.zero).normalized;
                    spawnPos += directionFromCenter * extraDistance;
                    break;
                }
        }

        return spawnPos;
    }
}