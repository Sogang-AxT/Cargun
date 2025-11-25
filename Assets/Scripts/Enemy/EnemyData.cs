using UnityEngine;

[CreateAssetMenu(fileName = "Enemy_", menuName = "ScriptableObjects/Enemy", order = 1)]
public class EnemyData : ScriptableObject {
    public GCEnumManager.ENEMY_TYPE enemyType;
    public float speed;
    public float hp;
    public float dmg;
}