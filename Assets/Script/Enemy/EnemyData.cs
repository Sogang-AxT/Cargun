using UnityEngine;

[CreateAssetMenu(fileName = "Enemy_", menuName = "ScriptableObjects/Enemy", order = 1)]
public class EnemyData : ScriptableObject {
    public GC_EnumManager.ENEMY_TYPE enemyType;
    
    public float moveSpeed;
    public float hp;

    public float zigzagAmplitude;
    public float zigzagFrequency;
}