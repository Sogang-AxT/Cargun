using UnityEngine;

public class CargoHitBox : MonoBehaviour {
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Enemy")) {
            if (Cargo.Instance != null) {
                Cargo.Instance.DecreaseCargo();
            }
            
            Destroy(other.gameObject);
        }
    }
}