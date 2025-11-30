using UnityEngine;
using UnityEngine.Events;

public class CargunShipManager : MonoBehaviour {
    public static UnityEvent<bool> OnTurretActivate = new();
}