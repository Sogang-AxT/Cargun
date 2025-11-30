using UnityEngine;
using UnityEngine.Pool;

public abstract class Projectile : MonoBehaviour {
    [SerializeField] protected ProjectileData projectileData;

    protected IObjectPool<Projectile> ProjectileSpawnPool { get; private set; }
    protected GCEnumManager.PROJECTILE_TYPE ProjectileType { get; private set; }
    
    public float Velocity { get; private set; }
    public float Damage { get; private set; }
    public float FireRate { get; private set; }
    public int UsableTime { get; private set; }
    
    
    public void Init(IObjectPool<Projectile> pool) {
        this.ProjectileSpawnPool = pool;
        
        this.ProjectileType = this.projectileData.projectileType;
        this.Velocity = this.projectileData.velocity;
        this.Damage = this.projectileData.damage;
        this.FireRate = this.projectileData.fireRate;
        this.UsableTime = this.projectileData.usableTime;
    }

    protected void Update() {
        Shoot();
    }
    
    protected void Deactivate() {
        this.ProjectileSpawnPool.Release(this);
    }
    
    protected abstract void Shoot();
}