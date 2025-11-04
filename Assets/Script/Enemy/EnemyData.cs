using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyData : MonoBehaviour
{
    public static EnemyData Instance;

    [Header("Enemy Prefab")]
    public GameObject EnemyObject;

    [Header("Spawn Settings")]
    public float spawnInterval = 2f; // 2�ʸ��� ����
    public int maxEnemies = 10; // �ִ� 10����

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

    // ���� ����
    public void StartSpawning()
    {
        isSpawning = true;
        StartCoroutine(SpawnRoutine());
    }

    // ���� ����
    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }

    // ��� �� ����
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

    // ���� ��ƾ
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

    // �� ����
    void SpawnEnemy()
    {
        if (EnemyObject == null) return;

        // �¿� ����
        bool spawnLeft = Random.value > 0.5f;
        float spawnX = spawnLeft ? leftSpawnRange.x : rightSpawnRange.x;
        float spawnY = Random.Range(spawnYMin, spawnYMax);

        Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0);

        GameObject enemy = Instantiate(EnemyObject, spawnPosition, Quaternion.identity);
        activeEnemies.Add(enemy);

        // EnemyUnit ��ũ��Ʈ�� ���� ����
        EnemyController enemyController = enemy.GetComponent<EnemyController>();
        if (enemyController != null)
        {
            enemyController.SetDirection(spawnLeft ? 1 : -1); // 1 = ������, -1 = ����
        }
    }

    // ���� �׾��� �� ����Ʈ���� ����
    public void RemoveEnemy(GameObject enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }
    }
}