using UnityEngine;

public abstract class Item : MonoBehaviour {
    protected void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Bullet")) {
            ItemActivate();
            Destroy(gameObject);
        }
    }

    protected abstract void ItemActivate();
}