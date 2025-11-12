using UnityEngine;

// TODO: 화물 체력 -> 함선 체력
public class CargunShipHitController : MonoBehaviour {
    private void Init() {
        CargoActivate(true);
        //CargunShipManager.OnCargoActivate.AddListener(CargoActivate);
    }
    
    private void Awake() {
        Init();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Enemy")) {
            CargoActivate(false);
        }
    }
    
    private void CargoActivate(bool isActivate) {
        this.gameObject.SetActive(isActivate);
    }
}