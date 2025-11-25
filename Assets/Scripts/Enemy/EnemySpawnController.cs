using System.Collections;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

public class EnemySpawnController : MonoBehaviour {
    [SerializeField] private EnemyController enemyPrefab;
    [SerializeField] private bool collectionCheck;
    [SerializeField] private int defaultPoolCapacity;
    [SerializeField] private int maxSize;
    
    private IObjectPool<EnemyController> enemySpawnPool;
    private float _enemySpawnTime;
    
    
    private void Init() {
        this.enemySpawnPool = new ObjectPool<EnemyController>(
            EnemySpawn, OnGetFromPool, OnReleaseToPool, OnDestroyPooledEnemy, 
            this.collectionCheck, this.defaultPoolCapacity, this.maxSize);
        
        this._enemySpawnTime = 2f;
        EnemyManager.OnEnemySpawnActivate.AddListener(EnemySpawnActivate);
    }
    
    private void Awake() {
        Init();
    }
    
    private EnemyController EnemySpawn() {
        var enemyInstance = Instantiate(this.enemyPrefab);
        enemyInstance.EnemySpawnPool = this.enemySpawnPool;

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
            
            yield return new WaitForSeconds(this._enemySpawnTime);
            
            var enemy = this.enemySpawnPool.Get();

            if (!enemy) {
                yield break;
            } 
        }
    }
}