using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CargunShipManager : MonoBehaviour {
    public static UnityEvent<bool> OnTurretActivate;
    public static UnityEvent<bool> OnCargoActivate;
    
    private List<CargunShipTurretController> _turrets;

    
    private void Init() {
        
    }
    
    private void Awake() {
        Init();
    }
    
}