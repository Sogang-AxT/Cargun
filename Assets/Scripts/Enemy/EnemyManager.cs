using UnityEngine;
using UnityEngine.Events;

public class EnemyManager : MonoBehaviour {
    public const string BulletTag = "Bullet";
    public const string ShipTag = "Player";

    public static UnityEvent<bool> OnEnemySpawnActivate = new();
    public static GameObject PlayerShip = GameObject.FindWithTag(ShipTag);
}