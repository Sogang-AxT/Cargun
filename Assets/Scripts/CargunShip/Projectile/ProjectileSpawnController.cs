using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ProjectileSpawnController : MonoBehaviour {
    private Dictionary<GCEnumManager.PROJECTILE_TYPE, IObjectPool<Projectile>> _projectileSpawnPoolDic;

    [SerializeField] private Projectile defaultProjectile;
    [SerializeField] private Projectile laserProjectile;
    [SerializeField] private Projectile rocketProjectile;
    [SerializeField] private Projectile shotgunProjectile;


    private void Init() {
        this._projectileSpawnPoolDic = new() {
            {
                GCEnumManager.PROJECTILE_TYPE.DEFAULT, new ObjectPool<Projectile>(
                    createFunc: () => {
                        var projectileInstance = Instantiate(this.defaultProjectile);
                        projectileInstance.Init(this._projectileSpawnPoolDic[GCEnumManager.PROJECTILE_TYPE.DEFAULT]);
                        return projectileInstance;
                    }, /* actionOnGet: OnGetFromPool,*/
                    actionOnRelease: OnReleaseToPool, actionOnDestroy: OnDestroyPooledBullet, maxSize: 100)
            }, {
                GCEnumManager.PROJECTILE_TYPE.LASER, new ObjectPool<Projectile>(
                    createFunc: () => {
                        var projectileInstance = Instantiate(this.laserProjectile);
                        projectileInstance.Init(this._projectileSpawnPoolDic[GCEnumManager.PROJECTILE_TYPE.LASER]);
                        return projectileInstance;
                    }, /* actionOnGet: OnGetFromPool,*/
                    actionOnRelease: OnReleaseToPool, actionOnDestroy: OnDestroyPooledBullet, maxSize: 100)
            }, {
                GCEnumManager.PROJECTILE_TYPE.ROCKET, new ObjectPool<Projectile>(
                    createFunc: () => {
                        var projectileInstance = Instantiate(this.rocketProjectile);
                        projectileInstance.Init(this._projectileSpawnPoolDic[GCEnumManager.PROJECTILE_TYPE.ROCKET]);
                        return projectileInstance;
                    }, /* actionOnGet: OnGetFromPool,*/
                    actionOnRelease: OnReleaseToPool, actionOnDestroy: OnDestroyPooledBullet, maxSize: 100)
            }, {
                GCEnumManager.PROJECTILE_TYPE.SHOTGUN, new ObjectPool<Projectile>(
                    createFunc: () => {
                        var projectileInstance = Instantiate(this.shotgunProjectile);
                        projectileInstance.Init(this._projectileSpawnPoolDic[GCEnumManager.PROJECTILE_TYPE.SHOTGUN]);
                        return projectileInstance;
                    }, /* actionOnGet: OnGetFromPool,*/
                    actionOnRelease: OnReleaseToPool, actionOnDestroy: OnDestroyPooledBullet, maxSize: 100)
            }
        };
        
        // Pre-warm
        var prewarmCount = 20; 
        
        foreach (var poolPair in _projectileSpawnPoolDic) {
            var pool = poolPair.Value;
            var tempProjectiles = new List<Projectile>();

            // 1. Instantiate
            for (var i = 0; i < prewarmCount; i++) {
                tempProjectiles.Add(pool.Get());
            }

            // 2. Release
            foreach (var projectile in tempProjectiles) {
                pool.Release(projectile);
            }
        }
    }

    private void Awake() {
        Init();
    }
    
    // private void OnGetFromPool(Projectile obj) {
    //     obj.gameObject.SetActive(true);
    // }

    private void OnReleaseToPool(Projectile obj) {
        obj.gameObject.SetActive(false);
    }

    private void OnDestroyPooledBullet(Projectile obj) {
        Destroy(obj.gameObject);
    }

    public Projectile GetProjectile(GCEnumManager.PROJECTILE_TYPE type, Vector3 muzzlePos, Quaternion muzzleRot) {
        var projectile = this._projectileSpawnPoolDic.ContainsKey(type) ? 
            this._projectileSpawnPoolDic[type].Get() : null;

        projectile?.gameObject.transform.SetPositionAndRotation(muzzlePos, muzzleRot);
        projectile?.gameObject.SetActive(true);
        
        return projectile;
    }
}