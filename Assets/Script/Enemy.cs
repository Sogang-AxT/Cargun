using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    public static Enemy Instance;

    [Header("Enemy Prefab")]
    public GameObject EnemyObject;

    [Header("Spawn Settings")]
    public float spawnInterval = 2f; // 2초마다 스폰
    public int maxEnemies = 10; // 최대 10마리

    [Header("Spawn Range")]
    public Vector2 leftSpawnRange = new Vector2(-10f, -5f); // x = -10, y = -5~5
    public Vector2 rightSpawnRange = new Vector2(10f, -5f); // x = 10, y = -5~5
    public float spawnYMin = -5f;
    public float spawnYMax = 5f;

    private bool isSpawning = false;
    private List<GameObject> activeEnemies = new List<GameObject>();

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

    // 스폰 시작
    public void StartSpawning()
    {
        isSpawning = true;
        StartCoroutine(SpawnRoutine());
    }

    // 스폰 중지
    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }

    // 모든 적 제거
    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        activeEnemies.Clear();
    }

    // 스폰 루틴
    IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            if (activeEnemies.Count < maxEnemies)
            {
                SpawnEnemy();
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // 적 스폰
    void SpawnEnemy()
    {
        if (EnemyObject == null) return;

        // 좌우 랜덤
        bool spawnLeft = Random.value > 0.5f;
        float spawnX = spawnLeft ? leftSpawnRange.x : rightSpawnRange.x;
        float spawnY = Random.Range(spawnYMin, spawnYMax);

        Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0);

        GameObject enemy = Instantiate(EnemyObject, spawnPosition, Quaternion.identity);
        activeEnemies.Add(enemy);

        // EnemyUnit 스크립트에 방향 전달
        EnemyUnit enemyUnit = enemy.GetComponent<EnemyUnit>();
        if (enemyUnit != null)
        {
            enemyUnit.SetDirection(spawnLeft ? 1 : -1); // 1 = 오른쪽, -1 = 왼쪽
        }
    }

    // 적이 죽었을 때 리스트에서 제거
    public void RemoveEnemy(GameObject enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }
    }
}