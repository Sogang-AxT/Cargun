using UnityEngine;

[CreateAssetMenu(fileName = "Projectile_", menuName = "ScriptableObjects/Projectile", order = 1)]
public class ProjectileData : ScriptableObject {
    public GCEnumManager.PROJECTILE_TYPE projectileType;
    public float velocity;
    public float damage;
    public float fireRate;
}