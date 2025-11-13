using System.Collections;
using UnityEngine;

public class EnemySpawnController : MonoBehaviour {
    private float _enemySpawnTime;

    
    private void Init() {
        this._enemySpawnTime = 2f;
        EnemyManager.OnEnemySpawnActivate.AddListener(EnemySpawn);
    }
    
    private void Awake() {
        Init();
    }

    private void EnemySpawn(bool isActivate) {
        if (!isActivate) {
            return;
        }
        
        StartCoroutine(EnemySpawnPooling());
    }
    
    private IEnumerator EnemySpawnPooling() {
        // TODO: Pooling
        yield return new WaitForSeconds(this._enemySpawnTime);
    }
}