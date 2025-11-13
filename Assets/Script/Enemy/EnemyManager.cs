using UnityEngine;
using UnityEngine.Events;

public class EnemyManager : MonoBehaviour {
    public static UnityEvent<bool> OnEnemySpawnActivate = new();
    
}