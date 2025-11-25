using System.Collections;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

public class EnemySpawnController : MonoBehaviour {
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private bool collectionCheck;
    [SerializeField] private int defaultPoolCapacity;
    [SerializeField] private int maxSize;
    
    private IObjectPool<GameObject> enemySpawnPool;
    private float _enemySpawnTime;
    
    
    private void Init() {
        this.enemySpawnPool = new ObjectPool<GameObject>(
            EnemySpawn, OnGetFromPool, OnReleaseToPool, OnDestroyPooledEnemy,
            this.collectionCheck, this.defaultPoolCapacity, this.maxSize);
        
        this._enemySpawnTime = 2f;
        // EnemyManager.OnEnemySpawnActivate.AddListener(EnemySpawn);
    }
    
    private void Awake() {
        Init();
    }

    private GameObject EnemySpawn() {
        throw new System.NotImplementedException();
    }

    private void OnGetFromPool(GameObject obj) {
        throw new System.NotImplementedException();
    }

    private void OnReleaseToPool(GameObject obj) {
        throw new System.NotImplementedException();
    }
    
    private void OnDestroyPooledEnemy(GameObject obj) {
        throw new System.NotImplementedException();
    }
    
    // private void EnemySpawn(bool isActivate) {
    //     if (!isActivate) {
    //         return;
    //     }
    //     
    //     StartCoroutine(EnemySpawnPooling());
    // }
    //
    // private IEnumerator EnemySpawnPooling() {
    //     yield return new WaitForSeconds(this._enemySpawnTime);
    // }
}