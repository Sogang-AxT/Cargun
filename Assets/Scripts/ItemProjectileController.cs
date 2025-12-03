using UnityEngine;

public class ItemProjectileController : Item {
    [SerializeField] private GCEnumManager.PROJECTILE_TYPE projectileType;
    [SerializeField] private float usableTime;


    protected override void ItemActivate() {
        return;
    }

    protected override void ItemActivate(GCEnumManager.TURRET_TYPE turretId) {
        CargunShipManager.OnProjectileItemGet.Invoke(this.projectileType, this.usableTime, turretId);
    }
}