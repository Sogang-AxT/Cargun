using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

public class EnemySpawnController : MonoBehaviour {
    [SerializeField] private EnemyController enemyPrefab;   // TODO: List<> -> Random.Range()
    [SerializeField] private bool collectionCheck;
    [SerializeField] private int defaultPoolCapacity;
    [SerializeField] private int maxSize;
    [SerializeField] private float enemySpawnTime;
    
    [Space(10f)] 
    
    [SerializeField] private List<Transform> spawnPivots;
    
    private IObjectPool<EnemyController> _enemySpawnPool;
    
    
    private void Init() {
        this._enemySpawnPool = new ObjectPool<EnemyController>(
            EnemySpawn, OnGetFromPool, OnReleaseToPool, OnDestroyPooledEnemy, 
            this.collectionCheck, this.defaultPoolCapacity, this.maxSize);
        
        EnemyManager.OnEnemySpawnActivate.AddListener(EnemySpawnActivate);
    }
    
    private void Awake() {
        Init();
    }
    
    private EnemyController EnemySpawn() {
        var enemyInstance = Instantiate(this.enemyPrefab);
        enemyInstance.EnemySpawnPool = this._enemySpawnPool;

        return enemyInstance;
    }

    private void OnReleaseToPool(EnemyController obj) {
        obj.gameObject.transform.position = obj.InitPos;
        obj.gameObject.SetActive(false);
    }
    
    private void OnGetFromPool(EnemyController obj) {
        obj.gameObject.SetActive(true);
    }
    
    private void OnDestroyPooledEnemy(EnemyController obj) {
        Destroy(obj.gameObject);
    }
    
    private void EnemySpawnActivate(bool isActivate) {
        StartCoroutine(EnemySpawnPooling(isActivate));
    }
    
    private IEnumerator EnemySpawnPooling(bool isActivate) {
        while (true) {
            if (!isActivate) {
                yield break;
            }
            
            yield return new WaitForSeconds(this.enemySpawnTime);
            
            var enemy = this._enemySpawnPool.Get();
            
            // 적 출현 포지션 무작위화
            var spawnPivot = this.spawnPivots[Random.Range(0, this.spawnPivots.Count)];
            var spawnRadius = 5f;
            
            var spawnRandomOffset = Random.insideUnitCircle * spawnRadius;
            var spawnPosition = (Vector2)spawnPivot.position + spawnRandomOffset;

            enemy.transform.position = spawnPosition;

            if (!enemy) {
                yield break;
            } 
        }
    }
}