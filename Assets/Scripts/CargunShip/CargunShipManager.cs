using UnityEngine;
using UnityEngine.Events;

public class CargunShipManager : MonoBehaviour {
    public static UnityEvent<bool> OnTurretActivate = new();
    public static UnityEvent<GCEnumManager.PROJECTILE_TYPE, float, GCEnumManager.TURRET_TYPE> OnProjectileItemGet = new();
    public static UnityEvent<GCEnumManager.ITEM_TYPE> OnBeffItemGet = new();
}