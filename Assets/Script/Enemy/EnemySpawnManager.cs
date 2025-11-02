using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class EnemySpawnManager : MonoBehaviour {
    public static UnityEvent<bool> OnEnemySpawnActivate;

    private float _enemySpawnTime;
    

    private void Init() {
        this._enemySpawnTime = 2f;
        
        OnEnemySpawnActivate.AddListener(EnemySpawn);
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