using UnityEngine;

public class CargoHitController : MonoBehaviour {
    private void Init() {
        this.gameObject.SetActive(true);
    }
    
    private void Awake() {
        Init();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Enemy")) {
            this.gameObject.SetActive(false);
        }
    }
}